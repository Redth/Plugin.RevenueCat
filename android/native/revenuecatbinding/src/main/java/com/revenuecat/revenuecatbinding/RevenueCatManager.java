package com.revenuecat.revenuecatbinding;

import android.app.Activity;
import android.content.Context;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.google.gson.Gson;
import com.revenuecat.purchases.CacheFetchPolicy;
import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.LogLevel;
import com.revenuecat.purchases.Offering;
import com.revenuecat.purchases.Offerings;
import com.revenuecat.purchases.Package;
import com.revenuecat.purchases.PackageType;
import com.revenuecat.purchases.PurchaseParams;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesAreCompletedBy;
import com.revenuecat.purchases.PurchasesConfiguration;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.interfaces.LogInCallback;
import com.revenuecat.purchases.interfaces.PurchaseCallback;
import com.revenuecat.purchases.interfaces.ReceiveCustomerInfoCallback;
import com.revenuecat.purchases.interfaces.ReceiveOfferingsCallback;
import com.revenuecat.purchases.interfaces.SyncAttributesAndOfferingsCallback;
import com.revenuecat.purchases.interfaces.SyncPurchasesCallback;
import com.revenuecat.purchases.models.StoreProduct;
import com.revenuecat.purchases.models.StoreTransaction;
import com.revenuecat.purchases.models.Period;

import java.util.ArrayList;
import java.util.Dictionary;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.HashMap;

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

    public static CompletableFuture<String> getOffering(@Nullable String offeringIdentifier) {
        CompletableFuture<String> future = new CompletableFuture<>();

        Purchases.getSharedInstance().getOfferings(new ReceiveOfferingsCallback() {
            @Override
            public void onReceived(@NonNull Offerings offerings) {
                Offering offering;

                if (offeringIdentifier != null)
                    offering = offerings.get(offeringIdentifier);
                else
                    offering = offerings.getCurrent();

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
                    packageInfo.put("type", pkg.getPackageType().getIdentifier());



                    StoreProduct storeProduct = pkg.getProduct();
                    Map<String, Object> productInfo = new HashMap<>();
                    productInfo.put("identifier", storeProduct.getId());
                    productInfo.put("title", storeProduct.getTitle());
                    productInfo.put("description", storeProduct.getDescription());
                    productInfo.put("price_string", storeProduct.getPrice().getFormatted());
                    productInfo.put("currency_code", storeProduct.getPrice().getCurrencyCode());
                    productInfo.put("is_family_shareable", false);


                    if (storeProduct.getPeriod() != null) {
                        Map<String, Object> subscriptionPeriodInfo = new HashMap<>();
                        subscriptionPeriodInfo.put("value", storeProduct.getPeriod().getValue());

                        String unit;
                        switch (storeProduct.getPeriod().getUnit()) {
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

                        productInfo.put("subscription_period", subscriptionPeriodInfo);
                    }

                    packageInfo.put("store_product", productInfo);
                    packageInfos.add(packageInfo);
                }

                offeringInfo.put("packages", packageInfos);

                Gson gson = new Gson();
                String json = gson.toJson(offeringInfo);
                future.complete(json);
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(new Exception(purchasesError.getMessage()));
            }
        });

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
                                future.completeExceptionally(new Exception(purchasesError.getMessage()));
                            }
                        }
                );
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                future.completeExceptionally(new Exception(purchasesError.getMessage()));
            }
        });

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
                future.completeExceptionally(new Exception(purchasesError.getMessage()));
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

	public static void setEmail(String email) {
		Purchases.getSharedInstance().setEmail(email);
	}

	public static void setDisplayName(String displayName) {
		Purchases.getSharedInstance().setDisplayName(displayName);
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

	public static CompletableFuture<Boolean> syncAttributesAndOfferingsIfNeeded() {
		CompletableFuture<Boolean> future = new CompletableFuture<>();

		Purchases.getSharedInstance().syncAttributesAndOfferingsIfNeeded(new SyncAttributesAndOfferingsCallback() {
			@Override
			public void onSuccess(@NonNull Offerings offerings) {
				future.complete(true);
			}

			@Override
			public void onError(@NonNull PurchasesError purchasesError) {
				future.completeExceptionally(new Exception(purchasesError.getMessage()));
			}
		});

		return future;
	}
}

