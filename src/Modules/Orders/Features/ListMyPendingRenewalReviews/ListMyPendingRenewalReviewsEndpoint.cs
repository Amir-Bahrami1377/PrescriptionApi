using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListMyPendingRenewalReviews;

public sealed class ListMyPendingRenewalReviewsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/renewals/pending/mine", async (
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyPendingRenewalReviewsQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyPendingRenewalReviews")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
