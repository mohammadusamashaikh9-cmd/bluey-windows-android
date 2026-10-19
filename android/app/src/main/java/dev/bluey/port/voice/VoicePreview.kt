package dev.bluey.port.voice

import androidx.compose.runtime.*
import androidx.compose.material3.*
import androidx.compose.foundation.layout.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable fun VoicePreview(state:VoiceSnapshot,radius:Float,setRadius:(Float)->Unit,toggle:()->Unit,sleep:()->Unit,close:()->Unit) {
    AlertDialog(onDismissRequest=close,title={Text("Offline voice simulation")},text={Column {
        Text("Double tap the face to wake/sleep. Hold for at least 300 ms, then release for a mock reply. No audio is captured or sent. State: ${state.mode}.")
        Text("Planned live permissions — all OFF:")
        for(label in listOf("Microphone capture","Cloud processing","Windows screen capture"))Row {
            Text(label,Modifier.weight(1f));Switch(checked=false,onCheckedChange={},enabled=false)
        }
        Text("Future live awake sessions may continuously stream microphone audio, even when you are not holding the face. These permissions will require separate explicit approval. No API key is needed here.")
        Text("Corner-radius fallback: ${radius.toInt()} dp (public device radii take precedence)")
        Slider(value=radius,onValueChange=setRadius,valueRange=0f..90f)
        TextButton(onClick=toggle){Text("Wake / sleep simulation")}
        TextButton(onClick=sleep){Text("Cancel and sleep")}
    }},confirmButton={TextButton(onClick=close){Text("Done")}})
}
