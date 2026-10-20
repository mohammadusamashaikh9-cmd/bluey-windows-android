package dev.bluey.port.live
import kotlin.math.*
// Streaming rational linear interpolation; input chunk boundaries do not change output.
// Android supplies mono PCM16; the wire is explicitly PCM16 little endian / 24kHz.
class Pcm24k(private val sourceRate:Int,private val frame:(ByteArray)->Unit) {
    init{require(sourceRate in setOf(16000,24000,44100,48000))}
    private var index=-1L;private var next=0L;private var previous=0.0
    private var buffer=ByteArray(960);private var used=0
    fun accept(samples:ShortArray,count:Int=samples.size) {
        require(count in 0..samples.size)
        for(i in 0 until count){index++;val current=samples[i].toDouble();if(index==0L)previous=current
            while(next<=index*24000) {
                val fraction=if(index==0L)1.0 else (next-(index-1)*24000)/24000.0
                val value=(previous+(current-previous)*fraction).roundToInt().coerceIn(-32768,32767)
                buffer[used++]=value.toByte();buffer[used++]=(value shr 8).toByte();next+=sourceRate
                if(used==960){frame(buffer);buffer=ByteArray(960);used=0}
            };previous=current
        }
    }
}
object Chirp {
    fun samples():ShortArray {
        val rate=24000;val result=ArrayList<Short>();val scale=doubleArrayOf(1174.7,1318.5,1568.0)
        for(i in scale.indices){val duration=if(i==2).13 else .08;val count=(duration*rate).toInt();var phase=0.0
            repeat(count){n->val time=n.toDouble()/rate;val t=n.toDouble()/count
                val scoop=1-.18*exp(-time/.012);val wobble=if(i==2)1+.012*sin(time*2*PI*18)else 1.0
                phase+=2*PI*(scale[i]*.5)*scoop*wobble/rate
                val envelope=min(1.0,time/.006)*exp(-t*3.2)*(1-t.pow(6));result.add(((sin(phase)+.12*sin(2*phase))*envelope*.26*32767).roundToInt().toShort())}
            repeat((rate*.028).toInt()){result.add(0)}
        };return result.toShortArray()
    }
}
