#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

public partial class Offering
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("packages")]
	public List<Package> Packages { get; set; } = new();
}

public partial class Package
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("localized_introductory_price_string")]
	public string LocalizedIntroductoryPriceString { get; set; }

	[JsonPropertyName("localized_price_string")]
	public string LocalizedPriceString { get; set; }

	[JsonPropertyName("offering_identifier")]
	public string OfferingIdentifier { get; set; }

	[JsonPropertyName("package_type")]
	public string PackageType { get; set; }
}

public partial class CustomerInfoRequest
{
	[JsonPropertyName("request_date")]
	public DateTimeOffset RequestDate { get; set; }

	[JsonPropertyName("request_date_ms")]
	public long? RequestDateMs { get; set; }

	[JsonPropertyName("subscriber")]
	public Subscriber Subscriber { get; set; }

	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; set; }

	[JsonPropertyName("verification_result")]
	public string VerificationResult { get; set; }

	[JsonPropertyName("customer_info_request_date")]
	public DateTimeOffset? CustomerInfoRequestDate { get; set; }
}

public partial class Subscriber
{
	[JsonPropertyName("entitlements")]
	public IDictionary<string, Entitlement> Entitlements { get; set; } = new Dictionary<string, Entitlement>();

	[JsonPropertyName("first_seen")]
	public DateTimeOffset FirstSeen { get; set; }

	[JsonPropertyName("last_seen")]
	public DateTimeOffset LastSeen { get; set; }

	[JsonPropertyName("management_url")]
	public string ManagementUrl { get; set; }

	[JsonPropertyName("non_subscriptions")]
	public NonSubscriptions NonSubscriptions { get; set; }

	[JsonPropertyName("original_app_user_id")]
	public string OriginalAppUserId { get; set; }

	[JsonPropertyName("original_application_version")]
	public string OriginalApplicationVersion { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset OriginalPurchaseDate { get; set; }

	[JsonPropertyName("other_purchases")]
	public NonSubscriptions OtherPurchases { get; set; }

	[JsonPropertyName("subscriptions")]
	public IDictionary<string, Subscription> Subscriptions { get; set; } = new Dictionary<string, Subscription>();
}

public partial class Entitlement
{
	[JsonPropertyName("expires_date")]
	public DateTimeOffset ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("product_identifier")]
	public string ProductIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset PurchaseDate { get; set; }
}

public partial class NonSubscriptions
{
}


public partial class Subscription
{
	[JsonPropertyName("auto_resume_date")]
	public DateTimeOffset? AutoResumeDate { get; set; }

	[JsonPropertyName("billing_issues_detected_at")]
	public DateTimeOffset? BillingIssuesDetectedAt { get; set; }

	[JsonPropertyName("display_name")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("expires_date")]
	public DateTimeOffset ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("is_sandbox")]
	public bool IsSandbox { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset OriginalPurchaseDate { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("ownership_type")]
	public string OwnershipType { get; set; }

	[JsonPropertyName("period_type")]
	public string PeriodType { get; set; }

	[JsonPropertyName("price")]
	public Price Price { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("product_plan_identifier")]
	public string ProductPlanIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset PurchaseDate { get; set; }

	[JsonPropertyName("refunded_at")]
	public DateTimeOffset? RefundedAt { get; set; }

	[JsonPropertyName("store")]
	public string Store { get; set; }

	[JsonPropertyName("store_transaction_id")]
	public string StoreTransactionId { get; set; }

	[JsonPropertyName("unsubscribe_detected_at")]
	public DateTimeOffset? UnsubscribeDetectedAt { get; set; }
}

public partial class Price
{
	[JsonPropertyName("amount")]
	public double Amount { get; set; }

	[JsonPropertyName("currency")]
	public string Currency { get; set; }
}

public partial class CustomerInfoRequest
{
	public static CustomerInfoRequest FromJson(string json) => JsonSerializer.Deserialize<CustomerInfoRequest>(json, ModelExtensions.Settings);
}

public partial class Offering
{
	public static Offering FromJson(string json) => JsonSerializer.Deserialize<Offering>(json, ModelExtensions.Settings);
}

public static class ModelExtensions
{
	public static string ToJson(this CustomerInfoRequest self) => JsonSerializer.Serialize(self, Settings);

	public static string ToJson(this Offering self) => JsonSerializer.Serialize(self, Settings);

	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		Converters =
		{
			new DateOnlyConverter(),
			new TimeOnlyConverter(),
			IsoDateTimeOffsetConverter.Singleton
		},
	};
}

public class DateOnlyConverter : JsonConverter<DateOnly>
{
	private readonly string serializationFormat;
	public DateOnlyConverter() : this(null) { }

	public DateOnlyConverter(string? serializationFormat)
	{
		this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
	}

	public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return DateOnly.Parse(value!);
	}

	public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(serializationFormat));
}

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
	private readonly string serializationFormat;

	public TimeOnlyConverter() : this(null) { }

	public TimeOnlyConverter(string? serializationFormat)
	{
		this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
	}

	public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return TimeOnly.Parse(value!);
	}

	public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(serializationFormat));
}

internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

	private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
	private string? _dateTimeFormat;
	private CultureInfo? _culture;

	public DateTimeStyles DateTimeStyles
	{
		get => _dateTimeStyles;
		set => _dateTimeStyles = value;
	}

	public string? DateTimeFormat
	{
		get => _dateTimeFormat ?? string.Empty;
		set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
	}

	public CultureInfo Culture
	{
		get => _culture ?? CultureInfo.CurrentCulture;
		set => _culture = value;
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		string text;


		if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
				|| (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
		{
			value = value.ToUniversalTime();
		}

		text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

		writer.WriteStringValue(text);
	}

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? dateText = reader.GetString();

		if (string.IsNullOrEmpty(dateText) == false)
		{
			if (!string.IsNullOrEmpty(_dateTimeFormat))
			{
				return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
			}
			else
			{
				return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
			}
		}
		else
		{
			return default(DateTimeOffset);
		}
	}


	public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
