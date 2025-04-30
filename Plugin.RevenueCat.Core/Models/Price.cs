#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;
using System.Text.Json.Serialization;

public partial class Price
{
	[JsonPropertyName("amount")]
	public double Amount { get; set; }

	[JsonPropertyName("currency")]
	public string Currency { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
