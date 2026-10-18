package dev.bluey.port.link

import com.google.gson.JsonParser
import org.junit.Assert.*
import org.junit.Test
import java.util.Base64
import java.util.UUID

class ProtocolTest {
    private val session="10000000-0000-0000-0000-000000000000"
    @Test fun sharedFixturesDecodeIdentically() {
        val lines=javaClass.getResourceAsStream("/v1-cases.jsonl")!!.bufferedReader().readLines()
        assertEquals(35,lines.size)
        for(line in lines) {
            val fixture=JsonParser.parseString(line).asJsonObject;val frames=Frames()
            val accepted=try {
                fixture["chunks"].asJsonArray.forEach{part->frames.feed(Base64.getDecoder().decode(part.asString)).forEach{Wire.decode(it)}}
                frames.end();true
            }catch(_:ProtocolException){false}
            assertEquals(fixture["name"].asString,fixture["valid"].asBoolean,accepted)
        }
    }
    private fun message(seq:Long,id:String=UUID.randomUUID().toString(),sid:String=session)=Wire.decode(Wire.encode("ping",sid,seq,Wire.payload(),id).dropLast(1).toByteArray())
    private fun fails(action:()->Unit){try{action();fail("Expected protocol rejection")}catch(_:ProtocolException){}}
    @Test fun exactSequencesDuplicateIdsSessionsAndFreshness() {
        val guard=SequenceGuard(session);val id=UUID.randomUUID().toString();guard.accept(message(0,id))
        fails{guard.accept(message(2))};fails{guard.accept(message(1,id))};fails{SequenceGuard(session).accept(message(0,sid=UUID.randomUUID().toString()))}
        val fresh=SequenceGuard(session);fresh.fresh(LinkFace(0.0,0.0,"happy",0.0,100),1000)
        fresh.fresh(LinkFace(0.0,0.0,"happy",0.0,1100),2000)
        fails{fresh.fresh(LinkFace(0.0,0.0,"happy",0.0,1100),5001)}
        fails{SequenceGuard(session).apply{fresh(LinkFace(0.0,0.0,"happy",0.0,100),1000);fresh(LinkFace(0.0,0.0,"happy",0.0,99),1001)}}
    }
    @Test fun invitationRequiresFullStrongIdentityAndFreshPrivateEndpoint() {
        val uri="bluey-secure://pair?host=192.168.1.20&port=12345&id=$session&pin=${"a".repeat(64)}&secret=${"b".repeat(43)}&exp=1200"
        assertEquals(12345,Bootstrap.parse(uri,1000).host.port)
        fails{Bootstrap.parse(uri,1200)};fails{Bootstrap.parse(uri,899)}
        fails{Bootstrap.parse(uri.replace("192.168.1.20","8.8.8.8"),1000)}
        fails{Bootstrap.parse(uri.replace("192.168.1.20","127.0.0.1"),1000)}
        fails{Bootstrap.parse(uri+"&pin=${"a".repeat(64)}",1000)}
        fails{Bootstrap.parse(uri.replace("bluey-secure","http"),1000)}
        fails{Bootstrap.parse(uri.replace("b".repeat(43),"123456"),1000)}
        fails{Bootstrap.parse(uri.replace("a".repeat(64),"00"),1000)}
        assertFalse(Bootstrap.privateAddress("192.168.001.1"))
    }
    @Test fun reconnectBackoffIsBoundedAndResettable() {
        val retry=RetrySchedule();assertEquals(listOf(1000L,2000L,4000L,8000L,15000L,15000L),List(6){retry.nextMs()})
        retry.reset();assertEquals(1000L,retry.nextMs())
    }
    @Test fun incompleteOversizeAndDeepPacketsFailClosed() {
        fails{Frames().feed(ByteArray(4097){32})}
        fails{Frames().apply{feed("{".toByteArray());end()}}
        fails{Wire.decode("{\"x\":{\"y\":{\"z\":{\"w\":{}}}}}".toByteArray())}
    }
}
