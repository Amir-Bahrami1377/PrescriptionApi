using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListOrdersByCustomer;

public sealed class ListOrdersByCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/users/{userId:guid}/orders", async (
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListOrdersByCustomerQuery(userId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListOrdersByCustomer")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
