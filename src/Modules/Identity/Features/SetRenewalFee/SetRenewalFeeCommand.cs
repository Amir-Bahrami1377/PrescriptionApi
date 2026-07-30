using MediatR;

namespace Prescription.Modules.Identity.Features.SetRenewalFee;

public sealed record SetRenewalFeeCommand(Guid DoctorId, long FeeInRials) : IRequest;
