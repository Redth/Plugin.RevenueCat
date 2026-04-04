//
//  DotnetNewBinding.swift
//  NewBinding
//
//  Created by .NET MAUI team on 6/18/24.
//

import Foundation
import UIKit
import RevenueCat
import RevenueCatUI

@objc(RevenueCatManager)
public class RevenueCatManager : NSObject
{
    private var activePaywallDelegates: [PaywallPresentationDelegate] = []
    private var activeCustomerCenterDelegates: [CustomerCenterPresentationDelegate] = []

    @objc(initialize:apiKey:userId:)
    public func initialize(debugLog: Bool, apiKey: NSString, userId: String?) {
        if (debugLog){
            Purchases.logLevel = .debug
        }
        Purchases.configure(withAPIKey: apiKey as String, appUserID: userId)
    }
    
    
    @objc(login:callback:)
    public func login(userId: NSString, callback: @escaping (NSString?, NSError?) -> Void) {
        Purchases.shared.logIn(userId as String) { customerInfo, created, error in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
    }
    
    @objc(getCustomerInfo:callback:)
    public func getCustomerInfo(force: Bool, callback: @escaping (NSString?, NSError?) -> Void) {
        let fetchPolicy = force ? CacheFetchPolicy.fetchCurrent : CacheFetchPolicy.notStaleCachedOrFetched

        Purchases.shared.getCustomerInfo(fetchPolicy: fetchPolicy) { customerInfo, error in
            self.processCustomerInfo(customerInfo: customerInfo, originalError: error, callback: callback)
        }
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
                
                var offeringInfo = [String: Any]()
                offeringInfo["id"] = currentOffering.id
                offeringInfo["identifier"] = currentOffering.identifier
                offeringInfo["description"] = currentOffering.serverDescription
                
                var metadataInfo = [String: Any]()
                for md in currentOffering.metadata {
                    metadataInfo[md.key] = md.value
                }
                
                offeringInfo["metadata"] = metadataInfo
                
                var packageInfos = [Any]()
                
                let apkgs = currentOffering.availablePackages
                
                for pkg in apkgs {
                    var packageInfo: [String: Any] = [:]
                    
                    packageInfo["id"] = pkg.id
                    packageInfo["identifier"] = pkg.identifier
                    //packageInfo["introductory_price_string"] = pkg.localizedIntroductoryPriceString
                    //packageInfo["price_string"] = pkg.localizedPriceString
                    //packageInfo["offering_identifier"] = pkg.offeringIdentifier
                    
                    let packageTypeMapping: [RevenueCat.PackageType: String] = [
                        .annual: "annual",
                        .monthly: "monthly",
                        .weekly: "weekly",
                        .custom: "custom",
                        .lifetime: "lifetime",
                        .sixMonth: "six_month",
                        .threeMonth: "three_month",
                        .twoMonth: "two_month",
                        .unknown: "unknown"
                    ]

                    packageInfo["type"] = packageTypeMapping[pkg.packageType] ?? "unknown"

                    let storeProduct = pkg.storeProduct
                    var productInfo: [String: Any] = [:]
                    productInfo["identifier"] = storeProduct.productIdentifier
                    productInfo["title"] = storeProduct.localizedTitle
                    productInfo["description"] = storeProduct.localizedDescription
                    productInfo["price_string"] = storeProduct.localizedPriceString
                    productInfo["currency_code"] = storeProduct.currencyCode
                    productInfo["is_family_shareable"] = storeProduct.isFamilyShareable
                    
                    
//                    productInfo["localized_price_per_day"] = storeProduct.localizedPricePerDay
//                    productInfo["localized_price_per_week"] = storeProduct.localizedPricePerWeek
//                    productInfo["localized_price_per_month"] = storeProduct.localizedPricePerMonth
//                    productInfo["localized_price_per_year"] = storeProduct.localizedPricePerYear
                    
                    if let subscriptionPeriod = storeProduct.subscriptionPeriod {
                        var subscriptionPeriodInfo: [String: Any] = [:]
                        subscriptionPeriodInfo["value"] = subscriptionPeriod.value
                        
                        let subPeriodUnitMapping: [RevenueCat.SubscriptionPeriod.Unit: String] = [
                            .day: "day",
                            .year: "year",
                            .month: "month",
                            .week: "week"
                        ]
                        subscriptionPeriodInfo["unit"] = subPeriodUnitMapping[subscriptionPeriod.unit] ?? "unknown"
                        
                        productInfo["subscription_period"] = subscriptionPeriodInfo
                    }
                    
                    packageInfo["store_product"] = productInfo
                    
                    packageInfos.append(packageInfo)
                }
                

                    
                offeringInfo["packages"] = packageInfos
                
                do {
                    let jsonData = try JSONSerialization.data(withJSONObject: offeringInfo, options: [])
                    let jsonStr = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)

                    callback(jsonStr, nil)
                } catch let error as NSError {
                    callback(nil, error)
                }
                
                //callback(currentOffering.description as NSString, nil)
            } else {
                callback(nil, nil)
            }
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

