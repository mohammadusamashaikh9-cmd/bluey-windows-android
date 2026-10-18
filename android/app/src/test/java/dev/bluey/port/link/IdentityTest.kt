package dev.bluey.port.link

import org.junit.Assert.*
import org.junit.Test
import java.security.cert.CertificateFactory
import java.security.cert.CertificateException
import java.security.cert.X509Certificate
class IdentityTest {
    @Test fun exactCertificatePinAcceptsOnlyIntendedPeer() {
        val c=CertificateFactory.getInstance("X.509").generateCertificate(javaClass.getResourceAsStream("/test-server.pem")) as X509Certificate
        val pin=PeerIdentity.fingerprint(c);assertEquals(64,pin.length)
        PeerIdentity.verify(arrayOf(c),pin)
        for(chain in listOf(null,emptyArray<X509Certificate>(),arrayOf(c))) {
            try{PeerIdentity.verify(chain,"0".repeat(64));fail("Untrusted peer accepted")}catch(_:CertificateException){}
        }
    }
}
