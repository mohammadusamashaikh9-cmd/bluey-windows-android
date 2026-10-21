package dev.bluey.port.link

import com.google.gson.*
import com.google.gson.stream.JsonReader
import com.google.gson.stream.JsonToken
import java.io.IOException
import java.io.StringReader
import java.math.BigDecimal
import java.nio.ByteBuffer
import java.nio.charset.CodingErrorAction
import java.util.UUID

class ProtocolException(message: String) : IOException(message)
data class LinkFace(val gazeX: Double, val gazeY: Double, val mood: String, val talk: Double, val atMs: Long)
data class Message(val type: String, val id: String, val session: String, val seq: Long, val payload: JsonObject)
private class RawNumber(private val raw: String) : Number() {
    override fun toByte()=raw.toBigDecimal().toByte();override fun toShort()=raw.toBigDecimal().toShort()
    override fun toInt()=raw.toBigDecimal().toInt();override fun toLong()=raw.toBigDecimal().toLong()
    override fun toFloat()=raw.toFloat();override fun toDouble()=raw.toDouble();override fun toString()=raw
}
object Wire {
    const val MAX_FRAME = 4096
    const val SERVICE = "_bluey-sec._tcp."
    val moods = setOf("listening", "resting", "thinking", "talking", "pointing", "happy", "sleepy")
    fun uuid(s: String) = runCatching { UUID.fromString(s).toString() == s }.getOrDefault(false)
    fun pin(s: String) = s.matches(Regex("[0-9a-f]{64}"))
    fun keys(p: JsonElement, vararg names: String) {
        if (!p.isJsonObject || p.asJsonObject.keySet() != names.toSet()) throw ProtocolException("fields")
    }
    fun text(p: JsonObject, key: String, max: Int = 128): String {
        val v = p[key] ?: throw ProtocolException("missing")
        if (!v.isJsonPrimitive || !v.asJsonPrimitive.isString) throw ProtocolException("text")
        return v.asString.also { if (it.isEmpty() || it.length > max || it.any { c -> c.isISOControl() } || it.codePoints().anyMatch { c -> c in 0xd800..0xdfff }) throw ProtocolException("text") }
    }
    fun integer(p: JsonObject, key: String): Long {
        val v = p[key] ?: throw ProtocolException("missing")
        if (!v.isJsonPrimitive || !v.asJsonPrimitive.isNumber) throw ProtocolException("integer")
        if (!v.asString.matches(Regex("0|[1-9][0-9]*"))) throw ProtocolException("integer")
        return v.asBigDecimal.longValueExact().also { if (it < 0) throw ProtocolException("integer") }
    }
    private fun id(p: JsonObject, key: String) { if (!uuid(text(p,key,36))) throw ProtocolException("identity") }
    fun face(m: Message): LinkFace {
        fun number(key: String, lo: Double, hi: Double): Double {
            val v = m.payload[key]
            if (v == null || !v.isJsonPrimitive || !v.asJsonPrimitive.isNumber) throw ProtocolException("number")
            return v.asDouble.also { if (!it.isFinite() || it < lo || it > hi) throw ProtocolException("range") }
        }
        val mood = text(m.payload,"mood",16); if (mood !in moods) throw ProtocolException("mood")
        return LinkFace(number("gazeX",-1.0,1.0),number("gazeY",-1.0,1.0),mood,number("talk",0.0,1.0),integer(m.payload,"atMs"))
    }
    fun decode(bytes: ByteArray): Message {
        try {
            if (bytes.isEmpty() || bytes.size > MAX_FRAME) throw ProtocolException("size")
            val text = Charsets.UTF_8.newDecoder().onMalformedInput(CodingErrorAction.REPORT)
                .onUnmappableCharacter(CodingErrorAction.REPORT).decode(ByteBuffer.wrap(bytes)).toString()
            if(text.startsWith("\uFEFF"))throw ProtocolException("json BOM")
            val r = JsonReader(StringReader(text)); r.setStrictness(Strictness.STRICT)
            fun value(depth: Int): JsonElement {
                if (depth > 4) throw ProtocolException("depth")
                return when (r.peek()) {
                    JsonToken.BEGIN_OBJECT -> {
                        val p = JsonObject(); r.beginObject()
                        while (r.hasNext()) { val k = r.nextName(); if (p.has(k)) throw ProtocolException("duplicate"); p.add(k,value(depth+1)) }
                        r.endObject(); p
                    }
                    JsonToken.STRING -> JsonPrimitive(r.nextString())
                    JsonToken.NUMBER -> JsonPrimitive(RawNumber(r.nextString()))
                    JsonToken.BOOLEAN -> JsonPrimitive(r.nextBoolean())
                    JsonToken.NULL -> { r.nextNull(); JsonNull.INSTANCE }
                    else -> throw ProtocolException("json")
                }
            }
            val e = value(0); if (r.peek() != JsonToken.END_DOCUMENT) throw ProtocolException("trailing")
            keys(e,"v","type","id","session","seq","payload"); val root = e.asJsonObject
            if (integer(root,"v") != 1L) throw ProtocolException("version")
            id(root,"id"); id(root,"session"); val p = root["payload"].asJsonObject
            val m = Message(text(root,"type",16),text(root,"id"),text(root,"session"),integer(root,"seq"),p)
            when(m.type) {
                "hello" -> { keys(p,"hostId","pin"); id(p,"hostId"); if (!pin(text(p,"pin"))) throw ProtocolException("pin") }
                "connect", "ready" -> { keys(p,"hostId","deviceId"); id(p,"hostId"); id(p,"deviceId") }
                "enroll" -> { keys(p,"deviceId","name","secret"); id(p,"deviceId"); text(p,"name",64); if (!text(p,"secret",43).matches(Regex("[A-Za-z0-9_-]{43}"))) throw ProtocolException("secret") }
                "approve" -> { keys(p,"requestId","clientPin"); id(p,"requestId"); if (!pin(text(p,"clientPin"))) throw ProtocolException("pin") }
                "confirm" -> { keys(p,"requestId"); id(p,"requestId") }
                "audioGrant","audioBegin","audioChunk","audioEnd","audioCancel","audioReply","audioDone" -> dev.bluey.port.live.AudioProtocol.validate(m.type,p)
                "liveConsent","liveRequest","liveGrant","liveToken","liveStop" -> dev.bluey.port.live.LiveProtocol.validate(m.type,p)
                "presentation" -> PresentationCodec.validate(p)
                "face" -> { keys(p,"gazeX","gazeY","mood","talk","atMs"); face(m) }
                "ping", "pong" -> keys(p)
                "error" -> { keys(p,"code"); if (text(p,"code") !in setOf("denied","expired","revoked","protocol","busy")) throw ProtocolException("error") }
                else -> throw ProtocolException("command")
            }
            return m
        } catch(e: Exception) { if (e is ProtocolException) throw e; throw ProtocolException("encoding/schema") }
    }
    fun encode(type: String, session: String, seq: Long, payload: JsonObject, id: String = UUID.randomUUID().toString()): ByteArray {
        val e = JsonObject().apply { addProperty("v",1);addProperty("type",type);addProperty("id",id);addProperty("session",session);addProperty("seq",seq);add("payload",payload) }
        val bytes = e.toString().toByteArray(Charsets.UTF_8);decode(bytes);return bytes + byteArrayOf(10)
    }
    fun payload(vararg pairs: Pair<String, String>) = JsonObject().apply { pairs.forEach { addProperty(it.first,it.second) } }
}
class Frames {
    private val buffer = ByteArray(Wire.MAX_FRAME); private var count = 0
    fun feed(bytes: ByteArray): List<ByteArray> {
        val frames = mutableListOf<ByteArray>()
        for (b in bytes) if (b == 10.toByte()) {
            if (count == 0) throw ProtocolException("empty");frames.add(buffer.copyOf(count));count=0
        } else { if (count == buffer.size) throw ProtocolException("size");buffer[count++]=b }
        return frames
    }
    fun end() { if (count != 0) throw ProtocolException("incomplete") }
}
class SequenceGuard(private val session: String) {
    private var next=0L;private val ids=LinkedHashSet<String>();private var firstAt:Long?=null;private var firstLocal:Long?=null;private var lastAt:Long?=null
    fun accept(m:Message) {
        if (m.session!=session || m.seq!=next || !ids.add(m.id)) throw ProtocolException("sequence")
        next++;if(ids.size>256)ids.remove(ids.first())
    }
    fun fresh(f:LinkFace,localMs:Long) {
        if(lastAt!=null && f.atMs<lastAt!!)throw ProtocolException("stale");lastAt=f.atMs
        if(firstAt==null){firstAt=f.atMs;firstLocal=localMs}
        val lag=(localMs-firstLocal!!)-(f.atMs-firstAt!!)
        if(lag>2500 || lag < -5000)throw ProtocolException("stale")
    }
}
class MessageRate {
    private var start=0L;private var count=0
    fun accept(now:Long){if(now-start>=1000){start=now;count=0};if(++count>120)throw ProtocolException("rate")}
}
