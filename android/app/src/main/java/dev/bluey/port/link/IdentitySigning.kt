package dev.bluey.port.link

import java.security.MessageDigest
import java.security.PrivateKey
import java.security.PublicKey
import java.security.SecureRandom
import java.security.Signature
import java.security.SignatureException

// Standard JCA operations against the supplied opaque key: never ask for encoded private bytes.
object IdentitySigning {
    fun check(key:PrivateKey,publicKey:PublicKey,event:(LinkStage)->Unit) {
        val challenge=ByteArray(32).also{SecureRandom().nextBytes(it)}
        val digest=MessageDigest.getInstance("SHA-256").digest(challenge)
        try {
            for((algorithm,message,started,passed,failed) in listOf(
                Probe("SHA256withECDSA",challenge,LinkStage.SIGN_SHA256_STARTED,LinkStage.SIGN_SHA256_PASSED,LinkStage.SIGN_SHA256_FAILED),
                Probe("NONEwithECDSA",digest,LinkStage.SIGN_PREHASH_STARTED,LinkStage.SIGN_PREHASH_PASSED,LinkStage.SIGN_PREHASH_FAILED))) {
                event(started)
                try {
                    val signature=Signature.getInstance(algorithm).run{initSign(key);update(message);sign()}
                    try {
                        if(!Signature.getInstance(algorithm).run{initVerify(publicKey);update(message);verify(signature)})
                            throw SignatureException("Identity signing verification failed")
                    } finally {signature.fill(0)}
                    event(passed)
                } catch(e:Exception) {event(failed);throw ClientIdentityFailure(e)}
            }
        } finally {challenge.fill(0);digest.fill(0)}
    }
    private data class Probe(val algorithm:String,val message:ByteArray,val started:LinkStage,val passed:LinkStage,val failed:LinkStage)
}
