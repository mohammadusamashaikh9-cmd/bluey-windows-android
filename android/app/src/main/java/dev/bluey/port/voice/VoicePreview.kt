package dev.bluey.port.voice

import androidx.compose.runtime.*
import androidx.compose.material3.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable fun VoicePreview(state:VoiceSnapshot,radius:Float,setRadius:(Float)->Unit,toggle:()->Unit,sleep:()->Unit,openLive:()->Unit,close:()->Unit) {
    AlertDialog(onDismissRequest=close,title={Text("Offline voice simulation")},text={Column(Modifier.verticalScroll(rememberScrollState())) {
        Text("Double tap the face to wake/sleep. Hold for at least 300 ms, then release for a mock reply. No audio is captured or sent. State: ${state.mode}.")
        if(state.caption.isNotEmpty())Text("Mock caption: ${state.caption}")
        Text("Offline status (read-only — these are not permission controls):")
        Text("Microphone capture: OFF in simulation")
        Text("Cloud processing: OFF in simulation")
        Text("Windows screen capture: unavailable until F4-C")
        Text("Offline Simulation never records audio or contacts Gemini. No API key is needed. Microphone permission, cloud consent and Stop controls are in Gemini hold-only settings. Continuous-awake recording is unavailable for Gemini.")
        TextButton(onClick=openLive){Text("Open Gemini hold-only — opt-in settings")}
        Text("Corner-radius fallback: ${radius.toInt()} dp (public device radii take precedence)")
        Slider(value=radius,onValueChange=setRadius,valueRange=0f..90f)
        TextButton(onClick=toggle){Text("Wake / sleep simulation")}
        TextButton(onClick=sleep){Text("Cancel and sleep")}
    }},confirmButton={TextButton(onClick=close){Text("Done")}})
}
