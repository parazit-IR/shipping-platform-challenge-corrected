using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

public sealed record CheckAgreementEligibilityQuery(
    string CustomerId,
    string AgreementId)
    : IQuery<CheckAgreementEligibilityResult>;