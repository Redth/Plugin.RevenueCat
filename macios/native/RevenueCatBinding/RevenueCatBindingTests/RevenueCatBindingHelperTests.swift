import Foundation
import RevenueCat
@testable import RevenueCatBinding
import XCTest

final class RevenueCatBindingHelperTests: XCTestCase {
    func testPurchaseOptionsParsingAcceptsEmptyValues() throws {
        XCTAssertTrue(try RevenueCatBindingHelpers.parsePurchaseOptions(nil as String?).isEmpty)
        XCTAssertTrue(try RevenueCatBindingHelpers.parsePurchaseOptions("").isEmpty)
        XCTAssertTrue(try RevenueCatBindingHelpers.parsePurchaseOptions(" \n\t ").isEmpty)
    }

    func testPurchaseOptionsParsingPreservesObjectValues() throws {
        let options = try RevenueCatBindingHelpers.parsePurchaseOptions("""
        {
          "store_product_discount_identifier": "intro-offer",
          "discount_identifier": "legacy-offer",
          "offer_identifier": "fallback-offer",
          "quantity": 2
        }
        """)

        XCTAssertEqual(options["store_product_discount_identifier"] as? String, "intro-offer")
        XCTAssertEqual(options["discount_identifier"] as? String, "legacy-offer")
        XCTAssertEqual(options["offer_identifier"] as? String, "fallback-offer")
        XCTAssertEqual((options["quantity"] as? NSNumber)?.intValue, 2)
        XCTAssertEqual(
            RevenueCatBindingHelpers.optionString(options, keys: ["store_product_discount_identifier", "discount_identifier", "offer_identifier"]),
            "intro-offer"
        )
    }

    func testPurchaseOptionsParsingRejectsNonObjectJson() {
        XCTAssertThrowsError(try RevenueCatBindingHelpers.parsePurchaseOptions("[\"not\", \"object\"]")) { error in
            let nsError = error as NSError
            XCTAssertEqual(nsError.domain, RevenueCatBindingHelpers.errorDomain)
            XCTAssertEqual(nsError.code, 400)
            XCTAssertEqual(nsError.localizedDescription, "Purchase options JSON must be an object.")
        }
    }

    func testPurchaseCancellationPayloadShape() throws {
        let payload = try decode(RevenueCatBindingHelpers.purchaseCancelledPayload())

        XCTAssertEqual(payload.count, 1)
        XCTAssertEqual((payload["user_cancelled"] as? NSNumber)?.boolValue, true)
    }

    func testWebPurchaseRedemptionStatusPayloads() throws {
        let success = RevenueCatBindingHelpers.webPurchaseRedemptionPayload(
            status: .success,
            customerInfoRawData: ["original_app_user_id": "user-1"]
        )
        XCTAssertEqual(success["status"] as? String, "success")
        XCTAssertEqual((success["customer_info"] as? [String: Any])?["original_app_user_id"] as? String, "user-1")

        let invalidToken = RevenueCatBindingHelpers.webPurchaseRedemptionPayload(status: .invalidToken)
        XCTAssertEqual(invalidToken["status"] as? String, "invalid_token")
        XCTAssertNil(invalidToken["customer_info"])

        let otherUser = RevenueCatBindingHelpers.webPurchaseRedemptionPayload(status: .purchaseBelongsToOtherUser)
        XCTAssertEqual(otherUser["status"] as? String, "purchase_belongs_to_other_user")

        let expired = RevenueCatBindingHelpers.webPurchaseRedemptionPayload(status: .expired, obfuscatedEmail: "u***@example.com")
        XCTAssertEqual(expired["status"] as? String, "expired")
        XCTAssertEqual(expired["obfuscated_email"] as? String, "u***@example.com")

        XCTAssertEqual(try decode(expired)["status"] as? String, "expired")
    }

    func testPackageAndProductTypeMappings() {
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.annual), "annual")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.monthly), "monthly")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.weekly), "weekly")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.custom), "custom")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.lifetime), "lifetime")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.sixMonth), "six_month")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.threeMonth), "three_month")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.twoMonth), "two_month")
        XCTAssertEqual(RevenueCatBindingHelpers.packageTypeIdentifier(.unknown), "unknown")

        XCTAssertEqual(RevenueCatBindingHelpers.productTypeIdentifier(.consumable), "consumable")
        XCTAssertEqual(RevenueCatBindingHelpers.productTypeIdentifier(.nonConsumable), "non_consumable")
        XCTAssertEqual(RevenueCatBindingHelpers.productTypeIdentifier(.nonRenewableSubscription), "non_renewing_subscription")
        XCTAssertEqual(RevenueCatBindingHelpers.productTypeIdentifier(.autoRenewableSubscription), "auto_renewable_subscription")

        XCTAssertEqual(RevenueCatBindingHelpers.productCategoryIdentifier(.subscription), "subscription")
        XCTAssertEqual(RevenueCatBindingHelpers.productCategoryIdentifier(.nonSubscription), "non_subscription")
    }

    func testDiscountAndSubscriptionMappings() {
        XCTAssertEqual(RevenueCatBindingHelpers.paymentModeIdentifier(.payAsYouGo), "pay_as_you_go")
        XCTAssertEqual(RevenueCatBindingHelpers.paymentModeIdentifier(.payUpFront), "pay_up_front")
        XCTAssertEqual(RevenueCatBindingHelpers.paymentModeIdentifier(.freeTrial), "free_trial")

        XCTAssertEqual(RevenueCatBindingHelpers.discountTypeIdentifier(.introductory), "introductory")
        XCTAssertEqual(RevenueCatBindingHelpers.discountTypeIdentifier(.promotional), "promotional")
        XCTAssertEqual(RevenueCatBindingHelpers.discountTypeIdentifier(.winBack), "win_back")

        XCTAssertEqual(RevenueCatBindingHelpers.subscriptionPeriodUnitIdentifier(.day), "day")
        XCTAssertEqual(RevenueCatBindingHelpers.subscriptionPeriodUnitIdentifier(.week), "week")
        XCTAssertEqual(RevenueCatBindingHelpers.subscriptionPeriodUnitIdentifier(.month), "month")
        XCTAssertEqual(RevenueCatBindingHelpers.subscriptionPeriodUnitIdentifier(.year), "year")
    }

    func testObjCSelectorAvailability() {
        let manager = RevenueCatManager()
        let selectorNames = [
            "initialize:apiKey:userId:proxyURL:purchasesAreCompletedBy:storeKitVersion:entitlementVerificationMode:diagnosticsEnabled:automaticDeviceIdentifierCollectionEnabled:",
            "purchaseWithResult:packageIdentifier:purchaseOptionsJson:callback:",
            "purchaseProduct:productType:purchaseOptionsJson:callback:",
            "getOfferingForPlacement:callback:",
            "getProducts:productType:callback:",
            "redeemWebPurchase:callback:",
            "getVirtualCurrencies:",
            "invalidateVirtualCurrenciesCache"
        ]

        for selectorName in selectorNames {
            XCTAssertTrue(manager.responds(to: NSSelectorFromString(selectorName)), selectorName)
        }
    }

    private func decode(_ value: Any) throws -> [String: Any] {
        let json = try XCTUnwrap(RevenueCatBindingHelpers.serializeJsonString(value) as String?)
        let data = try XCTUnwrap(json.data(using: .utf8))
        return try XCTUnwrap(JSONSerialization.jsonObject(with: data, options: []) as? [String: Any])
    }
}
