using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ClaimRenewalForReview;

public sealed class ClaimRenewalForReviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/renewals/{id:guid}/claim", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ClaimRenewalForReviewCommand(id, doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ClaimRenewalForReview")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
