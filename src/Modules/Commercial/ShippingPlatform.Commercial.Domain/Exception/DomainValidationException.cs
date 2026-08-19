namespace ShippingPlatform.Commercial.Domain.Exception;

public sealed class DomainValidationException(string message): CommercialDomainException(message);