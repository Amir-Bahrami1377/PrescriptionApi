using MediatR;

namespace Prescription.Modules.Orders.Features.AttachPrescriptionReference;

public sealed record AttachPrescriptionReferenceCommand(Guid OrderId, string PrescriptionReferenceNumber) : IRequest;
