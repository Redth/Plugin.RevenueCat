package com.revenuecat.revenuecatbinding;

import android.app.Activity;
import android.content.Context;

import androidx.annotation.NonNull;

import com.revenuecat.purchases.CacheFetchPolicy;
import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.LogLevel;
import com.revenuecat.purchases.PurchaseParams;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesConfiguration;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.interfaces.LogInCallback;
import com.revenuecat.purchases.interfaces.PurchaseCallback;
import com.revenuecat.purchases.interfaces.ReceiveCustomerInfoCallback;
import com.revenuecat.purchases.models.StoreProduct;
import com.revenuecat.purchases.models.StoreTransaction;

import java.util.Map;
import java.util.concurrent.CompletableFuture;

public class RevenueCatManager
{
    public static void initialize(Context context, boolean debugLog, String appStore, String apiKey, String userId) {

        if (debugLog)
            Purchases.setLogLevel(LogLevel.DEBUG);
        else
            Purchases.setLogLevel(LogLevel.ERROR);

        PurchasesConfiguration.Builder builder = new PurchasesConfiguration.Builder(context, apiKey);

        if (appStore.equals("amazon"))
            builder.store(Store.AMAZON);
        else
            builder.store(Store.PLAY_STORE);

        if (userId != null) {
            builder.appUserID(userId);
        }

        Purchases.configure(builder.build());
    }

    public static CompletableFuture<String> login(String userId) {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().logIn(userId, new LogInCallback() {
            @Override
            public void onReceived(@NonNull CustomerInfo customerInfo, boolean b) {
                handleCustomerInfoUpdated(customerInfo);
                future.complete(customerInfo.getRawData().toString());
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(new Exception(purchasesError.getMessage()));
            }
        });

        return future;
    }

    public void setAttributes(Map<String, String> attr)  {
        Purchases.getSharedInstance().setAttributes(attr);
    }

    public static CompletableFuture<String> getCustomerInfo(boolean force) {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getCustomerInfo(
            force ? CacheFetchPolicy.FETCH_CURRENT : CacheFetchPolicy.CACHED_OR_FETCHED,
            new ReceiveCustomerInfoCallback() {
                @Override
                public void onReceived(@NonNull CustomerInfo customerInfo) {
                    handleCustomerInfoUpdated(customerInfo);
                    future.complete(customerInfo.getRawData().toString());
                }

                @Override
                public void onError(@NonNull PurchasesError purchasesError) {
                    future.completeExceptionally(new Exception(purchasesError.getMessage()));
                }
            });

        return future;
    }

    public CompletableFuture<String> purchase(ProductInfo productInfo, Object platformObject)
    {
        Activity activity = ((Activity) platformObject);
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().purchase(
                new PurchaseParams.Builder(activity, (StoreProduct) productInfo.getRevenueCatProduct()).build(), new PurchaseCallback() {
                    @Override
                    public void onCompleted(@NonNull StoreTransaction storeTransaction, @NonNull CustomerInfo customerInfo) {
                        handleCustomerInfoUpdated(customerInfo);
                        future.complete(storeTransaction.getOriginalJson().toString());
                    }

                    @Override
                    public void onError(@NonNull PurchasesError purchasesError, boolean b) {
                        future.completeExceptionally(new Exception(purchasesError.getMessage()));
                    }
                }
        );

        return future;
    }

    public CompletableFuture<String> restore()
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().restorePurchases(new ReceiveCustomerInfoCallback() {
            @Override
            public void onReceived(@NonNull CustomerInfo customerInfo) {
                handleCustomerInfoUpdated(customerInfo);
                future.complete(customerInfo.getRawData().toString());
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(new Exception(purchasesError.getMessage()));
            }
        });

        return future;
    }


    static CustomerInfoUpdatedListener customerInfoUpdatedListener;

    public static void setCustomerInfoUpdatedListener(CustomerInfoUpdatedListener listener) {
        customerInfoUpdatedListener = listener;
    }

    static void handleCustomerInfoUpdated(CustomerInfo customerInfo)
    {
        if (customerInfoUpdatedListener != null) {
            customerInfoUpdatedListener.onCustomerInfoUpdated(customerInfo.getRawData().toString());
        }
    }
}

