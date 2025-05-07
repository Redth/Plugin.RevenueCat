using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class RevenueCatOptionsBuilder
{
	public const string AndroidPlatform = "android";
	public const string iOSPlatform = "ios";
	public const string MacCatalystPlatform = "maccatalyst";
	public const string AmazonPlatform = "amazon";
	
	public static RevenueCatOptionsBuilder Create()
		=> new ();
	
	readonly Dictionary<string, string> apiKeys = new();

	public string? GetApiKey(string platform)
		=> apiKeys.GetValueOrDefault(platform);

	public void SetApiKey(string platform, string? apiKey)
	{
		if (string.IsNullOrEmpty(apiKey))
		{
			apiKeys.Remove(platform);
		}
		else
		{
			apiKeys[platform] = apiKey;
		}
	}
	
	public string? AmazonApiKey
	{
		get => GetApiKey(AmazonPlatform);
		set => SetApiKey(AmazonPlatform, value);
	}
	public RevenueCatOptionsBuilder WithAmazonApiKey(string? apiKey)
	{
		SetApiKey(AmazonPlatform, apiKey);
		return this;
	}
	
	public string? AndroidApiKey
	{
		get => GetApiKey(AndroidPlatform);
		set => SetApiKey(AndroidPlatform, value);
	}
	public RevenueCatOptionsBuilder WithAndroidApiKey(string apiKey)
	{
		SetApiKey(AndroidPlatform, apiKey);
		return this;
	}
	
	public string? iOSApiKey
	{
		get => GetApiKey(iOSPlatform);
		set => SetApiKey(iOSPlatform, value);
	}
	public RevenueCatOptionsBuilder WithiOSApiKey(string apiKey)
	{
		SetApiKey(iOSPlatform, apiKey);
		return this;
	}
	
	public string? MacCatalystApiKey
	{
		get => GetApiKey(MacCatalystPlatform);
		set => SetApiKey(MacCatalystPlatform, value);
	}
	public RevenueCatOptionsBuilder WithMacCatalystApiKey(string apiKey)
	{
		SetApiKey(MacCatalystPlatform, apiKey);
		return this;
	}
	
	public string? AppleApiKey
	{
		get => GetApiKey(iOSPlatform);
		set
		{
			SetApiKey(iOSPlatform, value);
			SetApiKey(MacCatalystPlatform, value);
		}
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
			AmazonApiKey,
			iOSApiKey,
			MacCatalystApiKey,
			Debug,
			UserId,
			AppStore,
			CustomerInfoUpdatedCallback);
}