package dev.bluey.port.link

import java.net.InetAddress
import java.net.ServerSocket
import java.net.Socket
import java.io.IOException
import java.util.concurrent.CountDownLatch
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicBoolean
import kotlinx.coroutines.*
import org.junit.Assert.*
import org.junit.Test
class CancellationBoundaryTest {
    @Test fun cancelledBlockingSocketReadMustNotBecomeAnActivePairingFailure()=runBlocking {
        val log=SafeDiagnostics();val attempt=log.begin();attempt.phase(LinkStage.HELLO_READING)
        ServerSocket(0,1,InetAddress.getLoopbackAddress()).use { listener->
            Socket(InetAddress.getLoopbackAddress(),listener.localPort).use { client->
                listener.accept().use {
                    val reading=CountDownLatch(1);val incorrectlyReported=AtomicBoolean()
                    val job=launch(Dispatchers.IO) {
                        try {reading.countDown();client.getInputStream().read()}
                        catch(error:IOException) {
                            attempt.failureIfActive(error)
                            // DeviceLink would next clear trust intent/desired state and publish a failure.
                            incorrectlyReported.set(true)
                        }
                    }
                    assertTrue(reading.await(3,TimeUnit.SECONDS));job.cancel();client.close();job.join()
                    assertFalse("Cancelled socket IO must not run the active-attempt failure handler",incorrectlyReported.get())
                    assertTrue(log.events.value.any{it.stage==LinkStage.LIFECYCLE_CANCELLED})
                }
            }
        }
    }
}
