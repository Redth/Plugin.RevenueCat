package com.revenuecat.revenuecatbinding;

import android.app.Activity;
import android.content.Intent;

import androidx.annotation.Nullable;

import com.google.gson.Gson;
import com.revenuecat.purchases.CacheFetchPolicy;
import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.interfaces.ReceiveCustomerInfoCallback;

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;

final class RevenueCatUIPresenter {
    static final String EXTRA_MODE = "com.revenuecat.revenuecatbinding.mode";
    static final String EXTRA_REQUEST_ID = "com.revenuecat.revenuecatbinding.request_id";
    static final String EXTRA_OFFERING_IDENTIFIER = "com.revenuecat.revenuecatbinding.offering_identifier";
    static final String EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER = "com.revenuecat.revenuecatbinding.required_entitlement_identifier";
    static final String EXTRA_DISPLAY_CLOSE_BUTTON = "com.revenuecat.revenuecatbinding.display_close_button";

    static final String MODE_PAYWALL = "paywall";
    static final String MODE_CUSTOMER_CENTER = "customer_center";

    private static final Gson gson = new Gson();
    private static final Map<String, CompletableFuture<String>> pendingResults = new ConcurrentHashMap<>();

    private RevenueCatUIPresenter() {
    }

    static CompletableFuture<String> presentPaywall(
            @Nullable Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String requiredEntitlementIdentifier,
            boolean displayCloseButton)
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        if (activity == null) {
            future.complete(toResultJson("error", null, "A current Activity is required to present the paywall.", null, null));
            return future;
        }

        if (requiredEntitlementIdentifier == null || requiredEntitlementIdentifier.trim().isEmpty()) {
            launch(activity, future, MODE_PAYWALL, offeringIdentifier, requiredEntitlementIdentifier, displayCloseButton);
            return future;
        }

        Purchases.getSharedInstance().getCustomerInfo(
                CacheFetchPolicy.CACHED_OR_FETCHED,
                new ReceiveCustomerInfoCallback() {
                    @Override
                    public void onReceived(CustomerInfo customerInfo) {
                        if (customerInfo.getEntitlements().getActive().containsKey(requiredEntitlementIdentifier)) {
                            future.complete(toResultJson("notPresented", customerInfo, null, null, null));
                            return;
                        }

                        launch(activity, future, MODE_PAYWALL, offeringIdentifier, requiredEntitlementIdentifier, displayCloseButton);
                    }

                    @Override
                    public void onError(PurchasesError error) {
                        future.complete(toResultJson("error", null, error.getMessage(), null, null));
                    }
                });

        return future;
    }

    static CompletableFuture<String> presentCustomerCenter(@Nullable Activity activity)
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        if (activity == null) {
            future.complete(toResultJson("error", null, "A current Activity is required to present the customer center.", null, null));
            return future;
        }

        launch(activity, future, MODE_CUSTOMER_CENTER, null, null, true);
        return future;
    }

    static void complete(String requestId, String json)
    {
        CompletableFuture<String> future = pendingResults.remove(requestId);

        if (future != null && !future.isDone()) {
            future.complete(json);
        }
    }

    static void cancel(String requestId, @Nullable String errorMessage)
    {
        CompletableFuture<String> future = pendingResults.remove(requestId);

        if (future != null && !future.isDone()) {
            future.complete(toResultJson(
                    "error",
                    null,
                    errorMessage == null || errorMessage.isEmpty()
                            ? "RevenueCat UI was interrupted before a result was returned."
                            : errorMessage,
                    null,
                    null));
        }
    }

    static String toResultJson(
            String result,
            @Nullable CustomerInfo customerInfo,
            @Nullable String errorMessage,
            @Nullable String action,
            @Nullable String actionIdentifier)
    {
        Map<String, Object> payload = new HashMap<>();
        payload.put("result", result);

        if (customerInfo != null) {
            payload.put("customerInfo", customerInfo.getRawData());
        }

        if (errorMessage != null && !errorMessage.isEmpty()) {
            payload.put("errorMessage", errorMessage);
        }

        if (action != null && !action.isEmpty()) {
            payload.put("action", action);
        }

        if (actionIdentifier != null && !actionIdentifier.isEmpty()) {
            payload.put("actionIdentifier", actionIdentifier);
        }

        return gson.toJson(payload);
    }

    private static void launch(
            Activity activity,
            CompletableFuture<String> future,
            String mode,
            @Nullable String offeringIdentifier,
            @Nullable String requiredEntitlementIdentifier,
            boolean displayCloseButton)
    {
        if (activity.isFinishing() || activity.isDestroyed()) {
            future.complete(toResultJson("error", null, "The current Activity is not in a state that can present RevenueCat UI.", null, null));
            return;
        }

        String requestId = UUID.randomUUID().toString();
        pendingResults.put(requestId, future);

        activity.runOnUiThread(() -> {
            try {
                Intent intent = new Intent(activity, RevenueCatUIActivity.class);
                intent.putExtra(EXTRA_MODE, mode);
                intent.putExtra(EXTRA_REQUEST_ID, requestId);
                intent.putExtra(EXTRA_DISPLAY_CLOSE_BUTTON, displayCloseButton);

                if (offeringIdentifier != null && !offeringIdentifier.trim().isEmpty()) {
                    intent.putExtra(EXTRA_OFFERING_IDENTIFIER, offeringIdentifier);
                }

                if (mode.equals(MODE_PAYWALL) && requiredEntitlementIdentifier != null && !requiredEntitlementIdentifier.trim().isEmpty()) {
                    intent.putExtra(EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER, requiredEntitlementIdentifier);
                }

                activity.startActivity(intent);
            } catch (Exception ex) {
                pendingResults.remove(requestId);
                future.complete(toResultJson("error", null, ex.getMessage(), null, null));
            }
        });
    }
}
