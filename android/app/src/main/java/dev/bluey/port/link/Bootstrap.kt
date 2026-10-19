package dev.bluey.port.link

import java.net.URI
import java.net.URLDecoder

data class TrustedHost(val hostId:String,val pin:String,val address:String,val port:Int)
data class Bootstrap(val host:TrustedHost,val secret:String,val expires:Long) {
    companion object {
        fun privateAddress(s:String):Boolean {
            val p=s.split('.');if(p.size!=4||p.any{!it.matches(Regex("0|[1-9][0-9]{0,2}"))||it.toInt()>255})return false
            val b=p.map{it.toInt()};return b[0]==10 || b[0]==172 && b[1] in 16..31 || b[0]==192 && b[1]==168
        }
        fun parse(text:String,nowSeconds:Long):Bootstrap {
            if(text.length>2048)throw ProtocolException("invitation size")
            val uri=URI(text)
            if(uri.scheme!="bluey-secure" || uri.rawAuthority!="pair" || !uri.rawPath.isNullOrEmpty() || uri.rawFragment!=null)throw ProtocolException("invitation scheme")
            val fields=linkedMapOf<String,String>()
            for(part in (uri.rawQuery?:"").split('&')) {
                val parts=part.split('=',limit=2);if(parts.size!=2)throw ProtocolException("invitation fields")
                val key=URLDecoder.decode(parts[0],"UTF-8");if(fields.containsKey(key))throw ProtocolException("duplicate invitation field")
                fields[key]=URLDecoder.decode(parts[1],"UTF-8")
            }
            if(fields.keys!=setOf("host","port","id","pin","secret","exp"))throw ProtocolException("invitation fields")
            val address=fields.getValue("host");val port=fields.getValue("port").toIntOrNull()?:0
            val id=fields.getValue("id");val pin=fields.getValue("pin");val secret=fields.getValue("secret");val expires=fields.getValue("exp").toLongOrNull()?:0
            if(!privateAddress(address)||port !in 1..65535||!Wire.uuid(id)||!Wire.pin(pin)||!secret.matches(Regex("[A-Za-z0-9_-]{43}"))||expires<=nowSeconds||expires-nowSeconds>300)throw ProtocolException("invalid/expired invitation")
            return Bootstrap(TrustedHost(id,pin,address,port),secret,expires)
        }
    }
}
class RetrySchedule {
    private var failures=0
    fun nextMs():Long { val n=1000L shl failures.coerceAtMost(4);failures++;return n.coerceAtMost(15000) }
    fun reset(){failures=0}
}
