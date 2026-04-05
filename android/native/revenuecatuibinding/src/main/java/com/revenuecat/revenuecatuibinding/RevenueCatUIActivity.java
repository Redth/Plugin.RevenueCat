package com.revenuecat.revenuecatuibinding;

import android.os.Bundle;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.annotation.Nullable;

import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.Offering;
import com.revenuecat.purchases.Offerings;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.interfaces.ReceiveOfferingsCallback;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallDisplayCallback;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.ui.revenuecatui.customercenter.ShowCustomerCenter;

import java.util.HashMap;

import kotlin.Unit;

public class RevenueCatUIActivity extends ComponentActivity {
    private String requestId;
    private String mode;
    private boolean resultSent;

    private PaywallActivityLauncher paywallLauncher;
    private ActivityResultLauncher<Unit> customerCenterLauncher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        requestId = getIntent().getStringExtra(RevenueCatUIPresenter.EXTRA_REQUEST_ID);
        mode = getIntent().getStringExtra(RevenueCatUIPresenter.EXTRA_MODE);

        if (requestId == null || mode == null) {
            completeAndFinish("error", null, "RevenueCat UI request details were missing.");
            return;
        }

        paywallLauncher = new PaywallActivityLauncher(this, this::handlePaywallResult);
        customerCenterLauncher = registerForActivityResult(new ShowCustomerCenter(), result -> finishCustomerCenter());

        if (RevenueCatUIPresenter.MODE_PAYWALL.equals(mode)) {
            showPaywall();
            return;
        }

        if (RevenueCatUIPresenter.MODE_CUSTOMER_CENTER.equals(mode)) {
            customerCenterLauncher.launch(Unit.INSTANCE);
            return;
        }

        completeAndFinish("error", null, "Unknown RevenueCat UI presentation mode.");
    }

    @Override
    protected void onDestroy() {
        if (!resultSent && requestId != null && !isChangingConfigurations()) {
            RevenueCatUIPresenter.cancel(
                    requestId,
                    "RevenueCat UI was closed before it returned a result.");
        }

        super.onDestroy();
    }

    private void showPaywall() {
        final boolean displayCloseButton = getIntent().getBooleanExtra(RevenueCatUIPresenter.EXTRA_DISPLAY_CLOSE_BUTTON, true);
        final String offeringIdentifier = getIntent().getStringExtra(RevenueCatUIPresenter.EXTRA_OFFERING_IDENTIFIER);
        final String requiredEntitlementIdentifier = getIntent().getStringExtra(RevenueCatUIPresenter.EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER);

        if (offeringIdentifier == null || offeringIdentifier.trim().isEmpty()) {
            launchPaywall(null, requiredEntitlementIdentifier, displayCloseButton);
            return;
        }

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(Offerings offerings) {
                Offering offering = offerings.get(offeringIdentifier);

                if (offering == null) {
                    completeAndFinish("error", null, "Offering '" + offeringIdentifier + "' was not found.");
                    return;
                }

                launchPaywall(offering, requiredEntitlementIdentifier, displayCloseButton);
            }

            @Override
            public void onError(PurchasesError error) {
                completeAndFinish("error", null, error.getMessage());
            }
        });
    }

    private void launchPaywall(@Nullable Offering offering, @Nullable String requiredEntitlementIdentifier, boolean displayCloseButton) {
        if (requiredEntitlementIdentifier != null && !requiredEntitlementIdentifier.trim().isEmpty()) {
            paywallLauncher.launchIfNeeded(
                    requiredEntitlementIdentifier,
                    offering,
                    null,
                    displayCloseButton,
                    true,
                    new PaywallDisplayCallback() {
                        @Override
                        public void onPaywallDisplayResult(boolean wasDisplayed) {
                            if (!wasDisplayed) {
                                completeAndFinish("notPresented", null, null);
                            }
                        }
                    });
            return;
        }

        paywallLauncher.launch(offering, null, displayCloseButton, true, new HashMap<>());
    }

    private void handlePaywallResult(PaywallResult result) {
        if (result instanceof PaywallResult.Cancelled) {
            completeAndFinish("cancelled", null, null);
            return;
        }

        if (result instanceof PaywallResult.Purchased) {
            completeAndFinish("purchased", ((PaywallResult.Purchased) result).getCustomerInfo(), null);
            return;
        }

        if (result instanceof PaywallResult.Restored) {
            completeAndFinish("restored", ((PaywallResult.Restored) result).getCustomerInfo(), null);
            return;
        }

        if (result instanceof PaywallResult.Error) {
            completeAndFinish("error", null, ((PaywallResult.Error) result).getError().getMessage());
            return;
        }

        completeAndFinish("unknown", null, null);
    }

    private void finishCustomerCenter() {
        completeAndFinish("dismissed", null, null);
    }

    private void completeAndFinish(String result, @Nullable CustomerInfo customerInfo, @Nullable String errorMessage) {
        if (resultSent || requestId == null) {
            finish();
            return;
        }

        resultSent = true;
        RevenueCatUIPresenter.complete(
                requestId,
                RevenueCatUIPresenter.toResultJson(result, customerInfo, errorMessage, null, null));
        finish();
    }
}
