package dev.bluey.port.animation

import kotlin.math.PI
import kotlin.math.max
import kotlin.math.min
import kotlin.math.sin
import kotlin.math.sqrt
import kotlin.random.Random

enum class Mood { listening, resting, thinking, talking, pointing, happy, sleepy }
data class Gaze(val x: Double = 0.0, val y: Double = 0.0)
data class FaceState(
    val gazeX: Double = 0.0, val gazeY: Double = 0.0,
    val mood: Mood = Mood.listening, val talk: Double = 0.0,
)

fun interface AnimationClock { fun seconds(): Double }
fun interface RandomSource { fun unit(): Double }

/** Literal semi-implicit Euler step from iOS/Sources/FaceView.swift. */
class Spring(var value: Double, val stiffness: Double = 200.0, val damping: Double = 0.7) {
    var velocity = 0.0
    fun step(target: Double, dt: Double) {
        val c = 2 * sqrt(stiffness) * damping
        velocity += (stiffness * (target - value) - c * velocity) * dt
        value += velocity * dt
    }
}

data class FaceFrame(
    val gaze: Gaze, val mood: Mood, val talk: Double, val closed: Double,
    val breathe: Double, val time: Double, val pupil: Double, val browLift: Double,
    val browTilt: Double, val lean: Double, val hop: Double, val blush: Double,
    val squint: Double,
)

