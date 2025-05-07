using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class RevenueCatOptionsBuilder
{
	readonly Dictionary<string, string> apiKeys = new();

	public string? AndroidApiKey
	{
		get => apiKeys["android"];
		set => apiKeys["android"] = value;
	}
	public RevenueCatOptionsBuilder WithAndroidApiKey(string apiKey)
	{
		apiKeys["android"] = apiKey;
		return this;
	}
	
	public string? iOSApiKey
	{
		get => apiKeys["ios"];
		set => apiKeys["ios"] = value;
	}
	public RevenueCatOptionsBuilder WithiOSApiKey(string apiKey)
	{
		apiKeys["ios"] = apiKey;
		return this;
	}
	
	public string? MacCatalystApiKey
	{
		get => apiKeys["maccatalyst"];
		set => apiKeys["maccatalyst"] = value;
	}
	public RevenueCatOptionsBuilder WithMacCatalystApiKey(string apiKey)
	{
		apiKeys["maccatalyst"] = apiKey;
		return this;
	}
	
	public string? AppleApiKey
	{
		get => apiKeys["ios"];
		set => apiKeys["maccatalsyt"] = apiKeys["ios"] = value;
	}
	public RevenueCatOptionsBuilder WithAppleApiKey(string apiKey)
		=> WithiOSApiKey(apiKey).WithMacCatalystApiKey(apiKey);

	public bool Debug { get; set; }
	public RevenueCatOptionsBuilder WithDebug(bool debug)
	{
		Debug = debug;
		return this;
	}
	
	public string? UserId { get; set; }
	public RevenueCatOptionsBuilder WithUserId(string? userId)
	{
		UserId = userId;
		return this;
	}
	
	public string? AppStore { get; set; }
	public RevenueCatOptionsBuilder WithAppStore(string? appStore)
	{
		AppStore = appStore;
		return this;
	}
	
	public Action<CustomerInfoRequest>? CustomerInfoUpdatedCallback { get; set; }
	public RevenueCatOptionsBuilder WithCallback(Action<CustomerInfoRequest>? callback)
	{
		CustomerInfoUpdatedCallback = callback;
		return this;
	}

	public RevenueCatOptions Build()
		=> new (
			AndroidApiKey,
			iOSApiKey,
			MacCatalystApiKey,
			Debug,
			UserId,
			AppStore,
			CustomerInfoUpdatedCallback);
}