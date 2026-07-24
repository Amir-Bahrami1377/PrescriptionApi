using MediatR;

namespace Prescription.Modules.Identity.Features.GetMyConsultationFee;

public sealed record GetMyConsultationFeeQuery(Guid DoctorId) : IRequest<GetMyConsultationFeeResponse>;

public sealed record GetMyConsultationFeeResponse(long? FeeInRials);
