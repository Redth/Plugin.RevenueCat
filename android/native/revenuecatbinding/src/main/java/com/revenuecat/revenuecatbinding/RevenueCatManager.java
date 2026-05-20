package com.revenuecat.revenuecatbinding;

import android.app.Activity;
import android.content.Context;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.google.gson.Gson;
import com.revenuecat.purchases.CacheFetchPolicy;
import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.EntitlementVerificationMode;
import com.revenuecat.purchases.LogLevel;
import com.revenuecat.purchases.Offering;
import com.revenuecat.purchases.Offerings;
import com.revenuecat.purchases.Package;
import com.revenuecat.purchases.PackageType;
import com.revenuecat.purchases.ProductType;
import com.revenuecat.purchases.PurchaseParams;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesAreCompletedBy;
import com.revenuecat.purchases.PurchasesConfiguration;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.PresentedOfferingContext;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.WebPurchaseRedemption;
import com.revenuecat.purchases.interfaces.LogInCallback;
import com.revenuecat.purchases.interfaces.PurchaseCallback;
import com.revenuecat.purchases.interfaces.ReceiveCustomerInfoCallback;
import com.revenuecat.purchases.interfaces.ReceiveOfferingsCallback;
import com.revenuecat.purchases.interfaces.GetStoreProductsCallback;
import com.revenuecat.purchases.interfaces.GetStorefrontCallback;
import com.revenuecat.purchases.interfaces.GetVirtualCurrenciesCallback;
import com.revenuecat.purchases.interfaces.GetAmazonLWAConsentStatusCallback;
import com.revenuecat.purchases.interfaces.RedeemWebPurchaseListener;
import com.revenuecat.purchases.interfaces.Callback;
import com.revenuecat.purchases.interfaces.SyncAttributesAndOfferingsCallback;
import com.revenuecat.purchases.interfaces.SyncPurchasesCallback;
import com.revenuecat.purchases.models.StoreProduct;
import com.revenuecat.purchases.models.StoreTransaction;
import com.revenuecat.purchases.models.StoreReplacementMode;
import com.revenuecat.purchases.models.Period;
import com.revenuecat.purchases.models.Price;
import com.revenuecat.purchases.models.SubscriptionOption;
import com.revenuecat.purchases.models.PricingPhase;
import com.revenuecat.purchases.models.InstallmentsInfo;
import com.revenuecat.purchases.virtualcurrencies.VirtualCurrencies;
import com.revenuecat.purchases.virtualcurrencies.VirtualCurrency;
import com.revenuecat.purchases.AmazonLWAConsentStatus;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.HashMap;
import java.net.URL;

public class RevenueCatManager
{
    public static void initialize(Context context, boolean debugLog, String appStore, String apiKey, String userId, @Nullable String proxyUrl, @Nullable String purchasesAreCompletedBy, @Nullable String entitlementVerificationMode, @Nullable Boolean diagnosticsEnabled, @Nullable Boolean automaticDeviceIdentifierCollectionEnabled, @Nullable Boolean pendingTransactionsForPrepaidPlansEnabled) {

        if (debugLog)
            Purchases.setLogLevel(LogLevel.DEBUG);
        else
            Purchases.setLogLevel(LogLevel.ERROR);

        if (hasText(proxyUrl)) {
            try {
                Purchases.setProxyURL(new URL(proxyUrl));
            } catch (Exception ex) {
                throw new IllegalArgumentException("RevenueCat proxy URL must be an absolute URL.", ex);
            }
        }

        PurchasesConfiguration.Builder builder = new PurchasesConfiguration.Builder(context, apiKey);

        builder.store(parseStore(appStore));

        if (userId != null) {
            builder.appUserID(userId);
        }

        PurchasesAreCompletedBy purchasesCompletedBy = parsePurchasesAreCompletedBy(purchasesAreCompletedBy);
        if (purchasesCompletedBy != null) {
            builder.purchasesAreCompletedBy(purchasesCompletedBy);
        }

        EntitlementVerificationMode verificationMode = parseEntitlementVerificationMode(entitlementVerificationMode);
        if (verificationMode != null) {
            builder.entitlementVerificationMode(verificationMode);
        }

        if (diagnosticsEnabled != null) {
            builder.diagnosticsEnabled(diagnosticsEnabled);
        }

        if (automaticDeviceIdentifierCollectionEnabled != null) {
            builder.automaticDeviceIdentifierCollectionEnabled(automaticDeviceIdentifierCollectionEnabled);
        }

        if (pendingTransactionsForPrepaidPlansEnabled != null) {
            builder.pendingTransactionsForPrepaidPlansEnabled(pendingTransactionsForPrepaidPlansEnabled);
        }

        Purchases.configure(builder.build());
    }

    public static String getAppUserId() {
        return Purchases.getSharedInstance().getAppUserID();
    }

