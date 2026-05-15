import Foundation
import RevenueCat

enum RevenueCatBindingWebPurchaseRedemptionStatus {
    case success
    case invalidToken
    case purchaseBelongsToOtherUser
    case expired
}

enum RevenueCatBindingHelpers {
    static let errorDomain = "RevenueCatManager"

    static func parsePurchaseOptions(_ purchaseOptionsJson: NSString?) throws -> [String: Any] {
        return try parsePurchaseOptions(purchaseOptionsJson as String?)
    }

    static func parsePurchaseOptions(_ purchaseOptionsJson: String?) throws -> [String: Any] {
        guard let purchaseOptionsJson = purchaseOptionsJson,
              !purchaseOptionsJson.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            return [:]
        }

        guard let data = purchaseOptionsJson.data(using: .utf8) else {
            throw error(code: 400, message: "Purchase options JSON is not valid UTF-8.")
        }

        let parsedJson = try JSONSerialization.jsonObject(with: data, options: [])
        guard let options = parsedJson as? [String: Any] else {
            throw error(code: 400, message: "Purchase options JSON must be an object.")
        }

        return options
    }

    static func parsePurchasesAreCompletedBy(_ value: String?) -> PurchasesAreCompletedBy? {
        switch normalizedOption(value) {
        case "revenuecat":
            return .revenueCat
        case "myapp":
            return .myApp
        default:
            return nil
        }
    }

    static func parseStoreKitVersion(_ value: String?) -> StoreKitVersion? {
        switch normalizedOption(value) {
        case "storekit1", "sk1", "1":
            return .storeKit1
        case "storekit2", "sk2", "2":
            return .storeKit2
        default:
            return nil
        }
    }

    static func parseEntitlementVerificationMode(_ value: String?) -> Configuration.EntitlementVerificationMode? {
        switch normalizedOption(value) {
        case "disabled":
            return .disabled
        case "informational":
            return .informational
        default:
            return nil
        }
    }

    static func normalizedOption(_ value: String?) -> String {
        return (value ?? "")
            .trimmingCharacters(in: .whitespacesAndNewlines)
            .lowercased()
            .replacingOccurrences(of: "-", with: "")
            .replacingOccurrences(of: "_", with: "")
            .replacingOccurrences(of: " ", with: "")
    }

    static func optionString(_ options: [String: Any], keys: [String]) -> String? {
        for key in keys {
            if let value = options[key] as? String {
                return value
            }
        }

        return nil
    }

    static func purchaseCancelledPayload() -> [String: Any] {
        return ["user_cancelled": true]
    }

    static func purchaseResultPayload(customerInfoRawData: [String: Any]?, storeTransaction: StoreTransaction?) -> [String: Any] {
        var resultInfo = [String: Any]()
        resultInfo["user_cancelled"] = false
        resultInfo["customer_info"] = customerInfoRawData
        resultInfo["store_transaction"] = storeTransaction.map { serializeStoreTransaction($0) }
        return resultInfo
    }

    static func webPurchaseRedemptionPayload(status: RevenueCatBindingWebPurchaseRedemptionStatus, customerInfoRawData: [String: Any]? = nil, obfuscatedEmail: String? = nil) -> [String: Any] {
        var resultInfo = [String: Any]()

        switch status {
        case .success:
            resultInfo["status"] = "success"
            resultInfo["customer_info"] = customerInfoRawData
        case .invalidToken:
            resultInfo["status"] = "invalid_token"
        case .purchaseBelongsToOtherUser:
            resultInfo["status"] = "purchase_belongs_to_other_user"
        case .expired:
            resultInfo["status"] = "expired"
            resultInfo["obfuscated_email"] = obfuscatedEmail
        }

        return resultInfo
    }

    static func serializeStoreTransaction(_ storeTransaction: StoreTransaction) -> [String: Any] {
        return [
            "id": storeTransaction.id,
            "transaction_identifier": storeTransaction.transactionIdentifier,
            "product_identifier": storeTransaction.productIdentifier,
            "purchase_date": storeTransaction.purchaseDate.ISO8601Format(),
            "quantity": storeTransaction.quantity
        ]
    }

    static func serializeJsonString(_ value: Any) throws -> NSString? {
        let jsonData = try JSONSerialization.data(withJSONObject: value, options: [])
        return NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)
    }

    static func packageTypeIdentifier(_ packageType: RevenueCat.PackageType) -> String {
        switch packageType {
        case .annual: return "annual"
        case .monthly: return "monthly"
        case .weekly: return "weekly"
        case .custom: return "custom"
        case .lifetime: return "lifetime"
        case .sixMonth: return "six_month"
        case .threeMonth: return "three_month"
        case .twoMonth: return "two_month"
        case .unknown: return "unknown"
        @unknown default: return "unknown"
        }
    }

    static func subscriptionPeriodUnitIdentifier(_ unit: RevenueCat.SubscriptionPeriod.Unit) -> String {
        switch unit {
        case .day: return "day"
        case .year: return "year"
        case .month: return "month"
        case .week: return "week"
        @unknown default: return "unknown"
        }
    }

    static func productTypeIdentifier(_ productType: StoreProduct.ProductType) -> String {
        switch productType {
        case .consumable: return "consumable"
        case .nonConsumable: return "non_consumable"
        case .nonRenewableSubscription: return "non_renewing_subscription"
        case .autoRenewableSubscription: return "auto_renewable_subscription"
        @unknown default: return "unknown"
        }
    }

    static func productCategoryIdentifier(_ productCategory: StoreProduct.ProductCategory) -> String {
        switch productCategory {
        case .subscription: return "subscription"
        case .nonSubscription: return "non_subscription"
        @unknown default: return "unknown"
        }
    }

    static func paymentModeIdentifier(_ paymentMode: StoreProductDiscount.PaymentMode) -> String {
        switch paymentMode {
        case .payAsYouGo: return "pay_as_you_go"
        case .payUpFront: return "pay_up_front"
        case .freeTrial: return "free_trial"
        @unknown default: return "unknown"
        }
    }

    static func discountTypeIdentifier(_ discountType: StoreProductDiscount.DiscountType) -> String {
        switch discountType {
        case .introductory: return "introductory"
        case .promotional: return "promotional"
        case .winBack: return "win_back"
        @unknown default: return "unknown"
        }
    }

    private static func error(code: Int, message: String) -> NSError {
        return NSError(domain: errorDomain, code: code, userInfo: [NSLocalizedDescriptionKey: message])
    }
}
