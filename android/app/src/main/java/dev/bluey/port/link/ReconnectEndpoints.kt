package dev.bluey.port.link

import java.io.IOException

// Routes carry no authority. Every candidate retains the saved UUID/pin and uses pinned mTLS.
data class LanEndpoint(val address:String,val port:Int) {
    init {require(Bootstrap.privateAddress(address)&&port in 1..65535){"Private IPv4/port required"}}
}
enum class EndpointSource { SAVED, STABLE_PORT, DISCOVERY, MANUAL }
data class ReconnectTarget(val host:TrustedHost,val source:EndpointSource)
class ReconnectEndpoints {
    companion object {const val DEFAULT_PORT=47654}
    private val tried=linkedSetOf<LanEndpoint>()
    fun reset(){tried.clear()}
    fun next(saved:TrustedHost,hints:List<DiscoveredHost>,manual:LanEndpoint?=null):ReconnectTarget {
        val candidates=linkedMapOf<LanEndpoint,EndpointSource>()
        fun add(address:String,port:Int,source:EndpointSource) {
            if(Bootstrap.privateAddress(address)&&port in 1..65535)candidates.putIfAbsent(LanEndpoint(address,port),source)
        }
        manual?.let{add(it.address,it.port,EndpointSource.MANUAL)}
        add(saved.address,saved.port,EndpointSource.SAVED)
        add(saved.address,DEFAULT_PORT,EndpointSource.STABLE_PORT)
        for(h in hints.take(32))if(h.name.startsWith("Bluey-"+saved.hostId.take(8)))add(h.address,h.port,EndpointSource.DISCOVERY)
        tried.retainAll(candidates.keys)
        val entry=candidates.entries.firstOrNull{it.key !in tried}?:run{tried.clear();candidates.entries.first()}
        tried.add(entry.key)
        return ReconnectTarget(saved.copy(address=entry.key.address,port=entry.key.port),entry.value)
    }
}
// Retry transport/untrusted-candidate failures, never key/storage faults. No trust mutation here.
fun retryTrusted(error:Throwable,stage:LinkStage):Boolean {
    if(stage==LinkStage.IDENTITY_LOADING||stage==LinkStage.TRUST_PERSISTING)return false
    var cause:Throwable?=error
    repeat(8) {
        if(cause is ClientIdentityFailure)return false
        cause=cause?.cause
    }
    return error is IOException || error is PeerCertificateFailure
}
