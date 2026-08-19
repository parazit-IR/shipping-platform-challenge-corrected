using System.ComponentModel.DataAnnotations;

namespace ShippingPlatform.Booking.Web.Contract;

public sealed class CreateBookingRequest: IValidatableObject
{
    [Required]
    public string? CustomerId { get; init; }

    [Required]
    public string? AgreementId { get; init; }

    [Required]
    public string? Origin { get; init; }

    [Required]
    public string? Destination { get; init; }

    [Required]
    public string? VoyageId { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(CustomerId))
            yield return new ValidationResult("CustomerId is required.", [nameof(CustomerId)]);

        if (string.IsNullOrWhiteSpace(AgreementId))
            yield return new ValidationResult("AgreementId is required.", [nameof(AgreementId)]);

        if (string.IsNullOrWhiteSpace(Origin))
            yield return new ValidationResult("Origin is required.", [nameof(Origin)]);

        if (string.IsNullOrWhiteSpace(Destination))
            yield return new ValidationResult("Destination is required.", [nameof(Destination)]);

        if (string.IsNullOrWhiteSpace(VoyageId))
            yield return new ValidationResult("VoyageId is required.", [nameof(VoyageId)]);
    }
}