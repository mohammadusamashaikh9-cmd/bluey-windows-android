package dev.bluey.port.voice

import dev.bluey.port.animation.*
import dev.bluey.port.link.RemoteFace
import kotlin.math.sin

// Only data selection; the existing renderer, receive timestamps and spring math stay intact.
object VoiceFace {
    fun select(touch:Gaze?,mode:VoiceMode,remote:RemoteFace?,fresh:Boolean,time:Double):FaceState? {
        if(touch!=null)return FaceState(touch.x,touch.y,Mood.listening,0.0)
        if(mode==VoiceMode.asleep)return null
        val gaze=if(fresh&&remote!=null)Gaze(remote.face.gazeX,remote.face.gazeY)else Gaze(0.0,-.3)
        val mood=when(mode){VoiceMode.waking->Mood.happy;VoiceMode.thinking->Mood.thinking;VoiceMode.speaking->Mood.talking;else->Mood.listening}
        val talk=if(mode==VoiceMode.speaking).5+.5*sin(time*19)*sin(time*7.3) else 0.0
        return FaceState(gaze.x,gaze.y,mood,talk)
    }
}
