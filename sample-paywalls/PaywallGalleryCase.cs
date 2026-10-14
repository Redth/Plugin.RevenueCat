using System.Text.Json;
using System.Text.Json.Serialization;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;

namespace PaywallGallerySample;

// Gallery-only metadata; never part of the RevenueCat wire contract.
public sealed class PaywallGalleryCase
{
	public List<string> Sources { get; set; } = [];
	public List<Package> Packages { get; set; } = [];
	public List<PaywallGalleryVariant> Variants { get; set; } = [];

	public static PaywallGalleryCase? Read(PaywallOfferingsResponse response)
	{
		if (response.ExtensionData?.TryGetValue("gallery", out var metadata) != true)
		{
			return null;
		}
		var result = metadata.Deserialize(GallerySerializerContext.Default.PaywallGalleryCase)
			?? throw new JsonException("Gallery metadata must not be null.");
		if (result.Sources.Count == 0 || result.Variants.Count == 0 ||
			result.Variants.Any(v => string.IsNullOrWhiteSpace(v.Name) || string.IsNullOrWhiteSpace(v.ExpectedOutcome)))
		{
			throw new JsonException("Source-backed gallery cases require sources, named variants and expected outcomes.");
		}
		return result;
	}
}

public sealed class PaywallGalleryVariant
{
	public string Name { get; set; } = "";
	public string ExpectedOutcome { get; set; } = "";
	public string Locale { get; set; } = "en_US";
	public Dictionary<string, PaywallEligibility> IntroEligibility { get; set; } = new(StringComparer.Ordinal);
	public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
	public List<string>? AvailablePackages { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public int? CountdownSecondsFromNow { get; set; }
	public Dictionary<string, string> ExpectedText { get; set; } = new(StringComparer.Ordinal);
	public Dictionary<string, bool> ExpectedVisible { get; set; } = new(StringComparer.Ordinal);
	public string? ExpectedPurchasePackage { get; set; }

	public PaywallSemanticContext CreateContext() => new()
	{
		IntroOfferEligibility = IntroEligibility.ToDictionary(p => new PaywallOfferKey(p.Key), p => p.Value),
		CustomVariables = Variables,
		Width = Width,
		Height = Height
	};

	public IReadOnlyList<Package> SelectPackages(IReadOnlyList<Package> catalog) =>
		AvailablePackages is null ? catalog :
			catalog.Where(p => AvailablePackages.Contains(p.Identifier, StringComparer.Ordinal)).ToArray();

	public PaywallComponentsData? CreateTimedPaywall(PaywallComponentsData? data, DateTimeOffset now)
	{
		if (CountdownSecondsFromNow is not { } seconds || data is null)
		{
			return null;
		}
		var clone = JsonSerializer.Deserialize(
			JsonSerializer.Serialize(data, ModelSerializerContext.Default.PaywallComponentsData),
			ModelSerializerContext.Default.PaywallComponentsData)!;
		var countdown = clone.ComponentsConfig?.Base?.Stack?.Components.OfType<PaywallCountdownComponent>().SingleOrDefault()
			?? throw new InvalidOperationException("Timed gallery variants require one top-level countdown.");
		countdown.Style = JsonSerializer.SerializeToElement(
			new Dictionary<string, string> { ["type"] = "date", ["date"] = now.AddSeconds(seconds).ToString("O") },
			ModelSerializerContext.Default.DictionaryStringString);
		return clone;
	}
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, UseStringEnumConverter = true)]
[JsonSerializable(typeof(PaywallGalleryCase))]
internal partial class GallerySerializerContext : JsonSerializerContext;
