namespace ShippingPlatform.Commercial.Application.Port.In;

public sealed record CheckAgreementEligibilityCommand(
    string CustomerId,
    string AgreementId,
    string Origin,
    string Destination,
    string VoyageId);