package dev.bluey.port.render

import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.RectF
import android.graphics.Typeface
import dev.bluey.port.animation.*
import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config
import org.robolectric.annotation.GraphicsMode
import java.io.File

/** Same renderer as Compose, offscreen Skia on CI. Not a physical GPU/device test. */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
@GraphicsMode(GraphicsMode.Mode.NATIVE)
class FaceRendererTest {
    private fun renderer() = FaceRenderer(Typeface.createFromAsset(
        RuntimeEnvironment.getApplication().assets, "Fredoka.ttf"))
    private fun fixed(mood: Mood) = FaceFrame(
        gaze = Gaze(0.0, -0.3), mood = mood, talk = if (mood == Mood.talking) 0.55 else 0.0,
        closed = 0.0, breathe = 0.0, time = 1.25,
        pupil = when (mood) {
            Mood.listening -> 1.12; Mood.talking -> 1.05; Mood.pointing -> 0.95
            Mood.thinking, Mood.resting -> 0.9; Mood.happy -> 1.2; Mood.sleepy -> 0.88
        },
        browLift = when (mood) {
            Mood.listening -> 10.0; Mood.talking -> 15.9; Mood.pointing -> -4.0
            Mood.thinking -> 4.0; Mood.happy -> 16.0; Mood.resting -> -8.0; Mood.sleepy -> -10.0
        },
        browTilt = when (mood) { Mood.pointing -> -0.14; Mood.thinking -> 0.22; Mood.sleepy -> 0.12; else -> 0.0 },
        lean = 0.0, hop = 0.0, blush = if (mood == Mood.happy) 0.85 else 0.25,
        squint = if (mood == Mood.happy) 1.0 else 0.0,
    )
    private fun render(renderer: FaceRenderer, frame: FaceFrame, w: Int = 844, h: Int = 390) =
        Bitmap.createBitmap(w, h, Bitmap.Config.ARGB_8888).also {
            renderer.draw(Canvas(it), w.toFloat(), h.toFloat(), frame)
        }
    private fun save(bitmap: Bitmap, name: String) {
        val dir = File(requireNotNull(System.getProperty("bluey.renderDir")))
        dir.mkdirs()
        File(dir, "$name.png").outputStream().use { assertTrue(bitmap.compress(Bitmap.CompressFormat.PNG, 100, it)) }
    }
    @Test fun originalBlobAndCrownBoundsAreRetained() {
        val bounds = RectF()
        FaceRenderer.blobPath(820f, 700f).computeBounds(bounds, true)
        assertEquals(RectF(0f, 0f, 820f, 700f), bounds)
        FaceRenderer.crownPath(368f, -20f, 84f, 56f).computeBounds(bounds, true)
        assertEquals(379.2f, bounds.left, 0.001f)
        assertEquals(440.8f, bounds.right, 0.001f)
        assertEquals(-14.4f, bounds.top, 0.001f)
        assertEquals(30.4f, bounds.bottom, 0.001f)
    }
    @Test fun allSevenMoodsRenderOriginalLayersToPng() {
        val renderer = renderer()
        for (mood in Mood.entries) {
            val bitmap = render(renderer, fixed(mood))
            assertEquals(Color.BLACK, bitmap.getPixel(0, 0))
            assertEquals(Palette.nose, bitmap.getPixel(422, 335))
            assertNotEquals(Color.BLACK, bitmap.getPixel(422, 389))
            save(bitmap, mood.name)
            bitmap.recycle()
        }
    }
    @Test fun eyelidClippingAndHappySquintChangeTheEyes() {
        val renderer = renderer()
        val open = render(renderer, fixed(Mood.listening))
        val closed = render(renderer, fixed(Mood.listening).copy(closed = 1.0))
        val happy = render(renderer, fixed(Mood.happy))
        save(closed, "blink-closed")
        assertEquals(Color.WHITE, open.getPixel(225, 170))
        assertNotEquals(Color.WHITE, closed.getPixel(225, 170))
        assertNotEquals(Color.WHITE, happy.getPixel(225, 170))
        // Swift deliberately keeps a 0.06-open white sliver during a full blink.
        // Its center remains white; the old assertion sampled inside that sliver.
        assertEquals(Color.WHITE, closed.getPixel(225, 201))
    }
    @Test fun coverCropAndDoubledResolutionRenderWithoutChangingDesignCoordinates() {
        val renderer = renderer()
        val f = fixed(Mood.listening)
        val normal = render(renderer, f)
        val doubled = render(renderer, f, 1688, 780)
        assertEquals(normal.getPixel(422, 335), doubled.getPixel(844, 670))
        save(doubled, "listening-2x")
        save(render(renderer, f, 1000, 390), "listening-wide")
        save(render(renderer, f, 844, 500), "listening-tall")
    }
    @Test fun deterministicMoodChangeProducesTransitionFrames() {
        val a = FaceAnimator(AnimationClock { 0.0 }, RandomSource { 0.5 })
        a.receive(FaceState(), 0.0)
        a.step(0.0)
        a.localMood = Mood.happy
        val renderer = renderer()
        for (i in 1..60) {
            val f = a.step(i / 60.0)
            if (i in listOf(1, 5, 9, 18, 36, 60)) save(render(renderer, f), "happy-transition-$i")
        }
    }
    @Test fun manifestHasNoRequestedPermissionsAndFontsKeepTheirNotices() {
        val app = RuntimeEnvironment.getApplication()
        val info = app.packageManager.getPackageInfo(app.packageName, android.content.pm.PackageManager.GET_PERMISSIONS)
        // AndroidX may add its app-scoped signature receiver permission, but the
        // demo must not request system permissions for network, audio or storage.
        assertTrue(info.requestedPermissions.orEmpty().none { it.startsWith("android.permission.") })
        for (name in listOf("OFL-Fredoka.txt", "OFL-IBMPlex.txt")) {
            assertTrue(app.assets.open(name).bufferedReader().use { it.readText() }.contains("SIL OPEN FONT LICENSE"))
        }
    }
}
