using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListMyOrdersAwaitingPayment;

public sealed class ListMyOrdersAwaitingPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/mine/awaiting-payment", async (
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyOrdersAwaitingPaymentQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyOrdersAwaitingPayment")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
