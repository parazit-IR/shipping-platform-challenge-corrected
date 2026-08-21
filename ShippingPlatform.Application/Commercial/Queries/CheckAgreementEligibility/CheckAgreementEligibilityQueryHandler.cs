using ShippingPlatform.Application.Common.Ports;
using ShippingPlatform.Domain.Commercial.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Application;

using DomainEligibilityStatus = ShippingPlatform.Domain.Commercial.ValueObject.AgreementEligibilityStatus;

namespace ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

public sealed class CheckAgreementEligibilityQueryHandler
    : IQueryHandler<
        CheckAgreementEligibilityQuery,
        CheckAgreementEligibilityResult>,
      IAgreementEligibilityChecker
{
    private readonly IReadUnitOfWork _readUnitOfWork;
    private readonly ICustomerExistencePort _customerExistencePort;

    public CheckAgreementEligibilityQueryHandler(
        IReadUnitOfWork readUnitOfWork,
        ICustomerExistencePort customerExistencePort)
    {
        _readUnitOfWork = readUnitOfWork;
        _customerExistencePort = customerExistencePort;
    }

    public Task<CheckAgreementEligibilityResult> Handle(
        CheckAgreementEligibilityQuery query,
        CancellationToken cancellationToken = default)
    {
        return CheckAsync(
            query.CustomerId,
            query.AgreementId,
            cancellationToken);
    }

    public async Task<CheckAgreementEligibilityResult> CheckAsync(
        string customerId,
        string agreementId,
        CancellationToken cancellationToken = default)
    {
        var customer = CustomerId.Create(customerId);
        var agreementIdValue = AgreementId.Create(agreementId);

        var customerExists =
            await _customerExistencePort.ExistsAsync(
                customer,
                cancellationToken);

        if (!customerExists)
        {
            return new(
                CommercialEligibilityStatus.CustomerNotFound);
        }

        var agreement =
            await _readUnitOfWork.Agreements.FindByIdAsync(
                agreementIdValue,
                cancellationToken);

        if (agreement is null)
        {
            return new(
                CommercialEligibilityStatus.AgreementNotFound);
        }

        var status = agreement.CheckEligibility(customer);

        var applicationStatus = status switch
        {
            DomainEligibilityStatus.Eligible =>
                CommercialEligibilityStatus.Eligible,

            DomainEligibilityStatus.AgreementInactive =>
                CommercialEligibilityStatus.AgreementInactive,

            DomainEligibilityStatus.AgreementIneligible =>
                CommercialEligibilityStatus.AgreementIneligible,

            _ => throw new InvalidOperationException(
                $"Unsupported eligibility status: {status}")
        };

        return new(applicationStatus);
    }
}