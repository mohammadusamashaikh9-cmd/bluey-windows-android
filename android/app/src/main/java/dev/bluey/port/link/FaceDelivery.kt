package dev.bluey.port.link

import dev.bluey.port.animation.FaceAnimator
import dev.bluey.port.animation.FaceState
import dev.bluey.port.animation.Mood

data class RemoteFace(val face:LinkFace,val receivedMs:Long,val id:String)
// No sockets, locks, disk or crypto on the animation path; receipt time is translated once.
class FaceDelivery {
    private var delivered:String?=null
    fun apply(animator:FaceAnimator,packet:RemoteFace?,elapsedMs:Long,animationSeconds:Double):Boolean {
        if(packet==null || elapsedMs-packet.receivedMs !in 0L until 2500L)return false
        if(delivered!=packet.id) {
            val f=packet.face
            animator.receive(FaceState(f.gazeX,f.gazeY,Mood.valueOf(f.mood),f.talk),animationSeconds)
            delivered=packet.id
        }
        return true
    }
}
