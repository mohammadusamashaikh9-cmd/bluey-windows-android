package dev.bluey.port.link

import android.net.nsd.NsdManager
import android.net.nsd.NsdServiceInfo
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config
import org.robolectric.annotation.Implements
import org.robolectric.annotation.Implementation

// Callback-boundary simulation only. Does not claim physical multicast/permission success.
@Implements(NsdManager::class)
class RecordingNsd {
    companion object {
        val listeners=mutableListOf<NsdManager.DiscoveryListener>()
        val resolves=mutableListOf<Pair<NsdServiceInfo,NsdManager.ResolveListener>>()
    }
    @Implementation fun discoverServices(type:String,protocol:Int,listener:NsdManager.DiscoveryListener){listeners.add(listener)}
    @Implementation fun stopServiceDiscovery(listener:NsdManager.DiscoveryListener){}
    @Implementation fun resolveService(info:NsdServiceInfo,listener:NsdManager.ResolveListener){resolves.add(info to listener)}
}
@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35],shadows=[RecordingNsd::class])
class DiscoveryLifecycleTest {
    @Before fun clear(){RecordingNsd.listeners.clear();RecordingNsd.resolves.clear()}
    private fun info(name:String)=NsdServiceInfo().apply{serviceName=name;serviceType=Wire.SERVICE}
    @Test fun failedStartCanBeRetriedWithoutErasingSavedTrustOrReopeningAnInvitation() {
        val discovery=Discovery(RuntimeEnvironment.getApplication(),{},{})
        try {
            discovery.start();RecordingNsd.listeners.single().onStartDiscoveryFailed(Wire.SERVICE,0)
            discovery.start();assertEquals(2,RecordingNsd.listeners.size)
        } finally {discovery.stop()}
    }
    @Test fun stopWithPendingResolveCannotBlockNewGenerationOrPublishOldResults() {
        val hosts=mutableListOf<List<DiscoveredHost>>()
        val discovery=Discovery(RuntimeEnvironment.getApplication(),hosts::add,{})
        try {
            discovery.start();RecordingNsd.listeners[0].onServiceFound(info("old"))
            assertEquals(1,RecordingNsd.resolves.size)
            discovery.stop();discovery.start();RecordingNsd.listeners[1].onServiceFound(info("new"))
            assertEquals("Stale resolver must not keep the new generation busy",2,RecordingNsd.resolves.size)
            RecordingNsd.resolves[0].second.onServiceResolved(info("old").apply{host=java.net.InetAddress.getByName("192.168.1.5");port=12345})
            RecordingNsd.listeners[1].onServiceFound(info("queued"));assertEquals(2,RecordingNsd.resolves.size)
            RecordingNsd.resolves[1].second.onServiceResolved(info("new").apply{host=java.net.InetAddress.getByName("192.168.1.6");port=47654})
            assertEquals(3,RecordingNsd.resolves.size)
            assertEquals(listOf("new"),hosts.last().map{it.name})
        } finally {discovery.stop()}
    }
}
