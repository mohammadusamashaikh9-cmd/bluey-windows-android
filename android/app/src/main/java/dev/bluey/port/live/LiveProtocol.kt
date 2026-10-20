package dev.bluey.port.live
import com.google.gson.JsonObject
import dev.bluey.port.link.*
data class LiveLimits(val sessionSeconds:Int=300,val responses:Int=5,val audioSeconds:Int=60,val maxTokens:Int=256) {
    fun validate(){if(sessionSeconds !in 30..600||responses !in 1..20||audioSeconds !in 1..120||maxTokens !in 16..1024)throw ProtocolException("live limits")}
}
class LiveToken(val authorizationId:String,val requestId:String,val secret:String,val expiresAt:Long,val model:String,val limits:LiveLimits){override fun toString()="[client secret redacted]"}
object LiveProtocol {
    val models=setOf("gpt-realtime-2.1-mini","gpt-realtime-2.1")
    private fun id(p:JsonObject,k:String){if(!Wire.uuid(Wire.text(p,k)))throw ProtocolException("live identity")}
    fun flag(p:JsonObject,k:String):Boolean {val v=p[k];if(!v.isJsonPrimitive||!v.asJsonPrimitive.isBoolean)throw ProtocolException("live consent");return v.asBoolean}
    fun validate(type:String,p:JsonObject) {
        when(type) {
            "liveConsent"->{Wire.keys(p,"consentId","microphone","cloud","continuous");id(p,"consentId");flag(p,"microphone");flag(p,"cloud");flag(p,"continuous")}
            "liveRequest"->{Wire.keys(p,"authorizationId","consentId","requestId");id(p,"authorizationId");id(p,"consentId");id(p,"requestId")}
            "liveGrant"->{Wire.keys(p,"authorizationId","consentId");id(p,"authorizationId");id(p,"consentId")}
            "liveStop"->{Wire.keys(p,"code");if(Wire.text(p,"code") !in setOf("stopped","denied","expired","limit","issuer","disconnected"))throw ProtocolException("live stop")}
            "liveToken"->{Wire.keys(p,"authorizationId","requestId","secret","expiresAt","model","sessionSeconds","responses","audioSeconds","maxTokens");id(p,"authorizationId");id(p,"requestId")
                val secret=Wire.text(p,"secret",512);if(secret.length<16||secret.any{it.code !in 33..126})throw ProtocolException("live secret")
                if(Wire.text(p,"model",64) !in models)throw ProtocolException("model");Wire.integer(p,"expiresAt");limits(p).validate()}
            else->throw ProtocolException("live command")
        }
    }
    private fun number(p:JsonObject,k:String):Int{val n=Wire.integer(p,k);if(n>Int.MAX_VALUE)throw ProtocolException("live integer");return n.toInt()}
    fun limits(p:JsonObject)=LiveLimits(number(p,"sessionSeconds"),number(p,"responses"),number(p,"audioSeconds"),number(p,"maxTokens"))
    fun token(p:JsonObject):LiveToken {validate("liveToken",p);return LiveToken(Wire.text(p,"authorizationId"),Wire.text(p,"requestId"),Wire.text(p,"secret",512),Wire.integer(p,"expiresAt"),Wire.text(p,"model",64),limits(p))}
}
