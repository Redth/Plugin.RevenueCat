using Plugin.RevenueCat.Api.V2;

namespace Plugin.RevenueCat.Api;

public interface IRevenueCatApiV2
{
	Task<Customer> GetCustomer(string project_id, string customer_id, bool expandAttributes = true);

	Task SetCustomerAttributes(string project_id, string customer_id, IEnumerable<CustomerAttribute> attributes);

	Task<PagedList<Offering>> GetOfferings(string project_id, string customer_id);

	Task<PagedList<Subscription>> GetSubscriptions(string project_id, string customer_id);
}
