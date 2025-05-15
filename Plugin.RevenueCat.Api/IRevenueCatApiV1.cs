using Plugin.RevenueCat.Api.V1;
using Refit;

namespace Plugin.RevenueCat.Api;

[Headers("Authorization: Bearer")]
public interface IRevenueCatApiV1
{
	[Get("/subscribers/{customer_id}")]
	Task<SubscriberResponse> GetOrCreateCustomer(string customer_id);

	//[Post("/subscribers/{customer_id}/attributes")]
	//Task<SubscriberValueResponse> SetCustomerAttributes(string customer_id, [Body]SubscriberAttributes attributes);

	[Get("/subscribers/{customer_id}/offerings")]
	Task<OfferingsResponse> GetOfferings(string customer_id);
}
