package com.revenuecat.revenuecatbinding;

import androidx.activity.result.ActivityResultCaller;
import androidx.annotation.OptIn;

import com.revenuecat.purchases.ui.revenuecatui.ExperimentalPreviewRevenueCatUIPurchasesAPI;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResultHandler;

@OptIn(markerClass = ExperimentalPreviewRevenueCatUIPurchasesAPI.class)
public class PaywallManager implements PaywallResultHandler {
    private PaywallActivityLauncher launcher;
    private String requiredEntitlementIdentifier;

    public void onCreate(ActivityResultCaller activity, String entitlementIdentifier) {
        launcher = new PaywallActivityLauncher(activity, this);
        requiredEntitlementIdentifier = entitlementIdentifier;
    }

    public void launchPaywallActivity() {
        // This will launch the paywall only if the user doesn't have the given entitlement id active.
        launcher.launchIfNeeded(requiredEntitlementIdentifier);
        // or if you want to launch it without any conditions
      //  launcher.launch();
    }

    @Override
    public void onActivityResult(PaywallResult result) {
        // Handle result
    }
}
