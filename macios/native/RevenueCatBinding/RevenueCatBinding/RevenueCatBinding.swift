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
    public func login(userId: NSString, callback: @escaping (NSData?, NSError?) -> Void) {
        Purchases.shared.logIn(userId as String) { customerInfo, created, error in
            if (error != nil)
            {
                callback(nil, error)
                return
            }
            
            do {
                let jsonData = try JSONSerialization.data(withJSONObject: customerInfo!.rawData, options: [])
                
                callback(NSData(data: jsonData), nil)
                
                //let jsonString = try self.processCustomerInfo(customerInfo: customerInfo)
                //callback(jsonString, nil)
            } catch let error as NSError {
                callback(nil, error)
            }
        }
    }
    
    
    @objc(getCustomerInfo:callback:)
    public func getCustomerInfo(force: Bool, callback: @escaping (NSString?, NSError?) -> Void) {
        let fetchPolicy = force ? CacheFetchPolicy.fetchCurrent : CacheFetchPolicy.notStaleCachedOrFetched

        Purchases.shared.getCustomerInfo(fetchPolicy: fetchPolicy) { customerInfo, error in
            if (error != nil)
            {
                callback(nil, error)
                return
            }
            
            do {
                let jsonString = try self.processCustomerInfo(customerInfo: customerInfo)
                callback(jsonString, nil)
            } catch let error as NSError {
                callback(nil, error)
            }
        }
    }
    
    
    var customerInfoChangedHandler: ((NSString) -> Void)?

    @objc(setCustomerInfoChangedHandler:)
    public func setCustomerInfoChangedHandler(callback: @escaping (NSString) -> Void) -> Void {
        customerInfoChangedHandler = callback
    }
    
//    @objc(restore:)
//    public func restore(callback: @escaping (String?, NSError?) -> Void) {
//        Purchases.shared.restorePurchases() { (customerInfo, error) in
//                do {
//                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
//                    continuation.resume(returning: jsonString)
//                } catch {
//                    continuation.resume(throwing: error)
//                }
//            }
//        }
//    }
//    
//    @objc(purchase:err:)
//    public func restore(storeProduct: Any) async throws -> String {
//        return try await withCheckedThrowingContinuation { continuation in
//            Purchases.shared.purchase(product: storeProduct as! StoreProduct) { (transaction, customerInfo, error, ok) in
//                do {
//                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
//                    let transactionJsonString = try self.processTransaction(transaction: transaction, error: error)
//                    continuation.resume(returning: transactionJsonString)
//                } catch {
//                    continuation.resume(throwing: error)
//                }
//            }
//        }
//    }
    
    private func processCustomerInfo(customerInfo: CustomerInfo?) throws -> NSString? {
       
        guard let rawData = customerInfo?.rawData else {
            throw NSError(domain: "InvalidDataError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Customer info is nil or invalid"])
        }
        var jsonString: NSString?
        
        do {
            // Convert dictionary to JSON data
            let jsonData = try JSONSerialization.data(withJSONObject: rawData, options: [])
            
            // Convert JSON data to a JSON string
            let jsonDataString = NSString(data: jsonData, encoding: NSUTF8StringEncoding)
            
            let cleanJsonString = NSString(string: jsonDataString!)
            
            if (self.customerInfoChangedHandler != nil) {
                self.customerInfoChangedHandler!(cleanJsonString)
            }
            
            return cleanJsonString
        } catch {
            return nil
        }
        
        return nil
    }
    
    private func processTransaction(transaction: StoreTransaction?, error: Error?) throws -> String {
        if let error = error {
            throw error
        }
        
        guard let rawData = transaction else {
            throw NSError(domain: "InvalidDataError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Customer info is nil or invalid"])
        }
        
        guard let jsonString = anyToJson(rawData: rawData) else {
            throw NSError(domain: "SerializationError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to convert raw data to JSON"])
        }
        
        return jsonString
    }
    
    func anyToJson(rawData: Any) -> String? {
        if let data = rawData as? String {
            // If it's already a string
            return data
        } else if let data = rawData as? Data {
            // If it's raw Data
            if let jsonString = String(data: data, encoding: .utf8) {
                return jsonString
            } else {
                return nil
            }
        } else if let jsonObject = rawData as? [String: Any] {
            // If it's a Dictionary
            do {
                let jsonData = try JSONSerialization.data(withJSONObject: jsonObject, options: .prettyPrinted)
                if let jsonString = String(data: jsonData, encoding: .utf8) {
                    return jsonString
                }
            } catch {
                return nil
            }
        } else if let jsonArray = rawData as? [Any] {
            // If it's an Array
            do {
                let jsonData = try JSONSerialization.data(withJSONObject: jsonArray, options: .prettyPrinted)
                if let jsonString = String(data: jsonData, encoding: .utf8) {
                    return jsonString
                }
            } catch {
                return nil
            }
        } else {
            return nil
        }
        return nil
    }
}
