package dev.bluey.port.voice

import kotlin.math.PI
import kotlin.math.sin
import kotlin.math.min

data class ModeLook(val rgb:Int,val border:Double,val hz:Double,val label:String?,val icon:String) {
    fun opacity(seconds:Double)=if(hz==0.0)1.0 else .65+.35*(.5+.5*sin(2*PI*hz*seconds))
}
object ModeIndicator {
    const val TRANSITION_MS=250L
    fun look(mode:VoiceMode)=when(mode) {
        VoiceMode.asleep->ModeLook(0xB9B2CC,0.0,0.0,null,"eye")
        VoiceMode.waking->ModeLook(0xFFD66B,6.0,1.6,"Waking up","sun")
        VoiceMode.listening->ModeLook(0x4F8BFF,10.0,0.0,"Listening · hold to ask","ear")
        VoiceMode.asking->ModeLook(0x4F8BFF,16.0,1.2,"I'm all ears","mic")
        VoiceMode.thinking->ModeLook(0xC79BFF,8.0,1.1,"Thinking","sparkles")
        VoiceMode.speaking->ModeLook(0xFF9AD0,8.0,0.0,"Replying","bubble")
    }
    // Explicit cubic ease-in-out x inversion; no platform animation defaults.
    fun ease(t:Double):Double {
        if(t<=0)return 0.0;if(t>=1)return 1.0
        var lo=0.0;var hi=1.0
        repeat(32){val u=(lo+hi)/2;val x=3*(1-u)*(1-u)*u*.42+3*(1-u)*u*u*.58+u*u*u;if(x<t)lo=u else hi=u}
        val u=(lo+hi)/2;return 3*(1-u)*u*u+u*u*u
    }
}
data class IndicatorFrame(val rgb:Int,val border:Double,val opacity:Double,val look:ModeLook)
class IndicatorAnimation {
    private var mode=VoiceMode.asleep;private var start=0L
    private var from=IndicatorFrame(0xB9B2CC,0.0,1.0,ModeIndicator.look(mode))
    fun mode(next:VoiceMode,now:Long){if(next!=mode){from=frame(now);mode=next;start=now}}
    fun frame(now:Long):IndicatorFrame {
        val look=ModeIndicator.look(mode);val p=ModeIndicator.ease((now-start).toDouble()/250)
        fun channel(shift:Int)=(((from.rgb shr shift)and 255)*(1-p)+((look.rgb shr shift)and 255)*p).toInt()
        return IndicatorFrame((channel(16)shl 16)or(channel(8)shl 8)or channel(0),from.border+(look.border-from.border)*p,
            from.opacity+(look.opacity(now/1000.0)-from.opacity)*p,look)
    }
}
data class ScreenShape(val width:Float,val height:Float,val radii:List<Float>,val insetLeft:Float=0f,val insetTop:Float=0f) {
    init{require(width.isFinite()&&height.isFinite()&&width>0&&height>0&&insetLeft.isFinite()&&insetTop.isFinite()&&insetLeft>=0&&insetTop>=0&&radii.size==4&&radii.all{it>=0&&it.isFinite()})}
    fun centerRadii(stroke:Float)=radii.map{(min(it,min(width,height)/2)-stroke/2).coerceAtLeast(0f)}
}
