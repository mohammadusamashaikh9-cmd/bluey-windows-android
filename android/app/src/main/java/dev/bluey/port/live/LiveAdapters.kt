package dev.bluey.port.live
import com.google.gson.JsonObject
import dev.bluey.port.link.*
import kotlinx.coroutines.channels.ReceiveChannel
interface LiveLan {val connected:Boolean;val messages:ReceiveChannel<Message?>;fun send(type:String,payload:JsonObject):Boolean}
class DeviceLiveLan(private val link:DeviceLink):LiveLan {
    override val connected get()=link.ui.value.connected
    override val messages get()=link.liveMessages
    override fun send(type:String,payload:JsonObject)=link.sendLive(type,payload)
}
interface CaptureDevice {fun start(frame:(ByteArray)->Unit,failed:()->Unit);fun cancel();suspend fun finish()}
