using MediatR;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Features.CreateStaffUser;

public sealed record CreateStaffUserCommand(string PhoneNumber, UserRole Role) : IRequest<CreateStaffUserResponse>;

public sealed record CreateStaffUserResponse(Guid UserId, string Role, bool Created);
