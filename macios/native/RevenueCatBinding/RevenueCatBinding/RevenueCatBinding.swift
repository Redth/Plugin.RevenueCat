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

    @objc(initialize:apiKey:userId:)
    public func initialize(debugLog: Bool, apiKey: String, userId: String?) {
        if (debugLog){
            Purchases.logLevel = .debug
        }
        Purchases.configure(withAPIKey: apiKey, appUserID: userId)
    }
    
    
    @objc(login:err:)
    public func login(userId: String) async throws -> String {
        
        return try await withCheckedThrowingContinuation { continuation in
            Purchases.shared.logIn(userId) { (customerInfo, created, error) in
                do {
                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
                    continuation.resume(returning: jsonString)
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }
    
    
    @objc(getCustomerInfo:err:)
    public func getCustomerInfo(force: Bool) async throws -> String {
        
        let fetchPolicy = force ? CacheFetchPolicy.fetchCurrent : CacheFetchPolicy.notStaleCachedOrFetched

        return try await withCheckedThrowingContinuation { continuation in
            Purchases.shared.getCustomerInfo(fetchPolicy: fetchPolicy) { (customerInfo, error) in
                do {
                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
                    continuation.resume(returning: jsonString)
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }
    
    
    var customerInfoChangedHandler: ((String) -> Void)?

    @objc(setCustomerInfoChangedHandler:)
    public func setCustomerInfoChangedHandler(callback: @escaping (String) -> Void) -> Void {
        customerInfoChangedHandler = callback
    }
    
    @objc(restore:)
    public func restore() async throws -> String {
        return try await withCheckedThrowingContinuation { continuation in
            Purchases.shared.restorePurchases() { (customerInfo, error) in
                do {
                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
                    continuation.resume(returning: jsonString)
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }
    
    @objc(purchase:err:)
    public func restore(storeProduct: Any) async throws -> String {
        return try await withCheckedThrowingContinuation { continuation in
            Purchases.shared.purchase(product: storeProduct as! StoreProduct) { (transaction, customerInfo, error, ok) in
                do {
                    let jsonString = try self.processCustomerInfo(customerInfo: customerInfo, error: error)
                    let transactionJsonString = try self.processTransaction(transaction: transaction, error: error)
                    continuation.resume(returning: transactionJsonString)
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }
    
    private func processCustomerInfo(customerInfo: CustomerInfo?, error: Error?) throws -> String {
        if let error = error {
            throw error
        }
        
        guard let rawData = customerInfo?.rawData else {
            throw NSError(domain: "InvalidDataError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Customer info is nil or invalid"])
        }
        
        guard let jsonString = anyToJson(rawData: rawData) else {
            throw NSError(domain: "SerializationError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to convert raw data to JSON"])
        }
        
        if (customerInfoChangedHandler != nil) {
            customerInfoChangedHandler!(jsonString)
        }
        return jsonString
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
