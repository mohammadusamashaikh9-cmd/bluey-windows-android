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

enum class LinkStage {
    INVITATION_VALIDATED, INVITATION_INVALID, IDENTITY_LOADING, IDENTITY_READY,
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

data class LinkDiagnostic(val at:Long,val attempt:String,val stage:LinkStage,val failure:LinkFailure,val port:Int)
// Deliberately no free-form message/exception/URI/payload parameter. The only strings are local IDs.
class SafeDiagnostics {
    companion object {const val CAPACITY=96}
    private val next=AtomicInteger()
    private val entries=MutableStateFlow<List<LinkDiagnostic>>(emptyList())
    val events:StateFlow<List<LinkDiagnostic>> = entries
    fun begin()=LinkAttempt("A%08x".format(next.incrementAndGet()),this)
    @Synchronized internal fun record(id:String,stage:LinkStage,failure:LinkFailure,port:Int) {
        entries.value=(entries.value+LinkDiagnostic(System.currentTimeMillis(),id,stage,failure,port)).takeLast(CAPACITY)
    }
    fun text()="Bluey safe connection diagnostics v1 (UTC; ids local; port=client TCP source port)\n"+
        entries.value.joinToString("\n"){"${Instant.ofEpochMilli(it.at)} ${it.attempt} port=${it.port} ${it.stage} ${it.failure}"}
}
class LinkAttempt(val id:String,private val log:SafeDiagnostics) {
    @Volatile var current=LinkStage.IDENTITY_LOADING;private set
    @Volatile var port=0
    @Volatile var aliasRequested=false;private set
    @Volatile var aliasSelected=false;private set
    @Volatile var handshakeTimedOut=false
    fun phase(stage:LinkStage){current=stage;event(stage)}
    fun event(stage:LinkStage) {
        if(stage==LinkStage.CLIENT_ALIAS_REQUESTED)aliasRequested=true
        if(stage==LinkStage.CLIENT_ALIAS_SELECTED||stage==LinkStage.CLIENT_KEY_PROVIDED)aliasSelected=true
        log.record(id,stage,LinkFailure.NONE,port)
    }
    fun failed(error:Throwable):LinkFailure {
        val code=classify(error)
        log.record(id,current,code,port);return code
    }
    fun cancelled(){log.record(id,LinkStage.LIFECYCLE_CANCELLED,LinkFailure.CANCELLED,port)}
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
