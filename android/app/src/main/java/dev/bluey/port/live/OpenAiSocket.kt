package dev.bluey.port.live
import okhttp3.*
import okio.ByteString
import java.util.concurrent.TimeUnit
// Platform TLS verification is retained. No logging interceptor, redirects, beta headers or stored token.
class OpenAiSocket(token:LiveToken,private val event:(String)->Unit,private val failed:()->Unit):RealtimeTransport {
    private val client=OkHttpClient.Builder().connectTimeout(10,TimeUnit.SECONDS).writeTimeout(5,TimeUnit.SECONDS).readTimeout(0,TimeUnit.SECONDS).pingInterval(15,TimeUnit.SECONDS).followRedirects(false).followSslRedirects(false).build()
    @Volatile private var closed=false
    @Volatile private var socket:WebSocket?=null
    init {val created=client.newWebSocket(Request.Builder().url("wss://api.openai.com/v1/realtime?model="+token.model).header("Authorization","Bearer "+token.secret).build(),object:WebSocketListener(){
        override fun onMessage(webSocket:WebSocket,text:String){if(!closed){if(text.length>65536){close();failed()}else event(text)}}
        override fun onMessage(webSocket:WebSocket,bytes:ByteString){close();failed()}
        override fun onFailure(webSocket:WebSocket,t:Throwable,response:Response?){if(!closed){close();failed()}}
        override fun onClosing(webSocket:WebSocket,code:Int,reason:String){webSocket.close(code,null);if(!closed){close();failed()}}
        override fun onClosed(webSocket:WebSocket,code:Int,reason:String){if(!closed){close();failed()}}
    });socket=created;if(closed)created.cancel()}
    override fun send(json:String):Boolean {if(closed||json.length>65536||(socket?.queueSize()?:65536)+json.length>65536)return false;return socket?.send(json)?:false}
    override fun close(){if(closed)return;closed=true;socket?.cancel();client.dispatcher.executorService.shutdown();client.connectionPool.evictAll()}
}
