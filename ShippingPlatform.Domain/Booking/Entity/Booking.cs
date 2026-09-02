
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Domain.Booking.Event;
using ShippingPlatform.Domain.Booking.Exception;
using ShippingPlatform.Infrastructure.Domain;

namespace ShippingPlatform.Domain.Booking.Entity;

public sealed class Booking : AggregateRoot
{
    public BookingId Id { get; }
    public CustomerId CustomerId { get; }
    public AgreementId AgreementId { get; }
    public Origin Origin { get; }
    public Destination Destination { get; }
    public VoyageId VoyageId { get; }
    public BookingStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public long Version { get; private set; }
    private readonly List<ContainerRequest> _containerRequests = new List<ContainerRequest>();

    private Booking(
        BookingId id,
        CustomerId customerId,
        AgreementId agreementId,
        Origin origin,
        Destination destination,
        VoyageId voyageId,
        BookingStatus status,
        DateTimeOffset createdAt,
        long version = 1)
    {
        Id = id;
        CustomerId = customerId;
        AgreementId = agreementId;
        Origin = origin;
        Destination = destination;
        VoyageId = voyageId;
        Status = status;
        CreatedAt = createdAt;
        Version = version;
    }

    public static Booking Create(
        CustomerId customerId,
        AgreementId agreementId,
        Origin origin,
        Destination destination,
        VoyageId voyageId,
        DateTimeOffset? createdAt = null)
    {
        var booking = new Booking(
            BookingId.Create(),
            customerId,
            agreementId,
            origin,
            destination,
            voyageId,
            BookingStatus.Draft,
            createdAt ?? DateTimeOffset.UtcNow);

        booking.RaiseDomainEvent(
            new BookingCreatedDomainEvent(
                booking.Id.Value.ToString(),
                booking.CustomerId.ToString(),
                booking.AgreementId.ToString(),
                booking.Origin.Value,
                booking.Destination.Value,
                booking.VoyageId.Value,
                booking.CreatedAt));

        return booking;
    }

    public void RequestQuotation()
    {
        if (_containerRequests.Count == 0)
        {
            throw new DomainValidationException("At least one ContainerRequest is required before quotation can be requested.");
        }

        Status = BookingStatus.QuotationRequested;
    }

    public void AddContainerRequest(ContainerRequest containerRequest)
    {
        if (containerRequest is null)
        {
            throw new DomainValidationException("ContainerRequest is required.");
        }

        _containerRequests.Add(containerRequest);
    }

    //AddContainerRequest() todo
}