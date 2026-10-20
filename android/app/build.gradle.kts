plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("org.jetbrains.kotlin.plugin.compose")
}

android {
    namespace = "dev.bluey.port"
    compileSdk = 36
    defaultConfig {
        applicationId = "dev.bluey.port"
        minSdk = 26
        targetSdk = 36
        versionCode = 9
        versionName = "0.4.2-f4b-gemini-foundation"
    }
    buildFeatures { compose = true }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    // Reuse the original fonts AND their OFL notices; no duplicate binary assets.
    sourceSets.getByName("test").resources.srcDir(rootProject.file("../protocol"))
    sourceSets.getByName("main").assets.srcDir(rootProject.file("../Shared/Fonts"))
    testOptions {
        unitTests.isIncludeAndroidResources = true
        unitTests.all {
            it.maxHeapSize = "1g"
            it.maxParallelForks = 1
            it.systemProperty("bluey.renderDir", layout.buildDirectory.dir("renders").get().asFile.absolutePath)
        }
    }
}

kotlin {
    compilerOptions { jvmTarget.set(org.jetbrains.kotlin.gradle.dsl.JvmTarget.JVM_17) }
}

dependencies {
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.google.code.gson:gson:2.13.2")
    implementation(platform("androidx.compose:compose-bom:2025.12.01"))
    implementation("androidx.activity:activity-compose:1.12.2")
    implementation("androidx.compose.foundation:foundation")
    implementation("androidx.compose.material3:material3")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.9.4")
    implementation("androidx.lifecycle:lifecycle-runtime-compose:2.9.4")
    testImplementation("junit:junit:4.13.2")
    testImplementation("org.robolectric:robolectric:4.16.1")
}
