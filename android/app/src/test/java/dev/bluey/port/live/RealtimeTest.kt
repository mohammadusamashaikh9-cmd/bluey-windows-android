package dev.bluey.port.live
import com.google.gson.*
import dev.bluey.port.voice.*
import org.junit.Assert.*
import org.junit.Test
class RealtimeTest {
    private class Wire:RealtimeTransport {val sent=mutableListOf<JsonObject>();var closed=false;var accept=true
        override fun send(json:String):Boolean{sent.add(JsonParser.parseString(json).asJsonObject);return accept}
        override fun close(){closed=true}}
    private class Harness(limits:LiveLimits=LiveLimits()) {
        val wire=Wire();var time=0L;val events=mutableListOf<MockEvent>();val failures=mutableListOf<String>();var ready=0;var chirps=0;private var serial=0
        val engine=RealtimeEngine(wire,limits,{time},{events.add(it)},{failures.add(it)},{ready++},{chirps++})
        init{engine.bind(2,"turn")}
        fun event(type:String,extra:String="",id:String="e${serial++}"){engine.event("""{"type":"$type","event_id":"$id"${if(extra.isEmpty())"" else ",$extra"}}""")}
        fun configure(){event("session.created");event("session.updated",""""session":{"output_modalities":["text"],"tools":[],"audio":{"input":{"turn_detection":null}}}""")}
        fun audio(n:Int=5){repeat(n){engine.append(ByteArray(960))}}
        fun ask(){engine.ask(2,"turn");event("input_audio_buffer.committed");event("response.created",""""response":{"id":"r1","metadata":{"bluey_turn":"turn"}}""")}
    }
    @Test fun actualGaFlowStreamsReplyNotTranscriptionAndAccountsReportedUsage() {
        val h=Harness();h.configure();assertEquals(1,h.ready);assertEquals("session.update",h.wire.sent.first()["type"].asString)
        h.audio();h.ask();h.event("response.output_text.delta",""""response_id":"r1","delta":"Hello\nBluey"""","delta")
        h.event("response.output_text.delta",""""response_id":"r1","delta":"Hello\nBluey"""","delta")
        assertEquals(listOf("Hello Bluey"),h.events.map{it.text});assertEquals(1,h.chirps)
        h.event("conversation.item.input_audio_transcription.completed",""""transcript":"private input"""")
        assertEquals(1,h.events.size)
        h.event("response.done",""""response":{"id":"r1","status":"completed","usage":{"input_tokens":10,"output_tokens":3,"total_tokens":13}}""")
        assertEquals("done",h.events.last().kind);assertEquals(13L,h.engine.usage.totalTokens);assertEquals(100L,h.engine.usage.audioMs)
        assertEquals(1,h.wire.sent.count{it["type"].asString=="response.create"})
    }
    @Test fun emptyAudioRepeatedReleaseAndAckDoNotCreateExtraResponses() {
        val h=Harness();h.configure();h.engine.ask(2,"turn");assertEquals("empty",h.events.single().kind);assertFalse(h.wire.sent.any{it["type"].asString=="input_audio_buffer.commit"})
        h.audio();h.engine.ask(2,"turn");h.engine.ask(2,"turn");h.event("input_audio_buffer.committed");h.event("input_audio_buffer.committed")
        assertEquals(1,h.wire.sent.count{it["type"].asString=="response.create"})
    }
    @Test fun cancelSleepAndStaleResponseCannotEmitOrRestart() {
        val h=Harness();h.configure();h.audio();h.ask();h.engine.interrupt();h.engine.bind(3,"new")
        h.event("response.output_text.delta",""""response_id":"r1","delta":"late"""");assertTrue(h.events.isEmpty())
        h.engine.stop();h.event("session.created");h.engine.append(ByteArray(960));assertTrue(h.wire.closed);assertEquals(1,h.ready)
        assertEquals(listOf("stopped"),h.failures)
    }
    @Test fun providerErrorsUnexpectedAudioToolsBackpressureAndBadConfigurationStop() {
        for(type in listOf("error","response.output_audio.delta","response.function_call_arguments.delta")) {
            val h=Harness();h.configure();h.event(type);assertTrue(h.wire.closed)
        }
        val h=Harness();h.wire.accept=false;h.event("session.created");assertTrue(h.wire.closed)
        val bad=Harness();bad.event("session.created");bad.event("session.updated",""""session":{"output_modalities":["audio"],"tools":[],"audio":{"input":{"turn_detection":null}}}""");assertEquals(0,bad.ready);assertTrue(bad.wire.closed)
    }
    @Test fun localSessionAudioResponseAndFrameLimitsAreEnforced() {
        val time=Harness(LiveLimits(sessionSeconds=30));time.configure();time.time=30000;time.engine.check();assertEquals("session_limit",time.failures.single())
        val audio=Harness(LiveLimits(audioSeconds=1));audio.configure();audio.audio(51);assertEquals("audio_limit",audio.failures.single())
        val response=Harness(LiveLimits(responses=1));response.configure();response.audio();response.ask();response.event("response.done",""""response":{"id":"r1","status":"completed","usage":null}""");response.audio();response.engine.ask(2,"next");assertEquals("response_limit",response.failures.single())
        val malformed=Harness();malformed.engine.event("x".repeat(65537));assertEquals("provider_frame",malformed.failures.single())
    }
    @Test fun externalReadyKeepsAllExistingGesturesAndRejectsPostPauseCallbacks() {
        val c=VoiceController(MockProvider{_,_,_->emptyList()});c.configureLive(MockProvider{_,_,_->emptyList()},0);c.drain();c.toggle(1);val wake=c.snapshot
        c.tick(1000);assertEquals(VoiceMode.waking,c.snapshot.mode);c.accept(MockEvent(1001,wake.generation,wake.turnId,"ready"),1001)
        c.down(1100);c.up(1400);assertEquals(VoiceMode.thinking,c.snapshot.mode)
        val turn=c.snapshot;c.accept(MockEvent(1401,turn.generation,turn.turnId,"empty"),1401);assertEquals(VoiceMode.listening,c.snapshot.mode)
        c.pause(1500);c.accept(MockEvent(1600,turn.generation,turn.turnId,"delta","late"),1600);assertEquals(VoiceMode.asleep,c.snapshot.mode)
        c.configureLive(null,1601);c.toggle(1602);c.tick(2202);assertEquals(VoiceMode.listening,c.snapshot.mode)
    }
    @Test fun downsamplingFiltersOutOfBandEnergyAndProviderTimeoutsAreBounded() {
        fun energy(frequency:Double):Double {
            val data=mutableListOf<Byte>();Pcm24k(48000){data.addAll(it.toList())}.accept(ShortArray(48000){(10000*kotlin.math.sin(2*Math.PI*frequency*it/48000)).toInt().toShort()})
            return data.chunked(2).drop(100).map{((it[0].toInt() and 255)or(it[1].toInt() shl 8)).toShort().toDouble()}.map{it*it}.average()
        }
        assertTrue(energy(18000.0)<energy(3000.0)*.01)
        val h=Harness();h.time=10001;h.engine.check();assertEquals("session_config_timeout",h.failures.single())
        val reply=Harness();reply.configure();reply.audio();reply.engine.ask(2,"turn");reply.time=30001;reply.engine.check();assertEquals("response_timeout",reply.failures.single())
    }
    @Test fun pcmConversionIsChunkInvariantLittleEndianAndFramesAreBounded() {
        for(rate in listOf(16000,24000,44100,48000)) {
            val input=ShortArray(rate){(it%2000-1000).toShort()};val whole=mutableListOf<Byte>();val chunks=mutableListOf<Byte>()
            Pcm24k(rate){assertEquals(960,it.size);whole.addAll(it.toList())}.accept(input)
            val r=Pcm24k(rate){chunks.addAll(it.toList())};input.toList().chunked(137).forEach{r.accept(it.toShortArray())}
            assertEquals(whole,chunks);assertTrue(whole.size in 47040..48000)
        }
        var first:ByteArray?=null;Pcm24k(24000){first=it}.accept(ShortArray(480){0x1234});assertEquals(0x34,first!![0].toInt());assertEquals(0x12,first!![1].toInt())
        assertEquals(8976,Chirp.samples().size);assertTrue(Chirp.samples().all{kotlin.math.abs(it.toInt())<12000})
    }
}
