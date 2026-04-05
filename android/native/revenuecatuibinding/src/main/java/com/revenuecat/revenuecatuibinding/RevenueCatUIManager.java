package com.revenuecat.revenuecatuibinding;

import android.app.Activity;

import androidx.annotation.Nullable;

import java.util.concurrent.CompletableFuture;

public final class RevenueCatUIManager {
    private RevenueCatUIManager() {
    }

    public static CompletableFuture<String> presentPaywall(
            @Nullable Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String requiredEntitlementIdentifier,
            boolean displayCloseButton)
    {
        return RevenueCatUIPresenter.presentPaywall(
                activity,
                offeringIdentifier,
                requiredEntitlementIdentifier,
                displayCloseButton);
    }

    public static CompletableFuture<String> presentCustomerCenter(@Nullable Activity activity)
    {
        return RevenueCatUIPresenter.presentCustomerCenter(activity);
    }
}
