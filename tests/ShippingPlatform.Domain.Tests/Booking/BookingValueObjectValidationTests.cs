using ShippingPlatform.Domain.Booking.Exception;
using ShippingPlatform.Domain.Booking.ValueObject;

namespace ShippingPlatform.Domain.Tests.Booking;

public sealed class BookingValueObjectValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CustomerId_Create_ShouldThrow_WhenValueIsBlank(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => CustomerId.Create(value));

        Assert.Equal("CustomerId is required.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void AgreementId_Create_ShouldThrow_WhenValueIsBlank(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => AgreementId.Create(value));

        Assert.Equal("AgreementId is required.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Origin_Create_ShouldThrow_WhenValueIsBlank(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Origin.Create(value));

        Assert.Equal("Origin is required.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Destination_Create_ShouldThrow_WhenValueIsBlank(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Destination.Create(value));

        Assert.Equal("Destination is required.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void VoyageId_Create_ShouldThrow_WhenValueIsBlank(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => VoyageId.Create(value));

        Assert.Equal("VoyageId is required.", exception.Message);
    }
}
