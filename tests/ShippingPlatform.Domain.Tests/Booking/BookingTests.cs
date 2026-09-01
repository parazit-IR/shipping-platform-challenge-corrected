using ShippingPlatform.Domain.Booking.Event;
using ShippingPlatform.Domain.Booking.Exception;
using ShippingPlatform.Domain.Booking.ValueObject;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Domain.Tests.Booking;

public sealed class BookingTests
{
    [Fact]
    public void Create_ShouldReturnDraftBooking_WhenInputIsValid()
    {
        var createdAt = new DateTimeOffset(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"),
            createdAt);

        Assert.NotEqual(Guid.Empty, booking.Id.Value);
        Assert.Equal("CUST-001", booking.CustomerId.Value);
        Assert.Equal("AGR-001", booking.AgreementId.Value);
        Assert.Equal("Bandar Abbas", booking.Origin.Value);
        Assert.Equal("Rotterdam", booking.Destination.Value);
        Assert.Equal("VOY-001", booking.VoyageId.Value);
        Assert.Equal(BookingStatus.Draft, booking.Status);
        Assert.Equal(createdAt, booking.CreatedAt);
    }

    [Fact]
    public void Create_ShouldRaiseBookingCreatedDomainEvent()
    {
        var createdAt = new DateTimeOffset(2026, 8, 21, 13, 0, 0, TimeSpan.Zero);

        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"),
            createdAt);

        var domainEvent = Assert.IsType<BookingCreatedDomainEvent>(Assert.Single(booking.DomainEvents));

        Assert.Equal(booking.Id.Value.ToString(), domainEvent.BookingId);
        Assert.Equal(booking.CustomerId.Value, domainEvent.CustomerId);
        Assert.Equal(booking.AgreementId.Value, domainEvent.AgreementId);
        Assert.Equal(booking.Origin.Value, domainEvent.Origin);
        Assert.Equal(booking.Destination.Value, domainEvent.Destination);
        Assert.Equal(booking.VoyageId.Value, domainEvent.VoyageId);
        Assert.Equal(booking.CreatedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveEventsWithoutChangingBookingState()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        var bookingId = booking.Id.Value;
        var status = booking.Status;

        Assert.Single(booking.DomainEvents);

        booking.ClearDomainEvents();

        Assert.Empty(booking.DomainEvents);
        Assert.Equal(bookingId, booking.Id.Value);
        Assert.Equal("CUST-001", booking.CustomerId.Value);
        Assert.Equal("AGR-001", booking.AgreementId.Value);
        Assert.Equal("Bandar Abbas", booking.Origin.Value);
        Assert.Equal("Rotterdam", booking.Destination.Value);
        Assert.Equal("VOY-001", booking.VoyageId.Value);
        Assert.Equal(status, booking.Status);
    }

    [Fact]
    public void RequestQuotation_ShouldThrow_WhenNoContainerRequestsExist()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        var exception = Assert.Throws<DomainValidationException>(booking.RequestQuotation);

        Assert.Equal(
            "At least one ContainerRequest is required before quotation can be requested.",
            exception.Message);
        Assert.Equal(BookingStatus.Draft, booking.Status);
    }

    [Fact]
    public void RequestQuotation_ShouldSetStatusToQuotationRequested_WhenContainerRequestExists()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        booking.AddContainerRequest(ContainerRequest.Create("Dry", "20ft", 1, 1200));

        booking.RequestQuotation();

        Assert.Equal(BookingStatus.QuotationRequested, booking.Status);
    }

    [Fact]
    public void AddContainerRequest_ShouldThrow_WhenContainerRequestIsNull()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        var exception = Assert.Throws<DomainValidationException>(() => booking.AddContainerRequest(null!));

        Assert.Equal("ContainerRequest is required.", exception.Message);
    }
    
    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled_WhenBookingCanBeCancelled()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }
    
    [Fact]
    public void Cancel_ShouldThrow_WhenBookingIsAlreadyCancelled()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-001"),
            AgreementId.Create("AGR-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-001"));

        booking.Cancel();

        Assert.Throws<DomainValidationException>(() => booking.Cancel());
    }
}
