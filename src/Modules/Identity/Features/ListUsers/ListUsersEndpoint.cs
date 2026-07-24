using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Identity.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.ListUsers;

public sealed class ListUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/users", async (UserRole? role, ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListUsersQuery(role), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListUsers")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
