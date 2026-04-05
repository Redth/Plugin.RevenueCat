plugins {
    id("com.android.library")
}

android {
    namespace = "com.revenuecat.revenuecatuibinding"
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
    implementation("com.revenuecat.purchases:purchases:9.29.0")
    implementation("com.revenuecat.purchases:purchases-ui:9.29.0")
    implementation("com.google.code.gson:gson:2.12.0")
}
