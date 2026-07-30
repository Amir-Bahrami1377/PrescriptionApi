using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.CreateRenewal;

public sealed record CreateRenewalCommand(
    Guid CustomerId,
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance) : IRequest<CreateRenewalResponse>;

public sealed record CreateRenewalResponse(Guid RenewalId, string Status);
