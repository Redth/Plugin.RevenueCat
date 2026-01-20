using Plugin.RevenueCat.Api.V1;

namespace Plugin.RevenueCat.Api;

/// <summary>
/// RevenueCat REST API V1 client for subscriber and offerings operations.
/// </summary>
/// <remarks>
/// See <see href="https://www.revenuecat.com/docs/api-v1">RevenueCat API V1 Documentation</see> for more details.
/// </remarks>
public interface IRevenueCatApiV1
{
	/// <summary>
	/// Gets or creates a customer (subscriber) by their app user ID.
	/// </summary>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <returns>The subscriber response containing customer details, entitlements, and subscriptions.</returns>
	Task<SubscriberResponse> GetOrCreateCustomer(string customer_id);

	/// <summary>
	/// Gets the available offerings for a customer.
	/// </summary>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <returns>The offerings response containing available offerings and the current offering ID.</returns>
	Task<OfferingsResponse> GetOfferings(string customer_id);

	/// <summary>
	/// Gets the subscription management URL for a customer.
	/// </summary>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <returns>
	/// The URL where the customer can manage their subscription (e.g., App Store or Play Store subscription settings),
	/// or <c>null</c> if no management URL is available. A management URL may not be available if the customer
	/// has no active subscriptions or if the subscription was purchased through a store that doesn't provide
	/// management URLs.
	/// </returns>
	Task<string?> GetManagementUrl(string customer_id);
}
