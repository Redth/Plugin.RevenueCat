using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public record RevenueCatOptions(
	string? AndroidApiKey,
	string? iOSApiKey,
	string? MacCatalystApiKey,
	bool? Debug,
	string? UserId,
	string? AppStore,
	Action<CustomerInfoRequest>? CustomerInfoUpdatedCallback);