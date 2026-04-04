plugins {
    id("com.android.library")
}

android {
    namespace = "com.example.newbinding"
    compileSdk = 36

    defaultConfig {
        minSdk = 24
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
}

dependencies {

    // Add package dependency for binding library
    // Uncomment line below and replace {dependency.name.goes.here} with your dependency
    // implementation("{dependency.name.goes.here}")

    implementation("com.revenuecat.purchases:purchases:9.19.1")
    implementation("com.revenuecat.purchases:purchases-store-amazon:9.19.1")
    implementation("org.jetbrains.kotlinx:kotlinx-serialization-json-jvm:1.7.3")
//    implementation("com.revenuecat.purchases:purchases-ui:8.16.0")
    implementation("com.google.code.gson:gson:2.12.0")

}

