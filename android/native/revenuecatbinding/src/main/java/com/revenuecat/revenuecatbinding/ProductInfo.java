package com.revenuecat.revenuecatbinding;

import com.revenuecat.purchases.models.StoreProduct;

public class ProductInfo {

    Object storeProduct;

    public Object getRevenueCatProduct()
    {
        return storeProduct;
    }

    public void setRevenueCatProduct(Object storeProduct)
    {
        this.storeProduct = (StoreProduct) storeProduct;
    }
}
