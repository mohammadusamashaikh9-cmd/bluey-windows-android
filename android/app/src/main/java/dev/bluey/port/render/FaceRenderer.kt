package dev.bluey.port.render

import android.graphics.Bitmap
import android.graphics.BlurMaskFilter
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.LinearGradient
import android.graphics.Paint
import android.graphics.Path
import android.graphics.RadialGradient
import android.graphics.RectF
import android.graphics.Shader
import android.graphics.Typeface
import dev.bluey.port.animation.FaceFrame
import dev.bluey.port.animation.FaceGeometry
import dev.bluey.port.animation.Mood
import dev.bluey.port.animation.Palette
import kotlin.math.PI
import kotlin.math.hypot
import kotlin.math.max
import kotlin.math.sin

/** Source-assisted FaceView.draw/BlobShape port. Called by Compose's Canvas.
 * Draw order and all coordinates below are in the original design's units.
 */
class FaceRenderer(private val fredoka: Typeface) {
    private val paint = Paint(Paint.ANTI_ALIAS_FLAG)
    private val bitmapPaint = Paint(Paint.ANTI_ALIAS_FLAG or Paint.FILTER_BITMAP_FLAG)
    private val body = blobPath(820f, 700f)
    private val skin = LinearGradient(205f, 46.9f, 615f, 653.1f,
        Palette.gradient, Palette.stops, Shader.TileMode.CLAMP)
    private val shine = RadialGradient(246f, 126f, 243f,
        intArrayOf(alpha(Color.WHITE, 0.5), Color.TRANSPARENT), null, Shader.TileMode.CLAMP)

