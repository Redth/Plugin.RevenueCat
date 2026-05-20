#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class StoreProduct
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("is_family_shareable")]
    public bool IsFamilyShareable { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("price_string")]
    public string PriceString { get; set; }
    
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("product_type")]
    public string? ProductType { get; set; }

    [JsonPropertyName("product_category")]
    public string? ProductCategory { get; set; }

    [JsonPropertyName("base_plan_id")]
    public string? BasePlanId { get; set; }

    [JsonPropertyName("price")]
    public Price? Price { get; set; }
    
    [JsonPropertyName("subscription_period")]
    public SubscriptionPeriod? SubscriptionPeriod { get; set; }

    [JsonPropertyName("introductory_discount")]
    public StoreProductDiscount? IntroductoryDiscount { get; set; }

    [JsonPropertyName("discounts")]
    public List<StoreProductDiscount> Discounts { get; set; } = new();

    [JsonPropertyName("subscription_options")]
    public List<SubscriptionOption> SubscriptionOptions { get; set; } = new();

    [JsonPropertyName("default_subscription_option")]
    public SubscriptionOption? DefaultSubscriptionOption { get; set; }

    [JsonPropertyName("presented_offering_identifier")]
    public string? PresentedOfferingIdentifier { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}

#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
