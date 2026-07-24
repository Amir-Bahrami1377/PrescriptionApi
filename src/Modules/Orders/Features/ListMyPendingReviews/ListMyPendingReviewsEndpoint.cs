using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListMyPendingReviews;

public sealed class ListMyPendingReviewsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/pending/mine", async (
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyPendingReviewsQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyPendingReviews")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
