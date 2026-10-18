package dev.bluey.port.link

import java.security.KeyPairGenerator
import java.security.PrivateKey
import java.security.spec.ECGenParameterSpec
import org.junit.Assert.*
import org.junit.Test

class IdentitySigningTest {
    private fun key()=KeyPairGenerator.getInstance("EC").apply{initialize(ECGenParameterSpec("secp256r1"))}.generateKeyPair()
    @Test fun actualShaAndPrehashedEcdsaSignaturesVerifyAgainstTheCertificateKey() {
        val pair=key();val stages=mutableListOf<LinkStage>()
        IdentitySigning.check(pair.private,pair.public,stages::add)
        assertEquals(listOf(LinkStage.SIGN_SHA256_STARTED,LinkStage.SIGN_SHA256_PASSED,
            LinkStage.SIGN_PREHASH_STARTED,LinkStage.SIGN_PREHASH_PASSED),stages)
    }
    @Test fun certificateKeyMismatchFailsBeforeTls() {
        val stages=mutableListOf<LinkStage>()
        try {IdentitySigning.check(key().private,key().public,stages::add);fail("Wrong certificate key accepted")}
        catch(_:ClientIdentityFailure){}
        assertEquals(listOf(LinkStage.SIGN_SHA256_STARTED,LinkStage.SIGN_SHA256_FAILED),stages)
    }
    @Test fun unavailableOpaqueKeyFailsWithoutExportOrLoggingKeyMaterial() {
        val opaque=object:PrivateKey {
            override fun getAlgorithm()="EC"
            override fun getFormat():String?=null
            override fun getEncoded():ByteArray=throw AssertionError("Private key export attempted")
        }
        val stages=mutableListOf<LinkStage>()
        try {IdentitySigning.check(opaque,key().public,stages::add);fail("Unavailable signer accepted")}
        catch(_:ClientIdentityFailure){}
        assertEquals(LinkStage.SIGN_SHA256_FAILED,stages.last())
    }
}
