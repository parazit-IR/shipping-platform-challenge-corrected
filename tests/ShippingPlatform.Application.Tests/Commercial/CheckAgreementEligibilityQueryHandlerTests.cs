using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Application.Tests.Fakes;
using ShippingPlatform.Domain.Commercial.Entity;
using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Application.Tests.Commercial;

public sealed class CheckAgreementEligibilityQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCustomerNotFound_WhenCustomerDoesNotExist()
    {
        var agreementRepository = new FakeAgreementReadRepository();
        var readUnitOfWork = new FakeReadUnitOfWork(agreementRepository);
        var customerPort = new FakeCustomerExistencePort { ExistsResult = false };
        var handler = new CheckAgreementEligibilityQueryHandler(readUnitOfWork, customerPort);
        var query = new CheckAgreementEligibilityQuery("CUST-001", "AGR-001");
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await handler.Handle(query, cancellationToken);

        Assert.Equal(CommercialEligibilityStatus.CustomerNotFound, result.Status);
        Assert.Equal(1, customerPort.CallCount);
        Assert.Equal("CUST-001", customerPort.LastCustomerId!.Value);
        Assert.Equal(cancellationToken, customerPort.LastCancellationToken);
        Assert.Equal(0, agreementRepository.FindByIdCallCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnAgreementNotFound_WhenAgreementDoesNotExist()
    {
        var agreementRepository = new FakeAgreementReadRepository();
        var readUnitOfWork = new FakeReadUnitOfWork(agreementRepository);
        var customerPort = new FakeCustomerExistencePort { ExistsResult = true };
        var handler = new CheckAgreementEligibilityQueryHandler(readUnitOfWork, customerPort);

        var result = await handler.Handle(new CheckAgreementEligibilityQuery("CUST-001", "AGR-001"));

        Assert.Equal(CommercialEligibilityStatus.AgreementNotFound, result.Status);
        Assert.Equal(1, agreementRepository.FindByIdCallCount);
        Assert.Equal("AGR-001", agreementRepository.LastAgreementId!.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnAgreementInactive_WhenAgreementIsInactive()
    {
        var agreementRepository = new FakeAgreementReadRepository
        {
            AgreementToReturn = Agreement.Create(
                AgreementId.Create("AGR-001"),
                CustomerId.Create("CUST-001"),
                AgreementStatus.Inactive)
        };
        var readUnitOfWork = new FakeReadUnitOfWork(agreementRepository);
        var customerPort = new FakeCustomerExistencePort { ExistsResult = true };
        var handler = new CheckAgreementEligibilityQueryHandler(readUnitOfWork, customerPort);

        var result = await handler.Handle(new CheckAgreementEligibilityQuery("CUST-001", "AGR-001"));

        Assert.Equal(CommercialEligibilityStatus.AgreementInactive, result.Status);
    }

    [Fact]
    public async Task Handle_ShouldReturnAgreementIneligible_WhenAgreementBelongsToDifferentCustomer()
    {
        var agreementRepository = new FakeAgreementReadRepository
        {
            AgreementToReturn = Agreement.Create(
                AgreementId.Create("AGR-001"),
                CustomerId.Create("CUST-999"),
                AgreementStatus.Active)
        };
        var readUnitOfWork = new FakeReadUnitOfWork(agreementRepository);
        var customerPort = new FakeCustomerExistencePort { ExistsResult = true };
        var handler = new CheckAgreementEligibilityQueryHandler(readUnitOfWork, customerPort);

        var result = await handler.Handle(new CheckAgreementEligibilityQuery("CUST-001", "AGR-001"));

        Assert.Equal(CommercialEligibilityStatus.AgreementIneligible, result.Status);
    }

    [Fact]
    public async Task Handle_ShouldReturnEligible_WhenAgreementIsActiveAndBelongsToCustomer()
    {
        var agreementRepository = new FakeAgreementReadRepository
        {
            AgreementToReturn = Agreement.Create(
                AgreementId.Create("AGR-001"),
                CustomerId.Create("CUST-001"),
                AgreementStatus.Active)
        };
        var readUnitOfWork = new FakeReadUnitOfWork(agreementRepository);
        var customerPort = new FakeCustomerExistencePort { ExistsResult = true };
        var handler = new CheckAgreementEligibilityQueryHandler(readUnitOfWork, customerPort);

        var result = await handler.Handle(new CheckAgreementEligibilityQuery("CUST-001", "AGR-001"));

        Assert.Equal(CommercialEligibilityStatus.Eligible, result.Status);
        Assert.True(result.IsEligible);
    }
}
