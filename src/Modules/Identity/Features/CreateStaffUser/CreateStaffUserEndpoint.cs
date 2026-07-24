using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Identity.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.CreateStaffUser;

public sealed record CreateStaffUserRequest(string PhoneNumber, UserRole Role);

public sealed class CreateStaffUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/users", async (CreateStaffUserRequest body, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateStaffUserCommand(body.PhoneNumber, body.Role);
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("CreateStaffUser")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
