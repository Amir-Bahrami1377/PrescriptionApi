using MediatR;

namespace Prescription.Modules.Identity.Features.GetMyDoctorFee;

public sealed record GetMyDoctorFeeQuery(Guid DoctorId) : IRequest<GetMyDoctorFeeResponse>;

public sealed record GetMyDoctorFeeResponse(long? FeeInRials);
