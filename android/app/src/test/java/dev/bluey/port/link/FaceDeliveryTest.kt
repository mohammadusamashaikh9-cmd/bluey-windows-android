package dev.bluey.port.link

import dev.bluey.port.animation.*
import org.junit.Assert.*
import org.junit.Test
class FaceDeliveryTest {
    @Test fun repeatedLatestPacketDoesNotExtendOriginalStaleDeadline() {
        val animator=FaceAnimator(AnimationClock{0.0},RandomSource{0.5});val delivery=FaceDelivery()
        val packet=RemoteFace(LinkFace(.8,-.4,"happy",.7,123),1000,"packet-a")
        assertTrue(delivery.apply(animator,packet,1000,0.0));assertEquals(Mood.happy,animator.step(0.0).mood)
        assertTrue(delivery.apply(animator,packet,3400,2.4));animator.step(2.4)
        assertFalse(delivery.apply(animator,packet,3500,2.5))
        assertEquals(Mood.listening,animator.step(2.5).mood)
        assertFalse(delivery.apply(animator,null,4000,3.0))
        val next=packet.copy(receivedMs=4000,id="packet-b")
        assertTrue(delivery.apply(animator,next,4000,3.0));assertEquals(Mood.happy,animator.step(3.0).mood)
    }
    @Test fun touchStillOverridesFreshRemoteGaze() {
        val animator=FaceAnimator(AnimationClock{0.0},RandomSource{0.5});val delivery=FaceDelivery()
        animator.touchGaze=Gaze(-.8,.3)
        delivery.apply(animator,RemoteFace(LinkFace(.8,-.4,"listening",0.0,0),1000,"test"),1000,0.0)
        animator.step(0.0);var frame=animator.step(1.0/60)
        repeat(59){frame=animator.step((it+2)/60.0)}
        assertTrue(frame.gaze.x<-.5)
    }
}
