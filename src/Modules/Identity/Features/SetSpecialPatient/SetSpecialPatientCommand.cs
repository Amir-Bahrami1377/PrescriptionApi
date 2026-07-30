using MediatR;

namespace Prescription.Modules.Identity.Features.SetSpecialPatient;

public sealed record SetSpecialPatientCommand(Guid UserId, bool IsSpecialPatient) : IRequest;
