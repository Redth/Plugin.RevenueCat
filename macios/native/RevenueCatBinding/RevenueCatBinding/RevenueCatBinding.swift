//
//  DotnetNewBinding.swift
//  NewBinding
//
//  Created by .NET MAUI team on 6/18/24.
//

import Foundation
import UIKit
import StoreKit
import RevenueCat

@objc(RevenueCatManager)
public class RevenueCatManager : NSObject
{

    @objc(initialize:apiKey:userId:proxyURL:purchasesAreCompletedBy:storeKitVersion:entitlementVerificationMode:diagnosticsEnabled:automaticDeviceIdentifierCollectionEnabled:)
    public func initialize(debugLog: Bool, apiKey: NSString, userId: String?, proxyURL: String?, purchasesAreCompletedBy: String?, storeKitVersion: String?, entitlementVerificationMode: String?, diagnosticsEnabled: NSNumber?, automaticDeviceIdentifierCollectionEnabled: NSNumber?) {
        if (debugLog){
            Purchases.logLevel = .debug
        }

        if let proxyURL = proxyURL, let url = URL(string: proxyURL) {
            Purchases.proxyURL = url
        }

        let builder = Configuration.builder(withAPIKey: apiKey as String)

        if let userId = userId {
            builder.with(appUserID: userId)
        }

        if let purchasesAreCompletedBy = parsePurchasesAreCompletedBy(purchasesAreCompletedBy) {
            builder.with(purchasesAreCompletedBy: purchasesAreCompletedBy, storeKitVersion: parseStoreKitVersion(storeKitVersion) ?? .default)
        } else if let storeKitVersion = parseStoreKitVersion(storeKitVersion) {
            builder.with(storeKitVersion: storeKitVersion)
        }

        if let entitlementVerificationMode = parseEntitlementVerificationMode(entitlementVerificationMode) {
            builder.with(entitlementVerificationMode: entitlementVerificationMode)
        }

        if let diagnosticsEnabled = diagnosticsEnabled {
            builder.with(diagnosticsEnabled: diagnosticsEnabled.boolValue)
        }

        if let automaticDeviceIdentifierCollectionEnabled = automaticDeviceIdentifierCollectionEnabled {
            builder.with(automaticDeviceIdentifierCollectionEnabled: automaticDeviceIdentifierCollectionEnabled.boolValue)
        }

        Purchases.configure(with: builder)
    }
    
    
    @objc(login:callback:)
    public func login(userId: NSString, callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.logIn(userId as String) { customerInfo, created, error in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
    }

    @objc(logOut:)
    public func logOut(callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.logOut { customerInfo, error in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
    }

    @objc(appUserID)
    public func appUserID() -> NSString {
        return Purchases.shared.appUserID as NSString
    }

    @objc(isAnonymous)
    public func isAnonymous() -> Bool {
        return Purchases.shared.isAnonymous
    }
    
    @objc(getCustomerInfo:callback:)
    public func getCustomerInfo(force: Bool, callback: @escaping (NSString?, NSError?) -> Void) {
        let fetchPolicy = force ? CacheFetchPolicy.fetchCurrent : CacheFetchPolicy.notStaleCachedOrFetched

        Purchases.shared.getCustomerInfo(fetchPolicy: fetchPolicy) { customerInfo, error in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
    }

    @objc(invalidateCustomerInfoCache)
    public func invalidateCustomerInfoCache() {
        Purchases.shared.invalidateCustomerInfoCache()
    }
    
    
    var customerInfoChangedHandler: ((NSString) -> Void)?

    @objc(setCustomerInfoChangedHandler:)
    public func setCustomerInfoChangedHandler(callback: @escaping (NSString) -> Void) -> Void {
        customerInfoChangedHandler = callback
    }
    
    @objc(restore:)
    public func restore(callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.restorePurchases() { (customerInfo, error) in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
    }
	
	@objc(syncPurchases:)
	public func syncPurchases(callback: @escaping (NSString?, NSError?) -> Void) {
		Purchases.shared.syncPurchases() { (customerInfo, error) in
			self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
		}
	}

    @objc(getOffering:callback:)
    public func getOffering(offeringId: String, callback: @escaping (NSString?, NSError?) -> Void) {
        
        Purchases.shared.getOfferings { (offerings, error) in
            //Get by id or current if no id/offering
            if let currentOffering = offerings?.offering(identifier: offeringId) ?? offerings?.current {
                self.serializeJson(self.serializeOffering(currentOffering), callback: callback)
            } else {
                let requestedOffering = offeringId.trimmingCharacters(in: .whitespacesAndNewlines)
                let message = requestedOffering.isEmpty
                    ? "No current offering is configured."
                    : "No offering found for identifier '\(requestedOffering)' and no current offering is configured."
                let error = NSError(
                    domain: "RevenueCatManager",
                    code: 404,
                    userInfo: [NSLocalizedDescriptionKey: message]
                )
                callback(nil, error)
            }
        }
    }

    @objc(getOfferings:)
    public func getOfferings(callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.getOfferings { offerings, error in
            if let error = error {
                callback(nil, error as NSError)
                return
            }

            guard let offerings = offerings else {
                callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "No offerings found."]))
                return
            }

            var allOfferings = [String: Any]()
            var offeringList = [[String: Any]]()

            for (key, offering) in offerings.all {
                let offeringInfo = self.serializeOffering(offering)
                allOfferings[key] = offeringInfo
                offeringList.append(offeringInfo)
            }

            var offeringsInfo = [String: Any]()
            offeringsInfo["current_offering_id"] = offerings.current?.identifier
            offeringsInfo["current"] = offerings.current.map { self.serializeOffering($0) }
            offeringsInfo["all"] = allOfferings
            offeringsInfo["offerings"] = offeringList

            self.serializeJson(offeringsInfo, callback: callback)
        }
    }

    @objc(getOfferingForPlacement:callback:)
    public func getOfferingForPlacement(placementId: NSString, callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.getOfferings { offerings, error in
            if let error = error {
                callback(nil, error as NSError)
                return
            }

            if let offering = offerings?.currentOffering(forPlacement: placementId as String) {
                self.serializeJson(self.serializeOffering(offering), callback: callback)
            } else {
                callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "No offering found for placement '\(placementId)'."]))
            }
        }
    }

    @objc(getProducts:productType:callback:)
    public func getProducts(productIdentifiersCsv: NSString, productType: NSString?, callback: @escaping (NSString?, NSError?) -> Void) {
        let productIds = (productIdentifiersCsv as String)
            .split(separator: ",")
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }
            .filter { !$0.isEmpty }

        Purchases.shared.getProducts(productIds) { products in
            self.serializeJson(products.map { self.serializeStoreProduct($0) }, callback: callback)
        }
    }
    
    @objc(purchase:packageIdentifier:callback:)
    public func purchase(offeringIdentifier: NSString, packageIdentifier: NSString, callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.getOfferings { (offerings, error) in
            
            if let offering = offerings?.offering(identifier: offeringIdentifier as String) {
                if let pkg = offering.package(identifier: packageIdentifier as String) {
                    Purchases.shared.purchase(package: pkg) { storeTransaction, customerInfo, error, ok in
                        
                        // Send back updated customer info
                        self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
                        //self.processStoreTransaction(storeTransaction: storeTransaction, originalError: error, callback: callback)
                        return
                    }
                } else {
                    callback(nil, nil)
                }
            } else {
                callback(nil, nil)
            }
        }
    }

    @objc(purchaseWithResult:packageIdentifier:purchaseOptionsJson:callback:)
    public func purchaseWithResult(offeringIdentifier: NSString, packageIdentifier: NSString, purchaseOptionsJson: NSString?, callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.getOfferings { (offerings, error) in
            if let error = error {
                callback(nil, error as NSError)
                return
            }

            guard let purchaseOptions = self.parsePurchaseOptions(purchaseOptionsJson, callback: callback) else {
                return
            }

            guard let offering = offerings?.offering(identifier: offeringIdentifier as String),
                  let pkg = offering.package(identifier: packageIdentifier as String) else {
                callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "Offering and/or Package not found"]))
                return
            }

            self.purchase(package: pkg, options: purchaseOptions, callback: callback)
        }
    }

    @objc(purchaseProduct:productType:purchaseOptionsJson:callback:)
    public func purchaseProduct(productIdentifier: NSString, productType: NSString?, purchaseOptionsJson: NSString?, callback: @escaping (NSString?, NSError?) -> Void) {
        guard let purchaseOptions = parsePurchaseOptions(purchaseOptionsJson, callback: callback) else {
            return
        }

        Purchases.shared.getProducts([productIdentifier as String]) { products in
            guard let product = products.first else {
                callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "Product not found"]))
                return
            }

            self.purchase(product: product, options: purchaseOptions, callback: callback)
        }
    }

    private func parsePurchasesAreCompletedBy(_ value: String?) -> PurchasesAreCompletedBy? {
        switch normalizedOption(value) {
        case "revenuecat":
            return .revenueCat
        case "myapp", "my_app":
            return .myApp
        default:
            return nil
        }
    }

    private func parseStoreKitVersion(_ value: String?) -> StoreKitVersion? {
        switch normalizedOption(value) {
        case "storekit1", "sk1", "1":
            return .storeKit1
        case "storekit2", "sk2", "2":
            return .storeKit2
        default:
            return nil
        }
    }

    private func parseEntitlementVerificationMode(_ value: String?) -> Configuration.EntitlementVerificationMode? {
        switch normalizedOption(value) {
        case "disabled":
            return .disabled
        case "informational":
            return .informational
        default:
            return nil
        }
    }

    private func normalizedOption(_ value: String?) -> String {
        return (value ?? "")
            .trimmingCharacters(in: .whitespacesAndNewlines)
            .lowercased()
            .replacingOccurrences(of: "-", with: "")
            .replacingOccurrences(of: "_", with: "")
            .replacingOccurrences(of: " ", with: "")
    }
    
    private func processCustomerInfo(customerInfo: CustomerInfo?, originalError: Error?, callback: @escaping (NSString?, NSError?) -> Void) {
       
        if (originalError != nil) {
            callback(nil, (originalError as NSError?) ?? NSError(domain: "UnknownError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Unknown error"]))
            return
        }
        
        guard let rawData = customerInfo?.rawData else {
            callback(nil, NSError(domain: "InvalidDataError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Customer info is nil or invalid"]))
            return
        }
        
        do {
            let jsonData = try JSONSerialization.data(withJSONObject: customerInfo!.rawData, options: [])
            let jsonStr = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)
            
            if (customerInfoChangedHandler != nil) {
                customerInfoChangedHandler!(jsonStr!)
            }
            
            callback(jsonStr, nil)
            return
        } catch let error as NSError {
            callback(nil, error)
            return
        }
    }
    
    private func processStoreTransaction(storeTransaction: StoreTransaction?, originalError: Error?, callback: @escaping (NSString?, NSError?) -> Void) {
       
        if (originalError != nil) {
            callback(nil, (originalError as NSError?) ?? NSError(domain: "UnknownError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Unknown error"]))
            return
        }
        
        guard let rawData = storeTransaction else {
            callback(nil, NSError(domain: "InvalidDataError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Customer info is nil or invalid"]))
            return
        }
        
        do {
            var storeTransactionInfo = [String: Any]()
            storeTransactionInfo["id"] = rawData.id
            storeTransactionInfo["transaction_identifier"] = rawData.transactionIdentifier
            storeTransactionInfo["product_identifier"] = rawData.productIdentifier
            storeTransactionInfo["purchase_date"] = rawData.purchaseDate.ISO8601Format()
            storeTransactionInfo["quantity"] = rawData.quantity
            
            let jsonData = try JSONSerialization.data(withJSONObject: storeTransactionInfo, options: [])
            let jsonStr = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)

            callback(jsonStr, nil)
        } catch let error as NSError {
            callback(nil, error)
            return
        }
    }
	
	
	@objc(setEmail:)
	public func setEmail(email: NSString) {
		Purchases.shared.attribution.setEmail(email as String)
	}

    @objc(setPhoneNumber:)
    public func setPhoneNumber(phoneNumber: NSString?) {
        Purchases.shared.attribution.setPhoneNumber(phoneNumber as String?)
    }

    @objc(setPushToken:)
    public func setPushToken(pushToken: NSString?) {
        Purchases.shared.attribution.setPushTokenString(pushToken as String?)
    }
	
	@objc(setDisplayName:)
	public func setDisplayName(displayName: NSString) {
		Purchases.shared.attribution.setDisplayName(displayName as String)
	}

    @objc(setMediaSource:)
    public func setMediaSource(mediaSource: NSString?) {
        Purchases.shared.attribution.setMediaSource(mediaSource as String?)
    }
	
	@objc(setAd:)
	public func setAd(ad: NSString) {
		Purchases.shared.attribution.setAd(ad as String)
	}
	
	@objc(setAdGroup:)
	public func setAdGroup(adGroup: NSString) {
		Purchases.shared.attribution.setAdGroup(adGroup as String)
	}
	
	@objc(setCampaign:)
	public func setCampaign(campaign: NSString) {
		Purchases.shared.attribution.setCampaign(campaign as String)
	}
	
	@objc(setCreative:)
	public func setCreative(creative: NSString) {
		Purchases.shared.attribution.setCreative(creative as String)
	}
	
	@objc(setKeyword:)
	public func setKeyword(keyword: NSString) {
		Purchases.shared.attribution.setKeyword(keyword as String)
	}
	
	@objc(setAttribute:value:)
	public func setAttributes(key: NSString, value: NSString) {
		Purchases.shared.attribution.setAttributes([key: value] as [String: String])
	}
	
	@objc(setAttributes:)
	public func setAttributes(userAttributes: [NSString: NSString]) {
		Purchases.shared.attribution.setAttributes(userAttributes as [String: String])
	}
	
	@objc(syncOfferingsAndAttributesIfNeeded:)
	public func syncOfferingsAndAttributesIfNeeded(callback: @escaping (NSError?) -> Void) {
		Purchases.shared.syncAttributesAndOfferingsIfNeeded { offerings, err in
			callback(err)
		}
	}

    @objc(collectDeviceIdentifiers)
    public func collectDeviceIdentifiers() {
        Purchases.shared.attribution.collectDeviceIdentifiers()
    }

    @objc(canMakePayments)
    public func canMakePayments() -> Bool {
        return SKPaymentQueue.canMakePayments()
    }

    @objc(getStorefront:)
    public func getStorefront(callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.getStorefront { storefront in
            callback(storefront?.countryCode as NSString?, nil)
        }
    }

    @objc(getVirtualCurrencies:)
    public func getVirtualCurrencies(callback: @escaping (NSString?, NSError?) -> Void) {
        Task {
            do {
                let virtualCurrencies = try await Purchases.shared.virtualCurrencies()
                self.serializeJson(self.serializeVirtualCurrencies(virtualCurrencies), callback: callback)
            } catch let error as NSError {
                callback(nil, error)
            }
        }
    }

    @objc(invalidateVirtualCurrenciesCache)
    public func invalidateVirtualCurrenciesCache() {
        Purchases.shared.invalidateVirtualCurrenciesCache()
    }

    @objc(redeemWebPurchase:callback:)
    public func redeemWebPurchase(redemptionLink: NSString, callback: @escaping (NSString?, NSError?) -> Void) {
        guard let url = URL(string: redemptionLink as String),
              let redemption = Purchases.parseAsWebPurchaseRedemption(url) else {
            callback(nil, NSError(domain: "RevenueCatManager", code: 400, userInfo: [NSLocalizedDescriptionKey: "Invalid web purchase redemption link."]))
            return
        }

        Task {
            let result = await Purchases.shared.redeemWebPurchase(redemption)
            var resultInfo = [String: Any]()

            switch result {
            case .success(let customerInfo):
                resultInfo["status"] = "success"
                resultInfo["customer_info"] = customerInfo.rawData
                self.processCustomerInfo(customerInfo: customerInfo, originalError: nil) { _, _ in }
            case .error(let error):
                callback(nil, error as NSError)
                return
            case .invalidToken:
                resultInfo["status"] = "invalid_token"
            case .purchaseBelongsToOtherUser:
                resultInfo["status"] = "purchase_belongs_to_other_user"
            case .expired(let obfuscatedEmail):
                resultInfo["status"] = "expired"
                resultInfo["obfuscated_email"] = obfuscatedEmail
            }

            self.serializeJson(resultInfo, callback: callback)
        }
    }

    private func processPurchaseResult(storeTransaction: StoreTransaction?, customerInfo: CustomerInfo?, originalError: Error?, userCancelled: Bool, callback: @escaping (NSString?, NSError?) -> Void) {
        if userCancelled {
            serializeJson(["user_cancelled": true], callback: callback)
            return
        }

        if let originalError = originalError {
            callback(nil, originalError as NSError)
            return
        }

        var resultInfo = [String: Any]()
        resultInfo["user_cancelled"] = false
        resultInfo["customer_info"] = customerInfo?.rawData
        resultInfo["store_transaction"] = storeTransaction.map { serializeStoreTransaction($0) }

        if let customerInfo = customerInfo {
            processCustomerInfo(customerInfo: customerInfo, originalError: nil) { _, _ in }
        }

        serializeJson(resultInfo, callback: callback)
    }

    private func purchase(package: Package, options: [String: Any], callback: @escaping (NSString?, NSError?) -> Void) {
        do {
            if let discount = try selectedDiscount(for: package.storeProduct, options: options) {
                Purchases.shared.getPromotionalOffer(forProductDiscount: discount, product: package.storeProduct) { promotionalOffer, error in
                    if let error = error {
                        callback(nil, error as NSError)
                        return
                    }

                    guard let promotionalOffer = promotionalOffer else {
                        callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "Promotional offer could not be generated for discount '\(discount.offerIdentifier ?? "")'."]))
                        return
                    }

                    Purchases.shared.purchase(package: package, promotionalOffer: promotionalOffer) { storeTransaction, customerInfo, error, userCancelled in
                        self.processPurchaseResult(storeTransaction: storeTransaction, customerInfo: customerInfo, originalError: error, userCancelled: userCancelled, callback: callback)
                    }
                }
            } else {
                Purchases.shared.purchase(package: package) { storeTransaction, customerInfo, error, userCancelled in
                    self.processPurchaseResult(storeTransaction: storeTransaction, customerInfo: customerInfo, originalError: error, userCancelled: userCancelled, callback: callback)
                }
            }
        } catch let error as NSError {
            callback(nil, error)
        }
    }

    private func purchase(product: StoreProduct, options: [String: Any], callback: @escaping (NSString?, NSError?) -> Void) {
        do {
            if let discount = try selectedDiscount(for: product, options: options) {
                Purchases.shared.getPromotionalOffer(forProductDiscount: discount, product: product) { promotionalOffer, error in
                    if let error = error {
                        callback(nil, error as NSError)
                        return
                    }

                    guard let promotionalOffer = promotionalOffer else {
                        callback(nil, NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "Promotional offer could not be generated for discount '\(discount.offerIdentifier ?? "")'."]))
                        return
                    }

                    Purchases.shared.purchase(product: product, promotionalOffer: promotionalOffer) { storeTransaction, customerInfo, error, userCancelled in
                        self.processPurchaseResult(storeTransaction: storeTransaction, customerInfo: customerInfo, originalError: error, userCancelled: userCancelled, callback: callback)
                    }
                }
            } else {
                Purchases.shared.purchase(product: product) { storeTransaction, customerInfo, error, userCancelled in
                    self.processPurchaseResult(storeTransaction: storeTransaction, customerInfo: customerInfo, originalError: error, userCancelled: userCancelled, callback: callback)
                }
            }
        } catch let error as NSError {
            callback(nil, error)
        }
    }

    private func parsePurchaseOptions(_ purchaseOptionsJson: NSString?, callback: @escaping (NSString?, NSError?) -> Void) -> [String: Any]? {
        guard let purchaseOptionsJson = purchaseOptionsJson as String?,
              !purchaseOptionsJson.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            return [:]
        }

        guard let data = purchaseOptionsJson.data(using: .utf8) else {
            callback(nil, NSError(domain: "RevenueCatManager", code: 400, userInfo: [NSLocalizedDescriptionKey: "Purchase options JSON is not valid UTF-8."]))
            return nil
        }

        do {
            guard let options = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any] else {
                callback(nil, NSError(domain: "RevenueCatManager", code: 400, userInfo: [NSLocalizedDescriptionKey: "Purchase options JSON must be an object."]))
                return nil
            }

            return options
        } catch let error as NSError {
            callback(nil, error)
            return nil
        }
    }

    private func selectedDiscount(for product: StoreProduct, options: [String: Any]) throws -> StoreProductDiscount? {
        guard let discountIdentifier = optionString(options, keys: ["store_product_discount_identifier", "discount_identifier", "offer_identifier"]),
              !discountIdentifier.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            return nil
        }

        guard let discount = product.discounts.first(where: { $0.offerIdentifier == discountIdentifier }) else {
            throw NSError(domain: "RevenueCatManager", code: 404, userInfo: [NSLocalizedDescriptionKey: "Store product discount '\(discountIdentifier)' was not found for product '\(product.productIdentifier)'."])
        }

        return discount
    }

    private func optionString(_ options: [String: Any], keys: [String]) -> String? {
        for key in keys {
            if let value = options[key] as? String {
                return value
            }
        }

        return nil
    }

    private func serializeOffering(_ offering: Offering) -> [String: Any] {
        var offeringInfo = [String: Any]()
        offeringInfo["id"] = offering.id
        offeringInfo["identifier"] = offering.identifier
        offeringInfo["description"] = offering.serverDescription

        var metadataInfo = [String: Any]()
        for md in offering.metadata {
            metadataInfo[md.key] = md.value
        }
        offeringInfo["metadata"] = metadataInfo

        var packageInfos = [[String: Any]]()
        for pkg in offering.availablePackages {
            var packageInfo = [String: Any]()
            packageInfo["id"] = pkg.id
            packageInfo["identifier"] = pkg.identifier
            packageInfo["package_type"] = packageTypeIdentifier(pkg.packageType)
            packageInfo["type"] = packageTypeIdentifier(pkg.packageType)
            packageInfo["store_product"] = serializeStoreProduct(pkg.storeProduct)
            packageInfos.append(packageInfo)
        }

        offeringInfo["packages"] = packageInfos
        return offeringInfo
    }

    private func serializeStoreProduct(_ storeProduct: StoreProduct) -> [String: Any] {
        var productInfo = [String: Any]()
        productInfo["id"] = storeProduct.productIdentifier
        productInfo["identifier"] = storeProduct.productIdentifier
        productInfo["title"] = storeProduct.localizedTitle
        productInfo["description"] = storeProduct.localizedDescription
        productInfo["price_string"] = storeProduct.localizedPriceString
        productInfo["currency_code"] = storeProduct.currencyCode
        productInfo["price"] = [
            "amount": storeProduct.priceDecimalNumber.doubleValue,
            "currency": storeProduct.currencyCode ?? NSNull(),
            "currency_code": storeProduct.currencyCode ?? NSNull(),
            "formatted": storeProduct.localizedPriceString
        ]
        productInfo["product_type"] = productTypeIdentifier(storeProduct.productType)
        productInfo["product_category"] = productCategoryIdentifier(storeProduct.productCategory)
        productInfo["is_family_shareable"] = storeProduct.isFamilyShareable
        productInfo["localized_introductory_price_string"] = storeProduct.localizedIntroductoryPriceString
        productInfo["localized_price_per_day"] = storeProduct.localizedPricePerDay
        productInfo["localized_price_per_week"] = storeProduct.localizedPricePerWeek
        productInfo["localized_price_per_month"] = storeProduct.localizedPricePerMonth
        productInfo["localized_price_per_year"] = storeProduct.localizedPricePerYear

        if let subscriptionPeriod = storeProduct.subscriptionPeriod {
            productInfo["subscription_period"] = serializeSubscriptionPeriod(subscriptionPeriod)
        }

        if let introductoryDiscount = storeProduct.introductoryDiscount {
            productInfo["introductory_discount"] = serializeDiscount(introductoryDiscount)
        }

        productInfo["discounts"] = storeProduct.discounts.map { serializeDiscount($0) }
        return productInfo
    }

    private func serializeDiscount(_ discount: StoreProductDiscount) -> [String: Any] {
        var discountInfo = [String: Any]()
        discountInfo["offer_identifier"] = discount.offerIdentifier
        discountInfo["type"] = discountTypeIdentifier(discount.type)
        discountInfo["payment_mode"] = paymentModeIdentifier(discount.paymentMode)
        discountInfo["price_string"] = discount.localizedPriceString
        discountInfo["currency_code"] = discount.currencyCode
        discountInfo["price"] = [
            "amount": discount.priceDecimalNumber.doubleValue,
            "currency": discount.currencyCode ?? NSNull(),
            "currency_code": discount.currencyCode ?? NSNull(),
            "formatted": discount.localizedPriceString
        ]
        discountInfo["subscription_period"] = serializeSubscriptionPeriod(discount.subscriptionPeriod)
        discountInfo["number_of_periods"] = discount.numberOfPeriods
        return discountInfo
    }

    private func serializeSubscriptionPeriod(_ subscriptionPeriod: RevenueCat.SubscriptionPeriod) -> [String: Any] {
        return [
            "value": subscriptionPeriod.value,
            "unit": subscriptionPeriodUnitIdentifier(subscriptionPeriod.unit)
        ]
    }

    private func serializeStoreTransaction(_ storeTransaction: StoreTransaction) -> [String: Any] {
        return [
            "id": storeTransaction.id,
            "transaction_identifier": storeTransaction.transactionIdentifier,
            "product_identifier": storeTransaction.productIdentifier,
            "purchase_date": storeTransaction.purchaseDate.ISO8601Format(),
            "quantity": storeTransaction.quantity
        ]
    }

    private func serializeVirtualCurrencies(_ virtualCurrencies: VirtualCurrencies) -> [String: Any] {
        var all = [String: Any]()
        for (key, virtualCurrency) in virtualCurrencies.all {
            all[key] = serializeVirtualCurrency(virtualCurrency)
        }

        return ["all": all]
    }

    private func serializeVirtualCurrency(_ virtualCurrency: VirtualCurrency) -> [String: Any] {
        return [
            "code": virtualCurrency.code,
            "name": virtualCurrency.name,
            "balance": virtualCurrency.balance,
            "server_description": virtualCurrency.serverDescription ?? NSNull()
        ]
    }

    private func serializeJson(_ value: Any, callback: @escaping (NSString?, NSError?) -> Void) {
        do {
            let jsonData = try JSONSerialization.data(withJSONObject: value, options: [])
            let jsonStr = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)
            callback(jsonStr, nil)
        } catch let error as NSError {
            callback(nil, error)
        }
    }

    private func packageTypeIdentifier(_ packageType: RevenueCat.PackageType) -> String {
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

    private func subscriptionPeriodUnitIdentifier(_ unit: RevenueCat.SubscriptionPeriod.Unit) -> String {
        switch unit {
        case .day: return "day"
        case .year: return "year"
        case .month: return "month"
        case .week: return "week"
        @unknown default: return "unknown"
        }
    }

    private func productTypeIdentifier(_ productType: StoreProduct.ProductType) -> String {
        switch productType {
        case .consumable: return "consumable"
        case .nonConsumable: return "non_consumable"
        case .nonRenewableSubscription: return "non_renewing_subscription"
        case .autoRenewableSubscription: return "auto_renewable_subscription"
        @unknown default: return "unknown"
        }
    }

    private func productCategoryIdentifier(_ productCategory: StoreProduct.ProductCategory) -> String {
        switch productCategory {
        case .subscription: return "subscription"
        case .nonSubscription: return "non_subscription"
        @unknown default: return "unknown"
        }
    }

    private func paymentModeIdentifier(_ paymentMode: StoreProductDiscount.PaymentMode) -> String {
        switch paymentMode {
        case .payAsYouGo: return "pay_as_you_go"
        case .payUpFront: return "pay_up_front"
        case .freeTrial: return "free_trial"
        @unknown default: return "unknown"
        }
    }

    private func discountTypeIdentifier(_ discountType: StoreProductDiscount.DiscountType) -> String {
        switch discountType {
        case .introductory: return "introductory"
        case .promotional: return "promotional"
        case .winBack: return "win_back"
        @unknown default: return "unknown"
        }
    }
}
