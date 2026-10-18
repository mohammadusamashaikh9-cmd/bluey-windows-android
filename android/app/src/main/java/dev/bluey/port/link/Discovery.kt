package dev.bluey.port.link

import android.content.Context
import android.net.nsd.NsdManager
import android.net.nsd.NsdServiceInfo
import java.util.concurrent.ConcurrentLinkedQueue

// Legacy NSD APIs remain supported at minSdk 26. Names/endpoints are hints, never peer identities.
@Suppress("DEPRECATION")
class Discovery(context:Context,private val changed:(List<DiscoveredHost>)->Unit,private val status:(String)->Unit) {
    private val manager=context.getSystemService(NsdManager::class.java)
    private val hosts=linkedMapOf<String,DiscoveredHost>();private val queue=ConcurrentLinkedQueue<NsdServiceInfo>()
    private var listener:NsdManager.DiscoveryListener?=null;private var resolving=false;private var generation=0
    @Synchronized fun start() {
        if(listener!=null)return;val run=++generation
        val next=object:NsdManager.DiscoveryListener {
            override fun onDiscoveryStarted(type:String){status("Discovering secure Windows hosts")}
            override fun onDiscoveryStopped(type:String){}
            override fun onStartDiscoveryFailed(type:String,code:Int){status("Discovery unavailable ($code). Paste the complete invitation URI.")}
            override fun onStopDiscoveryFailed(type:String,code:Int){}
            override fun onServiceFound(service:NsdServiceInfo){synchronized(this@Discovery){if(run==generation && service.serviceType==Wire.SERVICE && service.serviceName.length<=128 && queue.size<32 && hosts.size<32){queue.add(service);resolveNext(run)}}}
            override fun onServiceLost(service:NsdServiceInfo){synchronized(this@Discovery){if(run==generation){hosts.remove(service.serviceName);changed(hosts.values.toList())}}}
        }
        listener=next
        try{manager.discoverServices(Wire.SERVICE,NsdManager.PROTOCOL_DNS_SD,next)}catch(e:RuntimeException){status("Discovery permission/network unavailable. Use the full invitation URI.")}
    }
    @Synchronized private fun resolveNext(run:Int) {
        if(resolving||run!=generation)return
        val item=queue.poll()?:return;resolving=true
        try {manager.resolveService(item,object:NsdManager.ResolveListener {
            override fun onResolveFailed(info:NsdServiceInfo,code:Int){synchronized(this@Discovery){resolving=false;resolveNext(run)}}
            override fun onServiceResolved(info:NsdServiceInfo){synchronized(this@Discovery){
                if(run==generation){val ip=info.host?.hostAddress
                    if(ip!=null && Bootstrap.privateAddress(ip) && info.port in 1..65535){hosts[info.serviceName]=DiscoveredHost(info.serviceName,ip,info.port);changed(hosts.values.toList())}}
                resolving=false;resolveNext(run)
            }}
        })}catch(e:RuntimeException){resolving=false;status("Discovery resolution unavailable")}
    }
    @Synchronized fun stop(){generation++;listener?.let{try{manager.stopServiceDiscovery(it)}catch(_:RuntimeException){}};listener=null;hosts.clear();queue.clear();changed(emptyList())}
}
data class DiscoveredHost(val name:String,val address:String,val port:Int)
