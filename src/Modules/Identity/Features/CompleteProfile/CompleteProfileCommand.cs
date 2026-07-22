using MediatR;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Features.CompleteProfile;

public sealed record CompleteProfileCommand(
    Guid UserId,
    string NationalCode,
    string FullName,
    int Age,
    Gender Gender) : IRequest<CompleteProfileResponse>;

public sealed record CompleteProfileResponse(bool IsProfileCompleted);
