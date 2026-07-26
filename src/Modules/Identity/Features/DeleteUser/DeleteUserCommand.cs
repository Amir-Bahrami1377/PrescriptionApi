using MediatR;

namespace Prescription.Modules.Identity.Features.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId, Guid RequestingAdminId) : IRequest;
