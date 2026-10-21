package dev.bluey.port.live
import com.google.gson.JsonObject
import dev.bluey.port.link.*
import java.util.Base64

object AudioProtocol {
    const val CHUNK_BYTES=960;const val MAX_CHUNKS=250;const val MAX_BYTES=240000
    const val FORMAT="pcm_s16le_24000_mono"
    val types=setOf("audioGrant","audioBegin","audioChunk","audioEnd","audioCancel","audioReply","audioDone")
    private fun id(p:JsonObject,k:String){if(!Wire.uuid(Wire.text(p,k)))throw ProtocolException("audio identity")}
    fun validate(type:String,p:JsonObject) {
        id(p,"authorizationId")
        if(type=="audioGrant") {
            Wire.keys(p,"authorizationId","consentId","provider","sessionSeconds");id(p,"consentId")
            if(Wire.text(p,"provider")!="GeminiFree"||Wire.integer(p,"sessionSeconds") !in 1..60)throw ProtocolException("audio grant");return
        }
        id(p,"turnId")
        when(type) {
            "audioBegin"->{Wire.keys(p,"authorizationId","turnId","format");if(Wire.text(p,"format")!=FORMAT)throw ProtocolException("audio format")}
            "audioChunk"->{Wire.keys(p,"authorizationId","turnId","index","data");if(Wire.integer(p,"index")>=MAX_CHUNKS)throw ProtocolException("audio index");decode(p).fill(0)}
            "audioEnd"->{Wire.keys(p,"authorizationId","turnId","chunks","bytes");val count=Wire.integer(p,"chunks");if(count !in 5..MAX_CHUNKS||Wire.integer(p,"bytes")!=count*CHUNK_BYTES)throw ProtocolException("audio length")}
            "audioCancel","audioDone"->Wire.keys(p,"authorizationId","turnId")
            "audioReply"->{Wire.keys(p,"authorizationId","turnId","text");if(Wire.text(p,"text",256).any{it.isSurrogate()})throw ProtocolException("audio text")}
            else->throw ProtocolException("audio command")
        }
    }
    fun decode(p:JsonObject):ByteArray {
        val text=Wire.text(p,"data",1280)
        val bytes=try{Base64.getDecoder().decode(text)}catch(_:IllegalArgumentException){throw ProtocolException("audio encoding")}
        if(bytes.size!=CHUNK_BYTES||Base64.getEncoder().encodeToString(bytes)!=text){bytes.fill(0);throw ProtocolException("audio encoding")};return bytes
    }
}
// Sends at most one 20ms frame per native callback; the existing LAN outbox has32 slots.
// It retains counters only, never a recording. No background upload or retry.
class AudioUpload(private val authorization:String,val turn:String,private val send:(String,JsonObject)->Boolean) {
    private var count=0;private var closed=false;private var begun=false
    private fun payload()=Wire.payload("authorizationId" to authorization,"turnId" to turn)
    @Synchronized fun begin():Boolean {
        if(begun||closed)return false;begun=true
        return send("audioBegin",payload().apply{addProperty("format",AudioProtocol.FORMAT)}).also{if(!it)closed=true}
    }
    @Synchronized fun append(bytes:ByteArray):Boolean {
        try {
            if(closed||!begun||count>=AudioProtocol.MAX_CHUNKS)return false
            if(bytes.size!=AudioProtocol.CHUNK_BYTES){closed=true;return false}
            val ok=send("audioChunk",payload().apply{addProperty("index",count);addProperty("data",Base64.getEncoder().encodeToString(bytes))})
            if(ok)count++ else closed=true;return ok
        } finally {bytes.fill(0)}
    }
    @Synchronized fun full()=count==AudioProtocol.MAX_CHUNKS
    @Synchronized fun complete():Boolean {
        if(closed||!begun||count<5){cancel();return false};closed=true
        return send("audioEnd",payload().apply{addProperty("chunks",count);addProperty("bytes",count*AudioProtocol.CHUNK_BYTES)})
    }
    @Synchronized fun cancel(){if(!closed&&begun)send("audioCancel",payload());closed=true}
    override fun toString()="[audio upload redacted]"
}
