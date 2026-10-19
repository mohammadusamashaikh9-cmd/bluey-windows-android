package dev.bluey.port.link

import java.net.SocketTimeoutException
import java.io.IOException
import javax.net.ssl.SSLHandshakeException
import org.junit.Assert.*
import org.junit.Test

class ReconnectEndpointsTest {
    private val saved=TrustedHost("12345678-1234-1234-1234-123456789abc","a".repeat(64),"192.168.1.20",58105)
    @Test fun unavailableDiscoveryMigratesOldDynamicPortThenKeepsRetryingWithoutNewEnrollment() {
        val plan=ReconnectEndpoints()
        repeat(4) {
            assertEquals(saved,plan.next(saved,emptyList()).host)
            val fallback=plan.next(saved,emptyList())
            assertEquals(saved.copy(port=47654),fallback.host);assertEquals(EndpointSource.STABLE_PORT,fallback.source)
        }
        val stable=saved.copy(port=47654)
        repeat(5){assertEquals(stable,plan.next(stable,emptyList()).host)}
    }
    @Test fun delayedDiscoveryAndChangedLanAddressAreUsedOnTheNextRetry() {
        val plan=ReconnectEndpoints();repeat(2){plan.next(saved,emptyList())}
        val hint=DiscoveredHost("Bluey-12345678","10.0.0.8",40000)
        val target=plan.next(saved,listOf(hint))
        assertEquals(saved.copy(address="10.0.0.8",port=40000),target.host)
        assertEquals(EndpointSource.DISCOVERY,target.source)
        assertEquals(58105,saved.port);assertEquals("192.168.1.20",saved.address)
    }
    @Test fun multipleHintsRotateAndNeverReplaceSavedIdentityOrPin() {
        val hints=listOf(DiscoveredHost("other","10.0.0.9",1),DiscoveredHost("Bluey-12345678","8.8.8.8",1),
            DiscoveredHost("Bluey-12345678","10.0.0.6",30000),DiscoveredHost("Bluey-12345678 (2)","10.0.0.7",30001))
        val plan=ReconnectEndpoints();val targets=List(4){plan.next(saved,hints)}
        assertEquals(listOf(58105,47654,30000,30001),targets.map{it.host.port})
        assertTrue(targets.all{it.host.hostId==saved.hostId&&it.host.pin==saved.pin})
    }
    @Test fun explicitPrivateEndpointWorksWithoutDiscoveryAndWithoutChangingTrust() {
        val manual=LanEndpoint("172.16.1.9",47654)
        val target=ReconnectEndpoints().next(saved,emptyList(),manual)
        assertEquals(EndpointSource.MANUAL,target.source)
        assertEquals(saved.copy(address=manual.address,port=manual.port),target.host)
        for((address,port) in listOf("8.8.8.8" to 47654,"127.0.0.1" to 47654,"192.168.1.2" to 0))
            try{LanEndpoint(address,port);fail("Unsafe routing hint accepted")}catch(_:IllegalArgumentException){}
    }
    @Test fun tcpTimeoutBeforeHostStartAndRejectedHintRemainRetryableButStorageAndKeyFaultsStop() {
        repeat(100){assertTrue(retryTrusted(SocketTimeoutException(),LinkStage.TCP_CONNECTING))}
        val rejected=SSLHandshakeException("not logged").apply{initCause(PeerCertificateFailure(LinkFailure.SERVER_PIN_MISMATCH))}
        assertTrue(retryTrusted(rejected,LinkStage.TLS_STARTED))
        assertFalse(retryTrusted(ClientIdentityFailure(),LinkStage.TLS_STARTED))
        assertFalse(retryTrusted(IOException(),LinkStage.TRUST_PERSISTING))
        assertFalse(retryTrusted(IOException(),LinkStage.IDENTITY_LOADING))
    }
}
