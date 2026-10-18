package dev.bluey.port.animation

import kotlin.math.max

object Palette {
    const val berry1 = 0xFFA9BCFF.toInt()
    const val berry2 = 0xFF6C86F5.toInt()
    const val berry3 = 0xFF4254D6.toInt()
    const val berry4 = 0xFF2B2F8F.toInt()
    const val nose = 0xFF1C1F66.toInt()
    const val ink = 0xFF17151F.toInt()
    const val inkSoft = 0xFFB9B2CC.toInt()
    const val panel = 0xFF1E1B29.toInt()
    val gradient get() = intArrayOf(berry1, berry2, berry3, berry4)
    val stops get() = floatArrayOf(0f, 0.34f, 0.68f, 1f)
}

data class CoverTransform(val scale: Float, val x: Float, val y: Float)
object FaceGeometry {
    const val width = 844f
    const val height = 390f
    fun cover(width: Float, height: Float): CoverTransform {
        require(width > 0 && height > 0)
        val scale = max(width / this.width, height / this.height)
        return CoverTransform(scale, (width - this.width * scale) / 2, height - this.height * scale)
    }
    // Original 60 x 40 crown polygon, retained as design data.
    val crown = listOf(30f to 4f, 36f to 16f, 52f to 12f, 40f to 24f, 46f to 36f,
        30f to 28f, 14f to 36f, 20f to 24f, 8f to 12f, 24f to 16f)
}
