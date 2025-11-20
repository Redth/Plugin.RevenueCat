using Plugin.RevenueCat.Api.V2;

namespace Plugin.RevenueCat.Api;

public interface IRevenueCatApiV2
{
	Task<Customer> GetCustomer(string project_id, string customer_id);

	Task SetCustomerAttributes(string project_id, string customer_id, IEnumerable<CustomerAttribute> attributes);

	Task<PagedList<Offering>> GetOfferings(string customer_id);
}
