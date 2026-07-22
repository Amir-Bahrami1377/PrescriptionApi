using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Identity.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.CompleteProfile;

public sealed record CompleteProfileRequest(string NationalCode, string FullName, int Age, Gender Gender);

public sealed class CompleteProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/profile/complete", async (
                CompleteProfileRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } userId)
                {
                    return Results.Unauthorized();
                }

                var command = new CompleteProfileCommand(userId, body.NationalCode, body.FullName, body.Age, body.Gender);
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("CompleteProfile")
            .WithTags("Auth")
            .RequireAuthorization();
    }
}
