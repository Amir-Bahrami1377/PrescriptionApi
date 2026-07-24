using MediatR;

namespace Prescription.Modules.Orders.Features.SubmitConsultationOpinion;

public sealed record SubmitConsultationOpinionCommand(Guid OrderId, string Opinion) : IRequest;
