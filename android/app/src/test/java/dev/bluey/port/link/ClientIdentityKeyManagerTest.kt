package dev.bluey.port.link

import java.net.InetAddress
import java.security.KeyStore
import java.security.PrivateKey
import java.security.cert.CertificateException
import java.security.cert.X509Certificate
import java.nio.file.Files
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit
import javax.net.ssl.*
import javax.security.auth.x500.X500Principal
import org.junit.Assert.*
import org.junit.Test

private data class TestIdentity(val key:PrivateKey,val cert:X509Certificate,val store:KeyStore)
private fun identity():TestIdentity {
    val folder=Files.createTempDirectory("bluey-r1-test-identity").toFile()
    try {
        val file=folder.resolve("identity.p12")
        // Ephemeral software test keys only; no Keystore/hardware interoperability claim.
        val command=ProcessBuilder(System.getProperty("java.home")+"/bin/keytool","-genkeypair","-alias","test",
            "-keyalg","EC","-groupname","secp256r1","-dname","CN=Bluey test identity","-validity","2",
            "-storetype","PKCS12","-keystore",file.path,"-storepass","test-only-password","-noprompt")
            .redirectErrorStream(true).redirectOutput(folder.resolve("keytool.log")).start()
        if(!command.waitFor(20,TimeUnit.SECONDS)){command.destroyForcibly();error("Test identity generation timeout")}
        check(command.exitValue()==0)
        val store=KeyStore.getInstance("PKCS12").apply{file.inputStream().use{load(it,"test-only-password".toCharArray())}}
        return TestIdentity(store.getKey("test","test-only-password".toCharArray()) as PrivateKey,store.getCertificate("test") as X509Certificate,store)
    } finally {folder.deleteRecursively()}
}
class ClientIdentityKeyManagerTest {
    @Test fun socketAndEngineSelectionReturnTheSameEcIdentityDespiteIssuerHints() {
        val i=identity();val events=mutableListOf<LinkStage>()
        val manager=ClientIdentityKeyManager("client",i.cert,{i.key},events::add)
        val issuers=arrayOf(X500Principal("CN=Unrelated discovery name"))
        assertEquals("client",manager.chooseClientAlias(arrayOf("RSA","EC"),issuers,null))
        assertEquals("client",manager.chooseEngineClientAlias(arrayOf("EC"),issuers,null))
        assertArrayEquals(arrayOf("client"),manager.getClientAliases("EC",issuers))
        assertNull(manager.chooseClientAlias(arrayOf("RSA"),null,null));assertNull(manager.chooseClientAlias(null,null,null))
        assertNull(manager.getPrivateKey("different"));assertNull(manager.getCertificateChain("different"))
        assertSame(i.key,manager.getPrivateKey("client"));assertArrayEquals(arrayOf(i.cert),manager.getCertificateChain("client"))
        assertTrue(events.containsAll(listOf(LinkStage.CLIENT_ALIAS_SELECTED,LinkStage.CLIENT_CHAIN_PROVIDED,LinkStage.CLIENT_KEY_PROVIDED,LinkStage.CLIENT_ALIAS_UNAVAILABLE)))
    }
    @Test fun opaquePrivateKeyIsNeverExportedAndMissingKeyFailsClosed() {
        val i=identity();val opaque=object:PrivateKey {
            override fun getAlgorithm()="EC"
            override fun getFormat():String?=null
            override fun getEncoded():ByteArray=throw AssertionError("Must not export a private key")
        }
        val manager=ClientIdentityKeyManager("client",i.cert,{opaque},{})
        assertSame(opaque,manager.getPrivateKey("client"))
        val missing=ClientIdentityKeyManager("client",i.cert,{null},{})
        try{missing.getPrivateKey("client");fail("Missing private identity accepted")}catch(_:ClientIdentityFailure){}
    }
    private fun handshake(protocol:String,provideIdentity:Boolean) {
        val server=identity();val client=identity();val stages=mutableListOf<LinkStage>()
        val keys=KeyManagerFactory.getInstance(KeyManagerFactory.getDefaultAlgorithm()).apply{init(server.store,"test-only-password".toCharArray())}
        fun trust(pin:String)=object:X509TrustManager {
            override fun getAcceptedIssuers()=emptyArray<X509Certificate>()
            override fun checkClientTrusted(chain:Array<out X509Certificate>?,authType:String?)=PeerIdentity.verify(chain,pin)
            override fun checkServerTrusted(chain:Array<out X509Certificate>?,authType:String?)=PeerIdentity.verify(chain,pin)
        }
        val serverContext=SSLContext.getInstance("TLS").apply{init(keys.keyManagers,arrayOf(trust(PeerIdentity.fingerprint(client.cert))),null)}
        val clientContext=SSLContext.getInstance("TLS").apply{init(arrayOf(ClientIdentityKeyManager("client",client.cert,{client.key},stages::add)).takeIf{provideIdentity}?:emptyArray(),arrayOf(trust(PeerIdentity.fingerprint(server.cert))),null)}
        val executor=Executors.newSingleThreadExecutor()
        (serverContext.serverSocketFactory.createServerSocket(0,1,InetAddress.getLoopbackAddress()) as SSLServerSocket).use{listener->
            listener.enabledProtocols=arrayOf(protocol);listener.needClientAuth=true
            val receiving=executor.submit<Boolean>{
                try{(listener.accept() as SSLSocket).use{socket->socket.soTimeout=5000;socket.startHandshake()
                    assertEquals(PeerIdentity.fingerprint(client.cert),PeerIdentity.fingerprint(socket.session.peerCertificates[0] as X509Certificate))
                    socket.outputStream.write(7);true}}
                catch(_:java.io.IOException){false}
            }
            try {
                var accepted=false
                try{(clientContext.socketFactory.createSocket(InetAddress.getLoopbackAddress(),listener.localPort) as SSLSocket).use{socket->
                    socket.enabledProtocols=arrayOf(protocol);socket.soTimeout=5000;socket.startHandshake()
                    accepted=socket.inputStream.read()==7
                }}catch(_:java.io.IOException){}
                assertEquals(provideIdentity,accepted);assertEquals(provideIdentity,receiving.get(8,TimeUnit.SECONDS))
                if(provideIdentity)assertTrue(stages.containsAll(listOf(LinkStage.CLIENT_ALIAS_SELECTED,LinkStage.CLIENT_KEY_PROVIDED)))
            } finally {executor.shutdownNow()}
        }
    }
    @Test fun actualJsseTls12UsesTheProductionClientKeyManager()=handshake("TLSv1.2",true)
    @Test fun actualJsseTls13UsesTheProductionClientKeyManager()=handshake("TLSv1.3",true)
    @Test fun actualJsseTlsRejectsAMissingClientIdentity()=handshake("TLSv1.3",false)
}
