using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class InstallmentsInfo
{
	[JsonPropertyName("commitment_payments_count")]
	public int? CommitmentPaymentsCount { get; set; }

	[JsonPropertyName("renewal_commitment_payments_count")]
	public int? RenewalCommitmentPaymentsCount { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
