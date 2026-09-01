using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

public class CreateBookingThenFailCommand: ITransactionalCommand<bool>;