using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListPendingOrders;

public sealed class ListPendingOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/pending", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListPendingOrdersQuery(), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListPendingOrders")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
