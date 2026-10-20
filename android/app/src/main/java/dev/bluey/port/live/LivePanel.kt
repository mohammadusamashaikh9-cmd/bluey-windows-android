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
    var enabled by remember{mutableStateOf(state.armed)};var cloud by remember{mutableStateOf(state.armed)};var continuous by remember{mutableStateOf(state.continuous)}
    var permission by remember{mutableStateOf(ContextCompat.checkSelfPermission(context,Manifest.permission.RECORD_AUDIO)==PackageManager.PERMISSION_GRANTED)}
    val launcher=rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()){granted->permission=granted}
    AlertDialog(onDismissRequest=close,title={Text("Live AI — billable, opt-in")},text={Column(Modifier.verticalScroll(rememberScrollState())){
        Text("Offline simulation remains available without microphone permission, an API key or cloud access.")
        Row{Text("Enable live AI",Modifier.weight(1f));Switch(enabled,onCheckedChange={enabled=it;if(!it)live.stop()})}
        Row{Text("I consent to sending microphone audio to OpenAI for cloud processing",Modifier.weight(1f));Checkbox(cloud,onCheckedChange={cloud=it;if(!it)live.stop("Cloud consent withdrawn")})}
        Text("Microphone permission: ${if(permission)"granted" else "not granted"}. Permission alone never starts recording.")
        TextButton(onClick={launcher.launch(Manifest.permission.RECORD_AUDIO)},enabled=enabled){Text("Request microphone permission (no capture)")}
        Row{Text("Continuous-awake audio context (separate opt-in)",Modifier.weight(1f));Switch(continuous,onCheckedChange={continuous=it;live.stop("Audio mode changed; Start again")})}
        Text(if(continuous)"CONTINUOUS: everything near the microphone while awake is streamed, even between holds; higher cost and privacy exposure. Background stops it." else "HOLD-ONLY (recommended): mic starts after a 300 ms hold. Hold while speaking, then release. Very short/empty audio produces no response.")
        var chirps by remember{mutableStateOf(live.chirps)}
        Row{Text("Local cartoon chirps (no cloud speech)",Modifier.weight(1f));Switch(chirps,onCheckedChange={chirps=it;live.chirps=it})}
        Text("Windows must save a protected key, review this exact phone/consent and explicitly authorize. Start here first, authorize on Windows, then double tap the face to wake. Sleep/Stop requires fresh Start and Windows approval; no auto wake or unlimited reconnect.")
        TextButton(enabled=enabled&&cloud&&permission,onClick={live.arm(cloud,continuous)}){Text("Start / request Windows authorization")}
        TextButton(onClick={live.stop("Stopped by user")}){Text("STOP / cancel live session")}
        Text(state.status)
        Text("Elapsed: ${state.elapsedMs/1000}s · audio: ${state.usage.audioMs/1000}s · responses: ${state.usage.responses}")
        Text("Provider-reported tokens (not a bill): input ${state.usage.inputTokens}, output ${state.usage.outputTokens}, total ${state.usage.totalTokens}. Limits are set on Windows. They cannot guarantee a hard spending cap; use OpenAI project budgets/usage notifications too.")
        Text("No input transcription, screen capture, audio files, transcripts or background microphone service. Assistant text is an AI reply, not a transcription.")
    }},confirmButton={TextButton(onClick=close){Text("Done")}})
}
