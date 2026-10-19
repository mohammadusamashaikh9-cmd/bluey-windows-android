package dev.bluey.port.link

import com.google.gson.JsonObject
import dev.bluey.port.voice.PresentationEvent
import dev.bluey.port.voice.VoiceMode

object PresentationCodec {
    fun payload(e:PresentationEvent)=Wire.payload("turnId" to e.turnId,"state" to e.state.name,"event" to e.event,"text" to e.text)
    fun validate(p:JsonObject) {
        Wire.keys(p,"turnId","state","event","text")
        if(!Wire.uuid(Wire.text(p,"turnId")))throw ProtocolException("presentation identity")
        val state=Wire.text(p,"state",16);if(state !in VoiceMode.entries.map{it.name})throw ProtocolException("presentation state")
        val event=Wire.text(p,"event",16);if(event !in setOf("state","delta","done","clear","cancel"))throw ProtocolException("presentation event")
        val element=p["text"];if(!element.isJsonPrimitive||!element.asJsonPrimitive.isString)throw ProtocolException("presentation text")
        val text=element.asString
        if(text.length>256||text.any{it.isISOControl()})throw ProtocolException("presentation text")
        var i=0
        while(i<text.length){val c=text[i++];if(c.isHighSurrogate()){if(i==text.length||!text[i++].isLowSurrogate())throw ProtocolException("unicode")}else if(c.isLowSurrogate())throw ProtocolException("unicode")}
        if(event=="delta"&&(state!="speaking"||text.isEmpty()))throw ProtocolException("presentation delta")
        if(event!="delta"&&text.isNotEmpty()||event=="done"&&state!="speaking"||event=="cancel"&&state!="asleep")throw ProtocolException("presentation role")
    }
}
