using MediatR;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Features.ListUsers;

public sealed record ListUsersQuery(UserRole? Role) : IRequest<IReadOnlyList<UserSummaryDto>>;

public sealed record UserSummaryDto(
    Guid Id,
    string PhoneNumber,
    string Role,
    string? FullName,
    bool IsProfileCompleted,
    long? DoctorFeeInRials,
    DateTimeOffset CreatedAtUtc);
