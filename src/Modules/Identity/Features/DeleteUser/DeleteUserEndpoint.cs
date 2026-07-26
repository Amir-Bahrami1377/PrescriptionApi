using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.DeleteUser;

public sealed class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/admin/users/{id:guid}", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } adminId)
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new DeleteUserCommand(id, adminId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteUser")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
