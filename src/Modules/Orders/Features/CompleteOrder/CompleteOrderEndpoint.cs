using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.CompleteOrder;

public sealed class CompleteOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/complete", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new CompleteOrderCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .WithName("CompleteOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