/** Pure state machine: no Android classes, sockets, audio or implicit wall clock. */
class FaceAnimator(
    private val clock: AnimationClock,
    private val random: RandomSource = RandomSource { Random.Default.nextDouble() },
) {
    private var target = FaceState()
    private var lastPacket = -100.0
    private val gazeX = Spring(0.0, 260.0, 0.82)
    private val gazeY = Spring(0.0, 260.0, 0.82)
    private val pupil = Spring(1.0, 160.0, 0.5)
    private val browLift = Spring(0.0, 180.0, 0.55)
    private val browTilt = Spring(0.0, 150.0, 0.6)
    private val lean = Spring(0.0, 60.0, 0.8)
    private val hop = Spring(0.0, 260.0, 0.35)
    private val blush = Spring(0.25, 40.0, 1.0)
    private val squint = Spring(0.0, 150.0, 0.7)
    private var talk = 0.0
    private var mood = Mood.listening
    private var blinkStart = -1.0
    private var doubleBlink = false
    private var nextBlink = 2.0
    private var lastTime: Double? = null
    private var nextSaccade = 0.0
    private var wanderTarget = Gaze(0.0, -0.3)
    private var jitter = Gaze()
    private var nextJitter = 0.0
    private var nextHop = 6.0

    var touchGaze: Gaze? = null
    var localMood: Mood? = null
    var localTalk: () -> Double = { 0.0 }
    var awake = false

    fun receive(face: FaceState, at: Double = clock.seconds()) {
        require(at.isFinite() && face.gazeX.isFinite() && face.gazeY.isFinite()
            && face.talk.isFinite())
        target = face
        lastPacket = at
    }

    private fun between(low: Double, high: Double): Double {
        val unit = random.unit()
        require(unit >= 0.0 && unit < 1.0)
        return low + (high - low) * unit
    }

    fun step(now: Double = clock.seconds()): FaceFrame {
        require(now.isFinite() && now >= (lastTime ?: now)) { "Animation time must be finite and monotonic" }
        val dt = min(now - (lastTime ?: now), 1.0 / 20)
        lastTime = now
        val live = now - lastPacket < 2.5
        val wantedMood = localMood ?: if (live) target.mood else Mood.listening
        if (wantedMood != mood) {
            mood = wantedMood
            blinkStart = now
            if (mood != Mood.sleepy && mood != Mood.resting) {
                hop.velocity += 260
                pupil.velocity += 3
            }
        }
        var want = touchGaze ?: if (live) Gaze(target.gazeX, target.gazeY) else {
            if (now > nextSaccade) {
                wanderTarget = Gaze(between(-0.9, 0.9), between(-0.9, 0.4))
                nextSaccade = now + between(0.6, 2.2)
            }
            wanderTarget
        }
        if (mood == Mood.thinking) want = Gaze(0.6 + 0.08 * sin(now * 1.3), -0.85)
        if (now > nextJitter) {
            val amount = if (!awake) 0.015 else when (mood) {
                Mood.talking -> 0.07
                Mood.thinking -> 0.05
                else -> 0.02
            }
            jitter = Gaze(between(-amount, amount), between(-amount, amount))
            nextJitter = now + between(0.5, 1.4)
        }
        gazeX.step(want.x + jitter.x, dt)
        gazeY.step(want.y + jitter.y, dt)
        var wantTalk = if (live) target.talk else if (localMood == Mood.talking)
            0.5 + 0.5 * sin(now * 19) * sin(now * 7.3) else 0.0
        wantTalk = max(wantTalk, localTalk())
        talk += (wantTalk - talk) * min(1.0, dt * 25)

        var wantPupil = 1.0
        var wantLift = 0.0
        var wantTilt = 0.0
        var wantBlush = 0.25
        var wantSquint = 0.0
        when (mood) {
            Mood.listening -> { wantPupil = 1.12; wantLift = 10.0 }
            Mood.talking -> { wantPupil = 1.05; wantLift = 6 + talk * 18; wantTilt = 0.05 * sin(now * 2.3) }
            Mood.pointing -> { wantPupil = 0.95; wantLift = -4.0; wantTilt = -0.14 }
            Mood.thinking -> { wantPupil = 0.9; wantLift = 4.0; wantTilt = 0.22 }
            Mood.happy -> { wantPupil = 1.2; wantLift = 16.0; wantBlush = 0.85; wantSquint = 1.0 }
            Mood.resting -> { wantPupil = 0.9; wantLift = -8.0; wantBlush = 0.15 }
            Mood.sleepy -> { wantPupil = 0.88; wantLift = -10.0; wantTilt = 0.12; wantBlush = 0.15 }
        }
        pupil.step(wantPupil, dt)
        browLift.step(wantLift, dt)
        browTilt.step(wantTilt, dt)
        blush.step(wantBlush, dt)
        squint.step(wantSquint, dt)
        lean.step(-gazeX.value * 0.045, dt)
        if (now > nextHop) {
            if (talk < 0.05 && mood != Mood.sleepy && mood != Mood.resting)
                hop.velocity += between(180.0, 320.0)
            nextHop = now + between(7.0, 14.0)
        }
        hop.step(0.0, dt)
        val drowsy = mood == Mood.sleepy
        if (now > nextBlink) {
            blinkStart = now
            doubleBlink = !drowsy && between(0.0, 1.0) < 0.25
            nextBlink = now + if (drowsy) between(1.6, 3.0) else between(2.0, 5.0)
        }
        val p = (now - blinkStart) / if (drowsy) 0.7 else 0.15
        var closed = if (p in 0.0..1.0) sin(PI * p) else 0.0
        if (doubleBlink && p in 1.3..2.3) closed = sin(PI * (p - 1.3))
        return FaceFrame(
            Gaze(gazeX.value, gazeY.value), mood, talk, closed,
            sin(now * if (mood == Mood.resting) 1.1 else 1.8), now,
            pupil.value, browLift.value, browTilt.value, lean.value, hop.value,
            blush.value, squint.value.coerceIn(0.0, 1.0),
        )
    }
}

/** Frame-clock adapter: paused/background time never advances the demo simulation. */
class ActiveTimeline {
    var seconds = 0.0
        private set
    private var previous: Long? = null
    fun pause() { previous = null }
    fun tick(nanos: Long): Double {
        previous?.let {
            require(nanos >= it)
            seconds += (nanos - it) / 1_000_000_000.0
        }
        previous = nanos
        return seconds
    }
}
