package dev.bluey.port.animation

import org.junit.Assert.*
import org.junit.Test
import kotlin.math.PI
import kotlin.math.sin
import kotlin.math.sqrt
import kotlin.random.Random

class FaceAnimatorTest {
    private fun animator(unit: Double = 0.5) =
        FaceAnimator(AnimationClock { 0.0 }, RandomSource { unit })

    @Test fun springUsesUpdatedVelocityAndSourceDampingRatio() {
        val spring = Spring(0.0)
        spring.step(1.0, 0.01)
        assertEquals(2.0, spring.velocity, 1e-12)
        assertEquals(0.02, spring.value, 1e-12)
        val velocity = 2 + (200 * 0.98 - 2 * sqrt(200.0) * 0.7 * 2) * 0.01
        spring.step(1.0, 0.01)
        assertEquals(velocity, spring.velocity, 1e-12)
        assertEquals(0.02 + velocity * 0.01, spring.value, 1e-12)
    }
    @Test fun firstFrameHasZeroDeltaAndUsesInjectedClock() {
        val a = FaceAnimator(AnimationClock { 0.0 }, RandomSource { 0.5 })
        a.receive(FaceState(gazeX = 1.0, talk = 1.0))
        val f = a.step()
        assertEquals(0.0, f.time, 0.0)
        assertEquals(0.0, f.gaze.x, 0.0)
        assertEquals(0.0, f.talk, 0.0)
        assertEquals(1.0, f.pupil, 0.0)
    }
    @Test fun frameGapIsCappedAtFiftyMilliseconds() {
        val a = animator(); val b = animator()
        for (v in listOf(a, b)) { v.receive(FaceState(gazeX = 1.0)); v.step(0.0) }
        assertEquals(a.step(0.05).gaze.x, b.step(1.0).gaze.x, 1e-12)
    }
    @Test fun localMoodOverridesFreshStateAndRemoteExpiresAtTwoPointFive() {
        val a = animator()
        a.receive(FaceState(mood = Mood.pointing), 0.0)
        a.localMood = Mood.happy
        assertEquals(Mood.happy, a.step(0.0).mood)
        a.localMood = null
        assertEquals(Mood.pointing, a.step(2.499).mood)
        assertEquals(Mood.listening, a.step(2.5).mood)
    }
    @Test fun touchOverridesRemoteGazeButThinkingOverridesTouch() {
        val a = animator()
        a.receive(FaceState(gazeX = 1.0), 0.0)
        a.touchGaze = Gaze(-1.0, 0.0)
        a.step(0.0)
        assertEquals(-0.026, a.step(0.01).gaze.x, 1e-12)
        val b = animator(); b.touchGaze = Gaze(-1.0, 1.0); b.localMood = Mood.thinking
        b.step(0.0)
        val f = b.step(0.01)
        assertEquals(0.026 * (0.6 + 0.08 * sin(0.013)), f.gaze.x, 1e-12)
        assertEquals(0.026 * -0.85, f.gaze.y, 1e-12)
    }
    @Test fun localTalkIsAMaximumFloorWithSourceSmoothing() {
        val a = animator(); a.receive(FaceState(talk = 0.2)); a.localTalk = { 0.8 }
        a.step(0.0)
        assertEquals(0.2, a.step(0.01).talk, 1e-12)
    }
    @Test fun talkingFallbackUsesOriginalSinProduct() {
        val a = animator(); a.localMood = Mood.talking; a.step(0.0)
        assertEquals((0.5 + 0.5 * sin(0.19) * sin(0.073)) * 0.25, a.step(0.01).talk, 1e-12)
    }
    @Test fun moodChangeBlinksAndAddsTheOriginalHopImpulse() {
        val a = animator(); a.localMood = Mood.happy
        a.step(0.0)
        val f = a.step(0.01)
        val velocity = 260 - 2 * sqrt(260.0) * 0.35 * 260 * 0.01
        assertEquals(velocity * 0.01, f.hop, 1e-12)
        assertEquals(sin(PI * 0.01 / 0.15), f.closed, 1e-12)
    }
    @Test fun restingAndSleepyDoNotGetMoodChangeHop() {
        for (m in listOf(Mood.resting, Mood.sleepy)) {
            val a = animator(); a.localMood = m; a.step(0.0)
            assertEquals(0.0, a.step(0.01).hop, 1e-12)
        }
    }
    @Test fun sleepyBlinkUsesSevenHundredMilliseconds() {
        val a = animator(); a.localMood = Mood.sleepy; a.step(0.0)
        assertEquals(1.0, a.step(0.35).closed, 1e-12)
        assertEquals(0.0, a.step(0.71).closed, 1e-12)
    }
    @Test fun scheduledDoubleBlinkUsesSecondPhase() {
        val a = animator(0.0); a.step(0.0); a.step(2.1)
        assertEquals(1.0, a.step(2.37).closed, 1e-12) // p = 1.8
        assertEquals(0.0, a.step(2.5).closed, 1e-12)
    }
    @Test fun allMoodTargetsSettleAndTransitionsStayFinite() {
        val targets = mapOf(Mood.listening to 1.12, Mood.talking to 1.05,
            Mood.pointing to 0.95, Mood.thinking to 0.9, Mood.happy to 1.2,
            Mood.resting to 0.9, Mood.sleepy to 0.88)
        for ((mood, expectedPupil) in targets) {
            val a = animator(); a.receive(FaceState(mood = mood)); a.step(0.0)
            var f = a.step(0.0)
            for (i in 1..120) f = a.step(i / 60.0)
            assertEquals(mood.name, expectedPupil, f.pupil, 1e-5)
            assertTrue(f.hop.isFinite() && f.browLift.isFinite() && f.lean.isFinite())
            if (mood == Mood.happy) assertTrue(f.squint > 0.99)
            assertEquals(sin(2.0 * if (mood == Mood.resting) 1.1 else 1.8), f.breathe, 1e-12)
        }
    }
    @Test fun fixedRandomSequenceReproducesWanderJitterAndIdleHop() {
        fun seeded(): FaceAnimator {
            val random = Random(39)
            return FaceAnimator(AnimationClock { 0.0 }, RandomSource { random.nextDouble() })
        }
        val a = seeded(); val b = seeded()
        var hopped = false
        for (i in 0..1200) {
            val f = a.step(i / 60.0)
            assertEquals(f, b.step(i / 60.0))
            if (i > 360 && f.hop > 0.1) hopped = true
        }
        assertTrue(hopped)
    }
    @Test fun pausedTimelineDoesNotIntegrateBackgroundTime() {
        val timeline = ActiveTimeline()
        assertEquals(0.0, timeline.tick(1_000_000_000), 0.0)
        assertEquals(0.1, timeline.tick(1_100_000_000), 1e-12)
        timeline.pause()
        assertEquals(0.1, timeline.tick(90_000_000_000), 1e-12)
        assertEquals(0.2, timeline.tick(90_100_000_000), 1e-12)
    }
    @Test(expected = IllegalArgumentException::class) fun backwardsTimeIsRejected() {
        val a = animator(); a.step(1.0); a.step(0.9)
    }
    @Test(expected = IllegalArgumentException::class) fun nonfiniteTimeIsRejected() { animator().step(Double.NaN) }
    @Test fun coverTransformKeepsBottomAlignmentAcrossAspectRatios() {
        assertEquals(CoverTransform(1f, 0f, 0f), FaceGeometry.cover(844f, 390f))
        val wide = FaceGeometry.cover(1000f, 390f)
        assertEquals(1000f / 844, wide.scale, 1e-6f)
        assertEquals(390f, wide.y + 390f * wide.scale, 1e-4f)
        val tall = FaceGeometry.cover(844f, 500f)
        assertTrue(tall.x < 0)
        assertEquals(500f, tall.y + 390f * tall.scale, 1e-4f)
    }
}