    public static boolean isAnonymous() {
        return Purchases.getSharedInstance().isAnonymous();
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
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> logout() {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().logOut(new ReceiveCustomerInfoCallback() {
            @Override
            public void onReceived(@NonNull CustomerInfo customerInfo) {
                handleCustomerInfoUpdated(customerInfo);
                future.complete(customerInfo.getRawData().toString());
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
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
                    future.completeExceptionally(purchasesException(purchasesError));
                }
            });

        return future;
    }

    public static CompletableFuture<String> getOffering(@Nullable String offeringIdentifier) {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Offering offering = null;
                String requestedOffering = offeringIdentifier == null ? "" : offeringIdentifier.trim();

                if (!requestedOffering.isEmpty()) {
                    offering = offerings.get(offeringIdentifier);
                }

                if (offering == null) {
                    offering = offerings.getCurrent();
                }

                if (offering == null) {
                    String message = requestedOffering.isEmpty()
                        ? "No current offering is configured."
                        : "No offering found for identifier '" + requestedOffering + "' and no current offering is configured.";
                    future.completeExceptionally(new Exception(message));
                    return;
                }

                Gson gson = new Gson();
                String json = gson.toJson(serializeOffering(offering));
                future.complete(json);
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> getOfferingForPlacement(@Nullable String placementIdentifier) {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Offering offering = offerings.getCurrentOfferingForPlacement(placementIdentifier);

                if (offering == null) {
                    future.completeExceptionally(new Exception("No offering found for placement '" + placementIdentifier + "'."));
                    return;
                }

                future.complete(new Gson().toJson(serializeOffering(offering)));
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> getOfferings() {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Map<String, Object> offeringsInfo = new HashMap<>();
                Map<String, Object> allOfferings = new HashMap<>();
                List<Map<String, Object>> offeringList = new ArrayList<>();

                for (Map.Entry<String, Offering> offeringEntry : offerings.getAll().entrySet()) {
                    Map<String, Object> offeringInfo = serializeOffering(offeringEntry.getValue());
                    allOfferings.put(offeringEntry.getKey(), offeringInfo);
                    offeringList.add(offeringInfo);
                }

                offeringsInfo.put("current_offering_id", offerings.getCurrent() == null ? null : offerings.getCurrent().getIdentifier());
                offeringsInfo.put("current", offerings.getCurrent() == null ? null : serializeOffering(offerings.getCurrent()));
                offeringsInfo.put("all", allOfferings);
                offeringsInfo.put("offerings", offeringList);

                future.complete(new Gson().toJson(offeringsInfo));
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> getProducts(String productIdentifiersCsv, @Nullable String productType) {
        CompletableFuture<String> future = new CompletableFuture<>();
        List<String> productIdentifiers = parseCsvIdentifiers(productIdentifiersCsv);

        GetStoreProductsCallback callback = new GetStoreProductsCallback() {
            @Override
            public void onReceived(@NonNull List<StoreProduct> storeProducts) {
                List<Map<String, Object>> productInfos = new ArrayList<>();

                for (StoreProduct storeProduct : storeProducts) {
                    productInfos.add(serializeStoreProduct(storeProduct));
                }

                future.complete(new Gson().toJson(productInfos));
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        };

        ProductType type = parseProductType(productType);
        if (type == null) {
            Purchases.getSharedInstance().getProducts(productIdentifiers, callback);
        } else {
            Purchases.getSharedInstance().getProducts(productIdentifiers, type, callback);
        }

        return future;
    }

    public static CompletableFuture<String> purchase(Activity activity, String offeringIdentifier, String packageIdentifier)
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Offering offering = offerings.get(offeringIdentifier);

                Package pkg = null;
                if (offering != null) {
                    pkg = offering.getPackage(packageIdentifier);
                }

                if (pkg == null) {
                    future.completeExceptionally(new Exception("Offering and/or Package not found"));
                    return;
                }

                Purchases.getSharedInstance().purchase(
                        new PurchaseParams.Builder(activity, pkg).build(), new PurchaseCallback() {
                            @Override
                            public void onCompleted(@NonNull StoreTransaction storeTransaction, @NonNull CustomerInfo customerInfo) {
                                handleCustomerInfoUpdated(customerInfo);
                                future.complete(customerInfo.getRawData().toString());
                            }

                            @Override
                            public void onError(@NonNull PurchasesError purchasesError, boolean b) {
                                future.completeExceptionally(purchasesException(purchasesError));
                            }
                        }
                );
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> purchaseWithResult(Activity activity, String offeringIdentifier, String packageIdentifier, @Nullable String purchaseOptionsJson)
    {
        CompletableFuture<String> future = new CompletableFuture<>();
        Map<String, Object> purchaseOptions;
        try {
            purchaseOptions = parsePurchaseOptions(purchaseOptionsJson);
        } catch (Exception ex) {
            future.completeExceptionally(ex);
            return future;
        }

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Offering offering = offerings.get(offeringIdentifier);

                Package pkg = null;
                if (offering != null) {
                    pkg = offering.getPackage(packageIdentifier);
                }

                if (pkg == null) {
                    future.completeExceptionally(new Exception("Offering and/or Package not found"));
                    return;
                }

                PurchaseParams.Builder builder = new PurchaseParams.Builder(activity, pkg);
                try {
                    applyPurchaseOptions(builder, purchaseOptions);
                } catch (Exception ex) {
                    future.completeExceptionally(ex);
                    return;
                }

                Purchases.getSharedInstance().purchase(
                        builder.build(), new PurchaseCallback() {
                            @Override
                            public void onCompleted(@NonNull StoreTransaction storeTransaction, @NonNull CustomerInfo customerInfo) {
                                handleCustomerInfoUpdated(customerInfo);
                                future.complete(new Gson().toJson(serializePurchaseResult(storeTransaction, customerInfo, false)));
                            }

                            @Override
                            public void onError(@NonNull PurchasesError purchasesError, boolean userCancelled) {
                                if (userCancelled) {
                                    future.complete(new Gson().toJson(serializePurchaseResult(null, null, true)));
                                } else {
                                    future.completeExceptionally(purchasesException(purchasesError));
                                }
                            }
                        }
                );
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> purchaseProduct(Activity activity, String productIdentifier, @Nullable String productType, @Nullable String purchaseOptionsJson)
    {
        CompletableFuture<String> future = new CompletableFuture<>();
        Map<String, Object> purchaseOptions;
        try {
            purchaseOptions = parsePurchaseOptions(purchaseOptionsJson);
        } catch (Exception ex) {
            future.completeExceptionally(ex);
            return future;
        }

        ProductType type = parseProductType(productType);
        GetStoreProductsCallback callback = new GetStoreProductsCallback() {
            @Override
            public void onReceived(@NonNull List<StoreProduct> storeProducts) {
                if (storeProducts.isEmpty()) {
                    future.completeExceptionally(new Exception("Product not found"));
                    return;
                }

                StoreProduct storeProduct = storeProducts.get(0);
                PurchaseParams.Builder builder;
                try {
                    builder = buildPurchaseParams(activity, storeProduct, purchaseOptions);
                } catch (Exception ex) {
                    future.completeExceptionally(ex);
                    return;
                }

                Purchases.getSharedInstance().purchase(
                        builder.build(), new PurchaseCallback() {
                            @Override
                            public void onCompleted(@NonNull StoreTransaction storeTransaction, @NonNull CustomerInfo customerInfo) {
                                handleCustomerInfoUpdated(customerInfo);
                                future.complete(new Gson().toJson(serializePurchaseResult(storeTransaction, customerInfo, false)));
                            }

                            @Override
                            public void onError(@NonNull PurchasesError purchasesError, boolean userCancelled) {
                                if (userCancelled) {
                                    future.complete(new Gson().toJson(serializePurchaseResult(null, null, true)));
                                } else {
                                    future.completeExceptionally(purchasesException(purchasesError));
                                }
                            }
                        }
                );
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        };

        if (type == null) {
            Purchases.getSharedInstance().getProducts(Arrays.asList(productIdentifier), callback);
        } else {
            Purchases.getSharedInstance().getProducts(Arrays.asList(productIdentifier), type, callback);
        }

        return future;
    }

    public static CompletableFuture<String> purchaseSubscriptionOption(Activity activity, String productIdentifier, String subscriptionOptionIdentifier, @Nullable String productType, @Nullable String purchaseOptionsJson)
    {
        CompletableFuture<String> future = new CompletableFuture<>();
        Map<String, Object> purchaseOptions;
        try {
            purchaseOptions = parsePurchaseOptions(purchaseOptionsJson);
            purchaseOptions.put("subscription_option_id", subscriptionOptionIdentifier);
        } catch (Exception ex) {
            future.completeExceptionally(ex);
            return future;
        }

        ProductType type = parseProductType(productType);
        GetStoreProductsCallback callback = new GetStoreProductsCallback() {
            @Override
            public void onReceived(@NonNull List<StoreProduct> storeProducts) {
                if (storeProducts.isEmpty()) {
                    future.completeExceptionally(new Exception("Product not found"));
                    return;
                }

                StoreProduct storeProduct = storeProducts.get(0);
                PurchaseParams.Builder builder;
                try {
                    builder = buildPurchaseParams(activity, storeProduct, purchaseOptions);
                } catch (Exception ex) {
                    future.completeExceptionally(ex);
                    return;
                }

                Purchases.getSharedInstance().purchase(
                        builder.build(), new PurchaseCallback() {
                            @Override
                            public void onCompleted(@NonNull StoreTransaction storeTransaction, @NonNull CustomerInfo customerInfo) {
                                handleCustomerInfoUpdated(customerInfo);
                                future.complete(new Gson().toJson(serializePurchaseResult(storeTransaction, customerInfo, false)));
                            }

                            @Override
                            public void onError(@NonNull PurchasesError purchasesError, boolean userCancelled) {
                                if (userCancelled) {
                                    future.complete(new Gson().toJson(serializePurchaseResult(null, null, true)));
                                } else {
                                    future.completeExceptionally(purchasesException(purchasesError));
                                }
                            }
                        }
                );
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        };

        if (type == null) {
            Purchases.getSharedInstance().getProducts(Arrays.asList(productIdentifier), callback);
        } else {
            Purchases.getSharedInstance().getProducts(Arrays.asList(productIdentifier), type, callback);
        }

        return future;
    }

    public static CompletableFuture<String> restore()
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
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

	public static CompletableFuture<String> syncPurchases()
	{
		CompletableFuture<String> future = new CompletableFuture<>();

		Purchases.getSharedInstance().syncPurchases(new SyncPurchasesCallback() {
			@Override
			public void onSuccess(@NonNull CustomerInfo customerInfo) {
				handleCustomerInfoUpdated(customerInfo);
				future.complete(customerInfo.getRawData().toString());
			}

			@Override
			public void onError(@NonNull PurchasesError purchasesError) {
				future.completeExceptionally(purchasesException(purchasesError));
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

	public static void setEmail(String email) {
		Purchases.getSharedInstance().setEmail(email);
	}

    public static void setPhoneNumber(String phoneNumber) {
        Purchases.getSharedInstance().setPhoneNumber(phoneNumber);
    }

    public static void setPushToken(String pushToken) {
        Purchases.getSharedInstance().setPushToken(pushToken);
    }

	public static void setDisplayName(String displayName) {
		Purchases.getSharedInstance().setDisplayName(displayName);
	}

    public static void setMediaSource(String mediaSource) {
        Purchases.getSharedInstance().setMediaSource(mediaSource);
    }

	public static void setAd(String ad) {
		Purchases.getSharedInstance().setAd(ad);
	}

	public static void setAdGroup(String adGroup) {
		Purchases.getSharedInstance().setAdGroup(adGroup);
	}

	public static void setCampaign(String campaign) {
		Purchases.getSharedInstance().setCampaign(campaign);
	}

	public static void setCreative(String creative) {
		Purchases.getSharedInstance().setCreative(creative);
	}

	public static void setKeyword(String keyword) {
		Purchases.getSharedInstance().setKeyword(keyword);
	}

	public static void setAttribute(String key, String value) {
		Map<String, String> attrs = new HashMap<>();
		attrs.put(key, value);
		Purchases.getSharedInstance().setAttributes(attrs);
	}

	public static void setAttributes(Map<String, String> userAttributes) {
		Purchases.getSharedInstance().setAttributes(userAttributes);
	}

    public static void collectDeviceIdentifiers() {
        Purchases.getSharedInstance().collectDeviceIdentifiers();
    }

    public static void invalidateCustomerInfoCache() {
        Purchases.getSharedInstance().invalidateCustomerInfoCache();
    }

    public static CompletableFuture<Boolean> canMakePayments(Context context) {
        CompletableFuture<Boolean> future = new CompletableFuture<>();

        Purchases.canMakePayments(context, new Callback<Boolean>() {
            @Override
            public void onReceived(Boolean canMakePayments) {
                future.complete(canMakePayments);
            }
        });

        return future;
    }

    public static CompletableFuture<String> getStorefront() {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getStorefrontCountryCode(new GetStorefrontCallback() {
            @Override
            public void onReceived(@NonNull String storefrontCountryCode) {
                future.complete(storefrontCountryCode);
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static CompletableFuture<String> getVirtualCurrencies() {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getVirtualCurrencies(new GetVirtualCurrenciesCallback() {
            @Override
            public void onReceived(@NonNull VirtualCurrencies virtualCurrencies) {
                future.complete(new Gson().toJson(serializeVirtualCurrencies(virtualCurrencies)));
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

    public static void invalidateVirtualCurrenciesCache() {
        Purchases.getSharedInstance().invalidateVirtualCurrenciesCache();
    }

    public static CompletableFuture<String> redeemWebPurchase(String redemptionLink) {
        CompletableFuture<String> future = new CompletableFuture<>();
        WebPurchaseRedemption redemption = Purchases.parseAsWebPurchaseRedemption(redemptionLink);

        if (redemption == null) {
            future.completeExceptionally(new IllegalArgumentException("Invalid web purchase redemption link."));
            return future;
        }

        Purchases.getSharedInstance().redeemWebPurchase(redemption, new RedeemWebPurchaseListener() {
            @Override
            public void handleResult(@NonNull RedeemWebPurchaseListener.Result result) {
                Map<String, Object> resultInfo = new HashMap<>();

                if (result instanceof RedeemWebPurchaseListener.Result.Success) {
                    CustomerInfo customerInfo = ((RedeemWebPurchaseListener.Result.Success) result).getCustomerInfo();
                    handleCustomerInfoUpdated(customerInfo);
                    resultInfo.put("status", "success");
                    resultInfo.put("customer_info", new Gson().fromJson(customerInfo.getRawData().toString(), Object.class));
                    future.complete(new Gson().toJson(resultInfo));
                    return;
                }

                if (result instanceof RedeemWebPurchaseListener.Result.Error) {
                    future.completeExceptionally(purchasesException(((RedeemWebPurchaseListener.Result.Error) result).getError()));
                    return;
                }

                if (result instanceof RedeemWebPurchaseListener.Result.InvalidToken) {
                    resultInfo.put("status", "invalid_token");
                } else if (result instanceof RedeemWebPurchaseListener.Result.PurchaseBelongsToOtherUser) {
                    resultInfo.put("status", "purchase_belongs_to_other_user");
                } else if (result instanceof RedeemWebPurchaseListener.Result.Expired) {
                    resultInfo.put("status", "expired");
                    resultInfo.put("obfuscated_email", ((RedeemWebPurchaseListener.Result.Expired) result).getObfuscatedEmail());
                } else {
                    resultInfo.put("status", "unknown");
                }

                future.complete(new Gson().toJson(resultInfo));
            }
        });

        return future;
    }

    public static CompletableFuture<String> getAmazonLwaConsentStatus() {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getAmazonLWAConsentStatus(new GetAmazonLWAConsentStatusCallback() {
            @Override
            public void onSuccess(@NonNull AmazonLWAConsentStatus status) {
                future.complete(status.name().toLowerCase());
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(purchasesException(purchasesError));
            }
        });

        return future;
    }

	public static CompletableFuture<Boolean> syncAttributesAndOfferingsIfNeeded() {
		CompletableFuture<Boolean> future = new CompletableFuture<>();

		Purchases.getSharedInstance().syncAttributesAndOfferingsIfNeeded(new SyncAttributesAndOfferingsCallback() {
			@Override
			public void onSuccess(@NonNull Offerings offerings) {
				future.complete(true);
			}

			@Override
			public void onError(@NonNull PurchasesError purchasesError) {
				future.completeExceptionally(purchasesException(purchasesError));
			}
		});

		return future;
	}

    @SuppressWarnings("unchecked")
    static Map<String, Object> parsePurchaseOptions(@Nullable String purchaseOptionsJson) {
        if (purchaseOptionsJson == null || purchaseOptionsJson.trim().isEmpty()) {
            return new HashMap<>();
        }

        Object parsed = new Gson().fromJson(purchaseOptionsJson, Map.class);
        if (!(parsed instanceof Map)) {
            throw new IllegalArgumentException("Purchase options JSON must be an object.");
        }

        return new HashMap<>((Map<String, Object>) parsed);
    }

    private static PurchaseParams.Builder buildPurchaseParams(Activity activity, StoreProduct storeProduct, Map<String, Object> purchaseOptions) {
        String subscriptionOptionId = optionString(purchaseOptions, "subscription_option_id");
        SubscriptionOption subscriptionOption = findSubscriptionOption(storeProduct, subscriptionOptionId);
        PurchaseParams.Builder builder = subscriptionOption == null
            ? new PurchaseParams.Builder(activity, storeProduct)
            : new PurchaseParams.Builder(activity, subscriptionOption);

        applyPurchaseOptions(builder, purchaseOptions);
        return builder;
    }

    private static void applyPurchaseOptions(PurchaseParams.Builder builder, Map<String, Object> purchaseOptions) {
        Boolean personalizedPrice = optionBoolean(purchaseOptions, "is_personalized_price");
        if (personalizedPrice != null) {
            builder.isPersonalizedPrice(personalizedPrice);
        }

        String oldProductIdentifier = optionString(purchaseOptions, "old_product_identifier");
        if (hasText(oldProductIdentifier)) {
            builder.oldProductId(oldProductIdentifier);
        }

        String replacementModeValue = optionString(purchaseOptions, "replacement_mode");
        if (hasText(replacementModeValue)) {
            builder.replacementMode(parseReplacementMode(replacementModeValue));
        }

        String presentedOfferingIdentifier = optionString(purchaseOptions, "presented_offering_identifier");
        if (hasText(presentedOfferingIdentifier)) {
            builder.presentedOfferingContext(new PresentedOfferingContext(presentedOfferingIdentifier));
        }
    }

    private static SubscriptionOption findSubscriptionOption(StoreProduct storeProduct, @Nullable String subscriptionOptionId) {
        if (!hasText(subscriptionOptionId)) {
            return null;
        }

        if (storeProduct.getSubscriptionOptions() == null) {
            throw new IllegalArgumentException("Product '" + storeProduct.getId() + "' does not have subscription options.");
        }

        for (SubscriptionOption option : storeProduct.getSubscriptionOptions()) {
            if (subscriptionOptionId.equals(option.getId())) {
                return option;
            }
        }

        throw new IllegalArgumentException("Subscription option '" + subscriptionOptionId + "' was not found for product '" + storeProduct.getId() + "'.");
    }

    static StoreReplacementMode parseReplacementMode(String replacementMode) {
        String normalized = replacementMode.trim().toLowerCase()
                .replace("_", "")
                .replace("-", "")
                .replace(" ", "");

        switch (normalized) {
            case "withoutproration":
                return StoreReplacementMode.WITHOUT_PRORATION;
            case "withtimeproration":
                return StoreReplacementMode.WITH_TIME_PRORATION;
            case "chargefullprice":
                return StoreReplacementMode.CHARGE_FULL_PRICE;
            case "chargeproratedprice":
                return StoreReplacementMode.CHARGE_PRORATED_PRICE;
            case "deferred":
                return StoreReplacementMode.DEFERRED;
            default:
                throw new IllegalArgumentException("Unsupported replacement mode '" + replacementMode + "'.");
        }
    }

    static String optionString(Map<String, Object> options, String key) {
        Object value = options.get(key);
        return value instanceof String ? (String)value : null;
    }

    static Boolean optionBoolean(Map<String, Object> options, String key) {
        Object value = options.get(key);
        if (value instanceof Boolean) {
            return (Boolean)value;
        }

        if (value instanceof String) {
            return Boolean.parseBoolean((String)value);
        }

        return null;
    }

    static boolean hasText(@Nullable String value) {
        return value != null && !value.trim().isEmpty();
    }

    static PurchasesAreCompletedBy parsePurchasesAreCompletedBy(@Nullable String value) {
        String normalized = normalizeOption(value);
        if (normalized.equals("revenuecat")) {
            return PurchasesAreCompletedBy.REVENUECAT;
        }

        if (normalized.equals("myapp")) {
            return PurchasesAreCompletedBy.MY_APP;
        }

        return null;
    }

    static EntitlementVerificationMode parseEntitlementVerificationMode(@Nullable String value) {
        String normalized = normalizeOption(value);
        if (normalized.equals("disabled")) {
            return EntitlementVerificationMode.DISABLED;
        }

        if (normalized.equals("informational")) {
            return EntitlementVerificationMode.INFORMATIONAL;
        }

        return null;
    }

    static String normalizeOption(@Nullable String value) {
        if (value == null) {
            return "";
        }

        return value.trim().toLowerCase()
                .replace("_", "")
                .replace("-", "")
                .replace(" ", "");
    }

    static Store parseStore(String appStore) {
        if (appStore.equals("amazon"))
            return Store.AMAZON;
        else if (appStore.equals("test"))
            return Store.TEST_STORE;
        else
            return Store.PLAY_STORE;
    }

    static ProductType parseProductType(@Nullable String productType) {
        if (productType == null || productType.trim().isEmpty()) {
            return null;
        }

        String normalized = productType.trim().toLowerCase();
        if (normalized.equals("subs") || normalized.equals("subscription") || normalized.equals("auto_renewable_subscription")) {
            return ProductType.SUBS;
        }

        if (normalized.equals("inapp") || normalized.equals("in_app") || normalized.equals("inapp") ||
            normalized.equals("consumable") || normalized.equals("non_consumable") || normalized.equals("non_renewing_subscription")) {
            return ProductType.INAPP;
        }

        return null;
    }

    static List<String> parseCsvIdentifiers(String identifiersCsv) {
        List<String> identifiers = new ArrayList<>();

        for (String identifier : identifiersCsv.split(",")) {
            String trimmed = identifier.trim();
            if (!trimmed.isEmpty()) {
                identifiers.add(trimmed);
            }
        }

        return identifiers;
    }

    private static Exception purchasesException(PurchasesError purchasesError) {
        return purchasesException(
            purchasesError.getCode().name(),
            purchasesError.getCode().getCode(),
            purchasesError.getMessage(),
            purchasesError.getUnderlyingErrorMessage());
    }

    static final String REVENUECAT_ERROR_PREFIX = "RevenueCatError:";

    static Exception purchasesException(String code, int nativeCode, String message, @Nullable String underlyingMessage) {
        return new Exception(REVENUECAT_ERROR_PREFIX + new Gson().toJson(serializeRevenueCatError(code, nativeCode, message, underlyingMessage)));
    }

    static Map<String, Object> serializeRevenueCatError(String code, int nativeCode, String message, @Nullable String underlyingMessage) {
        Map<String, Object> errorInfo = new HashMap<>();
        errorInfo.put("code", code);
        errorInfo.put("native_code", nativeCode);
        errorInfo.put("message", message);
        errorInfo.put("underlying_message", underlyingMessage);
        errorInfo.put("domain", "RevenueCat");
        errorInfo.put("source", "android");
        return errorInfo;
    }

    private static Map<String, Object> serializeOffering(Offering offering) {
        Map<String, Object> offeringInfo = new HashMap<>();
        offeringInfo.put("id", offering.getIdentifier());
        offeringInfo.put("identifier", offering.getIdentifier());
        offeringInfo.put("description", offering.getServerDescription());
        offeringInfo.put("metadata", offering.getMetadata());

        List<Map<String, Object>> packageInfos = new ArrayList<>();

        for (Package pkg : offering.getAvailablePackages()) {
            Map<String, Object> packageInfo = new HashMap<>();
            packageInfo.put("id", pkg.getIdentifier());
            packageInfo.put("identifier", pkg.getIdentifier());
            packageInfo.put("package_type", pkg.getPackageType().getIdentifier());
            packageInfo.put("type", pkg.getPackageType().getIdentifier());
            packageInfo.put("store_product", serializeStoreProduct(pkg.getProduct()));
            packageInfos.add(packageInfo);
        }

        offeringInfo.put("packages", packageInfos);
        return offeringInfo;
    }

    private static Map<String, Object> serializeStoreProduct(StoreProduct storeProduct) {
        Map<String, Object> productInfo = new HashMap<>();
        productInfo.put("id", storeProduct.getId());
        productInfo.put("identifier", storeProduct.getId());
        productInfo.put("name", storeProduct.getName());
        productInfo.put("title", storeProduct.getTitle());
        productInfo.put("description", storeProduct.getDescription());
        productInfo.put("price_string", storeProduct.getPrice().getFormatted());
        productInfo.put("currency_code", storeProduct.getPrice().getCurrencyCode());
        productInfo.put("price", serializePrice(storeProduct.getPrice()));
        productInfo.put("product_type", storeProduct.getType().name().toLowerCase());
        productInfo.put("is_family_shareable", false);

        if (storeProduct.getPeriod() != null) {
            productInfo.put("subscription_period", serializePeriod(storeProduct.getPeriod()));
        }

        if (storeProduct.getSubscriptionOptions() != null) {
            List<Map<String, Object>> optionInfos = new ArrayList<>();
            for (SubscriptionOption option : storeProduct.getSubscriptionOptions()) {
                optionInfos.add(serializeSubscriptionOption(option));
            }
            productInfo.put("subscription_options", optionInfos);
        }

        if (storeProduct.getDefaultOption() != null) {
            productInfo.put("default_subscription_option", serializeSubscriptionOption(storeProduct.getDefaultOption()));
        }

        return productInfo;
    }

    private static Map<String, Object> serializeSubscriptionOption(SubscriptionOption option) {
        Map<String, Object> optionInfo = new HashMap<>();
        optionInfo.put("id", option.getId());
        optionInfo.put("tags", option.getTags());
        optionInfo.put("is_base_plan", option.isBasePlan());
        optionInfo.put("is_prepaid", option.isPrepaid());

        if (option.getBillingPeriod() != null) {
            optionInfo.put("billing_period", serializePeriod(option.getBillingPeriod()));
        }

        List<Map<String, Object>> pricingPhases = new ArrayList<>();
        for (PricingPhase pricingPhase : option.getPricingPhases()) {
            pricingPhases.add(serializePricingPhase(pricingPhase));
        }
        optionInfo.put("pricing_phases", pricingPhases);

        if (option.getFreePhase() != null) {
            optionInfo.put("free_phase", serializePricingPhase(option.getFreePhase()));
        }
        if (option.getIntroPhase() != null) {
            optionInfo.put("intro_phase", serializePricingPhase(option.getIntroPhase()));
        }
        if (option.getFullPricePhase() != null) {
            optionInfo.put("full_price_phase", serializePricingPhase(option.getFullPricePhase()));
        }
        if (option.getInstallmentsInfo() != null) {
            optionInfo.put("installments_info", serializeInstallmentsInfo(option.getInstallmentsInfo()));
        }

        return optionInfo;
    }

    private static Map<String, Object> serializePricingPhase(PricingPhase pricingPhase) {
        Map<String, Object> pricingPhaseInfo = new HashMap<>();
        pricingPhaseInfo.put("billing_period", serializePeriod(pricingPhase.getBillingPeriod()));
        pricingPhaseInfo.put("recurrence_mode", pricingPhase.getRecurrenceMode().name().toLowerCase());
        pricingPhaseInfo.put("billing_cycle_count", pricingPhase.getBillingCycleCount());
        pricingPhaseInfo.put("offer_payment_mode", pricingPhase.getOfferPaymentMode() == null ? null : pricingPhase.getOfferPaymentMode().name().toLowerCase());
        pricingPhaseInfo.put("price", serializePrice(pricingPhase.getPrice()));
        pricingPhaseInfo.put("price_per_day", serializePrice(pricingPhase.pricePerDay()));
        pricingPhaseInfo.put("price_per_week", serializePrice(pricingPhase.pricePerWeek()));
        pricingPhaseInfo.put("price_per_month", serializePrice(pricingPhase.pricePerMonth()));
        pricingPhaseInfo.put("price_per_year", serializePrice(pricingPhase.pricePerYear()));
        return pricingPhaseInfo;
    }

    private static Map<String, Object> serializeInstallmentsInfo(InstallmentsInfo installmentsInfo) {
        Map<String, Object> installmentsInfoMap = new HashMap<>();
        installmentsInfoMap.put("commitment_payments_count", installmentsInfo.getCommitmentPaymentsCount());
        installmentsInfoMap.put("renewal_commitment_payments_count", installmentsInfo.getRenewalCommitmentPaymentsCount());
        return installmentsInfoMap;
    }

    private static Map<String, Object> serializePeriod(Period period) {
        Map<String, Object> subscriptionPeriodInfo = new HashMap<>();
        subscriptionPeriodInfo.put("value", period.getValue());

        String unit;
        switch (period.getUnit()) {
            case DAY:
                unit = "day";
                break;
            case YEAR:
                unit = "year";
                break;
            case MONTH:
                unit = "month";
                break;
            case WEEK:
                unit = "week";
                break;
            default:
                unit = "unknown";
                break;
        }
        subscriptionPeriodInfo.put("unit", unit);
        return subscriptionPeriodInfo;
    }

    private static Map<String, Object> serializePrice(Price price) {
        Map<String, Object> priceInfo = new HashMap<>();
        priceInfo.put("formatted", price.getFormatted());
        priceInfo.put("amount_micros", price.getAmountMicros());
        priceInfo.put("currency_code", price.getCurrencyCode());
        priceInfo.put("currency", price.getCurrencyCode());
        priceInfo.put("amount", price.getAmountMicros() / 1000000.0);
        return priceInfo;
    }

    static Map<String, Object> serializePurchaseResult(@Nullable StoreTransaction storeTransaction, @Nullable CustomerInfo customerInfo, boolean userCancelled) {
        Map<String, Object> purchaseResultInfo = new HashMap<>();
        purchaseResultInfo.put("user_cancelled", userCancelled);
        purchaseResultInfo.put("customer_info", customerInfo == null ? null : new Gson().fromJson(customerInfo.getRawData().toString(), Object.class));
        purchaseResultInfo.put("store_transaction", storeTransaction == null ? null : serializeStoreTransaction(storeTransaction));
        return purchaseResultInfo;
    }

    private static Map<String, Object> serializeVirtualCurrencies(VirtualCurrencies virtualCurrencies) {
        Map<String, Object> virtualCurrenciesInfo = new HashMap<>();
        Map<String, Object> all = new HashMap<>();

        for (Map.Entry<String, VirtualCurrency> entry : virtualCurrencies.getAll().entrySet()) {
            all.put(entry.getKey(), serializeVirtualCurrency(entry.getValue()));
        }

        virtualCurrenciesInfo.put("all", all);
        return virtualCurrenciesInfo;
    }

    private static Map<String, Object> serializeVirtualCurrency(VirtualCurrency virtualCurrency) {
        Map<String, Object> virtualCurrencyInfo = new HashMap<>();
        virtualCurrencyInfo.put("code", virtualCurrency.getCode());
        virtualCurrencyInfo.put("name", virtualCurrency.getName());
        virtualCurrencyInfo.put("balance", virtualCurrency.getBalance());
        virtualCurrencyInfo.put("server_description", virtualCurrency.getServerDescription());
        return virtualCurrencyInfo;
    }

    private static Map<String, Object> serializeStoreTransaction(StoreTransaction storeTransaction) {
        Map<String, Object> storeTransactionInfo = new HashMap<>();
        storeTransactionInfo.put("id", storeTransaction.getOrderId());
        storeTransactionInfo.put("transaction_identifier", storeTransaction.getOrderId());
        storeTransactionInfo.put("product_identifiers", storeTransaction.getProductIds());
        storeTransactionInfo.put("product_identifier", storeTransaction.getProductIds().isEmpty() ? null : storeTransaction.getProductIds().get(0));
        storeTransactionInfo.put("store", storeTransaction.getType().name().toLowerCase());
        return storeTransactionInfo;
    }
}
