package dev.bluey.port.link

import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.security.keystore.KeyInfo
import android.util.AtomicFile
import com.google.gson.JsonParser
import java.io.File
import java.net.Socket
import java.security.*
import java.security.cert.X509Certificate
import java.security.cert.CertificateException
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
    companion object {
        // Kept separately so the actual generated-key authorizations are regression tested.
        fun clientKeySpec(alias:String,deviceId:String,now:Long)=KeyGenParameterSpec.Builder(alias,KeyProperties.PURPOSE_SIGN or KeyProperties.PURPOSE_VERIFY)
            .setAlgorithmParameterSpec(ECGenParameterSpec("secp256r1"))
            .setDigests(KeyProperties.DIGEST_NONE,KeyProperties.DIGEST_SHA256,KeyProperties.DIGEST_SHA384,KeyProperties.DIGEST_SHA512)
            .setCertificateSubject(X500Principal("CN=Bluey Android $deviceId"))
            .setCertificateSerialNumber(java.math.BigInteger.ONE)
            .setCertificateNotBefore(Date(now-86400000))
            .setCertificateNotAfter(Date(now+10L*365*86400000)).build()
        fun replaceIdentity(context:Context) {
            // Called only after the user confirms, with the session stopped and initGate held.
            eraseTrust(context)
            KeyStore.getInstance("AndroidKeyStore").apply{load(null);deleteEntry("bluey.f3.client")}
        }
        fun eraseTrust(context:Context) { AtomicFile(File(context.noBackupFilesDir,"bluey-peer.gcm")).delete() }
    }
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
                initialize(clientKeySpec(identity,deviceId,System.currentTimeMillis()))
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
    fun signingSelfTest(event:(LinkStage)->Unit) {
        val privateKey=keys.getKey(identity,null) as? PrivateKey?:throw ClientIdentityFailure()
        try {
            val info=KeyFactory.getInstance(privateKey.algorithm,"AndroidKeyStore").getKeySpec(privateKey,KeyInfo::class.java)
            event(if(info.digests.contains(KeyProperties.DIGEST_NONE))LinkStage.DIGEST_NONE_AUTHORIZED else LinkStage.DIGEST_NONE_NOT_AUTHORIZED)
        } catch(_:GeneralSecurityException) {event(LinkStage.DIGEST_AUTHORIZATION_UNKNOWN)}
        try {certificate.verify(certificate.publicKey);event(LinkStage.CLIENT_CERT_SELF_SIGNATURE_VALID)}
        catch(_:GeneralSecurityException){event(LinkStage.CLIENT_CERT_SELF_SIGNATURE_INVALID)}
        val eku=certificate.extendedKeyUsage
        val ku=certificate.keyUsage
        event(if((eku==null||eku.contains("1.3.6.1.5.5.7.3.2")||eku.contains("2.5.29.37.0"))&&(ku==null||ku.firstOrNull()==true))
            LinkStage.CLIENT_CERT_CLIENT_AUTH_ALLOWED else LinkStage.CLIENT_CERT_CLIENT_AUTH_EXCLUDED)
        IdentitySigning.check(privateKey,certificate.publicKey,event)
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
    fun context(pin:String,event:(LinkStage)->Unit):SSLContext {
        val manager=ClientIdentityKeyManager(identity,certificate,{keys.getKey(identity,null) as? PrivateKey},event)
        val trust=object:X509TrustManager {
            override fun getAcceptedIssuers()=emptyArray<X509Certificate>()
            override fun checkClientTrusted(chain:Array<out X509Certificate>?,authType:String?) {throw java.security.cert.CertificateException("client role")}
            override fun checkServerTrusted(chain:Array<out X509Certificate>?,authType:String?) {
                try {PeerIdentity.verify(chain,pin);event(LinkStage.SERVER_PIN_VERIFIED)}
                catch(e:CertificateException){event(LinkStage.SERVER_CERTIFICATE_REJECTED);throw e}
            }
        }
        return SSLContext.getInstance("TLS").apply{init(arrayOf(manager),arrayOf(trust),SecureRandom())}
    }
}
object PeerIdentity {
    fun fingerprint(c:X509Certificate)=MessageDigest.getInstance("SHA-256").digest(c.encoded).joinToString(""){"%02x".format(it)}
    fun verify(chain:Array<out X509Certificate>?,pin:String) {
        if(chain==null||chain.isEmpty()||chain.size>4||!Wire.pin(pin))throw PeerCertificateFailure(LinkFailure.SERVER_CERTIFICATE_INVALID)
        val c=chain[0]
        try {c.checkValidity()}catch(_:CertificateException){throw PeerCertificateFailure(LinkFailure.SERVER_CERTIFICATE_INVALID)}
        if((c.publicKey as? java.security.interfaces.ECPublicKey)?.params?.curve?.field?.fieldSize!=256)throw PeerCertificateFailure(LinkFailure.SERVER_CERTIFICATE_INVALID)
        if(!MessageDigest.isEqual(fingerprint(c).toByteArray(Charsets.US_ASCII),pin.toByteArray(Charsets.US_ASCII)))throw PeerCertificateFailure(LinkFailure.SERVER_PIN_MISMATCH)
    }
}
