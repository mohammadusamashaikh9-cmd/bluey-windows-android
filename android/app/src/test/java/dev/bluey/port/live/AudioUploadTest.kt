package dev.bluey.port.live
import dev.bluey.port.link.*
import com.google.gson.JsonObject
import org.junit.Test
import org.junit.Assert.*
import java.util.UUID
class AudioUploadTest {
    private val auth=UUID.randomUUID().toString();private val turn=UUID.randomUUID().toString()
    @Test fun fiveSecondUploadIsBoundedFramedAndClearsOwnedBuffers() {
        val messages=mutableListOf<Pair<String,JsonObject>>()
        val upload=AudioUpload(auth,turn){type,p->messages.add(type to p);true};assertTrue(upload.begin())
        repeat(250){i->val bytes=ByteArray(960){31};assertTrue(upload.append(bytes));assertTrue(bytes.all{it==0.toByte()});assertEquals(i,messages.last().second["index"].asInt)}
        assertTrue(upload.full());assertFalse(upload.append(ByteArray(960)));assertTrue(upload.complete());assertFalse(upload.complete())
        assertEquals(252,messages.size);assertEquals(240000,messages.last().second["bytes"].asInt)
        for((type,p)in messages){val bytes=Wire.encode(type,UUID.randomUUID().toString(),0,p);assertTrue(bytes.size<2048);assertEquals(type,Wire.decode(bytes.dropLast(1).toByteArray()).type)}
        assertEquals(4096,Wire.MAX_FRAME);assertFalse(upload.toString().contains("data"))
    }
    @Test fun queueFailureIsTerminalAndNeverRetries() {
        var calls=0;val upload=AudioUpload(auth,turn){_,_->++calls<3};assertTrue(upload.begin());assertTrue(upload.append(ByteArray(960)));assertFalse(upload.append(ByteArray(960)));assertFalse(upload.append(ByteArray(960)));assertFalse(upload.complete());assertEquals(3,calls)
    }
    @Test fun shortAudioAndCancellationDoNotComplete() {
        val types=mutableListOf<String>();val upload=AudioUpload(auth,turn){type,_->types.add(type);true};upload.begin();repeat(4){upload.append(ByteArray(960))};assertFalse(upload.complete());assertEquals("audioCancel",types.last());assertFalse(types.contains("audioEnd"));val count=types.size;upload.cancel();assertEquals(count,types.size)
    }
    @Test fun invalidFrameNeverSendsAudio() {
        val types=mutableListOf<String>();val upload=AudioUpload(auth,turn){type,_->types.add(type);true};assertFalse(upload.append(ByteArray(960)));assertTrue(upload.begin());val bytes=ByteArray(961){42};assertFalse(upload.append(bytes));assertTrue(bytes.all{it==0.toByte()});assertEquals(listOf("audioBegin"),types)
    }
}
