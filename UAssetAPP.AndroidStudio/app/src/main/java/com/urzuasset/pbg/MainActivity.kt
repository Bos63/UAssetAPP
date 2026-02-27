package com.urzuasset.pbg

import android.net.Uri
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts.OpenDocument
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import java.io.InputStream
import java.security.MessageDigest

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                SurfaceScreen()
            }
        }
    }
}

@Composable
private fun SurfaceScreen() {
    val context = LocalContext.current

    var username by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var panelKey by remember { mutableStateOf("") }
    var status by remember { mutableStateOf("Önce giriş yapın.") }
    var loggedIn by remember { mutableStateOf(false) }

    var uassetUri by remember { mutableStateOf<Uri?>(null) }
    var uexpUri by remember { mutableStateOf<Uri?>(null) }
    var output by remember { mutableStateOf("Analiz çıktısı burada görünecek.") }

    val pickUasset = rememberLauncherForActivityResult(OpenDocument()) { uri ->
        if (uri != null && uri.toString().lowercase().contains(".uasset")) {
            uassetUri = uri
        } else if (uri != null) {
            status = "Hatalı dosya: .uasset seçmelisiniz."
        }
    }

    val pickUexp = rememberLauncherForActivityResult(OpenDocument()) { uri ->
        if (uri != null && uri.toString().lowercase().contains(".uexp")) {
            uexpUri = uri
        } else if (uri != null) {
            status = "Hatalı dosya: .uexp seçmelisiniz."
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFFF8F6FF))
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        verticalArrangement = Arrangement.spacedBy(10.dp)
    ) {
        Text("UAssetGUİ", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Bold)
        Text(status)

        OutlinedTextField(username, { username = it }, label = { Text("Kullanıcı adı") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(password, { password = it }, label = { Text("Şifre") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(panelKey, { panelKey = it }, label = { Text("Panel Key") }, modifier = Modifier.fillMaxWidth())

        Button(onClick = {
            loggedIn = username.isNotBlank() && password.isNotBlank() && panelKey.isNotBlank()
            status = if (loggedIn) "Giriş başarılı." else "Giriş başarısız: alanları doldurun."
        }, modifier = Modifier.fillMaxWidth()) { Text("Giriş Yap") }

        Spacer(Modifier.height(8.dp))
        Text("Dosya Seçimi", fontWeight = FontWeight.Bold)

        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            Button(onClick = { if (loggedIn) pickUasset.launch(arrayOf("*/*")) }, enabled = loggedIn) { Text("UASSET") }
            Button(onClick = { if (loggedIn) pickUexp.launch(arrayOf("*/*")) }, enabled = loggedIn) { Text("UEXP") }
        }

        Text("UAsset: ${uassetUri?.lastPathSegment ?: "seçilmedi"}")
        Text("UExp: ${uexpUri?.lastPathSegment ?: "seçilmedi"}")

        val pairReady = loggedIn && uassetUri != null && uexpUri != null &&
            baseName(uassetUri).equals(baseName(uexpUri), ignoreCase = true)

        Button(onClick = {
            val uasset = analyzeUri(context.contentResolver.openInputStream(uassetUri!!), "uasset")
            val uexp = analyzeUri(context.contentResolver.openInputStream(uexpUri!!), "uexp")
            output = "Pair: ${baseName(uassetUri)}\n\n$uasset\n\n$uexp"
            status = "Analiz tamamlandı."
        }, enabled = pairReady, modifier = Modifier.fillMaxWidth()) { Text("Dosyayı Aç ve Oku") }

        if (!pairReady) {
            Text("Not: Aynı ada sahip .uasset + .uexp çifti zorunlu.")
        }

        Text(output)
    }
}

private fun baseName(uri: Uri?): String {
    val path = uri?.lastPathSegment ?: return ""
    val file = path.substringAfterLast('/').substringAfterLast(':')
    return file.substringBeforeLast('.', file)
}

private fun analyzeUri(input: InputStream?, tag: String): String {
    if (input == null) return "[$tag] Dosya açılamadı."
    input.use {
        val bytes = it.readBytes()
        val preview = bytes.take(32).joinToString(" ") { b -> "%02X".format(b) }
        val hash = MessageDigest.getInstance("SHA-256").digest(bytes)
            .joinToString("") { "%02x".format(it) }
        return "[$tag]\nBoyut: ${bytes.size} byte\nSHA256: $hash\nHEX(32): $preview"
    }
}
