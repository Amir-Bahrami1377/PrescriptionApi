using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Ticketing.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.ListTickets;

public sealed class ListTicketsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/tickets", async (
                TicketStatus? status,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListTicketsQuery(status), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListTickets")
            .WithTags("Admin", "Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
