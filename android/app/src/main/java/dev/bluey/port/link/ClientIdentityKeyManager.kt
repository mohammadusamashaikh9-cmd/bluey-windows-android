package dev.bluey.port.link

import java.net.Socket
import java.security.Principal
import java.security.PrivateKey
import java.security.cert.X509Certificate
import javax.net.ssl.SSLEngine
import javax.net.ssl.X509ExtendedKeyManager

// Same EC selection policy as F3, now observable and independently exercisable by real JSSE TLS.
// Issuer hints are not identity authorities in D009: the host verifies possession/pin/enrollment.
class ClientIdentityKeyManager(
    private val alias:String,
    private val certificate:X509Certificate,
    private val key:()->PrivateKey?,
    private val event:(LinkStage)->Unit
):X509ExtendedKeyManager() {
    private fun ec(type:String?)=type?.startsWith("EC")==true
    override fun getClientAliases(keyType:String?,issuers:Array<out Principal>?):Array<String>? {
        event(LinkStage.CLIENT_ALIAS_REQUESTED)
        event(if(ec(keyType))LinkStage.CLIENT_EC_OFFERED else LinkStage.CLIENT_EC_NOT_OFFERED)
        event(if(ec(keyType))LinkStage.CLIENT_ALIAS_AVAILABLE else LinkStage.CLIENT_ALIAS_UNAVAILABLE)
        return if(ec(keyType))arrayOf(alias)else null
    }
    override fun chooseClientAlias(keyType:Array<out String>?,issuers:Array<out Principal>?,socket:Socket?):String? {
        event(LinkStage.CLIENT_ALIAS_REQUESTED)
        val matches=keyType?.any{ec(it)}==true
        event(if(matches)LinkStage.CLIENT_EC_OFFERED else LinkStage.CLIENT_EC_NOT_OFFERED)
        event(if(matches)LinkStage.CLIENT_ALIAS_SELECTED else LinkStage.CLIENT_ALIAS_UNAVAILABLE)
        return if(matches)alias else null
    }
    override fun chooseEngineClientAlias(keyType:Array<out String>?,issuers:Array<out Principal>?,engine:SSLEngine?)=chooseClientAlias(keyType,issuers,null)
    override fun getCertificateChain(alias:String?):Array<X509Certificate>? {
        if(alias!=this.alias)return null
        event(LinkStage.CLIENT_CHAIN_PROVIDED);return arrayOf(certificate)
    }
    override fun getPrivateKey(alias:String?):PrivateKey? {
        if(alias!=this.alias)return null
        try {
            val value=key()?:throw ClientIdentityFailure()
            if(value.algorithm!="EC")throw ClientIdentityFailure()
            event(LinkStage.CLIENT_KEY_PROVIDED);return value
        } catch(e:Exception) {event(LinkStage.CLIENT_KEY_UNAVAILABLE);throw ClientIdentityFailure(e)}
    }
    override fun getServerAliases(keyType:String?,issuers:Array<out Principal>?)=null
    override fun chooseServerAlias(keyType:String?,issuers:Array<out Principal>?,socket:Socket?)=null
}
