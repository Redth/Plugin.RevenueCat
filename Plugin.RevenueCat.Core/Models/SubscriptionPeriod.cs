using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

#pragma warning disable CS8618, CS8601, CS8603
public partial class SubscriptionPeriod
{
    [JsonPropertyName("value")]
    public int Value { get; set; } = 1;
    
    [JsonPropertyName("unit")]
    [JsonConverter(typeof(JsonStringEnumConverter<SubscriptionPeriodUnit>))]
    public SubscriptionPeriodUnit Unit { get; set; } = SubscriptionPeriodUnit.Unknown;
}

#pragma warning restore CS8618, CS8601, CS8603