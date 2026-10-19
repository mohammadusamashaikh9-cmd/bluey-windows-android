package dev.bluey.port.voice
import android.graphics.*
import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config
import org.robolectric.annotation.GraphicsMode
import java.io.File
@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35])
@GraphicsMode(GraphicsMode.Mode.NATIVE)
class IndicatorRenderTest {
    @Test fun productionVectorRendererEmitsSixStatesAndCutoutLandscapeReferences() {
        val renderer=ModeIndicatorRenderer(Typeface.createFromAsset(RuntimeEnvironment.getApplication().assets,"Fredoka.ttf"))
        for(mode in VoiceMode.entries)for(landscape in listOf(false,true)) {
            val w=if(landscape)844 else 390;val h=if(landscape)390 else 844
            val bitmap=Bitmap.createBitmap(w,h,Bitmap.Config.ARGB_8888);val canvas=Canvas(bitmap);canvas.drawColor(Color.BLACK)
            val look=ModeIndicator.look(mode);val cutout=Rect(0,160,24,230)
            renderer.draw(canvas,ScreenShape(w.toFloat(),h.toFloat(),List(4){55f},if(landscape)24f else 0f,0f),IndicatorFrame(look.rgb,look.border,look.opacity(0.0),look),1f,if(landscape)listOf(cutout)else emptyList())
            assertEquals(Color.BLACK,bitmap.getPixel(w/2,h/2)) // center remains available to face
            if(mode!=VoiceMode.asleep)assertNotEquals(Color.BLACK,bitmap.getPixel(w/2,2))
            else assertEquals(Color.BLACK,bitmap.getPixel(w/2,2))
            if(landscape)assertEquals(Color.BLACK,bitmap.getPixel(2,190))
            val dir=File(requireNotNull(System.getProperty("bluey.renderDir")));dir.mkdirs()
            File(dir,"voice-${mode}-${if(landscape)"landscape" else "portrait"}.png").outputStream().use{assertTrue(bitmap.compress(Bitmap.CompressFormat.PNG,100,it))}
        }
    }
}
