using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.MoveToPaymentList;

public sealed class MoveToPaymentListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/payment-list", async (ICurrentUserService currentUser, ISender sender, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new MoveToPaymentListQuery(customerId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("MoveToPaymentList")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
