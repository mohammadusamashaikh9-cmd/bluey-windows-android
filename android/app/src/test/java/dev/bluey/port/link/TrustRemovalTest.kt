package dev.bluey.port.link

import java.io.File
import kotlinx.coroutines.delay
import kotlinx.coroutines.runBlocking
import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35])
class TrustRemovalTest {
    @Test fun explicitForgetRemovesUnreadableTrustWithoutInitializingKeystoreOrConnecting()=runBlocking {
        val context=RuntimeEnvironment.getApplication()
        val stored=File(context.noBackupFilesDir,"bluey-peer.gcm")
        stored.parentFile!!.mkdirs();stored.writeBytes(byteArrayOf(1,2,3))
        val link=DeviceLink(context)
        try {
            link.forget()
            repeat(100){if(stored.exists())delay(10)}
            assertFalse("Forget must remove corrupt trust even before identity initialization",stored.exists())
            assertNull(link.ui.value.trusted);assertFalse(link.ui.value.connected)
        } finally {link.close();stored.delete()}
    }
}