    // MaskFilter blur is rasterized once on software canvases, then composited on
    // the hardware canvas. No per-frame bitmap allocation or software View layer.
    // Android/Swift blur kernel differences still require device calibration.
    private val glow = Bitmap.createBitmap(1100, 980, Bitmap.Config.ARGB_8888).also {
        val p = Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = alpha(Palette.berry2, 0.5)
            maskFilter = BlurMaskFilter(45f, BlurMaskFilter.Blur.NORMAL)
        }
        Canvas(it).apply { translate(140f, 130f); drawPath(body, p) }
    }
    private val cheek = Bitmap.createBitmap(150, 102, Bitmap.Config.ARGB_8888).also {
        Canvas(it).drawOval(30f, 30f, 120f, 72f, Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = 0xFFF3A6D8.toInt()
            maskFilter = BlurMaskFilter(10f, BlurMaskFilter.Blur.NORMAL)
        })
    }

    fun draw(canvas: Canvas, width: Float, height: Float, f: FaceFrame) {
        if (width <= 0 || height <= 0) return
        canvas.drawColor(Color.BLACK)
        val cover = FaceGeometry.cover(width, height)
        canvas.saved {
            translate(cover.x, cover.y)
            scale(cover.scale, cover.scale)
            val bounce = (-f.talk * 16 + f.breathe * 3 - f.hop).toFloat()
            val bodyY = 44f + bounce
            translate(422f, bodyY + 700)
            rotate((f.lean * 180 / PI).toFloat())
            val stretch = (1 + (f.hop / 900).coerceIn(-0.03, 0.03)).toFloat()
            scale(2 - stretch, stretch)
            translate(-422f, -bodyY - 700)
            translate(12f, bodyY)
            bitmapPaint.alpha = 255
            drawBitmap(glow, -140f, -140f, bitmapPaint) // shadow offset y = -10
            fill(this, body, Palette.berry3)
            fillSkin(this, body)
            saved {
                clipPath(body)
                oval(this, 190.0, 40.0, 46.0, 22.0, alpha(Color.WHITE, 0.35))
            }
            fill(this, crownPath(368f, (-20 - f.browLift * 0.3).toFloat(), 84f, 56f), Palette.nose)

            for ((i, x) in listOf(280.0, 540.0).withIndex()) {
                val side = if (i == 0) -1 else 1
                bitmapPaint.alpha = (255 * (0.25 + 0.45 * f.blush).coerceIn(0.0, 1.0)).toInt()
                drawBitmap(cheek, (x - 45 + side * 62 - 30).toFloat(), 223f, bitmapPaint)
            }
            for ((i, x) in listOf(280.0, 540.0).withIndex()) {
                val side = if (i == 0) -1 else 1
                saved {
                    translate((x + side * 6).toFloat(), (157 - 112 - f.browLift.coerceIn(-10.0, 18.0) * 0.7).toFloat())
                    rotate(((f.browTilt * side * -1 + if (f.mood == Mood.thinking && i == 1) -0.25 else 0.0) * 180 / PI).toFloat())
                    stroke(this, quad(-38.0, 6.0, 0.0, -12.0, 38.0, 6.0), Palette.nose, 14f)
                }
                when {
                    f.mood == Mood.resting -> stroke(this,
                        quad(x - 62, 163.0, x, 209.0, x + 62, 163.0), Palette.ink, 18f)
                    f.mood == Mood.happy && f.squint > 0.6 -> stroke(this,
                        quad(x - 60, 191.0, x, 103.0, x + 60, 191.0), Palette.ink, 24f)
                    else -> drawEye(this, x, side, f)
                }
            }
            if (f.mood == Mood.resting) for (i in 0..2) {
                val phase = (f.time * 0.35 + i / 3.0) % 1
                val size = 26f + i * 8
                val x = 660 + phase * 70 + sin(phase * 6) * 8
                val y = 90 - phase * 110
                solid(alpha(Color.WHITE, 0.9 * sin(PI * phase)))
                paint.typeface = Typeface.create(fredoka, Typeface.BOLD)
                paint.textSize = size
                paint.textAlign = Paint.Align.CENTER
                val metrics = paint.fontMetrics
                drawText("z", x.toFloat(), (y - (metrics.ascent + metrics.descent) / 2).toFloat(), paint)
            }
            if (f.mood == Mood.thinking) for (i in 0..2) {
                oval(this, 690.0 + i * 26, 40 - i * 12 + sin(f.time * 3 + i) * 4,
                    16.0, 16.0, alpha(Color.WHITE, 1 - i * 0.3))
            }
            val noseW = 60 - f.talk * 8
            val noseH = 38 + f.talk * 34
            val noseX = 410 - noseW / 2
            val noseY = 272 - f.talk * 6
            if (f.mood == Mood.happy && f.talk < 0.1) {
                val smile = quad(noseX - 8, noseY + 8, 410.0, noseY + 58, noseX + noseW + 8, noseY + 8)
                smile.close()
                fill(this, smile, Palette.nose)
            } else {
                oval(this, noseX, noseY, noseW, noseH, Palette.nose)
                if (f.talk > 0.25) oval(this, 410 - noseW * 0.28, noseY + noseH * 0.62,
                    noseW * 0.56, noseH * 0.3, alpha(0xFFE58BC4.toInt(), 0.9))
            }
        }
    }

    private fun drawEye(c: Canvas, x: Double, side: Int, f: FaceFrame) {
        val open = max(0.06, 1 - f.closed) * (1 - f.talk * 0.12)
        val whiteY = 157 - 95 * open
        val whiteH = 190 * open
        val white = ellipse(x - 95, whiteY, 190.0, whiteH)
        fill(c, white, Color.WHITE)
        if (open <= 0.2) return
        val length = hypot(f.gaze.x, f.gaze.y)
        val gx = f.gaze.x / max(1.0, length)
        val gy = f.gaze.y / max(1.0, length)
        val r = 46 * f.pupil
        val px = x + gx * 50 - side * 3
        var py = 157 + gy * 50 * open
        if (f.mood == Mood.sleepy) py = max(py, 157.0) + 28
        c.saved {
            clipPath(white)
            oval(this, px - r, py - r, r * 2, r * 2, Palette.ink)
            oval(this, px - r * 0.58, py - r * 0.72, r * 0.56, r * 0.56, Color.WHITE)
            oval(this, px + r * 0.28, py + r * 0.22, r * 0.24, r * 0.24, alpha(Color.WHITE, 0.85))
            if (f.squint > 0.02) fillSkin(this, ellipse(x - 110, 157 + 95 * open - 70 * f.squint, 220.0, 160.0))
            if (f.mood == Mood.sleepy) {
                val droop = 0.5 + 0.06 * sin(f.time * 1.3)
                fillSkin(this, rectangle(x - 100, whiteY - 10, 200.0, whiteH * droop + 10))
                val line = Path().apply {
                    moveTo((x - 92).toFloat(), (whiteY + whiteH * droop).toFloat())
                    lineTo((x + 92).toFloat(), (whiteY + whiteH * droop).toFloat())
                }
                stroke(this, line, alpha(Palette.nose, 0.7), 8f)
            }
            if (f.mood == Mood.pointing) fillSkin(this, rectangle(x - 100, whiteY - 20, 200.0, 40.0))
        }
    }

    private fun fillSkin(c: Canvas, path: Path) {
        solid(Color.WHITE)
        paint.shader = skin
        c.drawPath(path, paint)
        paint.shader = shine
        c.drawPath(path, paint)
        paint.shader = null
    }
    private fun solid(color: Int) {
        paint.shader = null
        paint.style = Paint.Style.FILL
        paint.color = color
    }
    private fun fill(c: Canvas, path: Path, color: Int) { solid(color); c.drawPath(path, paint) }
    private fun stroke(c: Canvas, path: Path, color: Int, width: Float) {
        solid(color)
        paint.style = Paint.Style.STROKE
        paint.strokeCap = Paint.Cap.ROUND
        paint.strokeWidth = width
        c.drawPath(path, paint)
        paint.style = Paint.Style.FILL
    }
    private fun oval(c: Canvas, x: Double, y: Double, w: Double, h: Double, color: Int) {
        solid(color)
        c.drawOval(x.toFloat(), y.toFloat(), (x + w).toFloat(), (y + h).toFloat(), paint)
    }
    companion object {
        fun blobPath(w: Float, h: Float): Path {
            val tlx = 0.52f * w; val tly = 0.58f * h
            val trx = 0.48f * w; val tryy = 0.56f * h
            val brx = 0.46f * w; val bry = 0.44f * h
            val blx = 0.54f * w; val bly = 0.42f * h
            val k = 0.5523f
            return Path().apply {
                moveTo(tlx, 0f); lineTo(w - trx, 0f)
                cubicTo(w - trx * (1 - k), 0f, w, tryy * (1 - k), w, tryy)
                lineTo(w, h - bry)
                cubicTo(w, h - bry * (1 - k), w - brx * (1 - k), h, w - brx, h)
                lineTo(blx, h)
                cubicTo(blx * (1 - k), h, 0f, h - bly * (1 - k), 0f, h - bly)
                lineTo(0f, tly)
                cubicTo(0f, tly * (1 - k), tlx * (1 - k), 0f, tlx, 0f)
                close()
            }
        }
        fun crownPath(x: Float, y: Float, w: Float, h: Float) = Path().apply {
            FaceGeometry.crown.forEachIndexed { i, (px, py) ->
                if (i == 0) moveTo(x + px / 60 * w, y + py / 40 * h)
                else lineTo(x + px / 60 * w, y + py / 40 * h)
            }
            close()
        }
        private fun alpha(color: Int, amount: Double) =
            (color and 0x00FFFFFF) or ((amount.coerceIn(0.0, 1.0) * 255).toInt() shl 24)
        private fun ellipse(x: Double, y: Double, w: Double, h: Double) = Path().apply {
            addOval(RectF(x.toFloat(), y.toFloat(), (x + w).toFloat(), (y + h).toFloat()), Path.Direction.CW)
        }
        private fun rectangle(x: Double, y: Double, w: Double, h: Double) = Path().apply {
            addRect(x.toFloat(), y.toFloat(), (x + w).toFloat(), (y + h).toFloat(), Path.Direction.CW)
        }
        private fun quad(x: Double, y: Double, cx: Double, cy: Double, ex: Double, ey: Double) = Path().apply {
            moveTo(x.toFloat(), y.toFloat()); quadTo(cx.toFloat(), cy.toFloat(), ex.toFloat(), ey.toFloat())
        }
        private inline fun Canvas.saved(block: Canvas.() -> Unit) {
            val count = save()
            try { block() } finally { restoreToCount(count) }
        }
    }
}
