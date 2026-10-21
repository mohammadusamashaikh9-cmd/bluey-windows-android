package dev.bluey.port.live

import com.google.gson.*
import java.util.UUID
import java.util.Base64
import dev.bluey.port.voice.MockEvent

interface RealtimeTransport {
    fun send(json: String): Boolean
    fun close()
}
data class Usage(val inputTokens: Long = 0, val outputTokens: Long = 0,
    val totalTokens: Long = 0, val responses: Int = 0, val audioMs: Long = 0)

/** Pure reducer shared by fake transport tests and the native WebSocket adapter. */
class RealtimeEngine(
    private val wire: RealtimeTransport,
    private val limits: LiveLimits,
    private val now: () -> Long,
    private val delivery: (MockEvent) -> Unit,
    private val failure: (String) -> Unit,
    private val ready: () -> Unit,
    private val chirp: () -> Unit = {},
) {
    private var closed = false
    private var configured = false
    private var created = false
    private val started = now()
    private var generation = 0L
    private var turn = ""
    private var response: String? = null
    private var pending = false
    private var committed = false
    private var pendingAt = 0L
    private var frames = 0
    private var totalFrames = 0
    private var textSize = 0
    private var chirped = false
    private val seen = LinkedHashSet<String>()
    private val ended = LinkedHashSet<String>()
    @Volatile var usage = Usage()
        private set

    @Synchronized fun bind(g: Long, id: String) { generation = g; turn = id }
    private fun send(type: String, extra: JsonObject = JsonObject()): Boolean {
        if (closed) return false
        extra.addProperty("type", type)
        extra.addProperty("event_id", "bluey_" + UUID.randomUUID())
        if (!wire.send(extra.toString())) { stop("backpressure"); return false }
        return true
    }
    @Synchronized fun stop(code: String = "stopped") {
        if (closed) return
        closed = true; pending = false; response = null
        wire.close(); failure(code)
    }
    @Synchronized fun check() {
        if (now() - started >= limits.sessionSeconds * 1000L) stop("session_limit")
        else if (!configured && now() - started > 10000) stop("session_config_timeout")
        else if (pending && now() - pendingAt > 30000) stop("response_timeout")
    }
    @Synchronized fun append(pcm: ByteArray) {
        if (closed || !configured) return
        if (pcm.size != 960) { stop("audio_format"); return }
        if (totalFrames >= limits.audioSeconds * 50) { stop("audio_limit"); return }
        if (send("input_audio_buffer.append", JsonObject().apply {
            addProperty("audio", Base64.getEncoder().encodeToString(pcm))
        })) {
            frames++; totalFrames++
            usage = usage.copy(audioMs = totalFrames * 20L)
        }
    }
    @Synchronized fun interrupt() {
        if (closed) return
        if (pending) send("response.cancel")
        pending = false
        response?.let { ended.add(it) }
        response = null; textSize = 0; chirped = false
    }
    @Synchronized fun clearInput() { frames = 0; send("input_audio_buffer.clear") }
    @Synchronized fun ask(g: Long, id: String) {
        if (closed || !configured || pending) return
        bind(g, id)
        if (frames < 5) { delivery(MockEvent(now(), g, id, "empty")); return }
        if (usage.responses >= limits.responses) { stop("response_limit"); return }
        pending = true; pendingAt = now(); committed = false; textSize = 0; chirped = false
        send("input_audio_buffer.commit")
        frames = 0
    }
    @Synchronized fun event(raw: String) {
        if (closed) return
        if (raw.length > 65536) { stop("provider_frame"); return }
        try {
            val p = JsonParser.parseString(raw).asJsonObject
            val type = p["type"].asString
            val eventId = p["event_id"]?.asString ?: throw IllegalArgumentException()
            require(eventId.length in 1..128)
            if (!seen.add(eventId)) return
            if (seen.size > 256) seen.remove(seen.first())
            when (type) {
                "session.created" -> {
                    if (created) return
                    created = true
                    val config = JsonParser.parseString("""{"type":"realtime","output_modalities":["text"],"tools":[],"tool_choice":"none","tracing":null,"audio":{"input":{"format":{"type":"audio/pcm","rate":24000},"turn_detection":null,"transcription":null}}}""").asJsonObject
                    config.addProperty("max_output_tokens", limits.maxTokens)
                    send("session.update", JsonObject().apply { add("session", config) })
                }
                "session.updated" -> {
                    val s = p["session"].asJsonObject
                    val input = s["audio"].asJsonObject["input"].asJsonObject
                    if (!created || s["output_modalities"].asJsonArray.map { it.asString } != listOf("text") ||
                        s["tools"].asJsonArray.size() != 0 || !input["turn_detection"].isJsonNull) {
                        stop("session_config"); return
                    }
                    if (!configured) { configured = true; ready() }
                }
                "input_audio_buffer.committed" -> {
                    if (pending && !committed) {
                        committed = true
                        usage = usage.copy(responses = usage.responses + 1)
                        val config = JsonObject().apply {
                            add("output_modalities", JsonArray().apply { add("text") })
                            addProperty("max_output_tokens", limits.maxTokens)
                            add("metadata", JsonObject().apply { addProperty("bluey_turn", turn) })
                        }
                        send("response.create", JsonObject().apply { add("response", config) })
                    }
                }
                "response.created" -> {
                    val r = p["response"].asJsonObject
                    val id = r["id"].asString
                    if (pending && committed && response == null && id.length in 1..128 && id !in ended &&
                        r["metadata"]?.asJsonObject?.get("bluey_turn")?.asString == turn) response = id
                }
                "response.output_text.delta" -> {
                    if (!pending || p["response_id"].asString != response) return
                    val clean = p["delta"].asString.map {
                        if (it == '\n' || it == '\r' || it == '\t') ' ' else it
                    }.filterNot { it.isISOControl() }.joinToString("")
                    if (textSize + clean.length > 2048) { stop("caption_limit"); return }
                    textSize += clean.length
                    var offset = 0
                    while (offset < clean.length) {
                        var end = minOf(offset + 256, clean.length)
                        if (end < clean.length && clean[end - 1].isHighSurrogate()) end--
                        if (end == offset) { stop("unicode"); return }
                        delivery(MockEvent(now(), generation, turn, "delta", clean.substring(offset, end)))
                        offset = end
                    }
                    if (clean.isNotBlank() && !chirped) { chirped = true; chirp() }
                }
                "response.done" -> {
                    val r = p["response"].asJsonObject
                    if (!pending || r["id"].asString != response) return
                    ended.add(response!!)
                    if (ended.size > 64) ended.remove(ended.first())
                    pending = false; response = null
                    r["usage"]?.takeUnless { it.isJsonNull }?.asJsonObject?.let { u ->
                        fun n(k: String) = u[k].asLong.also { require(it in 0..1000000000) }
                        usage = usage.copy(inputTokens = usage.inputTokens + n("input_tokens"),
                            outputTokens = usage.outputTokens + n("output_tokens"), totalTokens = usage.totalTokens + n("total_tokens"))
                    }
                    if (r["status"].asString !in setOf("completed", "incomplete")) { stop("response_failed"); return }
                    delivery(MockEvent(now(), generation, turn, if (textSize == 0) "empty" else "done"))
                }
                "error" -> {
                    val code = p["error"]?.asJsonObject?.get("code")?.takeUnless { it.isJsonNull }?.asString
                    if (code == "input_audio_buffer_commit_empty" && pending) {
                        pending = false; frames = 0
                        delivery(MockEvent(now(), generation, turn, "empty"))
                    } else stop("provider_error")
                }
                else -> {
                    if (type.contains("function_call") || type.contains("mcp_call") || type == "response.output_audio.delta")
                        stop("unsupported_output")
                }
            }
        } catch (_: Exception) { stop("provider_event") }
    }
}
