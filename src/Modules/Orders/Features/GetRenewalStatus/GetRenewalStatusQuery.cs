using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.GetRenewalStatus;

public sealed record GetRenewalStatusQuery(Guid RenewalId, Guid RequestingUserId, string RequestingUserRole) : IRequest<RenewalStatusDto>;

public sealed record RenewalStatusDto(
    Guid Id,
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance,
    string Status,
    long? PriceInRials,
    string? RejectionReason,
    string? PaymentReferenceId,
    // The whole point of the request: null until the doctor finishes and hands it back.
    string? NewPrescriptionReferenceNumber,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
