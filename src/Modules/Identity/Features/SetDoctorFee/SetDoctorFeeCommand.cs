using MediatR;

namespace Prescription.Modules.Identity.Features.SetDoctorFee;

public sealed record SetDoctorFeeCommand(Guid DoctorId, long FeeInRials) : IRequest;
