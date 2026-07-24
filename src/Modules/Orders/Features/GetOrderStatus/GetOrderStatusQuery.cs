using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.GetOrderStatus;

public sealed record GetOrderStatusQuery(Guid OrderId, Guid RequestingUserId, string RequestingUserRole) : IRequest<OrderStatusDto>;

public sealed record OrderStatusDto(
    Guid Id,
    IReadOnlyCollection<Guid> LabTestIds,
    long? PriceInRials,
    string Status,
    string? RejectionReason,
    string? PrescriptionReferenceNumber,
    string? PaymentReferenceId,
    bool HasResult,
    BasicInsuranceType BasicInsurance,
    SupplementaryInsuranceType SupplementaryInsurance,
    bool IsForThirdParty,
    string? ThirdPartyNationalCode,
    string? ThirdPartyPhoneNumber,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
