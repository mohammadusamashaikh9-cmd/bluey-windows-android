package dev.bluey.port.link

import java.io.IOException
import java.net.ConnectException
import java.net.SocketTimeoutException
import javax.net.ssl.SSLHandshakeException
import org.junit.Assert.*
import org.junit.Test
class FailureCategoryTest {
    @Test fun boundariesAndWrappedPinErrorsKeepDistinctSafeCategories() {
        val log=SafeDiagnostics();val attempt=log.begin()
        attempt.phase(LinkStage.TCP_CONNECTING)
        assertEquals(LinkFailure.TCP_CONNECT_FAILED,attempt.failed(ConnectException("NEVER_PRINT_THIS")))
        assertEquals(LinkFailure.TCP_TIMEOUT,attempt.failed(SocketTimeoutException("NEVER_PRINT_THIS")))
        attempt.phase(LinkStage.TLS_STARTED)
        assertEquals(LinkFailure.SERVER_PIN_MISMATCH,attempt.failed(SSLHandshakeException("NEVER_PRINT_THIS").apply{initCause(PeerCertificateFailure(LinkFailure.SERVER_PIN_MISMATCH))}))
        attempt.event(LinkStage.CLIENT_ALIAS_REQUESTED)
        assertEquals(LinkFailure.CLIENT_IDENTITY_NOT_SELECTED,attempt.failed(SSLHandshakeException("NEVER_PRINT_THIS")))
        attempt.phase(LinkStage.HELLO_READING)
        assertEquals(LinkFailure.HELLO_REJECTED,attempt.failed(IOException("NEVER_PRINT_THIS")))
        attempt.phase(LinkStage.AWAITING_WINDOWS_APPROVAL)
        assertEquals(LinkFailure.APPROVAL_TIMEOUT,attempt.failed(SocketTimeoutException("NEVER_PRINT_THIS")))
        attempt.cancelled();assertFalse(log.text().contains("NEVER_PRINT_THIS"))
    }
    @Test fun diagnosticTailIsBoundedAcrossRepeatedFailures() {
        val log=SafeDiagnostics()
        repeat(1000){log.begin().apply{phase(LinkStage.INVITATION_INVALID);failed(ConnectionProblem(LinkFailure.INVITATION_INVALID))}}
        assertEquals(SafeDiagnostics.CAPACITY,log.events.value.size)
        assertEquals(SafeDiagnostics.CAPACITY+1,log.text().lines().size)
    }
    @Test fun tlsProviderDetailsNeverEchoExceptionTextOrSecrets() {
        val cases=listOf(
            SSLHandshakeException("error SSLV3_ALERT_BAD_CERTIFICATE NEVER_LOG") to TlsDetail.ALERT_BAD_CERTIFICATE,
            SSLHandshakeException("NO SUITABLE SIGNATURE ALGORITHM NEVER_LOG") to TlsDetail.NO_SHARED_SIGNATURE_ALGORITHM,
            SSLHandshakeException("NEVER_LOG").apply{initCause(java.security.InvalidKeyException("PRIVATE_KEY_TEXT"))} to TlsDetail.INVALID_KEY,
            java.security.SignatureException("PRIVATE_SIGNATURE") to TlsDetail.SIGNATURE_FAILED,
            java.security.ProviderException("PRIVATE_PROVIDER") to TlsDetail.PROVIDER_FAILED)
        val log=SafeDiagnostics()
        for((error,expected) in cases) {
            assertEquals(expected,tlsDetail(error))
            log.begin().apply{phase(LinkStage.TLS_STARTED);failed(error)}
        }
        assertFalse(log.text().contains("NEVER_LOG"));assertFalse(log.text().contains("PRIVATE_"))
    }

}
