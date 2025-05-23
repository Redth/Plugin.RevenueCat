//
//  DotnetNewBinding.swift
//  NewBinding
//
//  Created by .NET MAUI team on 6/18/24.
//

import Foundation
import UIKit
import RevenueCat

@objc(RevenueCatManager)
public class RevenueCatManager : NSObject
{

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
}
