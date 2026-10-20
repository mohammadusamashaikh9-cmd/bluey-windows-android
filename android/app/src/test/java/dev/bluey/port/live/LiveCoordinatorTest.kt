package dev.bluey.port.live
import com.google.gson.*
import dev.bluey.port.link.*
import dev.bluey.port.voice.*
import android.Manifest
import android.os.SystemClock
import android.os.Looper
import kotlinx.coroutines.channels.Channel
import org.junit.Test
import org.junit.Assert.*
import org.junit.runner.RunWith
import org.robolectric.*
import org.robolectric.annotation.Config
import org.robolectric.shadows.ShadowSystemClock
import java.time.Duration
import java.util.UUID
@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35])
class LiveCoordinatorTest {
    private class Lan:LiveLan {
        override var connected=true;override val messages=Channel<Message?>(8);val sent=mutableListOf<Pair<String,JsonObject>>()
        override fun send(type:String,payload:JsonObject):Boolean{sent.add(type to payload);return connected}
        fun message(type:String,payload:JsonObject){messages.trySend(Wire.decode(Wire.encode(type,UUID.randomUUID().toString(),0,payload).dropLast(1).toByteArray()))}
    }
    private class Capture:CaptureDevice {var starts=0;var cancels=0;var callback:((ByteArray)->Unit)?=null;var failure:(()->Unit)?=null
        override fun start(frame:(ByteArray)->Unit,failed:()->Unit){starts++;callback=frame;failure=failed}
        override fun cancel(){cancels++}
        override suspend fun finish(){cancel()}
    }
    private class Socket:RealtimeTransport {val sent=mutableListOf<String>();var closed=false
        override fun send(json:String):Boolean{sent.add(json);return true};override fun close(){closed=true}}
    private fun idle(){Shadows.shadowOf(Looper.getMainLooper()).idle()}
    private fun advance(ms:Long){ShadowSystemClock.advanceBy(Duration.ofMillis(ms));idle()}
    @Test fun offlineAndDeniedConsentDoNotConstructCaptureOrProvider() {
        val context=RuntimeEnvironment.getApplication();Shadows.shadowOf(context).denyPermissions(Manifest.permission.RECORD_AUDIO);val lan=Lan();val voice=VoiceController();var native=0
        val live=LiveCoordinator(context,lan,voice,{native++;Capture()},{_,_,_->native++;Socket()});live.chirps=false
        live.arm(false,false);idle();assertFalse(live.ui.value.armed);assertEquals(0,native)
        live.arm(true,false);idle();assertFalse(live.ui.value.armed);assertEquals(0,native);live.close()
    }
    @Test fun authenticatedGrantHoldOnlyPauseAndStaleCallbacksUseFakeNativeAdapters() {scenario(false)}
    @Test fun continuousContextStartsOnlyAfterSeparateConsentGrantAndReady() {scenario(true)}
    @Test fun explicitStopStopsCaptureAndProvider() {scenario(false,"stop")}
    @Test fun sleepStopsCaptureAndProvider() {scenario(false,"sleep")}
    @Test fun lanDisconnectStopsCaptureAndProvider() {scenario(false,"disconnect")}
    @Test fun desktopWithdrawalStopsCaptureAndProvider() {scenario(false,"withdraw")}
    @Test fun microphoneInterruptionStopsCaptureAndProvider() {scenario(false,"audio")}
    private fun scenario(continuous:Boolean,ending:String="pause") {
        val context=RuntimeEnvironment.getApplication();Shadows.shadowOf(context).grantPermissions(Manifest.permission.RECORD_AUDIO)
        val lan=Lan();val voice=VoiceController();val capture=Capture();val socket=Socket();var event:((String)->Unit)?=null;var factories=0
        val live=LiveCoordinator(context,lan,voice,{factories++;capture},{_,callback,_->event=callback;socket});live.chirps=false
        live.arm(true,continuous);idle();assertTrue(live.ui.value.armed);assertEquals(0,factories)
        val consent=lan.sent.last{it.first=="liveConsent"}.second["consentId"].asString;val auth=UUID.randomUUID().toString()
        lan.message("liveGrant",Wire.payload("authorizationId" to auth,"consentId" to consent));idle();assertTrue(live.ui.value.authorized)
        voice.toggle(SystemClock.elapsedRealtime());live.observe();val request=lan.sent.last{it.first=="liveRequest"}.second["requestId"].asString
        val token=Wire.payload("authorizationId" to auth,"requestId" to request,"secret" to "offline-test-token-not-real","model" to "gpt-realtime-2.1-mini").apply{
            addProperty("expiresAt",System.currentTimeMillis()/1000+30);addProperty("sessionSeconds",300);addProperty("responses",5);addProperty("audioSeconds",60);addProperty("maxTokens",256)}
        lan.message("liveToken",token);idle();assertNotNull(event);assertEquals(0,factories)
        event!!("""{"type":"session.created","event_id":"s1"}""");idle()
        event!!("""{"type":"session.updated","event_id":"s2","session":{"output_modalities":["text"],"tools":[],"audio":{"input":{"turn_detection":null}}}}""");idle()
        assertEquals(VoiceMode.listening,voice.snapshot.mode);assertEquals(if(continuous)1 else 0,factories)
        voice.down(SystemClock.elapsedRealtime());advance(300);voice.tick(SystemClock.elapsedRealtime());live.observe()
        assertEquals(VoiceMode.asking,voice.snapshot.mode);assertEquals(1,capture.starts)
        repeat(6){capture.callback!!(ByteArray(960))};voice.up(SystemClock.elapsedRealtime());live.observe();idle()
        assertEquals(VoiceMode.thinking,voice.snapshot.mode);assertTrue(socket.sent.any{it.contains("input_audio_buffer.commit")})
        when(ending){
            "sleep"->{voice.sleep(SystemClock.elapsedRealtime());live.observe()}
            "disconnect"->{lan.messages.trySend(null);idle()}
            "withdraw"->{lan.message("liveStop",Wire.payload("code" to "stopped"));idle()}
            "audio"->{capture.failure!!();idle()}
            "stop"->live.stop()
            else->live.pause()
        }
        val before=socket.sent.size;capture.callback!!(ByteArray(960));event!!("""{"type":"session.created","event_id":"late"}""");idle()
        assertTrue(socket.closed);assertTrue(capture.cancels>0);assertEquals(before,socket.sent.size);assertEquals(VoiceMode.asleep,voice.snapshot.mode);assertFalse(live.ui.value.armed)
        live.resume();idle();assertEquals(before,socket.sent.size);live.close()
    }
}
