using Plugin.RevenueCat.Api.V2;

namespace Plugin.RevenueCat.Api;

/// <summary>
/// RevenueCat REST API V2 client for customer, offerings, and subscription operations.
/// </summary>
/// <remarks>
/// See <see href="https://www.revenuecat.com/docs/api-v2">RevenueCat API V2 Documentation</see> for more details.
/// V2 API requires a project ID for most operations and provides more detailed subscription information.
/// </remarks>
public interface IRevenueCatApiV2
{
	/// <summary>
	/// Gets a customer by their app user ID.
	/// </summary>
	/// <param name="project_id">The RevenueCat project ID.</param>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <param name="expandAttributes">If <c>true</c>, includes customer attributes in the response. Default is <c>true</c>.</param>
	/// <returns>The customer details including active entitlements and optionally attributes.</returns>
	Task<Customer> GetCustomer(string project_id, string customer_id, bool expandAttributes = true);

	/// <summary>
	/// Sets custom attributes on a customer.
	/// </summary>
	/// <param name="project_id">The RevenueCat project ID.</param>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <param name="attributes">The attributes to set on the customer.</param>
	/// <remarks>
	/// Requires an API key with <c>customer_information:customers:read_write</c> permission.
	/// </remarks>
	Task SetCustomerAttributes(string project_id, string customer_id, IEnumerable<CustomerAttribute> attributes);

	/// <summary>
	/// Gets the available offerings for a customer.
	/// </summary>
	/// <param name="project_id">The RevenueCat project ID.</param>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <returns>A paged list of offerings available to the customer.</returns>
	Task<PagedList<Offering>> GetOfferings(string project_id, string customer_id);

	/// <summary>
	/// Gets the subscriptions for a customer.
	/// </summary>
	/// <param name="project_id">The RevenueCat project ID.</param>
	/// <param name="customer_id">The app user ID of the customer.</param>
	/// <returns>
	/// A paged list of subscriptions. Each subscription includes a <see cref="Subscription.ManagementUrl"/>
	/// property that may contain a URL where the customer can manage that specific subscription.
	/// The management URL may be <c>null</c> if the store doesn't provide one.
	/// </returns>
	Task<PagedList<Subscription>> GetSubscriptions(string project_id, string customer_id);
}
