using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListMyOrdersInProgress;

public sealed class ListMyOrdersInProgressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/mine/in-progress", async (
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyOrdersInProgressQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyOrdersInProgress")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
