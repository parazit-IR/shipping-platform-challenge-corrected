using System.Security.Cryptography;
using System.Text;
using ShippingPlatform.Domain.Booking.ValueObject;

namespace ShippingPlatform.Application.Booking.Commands.Create;

internal static class CreateBookingRequestFingerprint
{
    public static string Compute(
        CustomerId customerId,
        AgreementId agreementId,
        Origin origin,
        Destination destination,
        VoyageId voyageId)
    {
        var payload = string.Join(
            "\n",
            customerId.Value,
            agreementId.Value,
            origin.Value,
            destination.Value,
            voyageId.Value);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
