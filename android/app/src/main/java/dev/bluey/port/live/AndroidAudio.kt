package dev.bluey.port.live
import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.media.*
import androidx.core.content.ContextCompat
import kotlinx.coroutines.*
import java.util.concurrent.atomic.AtomicBoolean
import java.util.concurrent.atomic.AtomicReference
class AndroidCapture(private val context:Context,private val scope:CoroutineScope):CaptureDevice {
    private val wanted=AtomicBoolean(false);private val recorder=AtomicReference<AudioRecord?>(null);private var job:Job?=null
    override fun start(frame:(ByteArray)->Unit,failed:()->Unit) {
        check(job==null);wanted.set(true)
        job=scope.launch(Dispatchers.IO) {
            var record:AudioRecord?=null
            try {
                if(ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)!=PackageManager.PERMISSION_GRANTED)throw SecurityException()
                var rate=0
                for(candidate in listOf(24000,48000,44100,16000)) {
                    if(!wanted.get())return@launch
                    val minimum=AudioRecord.getMinBufferSize(candidate,AudioFormat.CHANNEL_IN_MONO,AudioFormat.ENCODING_PCM_16BIT);if(minimum<=0)continue
                    val next=AudioRecord(MediaRecorder.AudioSource.VOICE_RECOGNITION,candidate,AudioFormat.CHANNEL_IN_MONO,AudioFormat.ENCODING_PCM_16BIT,maxOf(minimum,candidate/5*2))
                    if(next.state==AudioRecord.STATE_INITIALIZED){record=next;rate=candidate;break}else next.release()
                }
                val active=record?:throw IllegalStateException();recorder.set(active)
                if(!wanted.get())return@launch
                val conversion=Pcm24k(rate){if(wanted.get())frame(it)};val input=ShortArray(rate/50)
                active.startRecording();if(active.recordingState!=AudioRecord.RECORDSTATE_RECORDING)throw IllegalStateException()
                while(wanted.get()&&isActive){val n=active.read(input,0,input.size,AudioRecord.READ_BLOCKING);if(n<=0)throw IllegalStateException();if(wanted.get())conversion.accept(input,n)}
            }catch(_:Exception){if(wanted.get())failed()}
            finally {wanted.set(false);recorder.set(null);runCatching{record?.stop()};record?.release()}
        }
    }
    override fun cancel(){wanted.set(false);val active=recorder.get();if(active!=null)scope.launch(Dispatchers.IO){runCatching{active.stop()}}}
    override suspend fun finish(){cancel();withTimeout(1500){job?.join()}}
}
class LocalChirp(private val scope:CoroutineScope) {
    private var job:Job?=null
    fun play(){stop();job=scope.launch(Dispatchers.IO){var track:AudioTrack?=null
        try{val data=Chirp.samples();track=AudioTrack.Builder().setAudioAttributes(AudioAttributes.Builder().setUsage(AudioAttributes.USAGE_ASSISTANCE_SONIFICATION).setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION).build()).setAudioFormat(AudioFormat.Builder().setEncoding(AudioFormat.ENCODING_PCM_16BIT).setSampleRate(24000).setChannelMask(AudioFormat.CHANNEL_OUT_MONO).build()).setTransferMode(AudioTrack.MODE_STATIC).setBufferSizeInBytes(data.size*2).build();track.write(data,0,data.size);track.setVolume(.5f);track.play();delay(data.size*1000L/24000+30)}catch(_:Exception){}finally{runCatching{track?.stop()};track?.release()}}
    }
    fun stop(){job?.cancel();job=null}
}
