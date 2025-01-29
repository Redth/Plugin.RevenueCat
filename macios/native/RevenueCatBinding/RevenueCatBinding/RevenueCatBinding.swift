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

    @objc(getOffering:callback:)
    public func getOffering(offeringId: String, callback: @escaping (NSString?, NSError?) -> Void) {
        
        Purchases.shared.getOfferings { (offerings, error) in
            
            if let currentOffering = offerings?.offering(identifier: offeringId) {
                
                var offeringInfo = [String: Any]()
                offeringInfo["id"] = currentOffering.id
                offeringInfo["identifier"] = currentOffering.identifier
                
                for md in currentOffering.metadata {
                    offeringInfo[md.key] = md.value
                }
                
                var packageInfos = [Any]()
                
                let apkgs = currentOffering.availablePackages
                
                for pkg in apkgs {
                    var packageInfo: [String: Any] = [:]
                    
                    packageInfo["id"] = pkg.id
                    packageInfo["identifier"] = pkg.identifier
                    packageInfo["localized_introductory_price_string"] = pkg.localizedIntroductoryPriceString
                    packageInfo["localized_price_string"] = pkg.localizedPriceString
                    packageInfo["offering_identifier"] = pkg.offeringIdentifier
                    
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
                    packageInfo["package_type"] = packageTypeMapping[pkg.packageType] ?? "unknown"

                    let storeProduct = pkg.storeProduct
                    var productInfo: [String: Any] = [:]
                    productInfo["identifier"] = storeProduct.productIdentifier
                    productInfo["localized_title"] = storeProduct.localizedTitle
                    productInfo["localized_description"] = storeProduct.localizedDescription
                    productInfo["localized_price_string"] = storeProduct.localizedPriceString
                    productInfo["localized_price_per_day"] = storeProduct.localizedPricePerDay
                    productInfo["localized_price_per_week"] = storeProduct.localizedPricePerWeek
                    productInfo["localized_price_per_month"] = storeProduct.localizedPricePerMonth
                    productInfo["localized_price_per_year"] = storeProduct.localizedPricePerYear
                    productInfo["currency_code"] = storeProduct.currencyCode
                    productInfo["is_family_shareable"] = storeProduct.isFamilyShareable
                    
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
                        self.processStoreTransaction(storeTransaction: storeTransaction, originalError: error, callback: callback)
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
            let jsonData = try JSONSerialization.data(withJSONObject: storeTransaction!, options: [])
            let jsonStr = NSString(data: jsonData, encoding: String.Encoding.utf8.rawValue)
            
            callback(jsonStr, nil)
            return
        } catch let error as NSError {
            callback(nil, error)
            return
        }
    }
}
