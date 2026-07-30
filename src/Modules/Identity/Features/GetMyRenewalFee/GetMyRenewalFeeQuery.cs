using MediatR;

namespace Prescription.Modules.Identity.Features.GetMyRenewalFee;

public sealed record GetMyRenewalFeeQuery(Guid DoctorId) : IRequest<GetMyRenewalFeeResponse>;

public sealed record GetMyRenewalFeeResponse(long? FeeInRials);
