package dev.bluey.port.link

import android.content.Context
import android.os.Build
import android.os.SystemClock
import kotlinx.coroutines.*
import kotlinx.coroutines.channels.Channel
import dev.bluey.port.voice.PresentationEvent
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.io.EOFException
import java.net.InetSocketAddress
import java.net.Socket
import java.util.UUID
import javax.net.ssl.SSLSocket

class SocketWire(private val socket:SSLSocket,val session:String,hello:Message) {
    private val frames=Frames();private val rate=MessageRate();private val waiting=ArrayDeque<ByteArray>();val incoming=SequenceGuard(session)
    private var outgoing=0L;private val input=socket.inputStream;private val output=socket.outputStream
    init{incoming.accept(hello)}
    fun read(timeoutMs:Int=5000):Message {
        val deadline=SystemClock.elapsedRealtime()+timeoutMs
        while(waiting.isEmpty()) {
            val remaining=deadline-SystemClock.elapsedRealtime();if(remaining<=0)throw java.net.SocketTimeoutException()
            socket.soTimeout=remaining.toInt();val bytes=ByteArray(1024);val n=input.read(bytes)
            if(n<0){frames.end();throw EOFException()};waiting.addAll(frames.feed(bytes.copyOf(n)))
        }
        rate.accept(SystemClock.elapsedRealtime())
        return Wire.decode(waiting.removeFirst()).also{incoming.accept(it)}
    }
    private val sendGate=Mutex()
    suspend fun send(type:String,payload:com.google.gson.JsonObject):String = sendGate.withLock { coroutineScope {
        val id=UUID.randomUUID().toString();val bytes=Wire.encode(type,session,outgoing++,payload,id)
        val expired=java.util.concurrent.atomic.AtomicBoolean()
        val guard=launch { delay(2000);expired.set(true);socket.close() }
        try { output.write(bytes);output.flush();id }
        catch(e:java.io.IOException){if(expired.get())throw ConnectionProblem(LinkFailure.WRITE_TIMEOUT);throw e}
        finally { guard.cancel() }
    }}
    companion object {
        fun hello(socket:SSLSocket):Message {
            val deadline=SystemClock.elapsedRealtime()+5000;val bytes=java.io.ByteArrayOutputStream()
            while(true) {
                val remaining=deadline-SystemClock.elapsedRealtime();if(remaining<=0)throw java.net.SocketTimeoutException()
                socket.soTimeout=remaining.toInt();val b=socket.inputStream.read()
                if(b<0)throw EOFException();if(b==10)break
                if(bytes.size()==Wire.MAX_FRAME)throw ProtocolException("size");bytes.write(b)
            }
            return Wire.decode(bytes.toByteArray()).also{if(it.type!="hello"||it.seq!=0L)throw ProtocolException("hello")}
        }
    }
}
data class LinkUi(val status:String="Disconnected — offline demo",val hosts:List<DiscoveredHost> = emptyList(),val trusted:TrustedHost?=null,val connected:Boolean=false,val clientPin:String?=null,val discoveryStatus:String="Discovery idle")

