package com.revenuecat.revenuecatbinding;

import static com.google.common.truth.Truth.assertThat;
import static org.junit.Assert.assertThrows;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;
import com.revenuecat.purchases.EntitlementVerificationMode;
import com.revenuecat.purchases.ProductType;
import com.revenuecat.purchases.PurchasesAreCompletedBy;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.models.StoreReplacementMode;

import org.junit.Test;

import java.util.Map;

public class RevenueCatManagerTest {
    @Test
    public void parsePurchaseOptions_preservesSupportedOptionKeys() {
        Map<String, Object> options = RevenueCatManager.parsePurchaseOptions("{"
            + "\"subscription_option_id\":\"base-plan:offer\","
            + "\"old_product_identifier\":\"monthly\","
            + "\"replacement_mode\":\"charge-full-price\","
            + "\"is_personalized_price\":true,"
            + "\"presented_offering_identifier\":\"spring_sale\","
            + "\"store_product_discount_identifier\":\"store_discount\","
            + "\"discount_identifier\":\"winback_discount\""
            + "}");

        assertThat(RevenueCatManager.optionString(options, "subscription_option_id")).isEqualTo("base-plan:offer");
        assertThat(RevenueCatManager.optionString(options, "old_product_identifier")).isEqualTo("monthly");
        assertThat(RevenueCatManager.optionString(options, "replacement_mode")).isEqualTo("charge-full-price");
        assertThat(RevenueCatManager.optionBoolean(options, "is_personalized_price")).isTrue();
        assertThat(RevenueCatManager.optionString(options, "presented_offering_identifier")).isEqualTo("spring_sale");
        assertThat(RevenueCatManager.optionString(options, "store_product_discount_identifier")).isEqualTo("store_discount");
        assertThat(RevenueCatManager.optionString(options, "discount_identifier")).isEqualTo("winback_discount");
    }

    @Test
    public void parsePurchaseOptions_handlesBlankAndInvalidJson() {
        assertThat(RevenueCatManager.parsePurchaseOptions(null)).isEmpty();
        assertThat(RevenueCatManager.parsePurchaseOptions("  ")).isEmpty();

        assertThrows(IllegalArgumentException.class, () -> RevenueCatManager.parsePurchaseOptions("null"));
        assertThrows(JsonSyntaxException.class, () -> RevenueCatManager.parsePurchaseOptions("{"));
    }

    @Test
    public void optionBoolean_acceptsBooleansAndBooleanStringsOnly() {
        Map<String, Object> options = RevenueCatManager.parsePurchaseOptions("{"
            + "\"bool_true\":true,"
            + "\"string_true\":\"true\","
            + "\"string_false\":\"FALSE\","
            + "\"number\":1"
            + "}");

        assertThat(RevenueCatManager.optionBoolean(options, "bool_true")).isTrue();
        assertThat(RevenueCatManager.optionBoolean(options, "string_true")).isTrue();
        assertThat(RevenueCatManager.optionBoolean(options, "string_false")).isFalse();
        assertThat(RevenueCatManager.optionBoolean(options, "number")).isNull();
    }

    @Test
    public void parseReplacementMode_normalizesSupportedValues() {
        assertThat(RevenueCatManager.parseReplacementMode("without_proration")).isEqualTo(StoreReplacementMode.WITHOUT_PRORATION);
        assertThat(RevenueCatManager.parseReplacementMode("With Time Proration")).isEqualTo(StoreReplacementMode.WITH_TIME_PRORATION);
        assertThat(RevenueCatManager.parseReplacementMode("charge-full-price")).isEqualTo(StoreReplacementMode.CHARGE_FULL_PRICE);
        assertThat(RevenueCatManager.parseReplacementMode("ChargeProratedPrice")).isEqualTo(StoreReplacementMode.CHARGE_PRORATED_PRICE);
        assertThat(RevenueCatManager.parseReplacementMode("deferred")).isEqualTo(StoreReplacementMode.DEFERRED);

        IllegalArgumentException exception = assertThrows(
            IllegalArgumentException.class,
            () -> RevenueCatManager.parseReplacementMode("immediate"));
        assertThat(exception).hasMessageThat().contains("Unsupported replacement mode 'immediate'.");
    }

