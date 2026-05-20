using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RevenueCatProductType>))]
public enum RevenueCatProductType
{
	Unknown,
	Subscription,
	InApp,
	Consumable,
	NonConsumable,
	NonRenewingSubscription,
	AutoRenewableSubscription
}
