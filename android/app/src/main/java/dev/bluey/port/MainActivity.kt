package dev.bluey.port

import android.animation.ValueAnimator
import android.graphics.Typeface
import android.content.Intent
import android.os.SystemClock
import dev.bluey.port.link.DeviceLink
import dev.bluey.port.link.FaceDelivery
import dev.bluey.port.link.ConnectionPanel
import android.os.Build
import android.os.Bundle
import android.view.WindowManager
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.drawIntoCanvas
import androidx.compose.ui.graphics.nativeCanvas
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import androidx.core.view.WindowCompat
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.WindowInsetsControllerCompat
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.lifecycle.repeatOnLifecycle
import dev.bluey.port.animation.*
import dev.bluey.port.render.FaceRenderer

class MainActivity : ComponentActivity() {
    private var pairingUri by mutableStateOf<String?>(null)
    private fun readInvitation(intent: Intent?) {
        val uri = intent?.dataString
        if (uri != null && uri.length <= 2048 && uri.startsWith("bluey-secure://pair?")) pairingUri = uri
    }
    override fun onNewIntent(intent: Intent) { super.onNewIntent(intent); setIntent(intent); readInvitation(intent) }
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        readInvitation(intent)
        WindowCompat.setDecorFitsSystemWindows(window, false)
        if (Build.VERSION.SDK_INT >= 28) window.attributes = window.attributes.apply {
            layoutInDisplayCutoutMode = if (Build.VERSION.SDK_INT >= 30)
                WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_ALWAYS
            else WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES
        }
        setContent {
            MaterialTheme(colorScheme = darkColorScheme(primary = Color(0xFFA9BCFF))) {
                FaceDemo(pairingUri)
            }
        }
    }
    override fun onResume() {
        super.onResume()
        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        WindowCompat.getInsetsController(window, window.decorView).apply {
            systemBarsBehavior = WindowInsetsControllerCompat.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
            hide(WindowInsetsCompat.Type.systemBars())
        }
    }
    override fun onPause() {
        window.clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        super.onPause()
    }
}

