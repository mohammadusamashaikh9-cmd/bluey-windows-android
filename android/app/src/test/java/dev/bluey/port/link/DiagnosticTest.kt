package dev.bluey.port.link

import org.junit.Assert.*
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk=[35])
class DiagnosticTest {
    @Test fun invalidInvitationHasASafeStageWithoutEchoingInput() {
        val link=DeviceLink(RuntimeEnvironment.getApplication())
        try {
            val uri="bluey-secure://pair?secret=DO_NOT_LOG_THE_INVITATION"
            link.pair(uri)
            val method=DeviceLink::class.java.methods.firstOrNull{it.name=="diagnosticText"}
            assertNotNull("Pairing diagnostics must expose the failing stage",method)
            val text=method!!.invoke(link) as String
            assertTrue(text.contains("INVITATION_INVALID"))
            assertFalse(text.contains(uri));assertFalse(text.contains("DO_NOT_LOG_THE_INVITATION"))
        } finally {link.close()}
    }
}
