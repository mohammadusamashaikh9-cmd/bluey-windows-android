package dev.bluey.port.link

import java.io.IOException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.security.GeneralSecurityException
import java.security.cert.CertificateException
import java.time.Instant
import java.util.concurrent.atomic.AtomicInteger
import javax.net.ssl.SSLException
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.currentCoroutineContext
import kotlinx.coroutines.ensureActive
import kotlinx.coroutines.CancellationException

enum class LinkStage {
    INVITATION_VALIDATED, INVITATION_INVALID, IDENTITY_LOADING, IDENTITY_READY,
    DIGEST_NONE_AUTHORIZED, DIGEST_NONE_NOT_AUTHORIZED, DIGEST_AUTHORIZATION_UNKNOWN,
    SIGN_SHA256_STARTED, SIGN_SHA256_PASSED, SIGN_SHA256_FAILED,
    SIGN_PREHASH_STARTED, SIGN_PREHASH_PASSED, SIGN_PREHASH_FAILED,
    CLIENT_CERT_SELF_SIGNATURE_VALID, CLIENT_CERT_SELF_SIGNATURE_INVALID,
    CLIENT_CERT_CLIENT_AUTH_ALLOWED, CLIENT_CERT_CLIENT_AUTH_EXCLUDED,
    IDENTITY_REPLACED,
    TCP_CONNECTING, TCP_CONNECTED, TLS_STARTED, SERVER_PIN_VERIFIED, SERVER_CERTIFICATE_REJECTED,
    CLIENT_ALIAS_REQUESTED, CLIENT_EC_OFFERED, CLIENT_EC_NOT_OFFERED, CLIENT_ALIAS_SELECTED,
    CLIENT_ALIAS_AVAILABLE, CLIENT_ALIAS_UNAVAILABLE, CLIENT_CHAIN_PROVIDED, CLIENT_KEY_PROVIDED, CLIENT_KEY_UNAVAILABLE,
    TLS_COMPLETED, HELLO_READING, HELLO_VALIDATED, ENROLL_SENDING, AWAITING_WINDOWS_APPROVAL,
    WINDOWS_APPROVAL_VALIDATED, CONFIRM_SENDING, READY_READING, TRUST_PERSISTING, SESSION_CONFIRMED,
    SESSION_CLOSED, LIFECYCLE_CANCELLED
}
enum class LinkFailure {
    NONE, INVITATION_INVALID, CLIENT_IDENTITY_UNAVAILABLE, CLIENT_IDENTITY_NOT_SELECTED,
    SERVER_PIN_MISMATCH, SERVER_CERTIFICATE_INVALID, TCP_CONNECT_FAILED, TCP_TIMEOUT, TLS_TIMEOUT,
    TLS_HANDSHAKE_FAILED, HELLO_REJECTED, ENROLLMENT_REJECTED_OR_EXPIRED, APPROVAL_TIMEOUT,
    SESSION_REJECTED, WRITE_TIMEOUT, CONNECTION_CLOSED, PROTOCOL_REJECTED, STORAGE_FAILURE,
    CANCELLED, UNKNOWN
}
class PeerCertificateFailure(val category:LinkFailure):CertificateException(category.name)
class ClientIdentityFailure(cause:Throwable?=null):GeneralSecurityException(LinkFailure.CLIENT_IDENTITY_UNAVAILABLE.name,cause)
class ConnectionProblem(val category:LinkFailure):IOException(category.name)

