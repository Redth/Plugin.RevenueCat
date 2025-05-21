#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;
using System.Text.Json.Serialization;

public partial class StoreProduct
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }
    
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
    
    [JsonPropertyName("subscription_period")]
    public SubscriptionPeriod? SubscriptionPeriod { get; set; }
}

#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603