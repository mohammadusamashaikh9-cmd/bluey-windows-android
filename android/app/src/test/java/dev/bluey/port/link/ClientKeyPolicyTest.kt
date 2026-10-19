package dev.bluey.port.link

import android.security.keystore.KeyProperties
import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35])
class ClientKeyPolicyTest {
    @Test fun generatedTlsIdentityAuthorizesPrehashedEcdsaWithoutExportOrUserAuthentication() {
        val spec=AndroidIdentity.clientKeySpec("test-client","test-device",1800000000000)
        assertTrue("TLS opaque EC signing needs DIGEST_NONE; SHA authorization alone is insufficient",
            spec.digests.contains(KeyProperties.DIGEST_NONE))
        assertTrue(spec.digests.contains(KeyProperties.DIGEST_SHA256))
        assertEquals(KeyProperties.PURPOSE_SIGN or KeyProperties.PURPOSE_VERIFY,spec.purposes)
        assertFalse(spec.isUserAuthenticationRequired)
        assertEquals("secp256r1",(spec.algorithmParameterSpec as java.security.spec.ECGenParameterSpec).name)
    }
}