// Foreground-only lifetime, one read task and one heartbeat task. Latest face is a conflated single slot.
class DeviceLink(context:Context) {
    private val app=context.applicationContext
    private val safeLog=SafeDiagnostics()
    val diagnostics get()=safeLog.events
    fun diagnosticText()=safeLog.text()
    @Volatile private var enrollmentAttempt:LinkAttempt?=null
    private val scope=CoroutineScope(SupervisorJob()+Dispatchers.IO)
    private val initGate=Mutex();private val sessionGate=Mutex();private var identity:AndroidIdentity?=null
    private val mutable=MutableStateFlow(LinkUi());val ui:StateFlow<LinkUi> = mutable
    private val latest=MutableStateFlow<RemoteFace?>(null);val face:StateFlow<RemoteFace?> = latest
    @Volatile private var socket:Socket?=null
    @Volatile private var presentationSink:Channel<PresentationEvent>?=null
    fun present(event:PresentationEvent) {
        val sink=presentationSink?:return // Offline events never queue for reconnection.
        if(sink.trySend(event).isFailure)sink.close(java.io.IOException("presentation backpressure"))
    }
    @Volatile private var foreground=false
    @Volatile private var desired=true
    @Volatile private var replacingIdentity=false
    @Volatile private var invitation:Bootstrap?=null
    @Volatile private var currentHost:TrustedHost?=null
    @Volatile private var endpointHint:LanEndpoint?=null
    private var worker:Job?=null
    // Closing TLS may emit a close-notify; never do that from Compose/lifecycle callbacks.
    @Synchronized private fun closeSocket() {
        val old=socket;socket=null
        if(old!=null)scope.launch(NonCancellable){runCatching{old.close()}}
    }
    @Synchronized private fun installSocket(next:Socket) {
        if(!foreground||!desired){scope.launch(NonCancellable){runCatching{next.close()}};throw CancellationException()}
        socket=next
    }
    private val discovery=Discovery(app,{hosts->mutable.update{snapshot->snapshot.copy(hosts=hosts)}},{message->mutable.update{it.copy(discoveryStatus=message)}})
    private fun status(message:String){mutable.update{snapshot->snapshot.copy(status=message,trusted=currentHost)}}
    private suspend fun keys():AndroidIdentity=initGate.withLock {
        identity?:AndroidIdentity(app).also{identity=it;currentHost=it.load();mutable.update{snapshot->snapshot.copy(trusted=currentHost,clientPin=PeerIdentity.fingerprint(it.certificate))}}
    }
    @Synchronized fun resume(){foreground=true;discovery.start();restart()}
    @Synchronized fun pause(){foreground=false;worker?.cancel();closeSocket();latest.value=null;mutable.update{snapshot->snapshot.copy(connected=false,status="Paused — connection closed")};discovery.stop()}
    @Synchronized private fun restart(){worker?.cancel();closeSocket();latest.value=null;mutable.update{it.copy(connected=false)};if(foreground && desired && !replacingIdentity)worker=scope.launch{sessionGate.withLock{loop()}}}
    @Synchronized fun pair(text:String) {
        if(replacingIdentity){status("Identity replacement in progress");return}
        val attempt=safeLog.begin()
        try {
            invitation=Bootstrap.parse(text,System.currentTimeMillis()/1000);enrollmentAttempt=attempt;endpointHint=null
            attempt.phase(LinkStage.INVITATION_VALIDATED);desired=true;restart()
        } catch(_:Exception){
            attempt.phase(LinkStage.INVITATION_INVALID);attempt.failed(ConnectionProblem(LinkFailure.INVITATION_INVALID))
            status("${attempt.id}: INVITATION_INVALID. Copy the complete fresh Windows URI; check both device clocks.")
        }
    }
    @Synchronized fun connect(selected:DiscoveredHost?=null) {
        if(replacingIdentity){status("Identity replacement in progress");return}
        endpointHint=selected?.let{runCatching{LanEndpoint(it.address,it.port)}.getOrNull()}
        invitation=null;enrollmentAttempt=null;desired=true;restart()
    }
    fun refreshDiscovery(){if(foreground)discovery.refresh()}
    fun connectEndpoint(address:String,port:String) {
        val endpoint=runCatching{LanEndpoint(address.trim(),port.toInt())}.getOrNull()
        if(endpoint==null){status("Enter a private IPv4 address and port 1–65535. Saved identity unchanged.");return}
        connect(DiscoveredHost("manual endpoint hint",endpoint.address,endpoint.port))
    }
    @Synchronized fun disconnect(){desired=false;invitation=null;worker?.cancel();closeSocket();latest.value=null;mutable.update{snapshot->snapshot.copy(connected=false,status="Disconnected — offline demo")}}
    fun forget(){disconnect();scope.launch(NonCancellable){
        try {initGate.withLock{AndroidIdentity.eraseTrust(app);currentHost=null;mutable.update{snapshot->snapshot.copy(trusted=null)}}}
        catch(_:Exception){status("Saved trust could not be removed. Check protected storage.")}
    }}
    fun testIdentity() {
        if(replacingIdentity)return
        scope.launch {
            val attempt=safeLog.begin()
            try {initGate.withLock {
                attempt.phase(LinkStage.IDENTITY_LOADING)
                val value=identity?:AndroidIdentity(app).also{identity=it;currentHost=it.load()}
                value.signingSelfTest(attempt::event)
                mutable.update{it.copy(clientPin=PeerIdentity.fingerprint(value.certificate))}
                attempt.phase(LinkStage.IDENTITY_READY)
                status("${attempt.id}: SHA-256 and TLS prehashed signing checks passed. This is not a TLS session test.")
            }} catch(e:Exception){attempt.failed(e);status("${attempt.id}: Identity self-test failed; inspect safe diagnostics. No identity was replaced.")}
        }
    }
    @Synchronized fun replaceIdentity() {
        if(replacingIdentity)return
        replacingIdentity=true
        disconnect()
        scope.launch {
            // Wait for cancelled session cleanup before deleting the key it could still be using.
            try {sessionGate.withLock {initGate.withLock {
                val attempt=safeLog.begin()
                try {
                    AndroidIdentity.replaceIdentity(app);identity=null;currentHost=null
                    mutable.update{it.copy(trusted=null,clientPin=null)}
                    attempt.phase(LinkStage.IDENTITY_REPLACED)
                    status("${attempt.id}: Old phone identity removed. Revoke its Windows trust entry; use a fresh invitation to enroll a new identity.")
                } catch(e:Exception){attempt.failed(e);status("${attempt.id}: Identity replacement failed; inspect safe diagnostics.")}
            }}} finally {replacingIdentity=false}
        }
    }
    fun close(){pause();scope.cancel()}
    private suspend fun loop() {
        val retry=RetrySchedule();val endpoints=ReconnectEndpoints()
        try {
            while(currentCoroutineContext().isActive && foreground && desired) {
                val enroll=invitation
                val attempt=if(enroll!=null)enrollmentAttempt?:safeLog.begin() else safeLog.begin()
                try {
                    progress(attempt,LinkStage.IDENTITY_LOADING,"Loading protected client identity")
                    val keys=keys();keys.signingSelfTest(attempt::event);attempt.phase(LinkStage.IDENTITY_READY);currentCoroutineContext().ensureActive()
                    val saved=enroll?.host?:currentHost
                    if(saved==null){status("No trusted host. Scan/paste a Windows invitation, then Pair.");return}
                    discovery.start() // A platform start failure may now be retried; no trust mutation.
                    val target=if(enroll!=null)ReconnectTarget(saved,EndpointSource.SAVED)
                        else endpoints.next(saved,mutable.value.hosts,endpointHint)
                    session(keys,target.host,enroll,attempt)
                    throw EOFException()
                } catch(e:CancellationException){attempt.cancelled();throw e}
                catch(e:Exception) {
                    val code=attempt.failureIfActive(e)
                    latest.value=null;mutable.update{it.copy(connected=false)}
                    val failure="${attempt.id}: ${attempt.current} / $code. "+failureHint(code)
                    if(enroll!=null || !retryTrusted(e,attempt.current)) {
                        if(enroll!=null){invitation=null;enrollmentAttempt=null}
                        desired=false;status(failure);return
                    }
                    if(attempt.current==LinkStage.SESSION_CONFIRMED){retry.reset();endpoints.reset()}
                    status(failure+" Saved identity retained. Retrying routes securely; start Windows host or update its LAN endpoint.")
                    delay(retry.nextMs())
                } finally {closeSocket();attempt.event(LinkStage.SESSION_CLOSED)}
            }
        } finally{latest.value=null;mutable.update{it.copy(connected=false)}}
    }
    private fun progress(attempt:LinkAttempt,stage:LinkStage,message:String) {
        attempt.phase(stage);status("${attempt.id}: $message")
    }
    private fun failureHint(code:LinkFailure)=when(code) {
        LinkFailure.TCP_CONNECT_FAILED,LinkFailure.TCP_TIMEOUT->"TCP was not established; verify the endpoint and LAN reachability."
        LinkFailure.SERVER_PIN_MISMATCH->"Server certificate pin mismatch; verify the intended Windows identity. No fallback."
        LinkFailure.CLIENT_IDENTITY_UNAVAILABLE,LinkFailure.CLIENT_IDENTITY_NOT_SELECTED->"Inspect signing diagnostics. If the old key lacks DIGEST_NONE, explicitly replace the phone identity, revoke it on Windows and enroll anew."
        LinkFailure.HELLO_REJECTED->"TLS completed locally but Windows hello was rejected/closed; inspect Windows client-certificate diagnostics."
        LinkFailure.ENROLLMENT_REJECTED_OR_EXPIRED,LinkFailure.APPROVAL_TIMEOUT->"Check invitation expiry and Windows approval diagnostics."
        else->"Inspect both safe logs. If approval already completed, check Windows trusted devices before re-enrolling."
    }
    private suspend fun session(keys:AndroidIdentity,host:TrustedHost,enroll:Bootstrap?,attempt:LinkAttempt)=coroutineScope {
        if(enroll!=null && System.currentTimeMillis()/1000>=enroll.expires)throw ConnectionProblem(LinkFailure.ENROLLMENT_REJECTED_OR_EXPIRED)
        attempt.target=LanEndpoint(host.address,host.port)
        progress(attempt,LinkStage.TCP_CONNECTING,"Connecting TCP to ${host.address}:${host.port} with the saved/presented identity")
        val tcp=Socket();installSocket(tcp);tcp.tcpNoDelay=true
        try {tcp.connect(InetSocketAddress(host.address,host.port),5000)}finally{attempt.port=tcp.localPort.coerceAtLeast(0)}
        attempt.phase(LinkStage.TCP_CONNECTED)
        progress(attempt,LinkStage.TLS_STARTED,"Negotiating pinned mutual TLS")
        val tls=keys.context(host.pin,attempt::event).socketFactory.createSocket(tcp,host.address,host.port,true) as SSLSocket
        installSocket(tls);tls.useClientMode=true;tls.enabledProtocols=tls.supportedProtocols.filter{it=="TLSv1.2"||it=="TLSv1.3"}.toTypedArray();tls.soTimeout=5000
        // Socket.close from lifecycle/disconnect also interrupts a blocked handshake/read/write.
        val handshakeGuard=launch {delay(5000);attempt.handshakeTimedOut=true;tls.close()}
        try{tls.startHandshake()}finally{handshakeGuard.cancel()}
        if(tls.session.localCertificates.isNullOrEmpty())throw ConnectionProblem(LinkFailure.CLIENT_IDENTITY_NOT_SELECTED)
        attempt.phase(LinkStage.TLS_COMPLETED)
        progress(attempt,LinkStage.HELLO_READING,"TLS completed locally; reading authenticated Windows hello")
        val hello=SocketWire.hello(tls)
        if(Wire.text(hello.payload,"hostId")!=host.hostId || Wire.text(hello.payload,"pin")!=host.pin)throw ProtocolException("host identity")
        attempt.phase(LinkStage.HELLO_VALIDATED)
        val wire=SocketWire(tls,hello.session,hello)
        if(enroll!=null) {
            progress(attempt,LinkStage.ENROLL_SENDING,"Sending enrollment over pinned mutual TLS")
            val request=wire.send("enroll",Wire.payload("deviceId" to keys.deviceId,"name" to (Build.MODEL.take(64).ifEmpty{"Android"}),"secret" to enroll.secret))
            progress(attempt,LinkStage.AWAITING_WINDOWS_APPROVAL,"Server identity verified; awaiting Windows approval")
            val approve=wire.read(95000)
            if(approve.type!="approve"||Wire.text(approve.payload,"requestId")!=request||Wire.text(approve.payload,"clientPin")!=PeerIdentity.fingerprint(keys.certificate))throw ProtocolException("approval")
            attempt.phase(LinkStage.WINDOWS_APPROVAL_VALIDATED)
            attempt.phase(LinkStage.CONFIRM_SENDING)
            wire.send("confirm",Wire.payload("requestId" to request))
        } else wire.send("connect",Wire.payload("hostId" to host.hostId,"deviceId" to keys.deviceId))
        progress(attempt,LinkStage.READY_READING,"Waiting for confirmed session")
        val ready=wire.read()
        if(ready.type!="ready"||Wire.text(ready.payload,"hostId")!=host.hostId||Wire.text(ready.payload,"deviceId")!=keys.deviceId)throw ProtocolException("ready")
        ensureActive();if(!foreground||!desired)throw CancellationException()
        attempt.phase(LinkStage.TRUST_PERSISTING)
        keys.save(host);currentHost=host;invitation=null;enrollmentAttempt=null;endpointHint=null
        attempt.phase(LinkStage.SESSION_CONFIRMED)
        mutable.update{snapshot->snapshot.copy(connected=true,trusted=host,status="${attempt.id}: Session confirmed — authenticated Windows face connected")}
        val presentations=Channel<PresentationEvent>(32);presentationSink=presentations
        val sender=launch(Dispatchers.IO) {
            try {for(event in presentations)wire.send("presentation",PresentationCodec.payload(event))}
            finally {tls.close()} // A failed/full bounded queue closes this session; never replay it.
        }
        // Timeout closes the socket even if the platform's blocking write is backpressured.
        val heartbeat=launch(Dispatchers.IO) {
            while(isActive){delay(1000);val guard=launch{delay(2000);tls.close()};try{wire.send("ping",Wire.payload())}finally{guard.cancel()}}
        }
        try {
            while(isActive) {
                val message=wire.read()
                when(message.type) {
                    "face"->{val f=Wire.face(message);val now=SystemClock.elapsedRealtime();wire.incoming.fresh(f,now);latest.value=RemoteFace(f,now,message.id)}
                    "pong"->Unit
                    else->throw ProtocolException("session command")
                }
            }
        } finally {presentationSink=null;presentations.cancel();sender.cancel();heartbeat.cancel();tls.close()}
    }
}
