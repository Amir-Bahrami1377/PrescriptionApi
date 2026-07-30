using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListPendingRenewals;

public sealed class ListPendingRenewalsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/renewals/pending", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListPendingRenewalsQuery(), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListPendingRenewals")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
