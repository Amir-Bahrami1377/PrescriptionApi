using MediatR;

namespace Prescription.Modules.Identity.Features.SetConsultationFee;

public sealed record SetConsultationFeeCommand(Guid DoctorId, long FeeInRials) : IRequest;
