package dev.bluey.port.link

import android.content.Context
import android.os.Build
import android.os.SystemClock
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.io.EOFException
import java.net.InetSocketAddress
import java.net.Socket
import java.util.UUID
import javax.net.ssl.SSLSocket

class SocketWire(private val socket:SSLSocket,val session:String,hello:Message) {
    private val frames=Frames();private val waiting=ArrayDeque<ByteArray>();val incoming=SequenceGuard(session)
    private var outgoing=0L;private val input=socket.inputStream;private val output=socket.outputStream
    init{incoming.accept(hello)}
    fun read(timeoutMs:Int=5000):Message {
        val deadline=SystemClock.elapsedRealtime()+timeoutMs
        while(waiting.isEmpty()) {
            val remaining=deadline-SystemClock.elapsedRealtime();if(remaining<=0)throw java.net.SocketTimeoutException()
            socket.soTimeout=remaining.toInt();val bytes=ByteArray(1024);val n=input.read(bytes)
            if(n<0){frames.end();throw EOFException()};waiting.addAll(frames.feed(bytes.copyOf(n)))
        }
        return Wire.decode(waiting.removeFirst()).also{incoming.accept(it)}
    }
    private val sendGate=Mutex()
    suspend fun send(type:String,payload:com.google.gson.JsonObject):String = sendGate.withLock { coroutineScope {
        val id=UUID.randomUUID().toString();val bytes=Wire.encode(type,session,outgoing++,payload,id)
        val guard=launch { delay(2000);socket.close() }
        try { output.write(bytes);output.flush();id } finally { guard.cancel() }
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
data class RemoteFace(val face:LinkFace,val receivedMs:Long,val id:String)
data class LinkUi(val status:String="Disconnected — offline demo",val hosts:List<DiscoveredHost> = emptyList(),val trusted:TrustedHost?=null,val connected:Boolean=false,val clientPin:String?=null)

// Foreground-only lifetime, one read task and one heartbeat task. Latest face is a conflated single slot.
class DeviceLink(context:Context) {
    private val app=context.applicationContext
    private val scope=CoroutineScope(SupervisorJob()+Dispatchers.IO)
    private val initGate=Mutex();private val sessionGate=Mutex();private var identity:AndroidIdentity?=null
    private val mutable=MutableStateFlow(LinkUi());val ui:StateFlow<LinkUi> = mutable
    private val latest=MutableStateFlow<RemoteFace?>(null);val face:StateFlow<RemoteFace?> = latest
    @Volatile private var socket:Socket?=null
    @Volatile private var foreground=false
    @Volatile private var desired=true
    @Volatile private var invitation:Bootstrap?=null
    @Volatile private var currentHost:TrustedHost?=null
    private var worker:Job?=null
    private val discovery=Discovery(app,{hosts->mutable.value=mutable.value.copy(hosts=hosts)},{message->if(!mutable.value.connected)status(message)})
    private fun status(message:String){mutable.value=mutable.value.copy(status=message,trusted=currentHost)}
    private suspend fun keys():AndroidIdentity=initGate.withLock {
        identity?:AndroidIdentity(app).also{identity=it;currentHost=it.load();mutable.value=mutable.value.copy(trusted=currentHost,clientPin=PeerIdentity.fingerprint(it.certificate))}
    }
    @Synchronized fun resume(){foreground=true;discovery.start();restart()}
    @Synchronized fun pause(){foreground=false;worker?.cancel();socket?.close();socket=null;latest.value=null;mutable.value=mutable.value.copy(connected=false,status="Paused — connection closed");discovery.stop()}
    @Synchronized private fun restart(){worker?.cancel();socket?.close();if(foreground && desired)worker=scope.launch{sessionGate.withLock{loop()}}}
    fun pair(text:String) {
        try {invitation=Bootstrap.parse(text,System.currentTimeMillis()/1000);desired=true;restart()}
        catch(_:Exception){status("Invalid or expired invitation. Copy the complete fresh Windows URI.")}
    }
    fun connect(selected:DiscoveredHost?=null) {
        if(selected!=null && currentHost!=null)currentHost=currentHost!!.copy(address=selected.address,port=selected.port)
        invitation=null;desired=true;restart()
    }
    @Synchronized fun disconnect(){desired=false;invitation=null;worker?.cancel();socket?.close();latest.value=null;mutable.value=mutable.value.copy(connected=false,status="Disconnected — offline demo")}
    fun forget(){disconnect();scope.launch{initGate.withLock{identity?.forget();currentHost=null;mutable.value=mutable.value.copy(trusted=null)}}}
    fun close(){pause();scope.cancel()}
    private suspend fun loop() {
        try {
            val keys=keys();val retry=RetrySchedule()
            while(currentCoroutineContext().isActive && foreground && desired) {
                val enroll=invitation
                var host=enroll?.host?:currentHost
                if(host==null){status("No trusted host. Scan/paste a Windows invitation, then Pair.");return}
                // DNS-SD is only an address hint; full stored pin + host UUID still authenticate the endpoint.
                if(enroll==null)mutable.value.hosts.firstOrNull{it.name.startsWith("Bluey-"+host!!.hostId.take(8))}?.let{host=host!!.copy(address=it.address,port=it.port)}
                try {
                    status(if(enroll!=null)"Authenticating invitation…" else "Reconnecting to pinned Windows identity…")
                    session(keys,host!!,enroll)
                    throw EOFException()
                } catch(e:CancellationException){throw e}
                catch(e:Exception) {
                    latest.value=null;mutable.value=mutable.value.copy(connected=false)
                    if(enroll!=null){invitation=null;desired=false;status("Pairing failed or rejected. Nothing newly trusted here. Start a fresh invitation; check Windows trusted devices if approval completed.");return}
                    if(e is javax.net.ssl.SSLException || e is ProtocolException || e is java.security.GeneralSecurityException) {
                        desired=false;status("Authentication/protocol failed. No insecure fallback. Check host identity/revocation and pair again explicitly.");return
                    }
                    status("Connection lost — offline demo; retrying securely")
                    delay(retry.nextMs())
                } finally {socket?.close();socket=null}
            }
        } catch(e:CancellationException){throw e}
        catch(_:Exception){status("Protected identity/storage unavailable. No connection opened.")}
        finally{latest.value=null;mutable.value=mutable.value.copy(connected=false)}
    }
    private suspend fun session(keys:AndroidIdentity,host:TrustedHost,enroll:Bootstrap?)=coroutineScope {
        if(enroll!=null && System.currentTimeMillis()/1000>=enroll.expires)throw ProtocolException("expired")
        val tcp=Socket();socket=tcp;tcp.tcpNoDelay=true;tcp.connect(InetSocketAddress(host.address,host.port),5000)
        val tls=keys.context(host.pin).socketFactory.createSocket(tcp,host.address,host.port,true) as SSLSocket
        socket=tls;tls.useClientMode=true;tls.enabledProtocols=tls.supportedProtocols.filter{it=="TLSv1.2"||it=="TLSv1.3"}.toTypedArray();tls.soTimeout=5000
        // Socket.close from lifecycle/disconnect also interrupts a blocked handshake/read/write.
        val handshakeGuard=launch {delay(5000);tls.close()}
        try{tls.startHandshake()}finally{handshakeGuard.cancel()}
        val hello=SocketWire.hello(tls)
        if(Wire.text(hello.payload,"hostId")!=host.hostId || Wire.text(hello.payload,"pin")!=host.pin)throw ProtocolException("host identity")
        val wire=SocketWire(tls,hello.session,hello)
        if(enroll!=null) {
            val request=wire.send("enroll",Wire.payload("deviceId" to keys.deviceId,"name" to (Build.MODEL.take(64).ifEmpty{"Android"}),"secret" to enroll.secret))
            status("Server identity verified. Approve this phone in the Windows connection window.")
            val approve=wire.read(95000)
            if(approve.type!="approve"||Wire.text(approve.payload,"requestId")!=request||Wire.text(approve.payload,"clientPin")!=PeerIdentity.fingerprint(keys.certificate))throw ProtocolException("approval")
            wire.send("confirm",Wire.payload("requestId" to request))
        } else wire.send("connect",Wire.payload("hostId" to host.hostId,"deviceId" to keys.deviceId))
        val ready=wire.read()
        if(ready.type!="ready"||Wire.text(ready.payload,"hostId")!=host.hostId||Wire.text(ready.payload,"deviceId")!=keys.deviceId)throw ProtocolException("ready")
        ensureActive();if(!foreground||!desired)throw CancellationException()
        keys.save(host);currentHost=host;invitation=null
        mutable.value=mutable.value.copy(connected=true,trusted=host,status="Authenticated TLS — Windows face connected")
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
        } finally {heartbeat.cancel();tls.close()}
    }
}