enum class TlsDetail {
    NONE, INVALID_KEY, SIGNATURE_FAILED, PROVIDER_FAILED, CERTIFICATE_FAILED,
    ALERT_BAD_CERTIFICATE, ALERT_UNKNOWN_CA, ALERT_CERTIFICATE_UNKNOWN, ALERT_UNSUPPORTED_CERTIFICATE,
    ALERT_DECRYPT_ERROR, ALERT_HANDSHAKE_FAILURE, ALERT_INTERNAL_ERROR, ALERT_ILLEGAL_PARAMETER,
    NO_SHARED_SIGNATURE_ALGORITHM, TLS_PROTOCOL, TLS_HANDSHAKE, IO_CLOSED
}
// Inspect provider text only for these fixed markers; never include text itself in diagnostic output.
fun tlsDetail(error:Throwable):TlsDetail {
    var next:Throwable?=error
    var fallback=TlsDetail.NONE
    repeat(8) {
        val c=next?:return fallback
        when(c) {
            is java.security.InvalidKeyException->return TlsDetail.INVALID_KEY
            is java.security.SignatureException->return TlsDetail.SIGNATURE_FAILED
            is java.security.ProviderException->return TlsDetail.PROVIDER_FAILED
            is CertificateException->return TlsDetail.CERTIFICATE_FAILED
            is SSLException->{
                val text=c.message.orEmpty().take(4096).lowercase(java.util.Locale.ROOT)
                val markers=listOf("alert bad certificate" to TlsDetail.ALERT_BAD_CERTIFICATE,
                    "alert_bad_certificate" to TlsDetail.ALERT_BAD_CERTIFICATE,
                    "alert unknown ca" to TlsDetail.ALERT_UNKNOWN_CA,"alert_unknown_ca" to TlsDetail.ALERT_UNKNOWN_CA,
                    "alert certificate unknown" to TlsDetail.ALERT_CERTIFICATE_UNKNOWN,"alert_certificate_unknown" to TlsDetail.ALERT_CERTIFICATE_UNKNOWN,
                    "alert unsupported certificate" to TlsDetail.ALERT_UNSUPPORTED_CERTIFICATE,"alert_unsupported_certificate" to TlsDetail.ALERT_UNSUPPORTED_CERTIFICATE,
                    "alert decrypt error" to TlsDetail.ALERT_DECRYPT_ERROR,"alert_decrypt_error" to TlsDetail.ALERT_DECRYPT_ERROR,
                    "alert handshake failure" to TlsDetail.ALERT_HANDSHAKE_FAILURE,"alert_handshake_failure" to TlsDetail.ALERT_HANDSHAKE_FAILURE,
                    "alert internal error" to TlsDetail.ALERT_INTERNAL_ERROR,"alert_internal_error" to TlsDetail.ALERT_INTERNAL_ERROR,
                    "alert illegal parameter" to TlsDetail.ALERT_ILLEGAL_PARAMETER,"alert_illegal_parameter" to TlsDetail.ALERT_ILLEGAL_PARAMETER,
                    "no suitable signature algorithm" to TlsDetail.NO_SHARED_SIGNATURE_ALGORITHM,
                    "no shared signature algorithms" to TlsDetail.NO_SHARED_SIGNATURE_ALGORITHM)
                markers.firstOrNull{text.contains(it.first)}?.let{return it.second}
                fallback=if(c is javax.net.ssl.SSLProtocolException)TlsDetail.TLS_PROTOCOL else TlsDetail.TLS_HANDSHAKE
            }
            is IOException->if(fallback==TlsDetail.NONE)fallback=TlsDetail.IO_CLOSED
        }
        next=c.cause
    }
    return fallback
}
data class LinkDiagnostic(val at:Long,val attempt:String,val stage:LinkStage,val failure:LinkFailure,val port:Int,val detail:TlsDetail,val target:LanEndpoint?)
// Deliberately no free-form message/exception/URI/payload parameter. Strings are local IDs or validated private IPv4 endpoints.
class SafeDiagnostics {
    companion object {const val CAPACITY=96}
    private val next=AtomicInteger()
    private val entries=MutableStateFlow<List<LinkDiagnostic>>(emptyList())
    val events:StateFlow<List<LinkDiagnostic>> = entries
    fun begin()=LinkAttempt("A%08x".format(next.incrementAndGet()),this)
    @Synchronized internal fun record(id:String,stage:LinkStage,failure:LinkFailure,port:Int,detail:TlsDetail=TlsDetail.NONE,target:LanEndpoint?=null) {
        entries.value=(entries.value+LinkDiagnostic(System.currentTimeMillis(),id,stage,failure,port,detail,target)).takeLast(CAPACITY)
    }
    fun text()="Bluey safe connection diagnostics v3 (UTC; ids local; port=client source; endpoint=untrusted route)\n"+
        entries.value.joinToString("\n"){"${Instant.ofEpochMilli(it.at)} ${it.attempt} port=${it.port} ${it.stage} ${it.failure} detail=${it.detail}"+(it.target?.let{endpoint->" endpoint=${endpoint.address}:${endpoint.port}"}?:"")}
}
class LinkAttempt(val id:String,private val log:SafeDiagnostics) {
    @Volatile var current=LinkStage.IDENTITY_LOADING;private set
    @Volatile var port=0
    @Volatile var target:LanEndpoint?=null
    @Volatile var aliasRequested=false;private set
    @Volatile var aliasSelected=false;private set
    @Volatile var handshakeTimedOut=false
    fun phase(stage:LinkStage){current=stage;event(stage)}
    fun event(stage:LinkStage) {
        if(stage==LinkStage.CLIENT_ALIAS_REQUESTED)aliasRequested=true
        if(stage==LinkStage.CLIENT_ALIAS_SELECTED||stage==LinkStage.CLIENT_KEY_PROVIDED)aliasSelected=true
        log.record(id,stage,LinkFailure.NONE,port,target=target)
    }
    fun failed(error:Throwable):LinkFailure {
        val code=classify(error)
        log.record(id,current,code,port,tlsDetail(error),target);return code
    }
    fun cancelled(){log.record(id,LinkStage.LIFECYCLE_CANCELLED,LinkFailure.CANCELLED,port,target=target)}
    private fun classify(error:Throwable):LinkFailure {
        var cause:Throwable?=error
        repeat(8) {
            when(val c=cause) {
                is PeerCertificateFailure->return c.category
                is ClientIdentityFailure->return LinkFailure.CLIENT_IDENTITY_UNAVAILABLE
                is ConnectionProblem->return c.category
            }
            cause=cause?.cause
        }
        if(current==LinkStage.IDENTITY_LOADING)return LinkFailure.CLIENT_IDENTITY_UNAVAILABLE
        if(current==LinkStage.TRUST_PERSISTING)return LinkFailure.STORAGE_FAILURE
        if(current==LinkStage.TCP_CONNECTING)return if(error is SocketTimeoutException)LinkFailure.TCP_TIMEOUT else LinkFailure.TCP_CONNECT_FAILED
        if(current==LinkStage.TLS_STARTED) {
            if(handshakeTimedOut||error is SocketTimeoutException)return LinkFailure.TLS_TIMEOUT
            if(aliasRequested&&!aliasSelected)return LinkFailure.CLIENT_IDENTITY_NOT_SELECTED
            return LinkFailure.TLS_HANDSHAKE_FAILED
        }
        if(current==LinkStage.HELLO_READING)return LinkFailure.HELLO_REJECTED
        if(current==LinkStage.ENROLL_SENDING || current==LinkStage.AWAITING_WINDOWS_APPROVAL)
            return if(error is SocketTimeoutException)LinkFailure.APPROVAL_TIMEOUT else LinkFailure.ENROLLMENT_REJECTED_OR_EXPIRED
        if(current==LinkStage.CONFIRM_SENDING||current==LinkStage.READY_READING)return LinkFailure.SESSION_REJECTED
        return when(error) {
            is CertificateException,is SSLException->LinkFailure.TLS_HANDSHAKE_FAILED
            is ProtocolException->LinkFailure.PROTOCOL_REJECTED
            is GeneralSecurityException->LinkFailure.STORAGE_FAILURE
            is ConnectException->LinkFailure.TCP_CONNECT_FAILED
            is IOException->LinkFailure.CONNECTION_CLOSED
            else->LinkFailure.UNKNOWN
        }
    }
}

// Error-handler boundary, kept separate so cancelled blocking socket reads can be regression-tested.
suspend fun LinkAttempt.failureIfActive(error:Throwable):LinkFailure {
    try {currentCoroutineContext().ensureActive()}
    catch(cancel:CancellationException){cancelled();throw cancel}
    return failed(error)
}