    @objc(presentPaywall:requiredEntitlementIdentifier:displayCloseButton:callback:)
    public func presentPaywall(
        offeringIdentifier: NSString?,
        requiredEntitlementIdentifier: NSString?,
        displayCloseButton: Bool,
        callback: @escaping (NSString?, NSError?) -> Void
    ) {
        let proceedToPresentation = {
            self.loadOffering(offeringIdentifier: offeringIdentifier as String?) { offering in
                DispatchQueue.main.async {
                    guard let presenter = self.topViewController() else {
                        self.completeResult(
                            result: "error",
                            errorMessage: "Unable to find a view controller to present the paywall from.",
                            callback: callback)
                        return
                    }

                    let delegate = PaywallPresentationDelegate(owner: self, callback: callback)
                    self.activePaywallDelegates.append(delegate)

                    let controller = PaywallViewController(
                        offering: offering,
                        displayCloseButton: displayCloseButton)
                    controller.delegate = delegate

                    presenter.present(controller, animated: true)
                }
            } failure: { errorMessage in
                self.completeResult(result: "error", errorMessage: errorMessage, callback: callback)
            }
        }

        let trimmedRequiredEntitlement = (requiredEntitlementIdentifier as String?)?.trimmingCharacters(in: .whitespacesAndNewlines)

        guard let trimmedRequiredEntitlement, !trimmedRequiredEntitlement.isEmpty else {
            proceedToPresentation()
            return
        }

        Purchases.shared.getCustomerInfo(fetchPolicy: .notStaleCachedOrFetched) { customerInfo, error in
            if let error {
                self.completeResult(result: "error", errorMessage: error.localizedDescription, callback: callback)
                return
            }

            if customerInfo?.entitlements.active.keys.contains(trimmedRequiredEntitlement) == true {
                self.completeResult(
                    result: "notPresented",
                    extra: ["requiredEntitlementIdentifier": trimmedRequiredEntitlement],
                    callback: callback)
                return
            }

            proceedToPresentation()
        }
    }

    @objc(presentCustomerCenter:)
    public func presentCustomerCenter(callback: @escaping (NSString?, NSError?) -> Void) {
        DispatchQueue.main.async {
            guard let presenter = self.topViewController() else {
                self.completeResult(
                    result: "error",
                    errorMessage: "Unable to find a view controller to present the customer center from.",
                    callback: callback)
                return
            }

            let delegate = CustomerCenterPresentationDelegate(owner: self, callback: callback)
            self.activeCustomerCenterDelegates.append(delegate)

            let controller = CustomerCenterViewController(delegate: delegate)
            presenter.present(controller, animated: true)
        }
    }

    private func loadOffering(
        offeringIdentifier: String?,
        success: @escaping (Offering?) -> Void,
        failure: @escaping (String) -> Void
    ) {
        guard let offeringIdentifier, !offeringIdentifier.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            success(nil)
            return
        }

