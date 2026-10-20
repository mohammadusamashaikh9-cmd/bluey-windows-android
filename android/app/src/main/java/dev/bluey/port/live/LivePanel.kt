package dev.bluey.port.live
import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
@Composable fun LivePanel(live:LiveCoordinator,close:()->Unit) {
    val context=LocalContext.current;val state by live.ui.collectAsState()
    var enabled by remember{mutableStateOf(state.armed)};var cloud by remember{mutableStateOf(state.armed)}
    var permission by remember{mutableStateOf(ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)==PackageManager.PERMISSION_GRANTED)}
    val launcher=rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()){granted->permission=granted}
    AlertDialog(onDismissRequest=close,title={Text("Gemini hold-only — opt-in")},text={Column(Modifier.verticalScroll(rememberScrollState())){
        Text("Offline simulation remains available without microphone permission, an API key or cloud access.")
        Row{Text("Enable live AI",Modifier.weight(1f));Switch(enabled,onCheckedChange={enabled=it;if(!it)live.stop()})}
        Row{Text("I consent to encrypted audio transfer to Windows and Google cloud processing. Free-tier content may improve Google products; I will not share sensitive conversations.",Modifier.weight(1f));Checkbox(cloud,onCheckedChange={cloud=it;if(!it)live.stop("Cloud consent withdrawn")})}
        Text("Microphone permission: ${if(permission)"granted" else "not granted"}. Permission alone never starts recording.")
        TextButton(onClick={launcher.launch(Manifest.permission.RECORD_AUDIO)},enabled=enabled){Text("Request microphone permission (no capture)")}
        Row{Text("Continuous-awake context (unavailable for Gemini)",Modifier.weight(1f));Switch(false,onCheckedChange={},enabled=false)}
        Text("Gemini supports HOLD-ONLY in this port: mic starts after a 300 ms hold, at most5 seconds. Release submits one completed question. Continuous-awake context is unavailable; clips are never submitted automatically. Background stops recording.")
        var chirps by remember{mutableStateOf(live.chirps)}
        Row{Text("Local cartoon chirps (no cloud speech)",Modifier.weight(1f));Switch(chirps,onCheckedChange={chirps=it;live.chirps=it})}
        Text("Windows chooses the provider and keeps its key. Gemini authorization is BLOCKED pending official model/audio/free-tier contract verification. Saving a key never tests the service. No credential is sent to Android. Start/Stop requires fresh consent; reconnection never submits a question.")
        TextButton(enabled=enabled&&cloud&&permission,onClick={live.arm(cloud,false)}){Text("Start / request Windows authorization")}
        TextButton(onClick={live.stop("Stopped by user")}){Text("STOP / cancel live session")}
        Text(state.status)
        Text("Elapsed: ${state.elapsedMs/1000}s · audio: ${state.usage.audioMs/1000}s · responses: ${state.usage.responses}")
        Text("Provider-reported tokens (not a bill): input ${state.usage.inputTokens}, output ${state.usage.outputTokens}, total ${state.usage.totalTokens}. Gemini is limited to one answer per approval. The app cannot verify the billing status of an arbitrary key/project or guarantee zero cost; Windows must confirm Free Tier. Quota errors stop; no paid fallback.")
        Text("No input transcription, screen capture, audio files, transcripts or background microphone service. Assistant text is an AI reply, not a transcription.")
    }},confirmButton={TextButton(onClick=close){Text("Done")}})
}