    @Test
    public void parseProductTypeStoreAndConfigurationStrings() {
        assertThat(RevenueCatManager.parseProductType(null)).isNull();
        assertThat(RevenueCatManager.parseProductType(" ")).isNull();
        assertThat(RevenueCatManager.parseProductType("subs")).isEqualTo(ProductType.SUBS);
        assertThat(RevenueCatManager.parseProductType("SUBSCRIPTION")).isEqualTo(ProductType.SUBS);
        assertThat(RevenueCatManager.parseProductType("auto_renewable_subscription")).isEqualTo(ProductType.SUBS);
        assertThat(RevenueCatManager.parseProductType("in_app")).isEqualTo(ProductType.INAPP);
        assertThat(RevenueCatManager.parseProductType("consumable")).isEqualTo(ProductType.INAPP);
        assertThat(RevenueCatManager.parseProductType("unknown")).isNull();

        assertThat(RevenueCatManager.parseStore("amazon")).isEqualTo(Store.AMAZON);
        assertThat(RevenueCatManager.parseStore("test")).isEqualTo(Store.TEST_STORE);
        assertThat(RevenueCatManager.parseStore("play")).isEqualTo(Store.PLAY_STORE);

        assertThat(RevenueCatManager.parsePurchasesAreCompletedBy("revenue-cat")).isEqualTo(PurchasesAreCompletedBy.REVENUECAT);
        assertThat(RevenueCatManager.parsePurchasesAreCompletedBy("my_app")).isEqualTo(PurchasesAreCompletedBy.MY_APP);
        assertThat(RevenueCatManager.parsePurchasesAreCompletedBy("external")).isNull();

        assertThat(RevenueCatManager.parseEntitlementVerificationMode("disabled")).isEqualTo(EntitlementVerificationMode.DISABLED);
        assertThat(RevenueCatManager.parseEntitlementVerificationMode("Informational")).isEqualTo(EntitlementVerificationMode.INFORMATIONAL);
        assertThat(RevenueCatManager.parseEntitlementVerificationMode("enforced")).isNull();
    }

    @Test
    public void parseCsvIdentifiers_trimsAndDropsEmptyEntries() {
        assertThat(RevenueCatManager.parseCsvIdentifiers(" product_a, ,product_b,, product_c "))
            .containsExactly("product_a", "product_b", "product_c")
            .inOrder();
        assertThat(RevenueCatManager.parseCsvIdentifiers(" ,, ")).isEmpty();

        assertThrows(NullPointerException.class, () -> RevenueCatManager.parseCsvIdentifiers(null));
    }

    @Test
    public void revenueCatErrorSerialization_hasExpectedPrefixAndPayloadShape() {
        Exception exception = RevenueCatManager.purchasesException(
            "STORE_PROBLEM",
            23,
            "Store unavailable",
            "Billing service disconnected");

        assertThat(exception).hasMessageThat().startsWith(RevenueCatManager.REVENUECAT_ERROR_PREFIX);

        String payloadJson = exception.getMessage().substring(RevenueCatManager.REVENUECAT_ERROR_PREFIX.length());
        @SuppressWarnings("unchecked")
        Map<String, Object> payload = new Gson().fromJson(payloadJson, Map.class);

        assertThat(payload).containsEntry("code", "STORE_PROBLEM");
        assertThat(payload).containsEntry("native_code", 23.0);
        assertThat(payload).containsEntry("message", "Store unavailable");
        assertThat(payload).containsEntry("underlying_message", "Billing service disconnected");
        assertThat(payload).containsEntry("domain", "RevenueCat");
        assertThat(payload).containsEntry("source", "android");
    }

    @Test
    public void serializePurchaseResult_usesStableJsonKeyNamesForCancelledPurchases() {
        Map<String, Object> purchaseResult = RevenueCatManager.serializePurchaseResult(null, null, true);

        assertThat(purchaseResult).containsExactly(
            "user_cancelled", true,
            "customer_info", null,
            "store_transaction", null);
    }
}
