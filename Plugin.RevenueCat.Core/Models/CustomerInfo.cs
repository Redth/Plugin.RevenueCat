#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;

using System.Text.Json;
using System.Text.Json.Serialization;

public partial class CustomerInfo
{
	[JsonPropertyName("request_date")]
	public DateTimeOffset RequestDate { get; set; }

	[JsonPropertyName("request_date_ms")]
	public long? RequestDateMs { get; set; }

	[JsonPropertyName("subscriber")]
	public Subscriber Subscriber { get; set; }

	[JsonPropertyName("schema_version")]
	public int? SchemaVersion { get; set; }

	[JsonPropertyName("verification_result")]
	public string VerificationResult { get; set; }

	[JsonPropertyName("customer_info_request_date")]
	public DateTimeOffset? CustomerInfoRequestDate { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
