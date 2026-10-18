package dev.bluey.port.link

import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.AtomicFile
import com.google.gson.JsonParser
import java.io.File
import java.net.Socket
import java.security.*
import java.security.cert.X509Certificate
import java.security.spec.ECGenParameterSpec
import java.util.Date
import java.util.UUID
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec
import javax.net.ssl.*
import javax.security.auth.x500.X500Principal

// Never exports a private key. Android's TLS provider signs through AndroidKeyStore.
class AndroidIdentity(context:Context) {
    private val keys=KeyStore.getInstance("AndroidKeyStore").apply{load(null)}
    private val identity="bluey.f3.client";private val protection="bluey.f3.trust"
    private val file=AtomicFile(File(context.noBackupFilesDir,"bluey-peer.gcm"))
    val deviceId:String
    val certificate:X509Certificate
    init {
        val prefs=context.getSharedPreferences("bluey-device",Context.MODE_PRIVATE)
        deviceId=prefs.getString("id",null)?:UUID.randomUUID().toString().also{check(prefs.edit().putString("id",it).commit())}
        if(!keys.containsAlias(identity)) {
            KeyPairGenerator.getInstance(KeyProperties.KEY_ALGORITHM_EC,"AndroidKeyStore").apply {
                initialize(KeyGenParameterSpec.Builder(identity,KeyProperties.PURPOSE_SIGN or KeyProperties.PURPOSE_VERIFY)
                    .setAlgorithmParameterSpec(ECGenParameterSpec("secp256r1"))
                    .setDigests(KeyProperties.DIGEST_SHA256,KeyProperties.DIGEST_SHA384,KeyProperties.DIGEST_SHA512)
                    .setCertificateSubject(X500Principal("CN=Bluey Android $deviceId"))
                    .setCertificateSerialNumber(java.math.BigInteger.ONE)
                    .setCertificateNotBefore(Date(System.currentTimeMillis()-86400000))
                    .setCertificateNotAfter(Date(System.currentTimeMillis()+10L*365*86400000)).build())
                generateKeyPair()
            }
        }
        certificate=keys.getCertificate(identity) as X509Certificate
        certificate.checkValidity()
        if(!keys.containsAlias(protection))KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder(protection,KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build());generateKey()
        }
    }
    fun load():TrustedHost? {
        if(!file.baseFile.exists())return null
        val bytes=file.readFully();if(bytes.size !in 29..4096)throw ProtocolException("protected trust")
        val cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.DECRYPT_MODE,keys.getKey(protection,null) as SecretKey,GCMParameterSpec(128,bytes.copyOfRange(0,12)))
        val p=JsonParser.parseString(cipher.doFinal(bytes.copyOfRange(12,bytes.size)).toString(Charsets.UTF_8)).asJsonObject
        Wire.keys(p,"hostId","pin","address","port")
        return TrustedHost(Wire.text(p,"hostId"),Wire.text(p,"pin"),Wire.text(p,"address"),Wire.integer(p,"port").toInt()).also {
            if(!Wire.uuid(it.hostId)||!Wire.pin(it.pin)||!Bootstrap.privateAddress(it.address)||it.port !in 1..65535)throw ProtocolException("protected trust schema")
        }
    }
    fun save(host:TrustedHost) {
        val p=Wire.payload("hostId" to host.hostId,"pin" to host.pin,"address" to host.address).apply{addProperty("port",host.port)}
        val cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.ENCRYPT_MODE,keys.getKey(protection,null) as SecretKey)
        val bytes=cipher.iv+cipher.doFinal(p.toString().toByteArray(Charsets.UTF_8));val stream=file.startWrite()
        try {stream.write(bytes);file.finishWrite(stream)}catch(e:Exception){file.failWrite(stream);throw e}
    }
    fun forget(){file.delete()}
    fun context(pin:String):SSLContext {
        val manager=object:X509ExtendedKeyManager() {
            override fun getClientAliases(keyType:String?,issuers:Array<out java.security.Principal>?)=if(keyType?.startsWith("EC")==true)arrayOf(identity)else null
            override fun chooseClientAlias(keyType:Array<out String>?,issuers:Array<out java.security.Principal>?,socket:Socket?)=if(keyType?.any{it.startsWith("EC")}==true)identity else null
            override fun chooseEngineClientAlias(keyType:Array<out String>?,issuers:Array<out java.security.Principal>?,engine:SSLEngine?)=chooseClientAlias(keyType,issuers,null)
            override fun getServerAliases(keyType:String?,issuers:Array<out java.security.Principal>?)=null
            override fun chooseServerAlias(keyType:String?,issuers:Array<out java.security.Principal>?,socket:Socket?)=null
            override fun getCertificateChain(alias:String?)=if(alias==identity)arrayOf(certificate)else null
            override fun getPrivateKey(alias:String?)=if(alias==identity)keys.getKey(identity,null) as PrivateKey else null
        }
        val trust=object:X509TrustManager {
            override fun getAcceptedIssuers()=emptyArray<X509Certificate>()
            override fun checkClientTrusted(chain:Array<out X509Certificate>?,authType:String?) {throw java.security.cert.CertificateException("client role")}
            override fun checkServerTrusted(chain:Array<out X509Certificate>?,authType:String?) { PeerIdentity.verify(chain,pin) }
        }
        return SSLContext.getInstance("TLS").apply{init(arrayOf(manager),arrayOf(trust),SecureRandom())}
    }
}
object PeerIdentity {
    fun fingerprint(c:X509Certificate)=MessageDigest.getInstance("SHA-256").digest(c.encoded).joinToString(""){"%02x".format(it)}
    fun verify(chain:Array<out X509Certificate>?,pin:String) {
        if(chain==null||chain.isEmpty()||chain.size>4||!Wire.pin(pin))throw java.security.cert.CertificateException("identity")
        val c=chain[0];c.checkValidity()
        if(c.publicKey.algorithm!="EC"||!MessageDigest.isEqual(fingerprint(c).toByteArray(Charsets.US_ASCII),pin.toByteArray(Charsets.US_ASCII)))throw java.security.cert.CertificateException("identity mismatch")
    }
}
