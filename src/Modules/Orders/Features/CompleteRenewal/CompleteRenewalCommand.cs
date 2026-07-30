using MediatR;

namespace Prescription.Modules.Orders.Features.CompleteRenewal;

public sealed record CompleteRenewalCommand(Guid RenewalId, string NewPrescriptionReferenceNumber) : IRequest;
