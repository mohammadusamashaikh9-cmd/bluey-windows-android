package dev.bluey.port.voice

import java.util.UUID

enum class VoiceMode { asleep, waking, listening, asking, thinking, speaking }
data class PresentationEvent(val turnId:String,val state:VoiceMode,val event:String,val text:String="")
data class VoiceSnapshot(val mode:VoiceMode,val caption:String,val turnId:String,val generation:Long)
data class MockEvent(val due:Long,val generation:Long,val turnId:String,val kind:String,val text:String="")
fun interface MockProvider { fun reply(at:Long,generation:Long,turnId:String):List<MockEvent> }
class OfflineProvider:MockProvider {
    override fun reply(at:Long,generation:Long,turnId:String)=listOf(
        MockEvent(at+700,generation,turnId,"delta","This is an offline "),
        MockEvent(at+1000,generation,turnId,"delta","Bluey preview. "),
        MockEvent(at+1300,generation,turnId,"delta","No audio or cloud is active."),
        MockEvent(at+1600,generation,turnId,"done"))
}
// All input is monotonic milliseconds; no Android, sockets, coroutine jobs, audio or implicit clock.
class VoiceController(private var provider:MockProvider=OfflineProvider(),private val id:()->String={UUID.randomUUID().toString()}) {
    companion object {
        const val HOLD_MS=300L;const val DOUBLE_MS=300L
        val legal=mapOf(
            VoiceMode.asleep to setOf(VoiceMode.waking),
            VoiceMode.waking to setOf(VoiceMode.listening,VoiceMode.asking,VoiceMode.thinking,VoiceMode.asleep),
            VoiceMode.listening to setOf(VoiceMode.asking,VoiceMode.asleep),
            VoiceMode.asking to setOf(VoiceMode.thinking,VoiceMode.listening,VoiceMode.asleep),
            VoiceMode.thinking to setOf(VoiceMode.speaking,VoiceMode.listening,VoiceMode.asleep),
            VoiceMode.speaking to setOf(VoiceMode.listening,VoiceMode.asking,VoiceMode.asleep))
    }
    private var externalReady=false
    fun configureLive(next:MockProvider?,at:Long){sleep(at);externalReady=next!=null;provider=next?:OfflineProvider()}
    private var now=0L;private var mode=VoiceMode.asleep;private var generation=0L
    private var turn=id();private var caption="";private var down:Long?=null
    private var lastTap:Long?=null;private var second=false;private var moved=false
    private var holding=false;private var askWhenReady=false
    private var x=0.0;private var y=0.0;private var lastX=0.0;private var lastY=0.0
    private val scheduled=mutableListOf<MockEvent>();private val outgoing=ArrayDeque<PresentationEvent>()
    val snapshot get()=VoiceSnapshot(mode,caption,turn,generation)
    fun drain():List<PresentationEvent> = outgoing.toList().also{outgoing.clear()}
    private fun emit(kind:String,text:String="") {
        if(outgoing.size>=32){outgoing.clear();sleep(now);return}
        outgoing.addLast(PresentationEvent(turn,mode,kind,text))
    }
    private fun transition(next:VoiceMode) {
        if(mode==next)return
        check(next in legal.getValue(mode));mode=next;emit("state")
    }
    fun tick(at:Long) {
        require(at>=now);now=at
        while(scheduled.firstOrNull()?.due?.let{it<=at}==true)accept(scheduled.removeAt(0),at)
        if(down?.let{at-it>=HOLD_MS}==true&&!holding&&!second)beginHold()
    }
    fun accept(event:MockEvent,at:Long) {
        require(at>=now);now=at
        if(event.generation!=generation||event.turnId!=turn||mode==VoiceMode.asleep||event.due>at)return
        when(event.kind) {
            "error"->sleep(at)
            "empty"->if(mode==VoiceMode.thinking){transition(VoiceMode.listening)}
            "ready"->if(mode==VoiceMode.waking) {
                if(askWhenReady){askWhenReady=false;startReply()}
                else transition(if(holding)VoiceMode.asking else VoiceMode.listening)
            }
            "delta"->if(mode==VoiceMode.thinking||mode==VoiceMode.speaking) {
                if(event.text.isEmpty()||event.text.length>256||caption.length+event.text.length>2048||event.text.any{it.isISOControl()})return
                transition(VoiceMode.speaking);if(mode!=VoiceMode.speaking)return
                caption+=event.text;emit("delta",event.text)
            }
            "done"->if(mode==VoiceMode.speaking){emit("done");if(mode!=VoiceMode.speaking)return
                transition(VoiceMode.listening);if(mode!=VoiceMode.listening)return
                var words=0;var previousSpace=true
                for(c in caption){val space=c.isWhitespace();if(!space&&previousSpace)words++;previousSpace=space}
                scheduled.add(MockEvent(now+((1.2+words*.32).coerceIn(2.8,12.0)+3).times(1000).toLong(),generation,turn,"clear"));scheduled.sortBy{it.due}
            }
            "clear"->if(mode==VoiceMode.listening){caption="";emit("clear")}
        }
    }
    private fun wake() {
        generation++;turn=id();caption="";transition(VoiceMode.waking)
        if(!externalReady)scheduled.add(MockEvent(now+600,generation,turn,"ready"))
    }
    fun toggle(at:Long){tick(at);if(mode==VoiceMode.asleep){clearGesture();wake()}else sleep(at)}
    fun sleep(at:Long) {
        require(at>=now);now=at;generation++;scheduled.clear();outgoing.clear();caption="";clearGesture()
        mode=VoiceMode.asleep;emit("cancel")
    }
    fun pause(at:Long)=sleep(at)
    fun connectionChanged(at:Long)=sleep(at) // No queued/offline reply replay into a new TLS session.
    fun down(at:Long,px:Double=0.0,py:Double=0.0) {
        tick(at);if(down!=null)return
        second=lastTap?.let{at-it in 0..DOUBLE_MS&&(px-lastX)*(px-lastX)+(py-lastY)*(py-lastY)<=24*24}==true
        down=at;x=px;y=py;moved=false
    }
    fun move(at:Long,px:Double,py:Double) {tick(at);if((px-x)*(px-x)+(py-y)*(py-y)>24*24)moved=true}
    fun up(at:Long) {
        tick(at);val pressed=down?:return;down=null
        if(holding) {
            holding=false
            if(mode==VoiceMode.waking)askWhenReady=true else if(mode==VoiceMode.asking)startReply()
            lastTap=null;second=false;return
        }
        if(at-pressed<HOLD_MS&&!moved) {
            if(second){lastTap=null;second=false;toggle(at)}else {lastTap=at;lastX=x;lastY=y}
        }else lastTap=null
        second=false
    }
    private fun beginHold() {
        if(mode !in setOf(VoiceMode.asleep,VoiceMode.waking,VoiceMode.listening,VoiceMode.speaking))return
        holding=true;lastTap=null;caption="";scheduled.removeAll{it.kind!="ready"}
        if(mode==VoiceMode.asleep)wake()
        else if(mode!=VoiceMode.waking){generation++;turn=id();transition(VoiceMode.asking)}
    }
    private fun startReply() {
        transition(VoiceMode.thinking)
        val events=provider.reply(now,generation,turn)
        require(events.size<=24&&events.all{it.due>=now});scheduled.addAll(events);scheduled.sortBy{it.due}
    }
    fun cancelGesture(at:Long) {
        tick(at);if(down==null)return
        if(holding&&mode==VoiceMode.asking)transition(VoiceMode.listening)
        clearGesture()
    }
    private fun clearGesture(){down=null;lastTap=null;holding=false;askWhenReady=false;second=false;moved=false}
}
// Planned permissions are intentionally ungrantable in the offline checkpoint.
class PlannedConsent {
    val microphone=false;val cloud=false;val windowsScreen=false
    val liveCaptureAllowed=false
}