@Composable
private fun FaceDemo(incoming: String?) {
    val context = LocalContext.current
    val lifecycle = LocalLifecycleOwner.current.lifecycle
    val link = remember(context) { DeviceLink(context) }
    val remote by link.face.collectAsState()
    val remoteInput by rememberUpdatedState(remote)
    var connectionPanel by remember { mutableStateOf(false) }
    LaunchedEffect(incoming) { if (incoming != null) connectionPanel = true }
    DisposableEffect(lifecycle, link) {
        val observer = androidx.lifecycle.LifecycleEventObserver { _, event ->
            if (event == Lifecycle.Event.ON_RESUME) link.resume()
            if (event == Lifecycle.Event.ON_PAUSE) link.pause()
        }
        lifecycle.addObserver(observer)
        if (lifecycle.currentState.isAtLeast(Lifecycle.State.RESUMED)) link.resume()
        onDispose { lifecycle.removeObserver(observer); link.close() }
    }
    val timeline = remember { ActiveTimeline() }
    val animator = remember { FaceAnimator(AnimationClock { timeline.seconds }) }
    val renderer = remember(context) {
        FaceRenderer(Typeface.createFromAsset(context.assets, "Fredoka.ttf"))
    }
    val frame = remember { mutableStateOf(animator.step()) }
    var panel by rememberSaveable { mutableStateOf(false) }
    var selectedMood by rememberSaveable { mutableStateOf(Mood.listening.name) }
    var gazeX by rememberSaveable { mutableFloatStateOf(0f) }
    var gazeY by rememberSaveable { mutableFloatStateOf(-0.3f) }
    var talk by rememberSaveable { mutableFloatStateOf(0f) }
    var wander by rememberSaveable { mutableStateOf(true) }
    var awake by rememberSaveable { mutableStateOf(false) }
    var motion by rememberSaveable { mutableStateOf(ValueAnimator.areAnimatorsEnabled()) }
    val inputs by rememberUpdatedState(DemoInput(Mood.valueOf(selectedMood), gazeX, gazeY, talk, wander, awake, motion))

    LaunchedEffect(lifecycle, animator) {
        lifecycle.repeatOnLifecycle(Lifecycle.State.RESUMED) {
            timeline.pause()
            val delivery = FaceDelivery()
            while (true) withFrameNanos { nanos ->
                val input = inputs
                if (!input.motion) timeline.pause()
                val now = timeline.tick(nanos)
                val packet = remoteInput
                val fresh = delivery.apply(animator, packet, SystemClock.elapsedRealtime(), now)
                animator.localMood = if (fresh) null else input.mood
                animator.awake = input.awake
                animator.localTalk = { if (fresh) 0.0 else input.talk.toDouble() }
                if (!fresh && !input.wander) animator.receive(
                    FaceState(input.x.toDouble(), input.y.toDouble(), input.mood, input.talk.toDouble()), now)
                frame.value = animator.step(now)
            }
        }
    }
    Box(Modifier.fillMaxSize().background(Color.Black)) {
        Canvas(Modifier.fillMaxSize()
            .semantics { contentDescription = "Bluey face, $selectedMood. Drag to guide his eyes." }
            .pointerInput(animator) {
                fun gaze(x: Float, y: Float) = Gaze(
                    (x / size.width * 2 - 1).toDouble().coerceIn(-1.0, 1.0),
                    (y / size.height * 2 - 1).toDouble().coerceIn(-1.0, 1.0))
                detectDragGestures(
                    onDragStart = { animator.touchGaze = gaze(it.x, it.y) },
                    onDragEnd = { animator.touchGaze = null },
                    onDragCancel = { animator.touchGaze = null },
                    onDrag = { change, _ ->
                        change.consume()
                        animator.touchGaze = gaze(change.position.x, change.position.y)
                    },
                )
            }) {
            drawIntoCanvas { renderer.draw(it.nativeCanvas, size.width, size.height, frame.value) }
        }
        TextButton(onClick = { connectionPanel = true }, modifier = Modifier.align(Alignment.TopStart).safeDrawingPadding()) {
            Text("Connection", color = Color(0xFFB9B2CC))
        }
        if (connectionPanel) ConnectionPanel(link, incoming) { connectionPanel = false }
        TextButton(onClick = { panel = !panel }, modifier = Modifier.align(Alignment.TopEnd).safeDrawingPadding()) {
            Text(if (panel) "Hide demo" else "Demo", color = Color(0xFFB9B2CC))
        }
        if (panel) Surface(
            modifier = Modifier.align(Alignment.CenterEnd).safeDrawingPadding().widthIn(max = 320.dp)
                .fillMaxHeight().padding(top = 48.dp),
            color = Color(0xEC1E1B29),
        ) {
            Column(Modifier.verticalScroll(rememberScrollState()).padding(12.dp)) {
                Text("Offline face demo", style = MaterialTheme.typography.titleSmall)
                Row(Modifier.horizontalScroll(rememberScrollState())) {
                    Mood.entries.forEach { mood ->
                        FilterChip(selected = selectedMood == mood.name,
                            onClick = { selectedMood = mood.name }, label = { Text(mood.name) })
                        Spacer(Modifier.width(6.dp))
                    }
                }
                Text("Gaze X")
                Slider(value = gazeX, onValueChange = { gazeX = it; wander = false }, valueRange = -1f..1f)
                Text("Gaze Y")
                Slider(value = gazeY, onValueChange = { gazeY = it; wander = false }, valueRange = -1f..1f)
                Text("Talking amplitude")
                Slider(value = talk, onValueChange = { talk = it }, valueRange = 0f..1f)
                Toggle("Idle wander", wander) { wander = it }
                Toggle("Awake jitter", awake) { awake = it }
                Toggle("Motion", motion) { motion = it }
                Text("Drag the face to guide his eyes. Thinking keeps its original upward gaze. " +
                    "Talking in idle mode uses the source's synthetic bounce. Hide demo for comparison.",
                    style = MaterialTheme.typography.bodySmall)
            }
        }
    }
    DisposableEffect(lifecycle, animator) {
        val observer = androidx.lifecycle.LifecycleEventObserver { _, event ->
            if (event == Lifecycle.Event.ON_PAUSE) animator.touchGaze = null
        }
        lifecycle.addObserver(observer)
        onDispose { lifecycle.removeObserver(observer); animator.touchGaze = null }
    }
}

private data class DemoInput(val mood: Mood, val x: Float, val y: Float,
    val talk: Float, val wander: Boolean, val awake: Boolean, val motion: Boolean)

@Composable
private fun Toggle(label: String, checked: Boolean, onChange: (Boolean) -> Unit) {
    Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
        Text(label, Modifier.weight(1f))
        Switch(checked = checked, onCheckedChange = onChange)
    }
}
