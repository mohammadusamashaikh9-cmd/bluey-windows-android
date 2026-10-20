package dev.bluey.port.live
import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.SystemClock
import androidx.core.content.ContextCompat
import dev.bluey.port.link.*
import dev.bluey.port.voice.*
import kotlinx.coroutines.*
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.flow.*
import java.util.UUID
import java.util.concurrent.atomic.AtomicLong

data class LiveUi(val armed:Boolean=false,val authorized:Boolean=false,val connected:Boolean=false,val continuous:Boolean=false,val status:String="Live OFF — offline simulation available",val elapsedMs:Long=0,val usage:Usage=Usage())
// Own controller transitions on Main; capture and native socket adapters run off rendering callbacks.
class LiveCoordinator(private val context:Context,private val port:LiveLan,private val voice:VoiceController,
    private val captureFactory:((CoroutineScope)->CaptureDevice)?=null,
    private val transportFactory:(LiveToken,(String)->Unit,()->Unit)->RealtimeTransport={token,event,failed->OpenAiSocket(token,event,failed)},private val allowOpenAi:Boolean=false) {
    constructor(context:Context,link:DeviceLink,voice:VoiceController):this(context,DeviceLiveLan(link),voice)
    private val scope=CoroutineScope(SupervisorJob()+Dispatchers.Main.immediate);private val packets=Channel<Pair<Long,String>>(8)
    private val mutable=MutableStateFlow(LiveUi());val ui:StateFlow<LiveUi> = mutable
    private var consent:String?=null;private var authorization:String?=null;private var request:String?=null
    private var audioMode=false;private var upload:AudioUpload?=null;private var audioDeadline=0L;private var audioReply=false;private var audioComplete=false
    private var continuous=false;var chirps=true;private var foreground=true;private var last=VoiceMode.asleep
    private var engine:RealtimeEngine?=null;private var capture:CaptureDevice?=null;private val chirp=LocalChirp(scope)
    private val captureEpoch=AtomicLong(0)
    @Volatile private var providerEpoch=0L;private var started=0L;private var requestAt=0L
    private var pendingGen=0L;private var pendingTurn=""
    private fun now()=SystemClock.elapsedRealtime()
    init {
        scope.launch{for(message in port.messages){if(message==null){if(mutable.value.armed)stop("LAN disconnected")}else lan(message)}}
        scope.launch{for(packet in packets)if(packet.first==providerEpoch)engine?.event(packet.second)}
        scope.launch{while(isActive){delay(200);engine?.check();if(audioMode&&now()>=audioDeadline)stop("Audio authorization/request expired; Start again explicitly");if(request!=null&&now()-requestAt>15000)stop("Credential request timed out")
            if(audioMode)mutable.update{it.copy(elapsedMs=now()-started)}
            if(engine!=null)mutable.update{it.copy(elapsedMs=now()-started,usage=engine?.usage?:it.usage)}}}
    }
    fun arm(cloud:Boolean,continuousAwake:Boolean) {
        stop("Live OFF")
        if(!foreground||!port.connected||!cloud||ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)!=PackageManager.PERMISSION_GRANTED){mutable.value=LiveUi(status="Live denied: foreground, authenticated Windows, microphone permission and cloud consent required");return}
        continuous=continuousAwake;consent=UUID.randomUUID().toString()
        voice.configureLive(MockProvider{_,g,t->commit(g,t);emptyList()},now());last=VoiceMode.asleep
        mutable.value=LiveUi(armed=true,continuous=continuous,status="Waiting for explicit Windows authorization; then double tap to wake")
        if(!port.send("liveConsent",Wire.payload("consentId" to consent!!).apply{addProperty("microphone",true);addProperty("cloud",true);addProperty("continuous",continuous)}))stop("LAN unavailable")
    }
    fun observe() {
        if(!mutable.value.armed)return
        val state=voice.snapshot;val changed=state.mode!=last;val old=last;last=state.mode
        if(!changed)return
        when(state.mode) {
            VoiceMode.waking->{if(audioMode){pendingGen=state.generation;pendingTurn=state.turnId;voice.accept(MockEvent(now(),pendingGen,pendingTurn,"ready"),now());return}
                if(authorization==null){stop("Authorize this phone on Windows first");return}
                pendingGen=state.generation;pendingTurn=state.turnId;request=UUID.randomUUID().toString();requestAt=now()
                if(!port.send("liveRequest",Wire.payload("authorizationId" to authorization!!,"consentId" to consent!!,"requestId" to request!!)))stop("LAN unavailable")
                else mutable.update{it.copy(status="Requesting short-lived credential; microphone OFF")}}
            VoiceMode.asking->{if(audioMode){startAudioTurn(state);return};engine?.interrupt();engine?.bind(state.generation,state.turnId);if(!continuous){engine?.clearInput();startCapture()}}
            VoiceMode.listening->if(!continuous&&old==VoiceMode.asking){
                if(audioMode){stop("Audio hold canceled — authorize a fresh turn");return}
                captureEpoch.incrementAndGet();capture?.cancel();capture=null;engine?.clearInput()
            }
            VoiceMode.asleep->if(old!=VoiceMode.asleep)stop("Slept — authorize a fresh live session to restart")
            else->Unit
        }
    }
    private fun startCapture() {
        if(capture!=null||engine==null||!foreground||!mutable.value.armed||!mutable.value.connected||authorization==null||ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)!=PackageManager.PERMISSION_GRANTED)return
        val epoch=providerEpoch;val captureTag=captureEpoch.incrementAndGet();val active=engine!!;val next=captureFactory?.invoke(scope)?:AndroidCapture(context,scope);capture=next
        next.start({bytes->if(providerEpoch==epoch&&captureEpoch.get()==captureTag&&mutable.value.armed)active.append(bytes)},{scope.launch{if(providerEpoch==epoch&&captureEpoch.get()==captureTag)stop("Microphone interrupted/unavailable")}})
    }
    private fun commit(g:Long,t:String) {
        val epoch=providerEpoch;val active=engine;val audio=upload
        scope.launch{try {if(!continuous){val old=capture;capture=null;captureEpoch.incrementAndGet();old?.finish()}
            if(epoch!=providerEpoch||engine!==active||voice.snapshot.generation!=g||voice.snapshot.turnId!=t)return@launch
            if(audioMode){if(audio!=null&&voice.snapshot.mode==VoiceMode.thinking&&audio.complete()){audioDeadline=minOf(audioDeadline,now()+25000);mutable.update{it.copy(status="Audio transferred — Windows processing; no automatic retry")}}else stop("Empty/canceled audio; no provider request");return@launch}
            if(active!=null&&voice.snapshot.mode==VoiceMode.thinking)active.ask(g,t)
            else if(mutable.value.armed)stop("Session not ready; no audio committed")
        }catch(_:Exception){if(epoch==providerEpoch)stop("Audio cleanup failed")}}
    }
    private fun lan(message:Message) {
        if(!mutable.value.armed)return
        val p=message.payload
        when(message.type) {
            "audioGrant"->{
                if(Wire.text(p,"consentId")!=consent)return
                if(continuous){stop("Gemini requires current hold-only consent");return}
                if(authorization!=null){if(audioMode&&authorization==Wire.text(p,"authorizationId"))return;stop("Authorization changed — Start again explicitly");return}
                authorization=Wire.text(p,"authorizationId");audioMode=true;audioReply=false;audioComplete=false;audioDeadline=now()+Wire.integer(p,"sessionSeconds")*1000;started=now();++providerEpoch
                mutable.update{it.copy(authorized=true,connected=true,status="GeminiFree authorized for ONE hold-only question (5 seconds maximum); double tap to wake")}
            }
            "audioReply"->{
                if(!currentAudio(p)||audioReply||audioComplete||voice.snapshot.mode!=VoiceMode.thinking)return
                audioReply=true;voice.accept(MockEvent(now(),pendingGen,pendingTurn,"delta",Wire.text(p,"text",256)),now());if(chirps)chirp.play()
                mutable.update{it.copy(status="Completed answer received — local chirp; text is an answer, not a transcription")}
            }
            "audioDone"->{
                if(!currentAudio(p)||!audioReply||audioComplete)return;audioComplete=true
                val epoch=providerEpoch;scope.launch{chirp.awaitQuiet();if(epoch==providerEpoch&&foreground&&voice.snapshot.generation==pendingGen&&voice.snapshot.turnId==pendingTurn){voice.accept(MockEvent(now(),pendingGen,pendingTurn,"done"),now());mutable.update{it.copy(status="Question complete — one-response budget used. Stop/Start and authorize again for another.")}}}
            }
            "liveGrant"->{if(!allowOpenAi){stop("OpenAI is disabled in this free-tier build");return};if(Wire.text(p,"consentId")!=consent)return;authorization=Wire.text(p,"authorizationId");mutable.update{it.copy(authorized=true,status="Windows authorized — double tap to wake; OpenAI API use is billable")}}
            "liveStop"->stop(when(Wire.text(p,"code")){"limit"->"Live limit or Google quota exhausted — stopped. Offline simulation is available; no paid fallback.";"denied"->"Windows consent, credential or safety check rejected this request. Offline simulation is available.";"expired"->"Authorization or provider request expired — stopped; no automatic retry.";"issuer"->"Provider unavailable, invalid or empty answer — stopped; offline simulation is available.";else->"Windows ended live authorization: "+Wire.text(p,"code")},notify=false)
            "liveToken"->{if(!allowOpenAi){stop("OpenAI is disabled in this free-tier build");return};val token=LiveProtocol.token(p)
                if(token.authorizationId!=authorization||token.requestId!=request||voice.snapshot.mode!=VoiceMode.waking||voice.snapshot.generation!=pendingGen||voice.snapshot.turnId!=pendingTurn)return
                request=null
                if(token.expiresAt<=System.currentTimeMillis()/1000||token.expiresAt>System.currentTimeMillis()/1000+60){stop("Credential expired or clock mismatch; start again explicitly");return}
                connect(token)
            }
        }
    }
    private fun currentAudio(p:com.google.gson.JsonObject)=audioMode&&Wire.text(p,"authorizationId")==authorization&&Wire.text(p,"turnId")==pendingTurn&&voice.snapshot.generation==pendingGen&&voice.snapshot.turnId==pendingTurn
    private fun startAudioTurn(state:VoiceSnapshot) {
        if(upload!=null||!foreground||!port.connected||!mutable.value.armed||authorization==null||ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)!=PackageManager.PERMISSION_GRANTED){stop("Audio turn unavailable; authorize a fresh question");return}
        pendingGen=state.generation;pendingTurn=state.turnId;audioReply=false;audioComplete=false
        val audio=AudioUpload(authorization!!,pendingTurn,port::send);upload=audio
        if(!audio.begin()){stop("LAN unavailable; no recording");return}
        val epoch=providerEpoch;val tag=captureEpoch.incrementAndGet();val next=captureFactory?.invoke(scope)?:AndroidCapture(context,scope);capture=next
        next.start({bytes->
            if(providerEpoch!=epoch||captureEpoch.get()!=tag||!mutable.value.armed){bytes.fill(0)}
            else if(!audio.append(bytes)){scope.launch{if(epoch==providerEpoch&&tag==captureEpoch.get())stop("Audio transfer backpressure/invalid frame; canceled")}}
            else if(audio.full()){next.cancel();scope.launch{if(epoch==providerEpoch&&tag==captureEpoch.get()){captureEpoch.incrementAndGet();mutable.update{it.copy(status="5-second input limit reached — microphone OFF; release to submit")}}}}
        },{scope.launch{if(epoch==providerEpoch&&tag==captureEpoch.get())stop("Microphone interrupted/unavailable")}})
        scope.launch{delay(5000);if(epoch==providerEpoch&&tag==captureEpoch.get()){captureEpoch.incrementAndGet();next.cancel();mutable.update{it.copy(status="5-second input limit reached — microphone OFF; release to submit")}}}
        mutable.update{it.copy(status="Hold-only capture and encrypted transfer — release to submit; maximum 5 seconds")}
    }
    private fun connect(token:LiveToken) {
        val epoch=++providerEpoch;started=now();while(packets.tryReceive().isSuccess){}
        val transport=transportFactory(token,{text->if(providerEpoch==epoch&&!packets.trySend(epoch to text).isSuccess)scope.launch{stop("Provider queue full")}}, {scope.launch{if(providerEpoch==epoch)stop("Provider connection ended; no automatic reconnect")}})
        val active=RealtimeEngine(transport,token.limits,::now,{event->scope.launch{if(event.kind=="done")chirp.awaitQuiet();if(providerEpoch==epoch&&foreground)voice.accept(event,now())}}, {code->scope.launch{if(providerEpoch==epoch)stop("Live stopped: $code")}}, {
            if(providerEpoch==epoch){val s=voice.snapshot
                voice.accept(MockEvent(now(),pendingGen,pendingTurn,"ready"),now());mutable.update{it.copy(connected=true,status="Live text session — billable; ${if(continuous)"CONTINUOUS AWAKE audio"else "HOLD-ONLY audio"}")};if(chirps)chirp.play();if(continuous)startCapture()}
        },{if(chirps)chirp.play()})
        active.bind(pendingGen,pendingTurn);engine=active
        mutable.update{it.copy(status="Connecting to OpenAI; microphone OFF")}
    }
    fun stop(reason:String="Stopped",notify:Boolean=true) {
        providerEpoch++;captureEpoch.incrementAndGet();upload?.cancel();upload=null;audioMode=false;audioReply=false;audioComplete=false;request=null;capture?.cancel();capture=null;chirp.stop();val old=engine;engine=null;old?.stop()
        if(notify&&mutable.value.armed)port.send("liveStop",Wire.payload("code" to "stopped"))
        consent=null;authorization=null;last=VoiceMode.asleep;voice.configureLive(null,now());mutable.value=mutable.value.copy(armed=false,authorized=false,connected=false,status=reason,elapsedMs=if(old!=null)now()-started else mutable.value.elapsedMs,usage=old?.usage?:mutable.value.usage)
        while(packets.tryReceive().isSuccess){}
    }
    fun pause(){foreground=false;stop("Paused/background — microphone and live session stopped")}
    fun resume(){foreground=true}
    fun close(){pause();scope.cancel()}
}
