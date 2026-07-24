using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ClaimOrderForReview;

public sealed class ClaimOrderForReviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/claim", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ClaimOrderForReviewCommand(id, doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ClaimOrderForReview")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
