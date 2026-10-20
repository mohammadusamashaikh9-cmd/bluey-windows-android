package dev.bluey.port.voice

import android.graphics.*

/** Vector-only indicator, no shaders, blur, raster animation or gesture handler. */
class ModeIndicatorRenderer(private val typeface:Typeface) {
    fun draw(canvas:Canvas,shape:ScreenShape,frame:IndicatorFrame,density:Float,cutouts:List<Rect> = emptyList()) {
        val d=density;val width=(frame.border*d).toFloat();val color=0xff000000.toInt() or frame.rgb
        val paint=Paint(Paint.ANTI_ALIAS_FLAG);val saved=canvas.save()
        for(rect in cutouts)canvas.clipOutRect(rect)
        if(width>0) {
            val half=width/2;val radii=shape.centerRadii(width).flatMap{listOf(it,it)}.toFloatArray()
            val path=Path().apply{addRoundRect(RectF(half,half,shape.width-half,shape.height-half),radii,Path.Direction.CW)}
            paint.style=Paint.Style.STROKE;paint.strokeWidth=width;paint.color=color;paint.alpha=(frame.opacity*255).toInt().coerceIn(0,255)
            canvas.drawPath(path,paint)
        }
        canvas.restoreToCount(saved)
        val text=frame.look.label;val labelPaint=Paint(Paint.ANTI_ALIAS_FLAG).apply{this.typeface=this@ModeIndicatorRenderer.typeface;textSize=16*d;this.color=Color.WHITE}
        val pad=if(text==null)0f else 14*d;val iconSize=14*d;val gap=if(text==null)0f else 7*d
        val boxWidth=maxOf(34*d,pad*2+iconSize+gap+(text?.let{labelPaint.measureText(it)}?:0f));val height=34*d
        val x=shape.insetLeft+24*d+width;val y=shape.insetTop+14*d+width
        paint.style=Paint.Style.FILL;paint.color=if(text==null)0x0fffffff else color;paint.alpha=255
        canvas.drawRoundRect(RectF(x,y,x+boxWidth,y+height),height/2,height/2,paint)
        val iconX=x+if(text==null)(boxWidth-iconSize)/2 else pad
        val iconY=y+(height-iconSize)/2
        paint.color=if(text==null)(0xb3000000.toInt() or frame.rgb)else Color.WHITE;paint.strokeWidth=1.8f*d
        paint.strokeCap=Paint.Cap.ROUND;paint.strokeJoin=Paint.Join.ROUND;paint.style=Paint.Style.STROKE
        val checkpoint=canvas.save();canvas.translate(iconX,iconY);canvas.scale(iconSize/20,iconSize/20)
        fun path(block:Path.()->Unit){canvas.drawPath(Path().apply(block),paint)}
        when(frame.look.icon) {
            "eye"->{path{moveTo(1f,10f);cubicTo(6f,2f,14f,2f,19f,10f);cubicTo(14f,18f,6f,18f,1f,10f)};canvas.drawCircle(10f,10f,3f,paint)}
            "sun"->{canvas.drawCircle(10f,10f,4f,paint);for(i in 0..7){val a=i*Math.PI/4;canvas.drawLine((10+7*kotlin.math.cos(a)).toFloat(),(10+7*kotlin.math.sin(a)).toFloat(),(10+9*kotlin.math.cos(a)).toFloat(),(10+9*kotlin.math.sin(a)).toFloat(),paint)}}
            "ear"->path{moveTo(5f,8f);cubicTo(4f,-1f,18f,0f,17f,8f);cubicTo(16f,12f,11f,11f,11f,16f);cubicTo(10f,21f,4f,19f,5f,15f);moveTo(8f,8f);cubicTo(8f,3f,14f,4f,13f,8f);lineTo(9f,12f)}
            "mic"->{canvas.drawRoundRect(RectF(7f,1f,13f,12f),3f,3f,paint);path{moveTo(4f,9f);cubicTo(3f,18f,17f,18f,16f,9f);moveTo(10f,16f);lineTo(10f,20f);moveTo(7f,20f);lineTo(13f,20f)}}
            "sparkles"->for((cx,cy,r) in listOf(Triple(8f,11f,7f),Triple(16f,4f,3f),Triple(17f,17f,2f)))path{moveTo(cx,cy-r);quadTo(cx+1,cy-1,cx+r,cy);quadTo(cx+1,cy+1,cx,cy+r);quadTo(cx-1,cy+1,cx-r,cy);quadTo(cx-1,cy-1,cx,cy-r)}
            else->path{moveTo(4f,2f);lineTo(16f,2f);quadTo(19f,2f,19f,5f);lineTo(19f,13f);quadTo(19f,16f,16f,16f);lineTo(8f,16f);lineTo(3f,20f);lineTo(3f,16f);quadTo(1f,16f,1f,13f);lineTo(1f,5f);quadTo(1f,2f,4f,2f)}
        }
        canvas.restoreToCount(checkpoint)
        text?.let{canvas.drawText(it,iconX+iconSize+gap,y+height/2-(labelPaint.ascent()+labelPaint.descent())/2,labelPaint)}
    }
}
