using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.ListMyTickets;

public sealed class ListMyTicketsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tickets/mine", async (ICurrentUserService currentUser, ISender sender, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyTicketsQuery(customerId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyTickets")
            .WithTags("Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
