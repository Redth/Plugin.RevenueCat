using Plugin.RevenueCat.Api.V2;
using Refit;

namespace Plugin.RevenueCat.Api;

[Headers("Authorization: Bearer")]
public interface IRevenueCatApiV2
{
	[Get("/projects/{project_id}/customers/{customer_id}")]
	Task<Customer> GetCustomer(string project_id, string customer_id);

	[Post("/projects/{project_id}/customers/{customer_id}/attributes")]
	Task SetCustomerAttributes(string project_id, string customer_id, [Body] IEnumerable<CustomerAttribute> attributes);

	[Get("/subscribers/{customer_id}")]
	Task<PagedList<Offering>> GetOfferings(string customer_id);
}
