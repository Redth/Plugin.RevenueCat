using Plugin.RevenueCat.Api.V1;

namespace Plugin.RevenueCat.Api;

public interface IRevenueCatApiV1
{
	Task<SubscriberResponse> GetOrCreateCustomer(string customer_id);

	Task<OfferingsResponse> GetOfferings(string customer_id);
}
