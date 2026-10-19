package dev.bluey.port.voice
import org.junit.Assert.*
import org.junit.Test
import dev.bluey.port.animation.*
class VoiceControllerTest {
    private fun awake()=VoiceController().apply{toggle(0);tick(600);drain()}
    @Test fun sixStatesHoldThresholdAndStreaming() {
        val c=VoiceController();assertEquals(VoiceMode.asleep,c.snapshot.mode)
        c.toggle(0);assertEquals(VoiceMode.waking,c.snapshot.mode);c.tick(600);assertEquals(VoiceMode.listening,c.snapshot.mode)
        c.down(700);c.tick(999);assertEquals(VoiceMode.listening,c.snapshot.mode)
        c.tick(1000);assertEquals(VoiceMode.asking,c.snapshot.mode);c.up(1001);assertEquals(VoiceMode.thinking,c.snapshot.mode)
        c.tick(1701);assertEquals(VoiceMode.speaking,c.snapshot.mode);assertEquals("This is an offline ",c.snapshot.caption)
        c.tick(2601);assertEquals(VoiceMode.listening,c.snapshot.mode)
        assertEquals("This is an offline Bluey preview. No audio or cloud is active.",c.snapshot.caption)
        assertEquals(3,c.drain().count{it.event=="delta"});c.sleep(2602);assertEquals("",c.snapshot.caption)
    }
    @Test fun shortTapAndDoubleTapNeverAskEvenWhenSecondPressIsLong() {
        val c=VoiceController();c.down(0);c.up(50);c.cancelGesture(50);c.down(150);c.tick(500)
        assertEquals(VoiceMode.asleep,c.snapshot.mode);c.up(501);assertEquals(VoiceMode.asleep,c.snapshot.mode)
        c.down(600);c.up(630);c.down(700);c.up(730);assertEquals(VoiceMode.waking,c.snapshot.mode)
        assertFalse(c.drain().any{it.state==VoiceMode.asking})
    }
    @Test fun doubleTapSleepCancelsAllScheduledWork() {
        val c=awake();c.down(700);c.up(730);c.down(800);c.up(830)
        assertEquals(VoiceMode.asleep,c.snapshot.mode);c.tick(10000);assertEquals(VoiceMode.asleep,c.snapshot.mode)
    }
    @Test fun holdWhileAsleepAndReleaseDuringWakeQueuesOnlyOneReply() {
        val c=VoiceController();c.down(0);c.tick(300);assertEquals(VoiceMode.waking,c.snapshot.mode)
        c.up(350);c.up(351);c.tick(900);assertEquals(VoiceMode.thinking,c.snapshot.mode)
        c.tick(1600);assertEquals(VoiceMode.speaking,c.snapshot.mode)
    }
    @Test fun pauseSleepAndConnectionChangeRejectDelayedCompletions() {
        for(cancel in listOf<(VoiceController,Long)->Unit>({c,t->c.sleep(t)},{c,t->c.pause(t)},{c,t->c.connectionChanged(t)})) {
            val c=awake();c.down(700);c.up(1000);val old=c.snapshot
            cancel(c,1100);c.accept(MockEvent(1200,old.generation,old.turnId,"delta","late"),1200);c.tick(20000)
            assertEquals(VoiceMode.asleep,c.snapshot.mode);assertEquals("",c.snapshot.caption)
            assertEquals(listOf("cancel"),c.drain().map{it.event})
        }
    }
    @Test fun canceledHoldDoesNotCommitAndClockCannotReverse() {
        val c=awake();c.down(700);c.tick(1000);c.cancelGesture(1001);c.up(1002)
        assertEquals(VoiceMode.listening,c.snapshot.mode);c.tick(10000);assertEquals("",c.snapshot.caption)
        try{c.tick(1);fail()}catch(_:IllegalArgumentException){}
    }
    @Test fun expressionPrecedenceAndDeniedConsent() {
        assertEquals(Mood.listening,VoiceFace.select(Gaze(.4,.5),VoiceMode.thinking,null,false,0.0)!!.mood)
        assertEquals(Mood.thinking,VoiceFace.select(null,VoiceMode.thinking,null,false,0.0)!!.mood)
        assertNull(VoiceFace.select(null,VoiceMode.asleep,null,false,0.0))
        val p=PlannedConsent();assertFalse(p.microphone||p.cloud||p.windowsScreen||p.liveCaptureAllowed)
        assertEquals(6,VoiceController.legal.size)
        assertFalse(VoiceMode.speaking in VoiceController.legal.getValue(VoiceMode.asleep))
    }
    @Test fun exactSourceIndicatorParametersPulseAndTransitions() {
        val colors=listOf(0xB9B2CC,0xFFD66B,0x4F8BFF,0x4F8BFF,0xC79BFF,0xFF9AD0)
        val widths=listOf(0.0,6.0,10.0,16.0,8.0,8.0);val rates=listOf(0.0,1.6,0.0,1.2,1.1,0.0)
        VoiceMode.entries.forEachIndexed{i,m->val l=ModeIndicator.look(m);assertEquals(colors[i],l.rgb);assertEquals(widths[i],l.border,0.0);assertEquals(rates[i],l.hz,0.0)
            if(l.hz>0){assertEquals(.825,l.opacity(0.0),1e-12);assertEquals(1.0,l.opacity(.25/l.hz),1e-12);assertEquals(.65,l.opacity(.75/l.hz),1e-12)}}
        assertEquals(250L,ModeIndicator.TRANSITION_MS);assertEquals(.5,ModeIndicator.ease(.5),1e-8)
        val a=IndicatorAnimation();a.mode(VoiceMode.asking,100);assertEquals(0.0,a.frame(100).border,0.0)
        assertEquals(8.0,a.frame(225).border,1e-7);val interrupted=a.frame(225);a.mode(VoiceMode.thinking,225)
        assertEquals(interrupted,a.frame(225).copy(look=interrupted.look));assertEquals(8.0,a.frame(475).border,1e-8)
    }
    @Test fun landscapeInsetsAndCornerClamping() {
        val a=ScreenShape(844f,390f,listOf(55f,0f,500f,16f),32f,0f)
        assertEquals(listOf(47f,0f,187f,8f),a.centerRadii(16f));assertEquals(32f,a.insetLeft,0f)
        val b=ScreenShape(390f,844f,listOf(55f,55f,55f,55f),0f,32f);assertEquals(a.centerRadii(16f)[0],b.centerRadii(16f)[0])
    }
}
