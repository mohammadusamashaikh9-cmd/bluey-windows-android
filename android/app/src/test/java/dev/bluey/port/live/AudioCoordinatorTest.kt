package dev.bluey.port.live
import android.Manifest
import android.os.Looper
import android.os.SystemClock
import com.google.gson.JsonObject
import dev.bluey.port.link.*
import dev.bluey.port.voice.*
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
class AudioCoordinatorTest {
    private class Lan:LiveLan {
        override var connected=true;override val messages=Channel<Message?>(8);val sent=mutableListOf<Pair<String,JsonObject>>()
        override fun send(type:String,payload:JsonObject):Boolean{sent.add(type to payload);return connected}
        fun message(type:String,p:JsonObject){messages.trySend(Wire.decode(Wire.encode(type,UUID.randomUUID().toString(),0,p).dropLast(1).toByteArray()))}
    }
    private class Capture:CaptureDevice {
        var callback:((ByteArray)->Unit)?=null;var stops=0
        override fun start(frame:(ByteArray)->Unit,failed:()->Unit){callback=frame}
        override fun cancel(){stops++};override suspend fun finish(){cancel()}
    }
    private fun idle(){Shadows.shadowOf(Looper.getMainLooper()).idle()}
    private fun advance(ms:Long){ShadowSystemClock.advanceBy(Duration.ofMillis(ms));idle()}
    @Test fun completedAnswerUsesExistingStatesCaptionAndOneTurnBudget(){scenario("reply")}
    @Test fun coalescedReplyAndDoneKeepPinkVisibleWithoutAudioHardware(){scenario("coalesced")}
    @Test fun pauseDiscardsStaleCaptureAndReplies(){scenario("pause")}
    @Test fun sleepDiscardsStaleCaptureAndReplies(){scenario("sleep")}
    @Test fun disconnectDiscardsStaleCaptureAndReplies(){scenario("disconnect")}
    @Test fun canceledHoldNeverCompletes(){scenario("cancel")}
    @Test fun fiveSecondLimitStopsCaptureWithoutAutomaticSubmission(){scenario("limit")}
    @Test fun desktopWithdrawalDiscardsStaleCaptureAndReplies(){scenario("revoke")}
    @Test fun legacyOpenAiGrantIsRejectedBeforeAnySocketOrMicrophone() {
        val context=RuntimeEnvironment.getApplication();Shadows.shadowOf(context).grantPermissions(Manifest.permission.RECORD_AUDIO)
        val lan=Lan();var factories=0;val live=LiveCoordinator(context,lan,VoiceController(),{factories++;Capture()},{_,_,_->factories++;error("disabled")})
        live.arm(true,false);idle();val consent=lan.sent.last().second["consentId"].asString
        lan.message("liveGrant",Wire.payload("authorizationId" to UUID.randomUUID().toString(),"consentId" to consent));idle()
        assertFalse(live.ui.value.armed);assertEquals(0,factories);live.close()
    }
    @Test fun quotaStopOffersOfflineWithoutRetryOrCapture() {
        val context=RuntimeEnvironment.getApplication();Shadows.shadowOf(context).grantPermissions(Manifest.permission.RECORD_AUDIO)
        val lan=Lan();var factories=0;val live=LiveCoordinator(context,lan,VoiceController(),{factories++;Capture()},{_,_,_->factories++;error("disabled")})
        live.arm(true,false);idle();lan.message("liveStop",Wire.payload("code" to "limit"));idle()
        assertFalse(live.ui.value.armed);assertTrue(live.ui.value.status.contains("quota exhausted"));assertTrue(live.ui.value.status.contains("Offline simulation"));assertEquals(0,factories)
        val count=lan.sent.size;live.resume();advance(2000);assertEquals(count,lan.sent.size);live.close()
    }
    private fun scenario(end:String) {
        val context=RuntimeEnvironment.getApplication();Shadows.shadowOf(context).grantPermissions(Manifest.permission.RECORD_AUDIO)
        val lan=Lan();val voice=VoiceController();val capture=Capture();var native=0;var sockets=0
        val live=LiveCoordinator(context,lan,voice,{native++;capture},{_,_,_->sockets++;error("Gemini never creates an OpenAI socket")});live.chirps=false
        live.arm(true,false);idle();assertEquals(0,native);val consent=lan.sent.last().second["consentId"].asString;val auth=UUID.randomUUID().toString()
        val grant=Wire.payload("authorizationId" to auth,"consentId" to consent,"provider" to "GeminiFree").apply{addProperty("sessionSeconds",60)}
        lan.message("audioGrant",grant);idle()
        voice.toggle(SystemClock.elapsedRealtime());live.observe();live.observe();assertEquals(VoiceMode.listening,voice.snapshot.mode);assertEquals(0,native);assertFalse(lan.sent.any{it.first=="liveRequest"})
        voice.down(SystemClock.elapsedRealtime());advance(299);voice.tick(SystemClock.elapsedRealtime());live.observe();assertEquals(0,native)
        advance(1);voice.tick(SystemClock.elapsedRealtime());live.observe();assertEquals(VoiceMode.asking,voice.snapshot.mode);assertEquals(1,native)
        lan.message("audioGrant",grant);idle();assertEquals(VoiceMode.asking,voice.snapshot.mode);assertEquals(1,native)
        repeat(if(end=="limit")250 else 6){capture.callback!!(ByteArray(960))};idle()
        if(end=="limit"){assertTrue(capture.stops>0);assertFalse(lan.sent.any{it.first=="audioEnd"});val before=lan.sent.size;capture.callback!!(ByteArray(960));idle();assertEquals(before,lan.sent.size)}
        if(end=="cancel"){voice.cancelGesture(SystemClock.elapsedRealtime());live.observe();idle();assertFalse(lan.sent.any{it.first=="audioEnd"});assertFalse(live.ui.value.armed);assertTrue(capture.stops>0);live.close();return}
        voice.up(SystemClock.elapsedRealtime());live.observe();idle();assertEquals(VoiceMode.thinking,voice.snapshot.mode);assertEquals(1,lan.sent.count{it.first=="audioEnd"});assertTrue(capture.stops>0)
        val turn=voice.snapshot.turnId
        when(end){"pause"->live.pause();"sleep"->{voice.sleep(SystemClock.elapsedRealtime());live.observe()};"disconnect"->{lan.messages.trySend(null);idle()};"revoke"->{lan.message("liveStop",Wire.payload("code" to "stopped"));idle()}}
        val before=lan.sent.size;capture.callback!!(ByteArray(960));idle();assertEquals(before,lan.sent.size)
        val reply=Wire.payload("authorizationId" to auth,"turnId" to turn,"text" to "A completed synthetic answer.")
        lan.message("audioReply",reply);if(end=="coalesced")lan.message("audioDone",Wire.payload("authorizationId" to auth,"turnId" to turn));idle()
        if(end in setOf("reply","limit","coalesced")){
            assertEquals(VoiceMode.speaking,voice.snapshot.mode);assertEquals("A completed synthetic answer.",voice.snapshot.caption)
            assertEquals(0xFF9AD0,ModeIndicator.look(voice.snapshot.mode).rgb)
            if(end=="coalesced"){advance(16);assertEquals(VoiceMode.speaking,voice.snapshot.mode);advance(384)}
            lan.message("audioReply",reply);idle();assertEquals("A completed synthetic answer.",voice.snapshot.caption)
            lan.message("audioDone",Wire.payload("authorizationId" to auth,"turnId" to turn));idle();assertEquals(VoiceMode.listening,voice.snapshot.mode)
            lan.message("audioDone",Wire.payload("authorizationId" to auth,"turnId" to turn));idle();assertEquals(VoiceMode.listening,voice.snapshot.mode)
            advance(14000);voice.tick(SystemClock.elapsedRealtime());assertEquals("",voice.snapshot.caption)
        }else{assertEquals(VoiceMode.asleep,voice.snapshot.mode);assertFalse(live.ui.value.armed)}
        assertEquals(0,sockets);live.close()
    }
}
