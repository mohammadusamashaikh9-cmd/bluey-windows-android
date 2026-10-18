package dev.bluey.port.link

import android.Manifest
import android.os.Build
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable
fun ConnectionPanel(link:DeviceLink,incoming:String?,close:()->Unit) {
    val state by link.ui.collectAsState()
    var invitation by remember(incoming){mutableStateOf(incoming?:"")}
    val displayedInvitation=invitation
    val parsed=remember(displayedInvitation){runCatching{Bootstrap.parse(displayedInvitation,System.currentTimeMillis()/1000)}.getOrNull()}
    val nearby=rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()){granted->if(granted){link.pause();link.resume()}}
    AlertDialog(onDismissRequest=close,title={Text("Secure Windows connection")},
        text={Column(Modifier.verticalScroll(rememberScrollState())) {
            Text(state.status)
            state.clientPin?.let{Text("This phone certificate SHA-256 (compare on Windows): $it")}
            state.trusted?.let{Text("Saved host: ${it.hostId}\nSHA-256: ${it.pin}")}
            Text("Secure hosts discovered (names are untrusted hints):")
            state.hosts.forEach{host->TextButton(onClick={link.connect(host)}){Text("${host.name} · ${host.address}:${host.port}\nConnect using saved identity")}}
            if(state.hosts.isEmpty())Text("No host discovered. Start Windows secure host; QR/URI works without discovery.")
            OutlinedTextField(value=invitation,onValueChange={if(it.length<=2048)invitation=it},label={Text("Complete Windows pairing URI")},modifier=Modifier.fillMaxWidth(),maxLines=4)
            parsed?.let{Text("Invitation host: ${it.host.hostId}\n${it.host.address}:${it.host.port}\nServer SHA-256: ${it.host.pin}\nCompare this fingerprint with the Windows invitation. Pair explicitly approves this identity; Windows will also ask for approval.")}
            Button(onClick={link.pair(displayedInvitation);invitation=""},enabled=parsed!=null){Text("Pair with this Windows identity")}
            TextButton(onClick={link.connect()}){Text("Reconnect saved host")}
            TextButton(onClick={link.disconnect()}){Text("Disconnect / use offline demo")}
            TextButton(onClick={link.forget()}){Text("Forget saved Windows trust")}
            if(Build.VERSION.SDK_INT>=33)TextButton(onClick={nearby.launch(Manifest.permission.NEARBY_WIFI_DEVICES)}){Text("Allow Nearby devices if Android 16 local-network opt-in requires it")}
            Text("No camera permission: use your system QR scanner or paste privately. Backgrounding closes the link. No plaintext fallback. Forgetting here does not revoke this phone on Windows; revoke it there too.")
        }},confirmButton={TextButton(onClick=close){Text("Done")}})
}