        Purchases.shared.getOfferings { offerings, error in
            if let error {
                failure(error.localizedDescription)
                return
            }

            guard let offering = offerings?.offering(identifier: offeringIdentifier) else {
                failure("Offering '\(offeringIdentifier)' was not found.")
                return
            }

            success(offering)
        }
    }

    private func topViewController() -> UIViewController? {
        let keyWindow = UIApplication.shared.connectedScenes
            .compactMap { $0 as? UIWindowScene }
            .flatMap { $0.windows }
            .first { $0.isKeyWindow }

        guard var topController = keyWindow?.rootViewController else {
            return nil
        }

        while let presented = topController.presentedViewController {
            topController = presented
        }

        if let navigationController = topController as? UINavigationController {
            return navigationController.visibleViewController ?? navigationController
        }

        if let tabBarController = topController as? UITabBarController {
            return tabBarController.selectedViewController ?? tabBarController
        }

        return topController
    }

    fileprivate func completeResult(
        result: String,
        customerInfo: CustomerInfo? = nil,
        errorMessage: String? = nil,
        extra: [String: Any] = [:],
        callback: @escaping (NSString?, NSError?) -> Void
    ) {
        var payload = extra
        payload["result"] = result

        if let customerInfo {
            payload["customerInfo"] = customerInfo.rawData
        }

        if let errorMessage {
            payload["errorMessage"] = errorMessage
        }

        do {
            let jsonData = try JSONSerialization.data(withJSONObject: payload, options: [])
            let jsonString = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)
            callback(jsonString, nil)
        } catch let error as NSError {
            callback(nil, error)
        }
    }

    fileprivate func release(paywallDelegate: PaywallPresentationDelegate) {
        activePaywallDelegates.removeAll { $0 === paywallDelegate }
    }

    fileprivate func release(customerCenterDelegate: CustomerCenterPresentationDelegate) {
        activeCustomerCenterDelegates.removeAll { $0 === customerCenterDelegate }
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
	
	@objc(setDisplayName:)
	public func setDisplayName(displayName: NSString) {
		Purchases.shared.attribution.setDisplayName(displayName as String)
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
}

private final class PaywallPresentationDelegate: NSObject, PaywallViewControllerDelegate {
    private weak var owner: RevenueCatManager?
    private let callback: (NSString?, NSError?) -> Void
    private var completed = false

    init(owner: RevenueCatManager, callback: @escaping (NSString?, NSError?) -> Void) {
        self.owner = owner
        self.callback = callback
        super.init()
    }

    private func finish(
        controller: PaywallViewController,
        result: String,
        customerInfo: CustomerInfo? = nil,
        errorMessage: String? = nil
    ) {
        guard !completed else {
            return
        }

        completed = true
        owner?.completeResult(result: result, customerInfo: customerInfo, errorMessage: errorMessage, callback: callback)
        owner?.release(paywallDelegate: self)

        if controller.presentingViewController != nil {
            controller.dismiss(animated: true)
        }
    }

    func paywallViewController(_ controller: PaywallViewController, didFinishPurchasingWith customerInfo: CustomerInfo, transaction: StoreTransaction?) {
        finish(controller: controller, result: "purchased", customerInfo: customerInfo)
    }

    func paywallViewControllerDidCancelPurchase(_ controller: PaywallViewController) {
        finish(controller: controller, result: "cancelled")
    }

    func paywallViewController(_ controller: PaywallViewController, didFailPurchasingWith error: NSError) {
        finish(controller: controller, result: "error", errorMessage: error.localizedDescription)
    }

    func paywallViewController(_ controller: PaywallViewController, didFinishRestoringWith customerInfo: CustomerInfo) {
        finish(controller: controller, result: "restored", customerInfo: customerInfo)
    }

    func paywallViewController(_ controller: PaywallViewController, didFailRestoringWith error: NSError) {
        finish(controller: controller, result: "error", errorMessage: error.localizedDescription)
    }

    func paywallViewControllerWasDismissed(_ controller: PaywallViewController) {
        finish(controller: controller, result: "cancelled")
    }
}

private final class CustomerCenterPresentationDelegate: NSObject, CustomerCenterViewControllerDelegate {
    private weak var owner: RevenueCatManager?
    private let callback: (NSString?, NSError?) -> Void
    private var completed = false
    private var latestAction: String?
    private var latestActionIdentifier: String?
    private var latestErrorMessage: String?
    private var latestCustomerInfo: CustomerInfo?

    init(owner: RevenueCatManager, callback: @escaping (NSString?, NSError?) -> Void) {
        self.owner = owner
        self.callback = callback
        super.init()
    }

    private func finish(with result: String) {
        guard !completed else {
            return
        }

        completed = true

        var extra: [String: Any] = [:]
        if let latestAction {
            extra["action"] = latestAction
        }
        if let latestActionIdentifier {
            extra["actionIdentifier"] = latestActionIdentifier
        }

        owner?.completeResult(
            result: result,
            customerInfo: latestCustomerInfo,
            errorMessage: latestErrorMessage,
            extra: extra,
            callback: callback)
        owner?.release(customerCenterDelegate: self)
    }

    func customerCenterViewControllerDidStartRestore(_ controller: CustomerCenterViewController) {
        latestAction = "restoreStarted"
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didFinishRestoringWith customerInfo: CustomerInfo) {
        latestAction = "restored"
        latestCustomerInfo = customerInfo
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didFailRestoringWith error: NSError) {
        latestAction = "restoreFailed"
        latestErrorMessage = error.localizedDescription
    }

    func customerCenterViewControllerDidShowManageSubscriptions(_ controller: CustomerCenterViewController) {
        latestAction = "showedManageSubscriptions"
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didCompleteFeedbackSurveyWith optionId: String) {
        latestAction = "feedbackSurveyCompleted"
        latestActionIdentifier = optionId
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didSelectChangePlansWith optionId: String) {
        latestAction = "changePlansSelected"
        latestActionIdentifier = optionId
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didSelectCustomActionWith actionIdentifier: String, purchaseIdentifier: String?) {
        latestAction = "customActionSelected"
        latestActionIdentifier = actionIdentifier
    }

    func customerCenterViewController(_ controller: CustomerCenterViewController, didSucceedWithPromotionalOffer offerId: String, customerInfo: CustomerInfo, transaction: StoreTransaction) {
        latestAction = "promotionalOfferSucceeded"
        latestActionIdentifier = offerId
        latestCustomerInfo = customerInfo
    }

    func customerCenterViewControllerWasDismissed(_ controller: CustomerCenterViewController) {
        finish(with: "dismissed")
    }
}
