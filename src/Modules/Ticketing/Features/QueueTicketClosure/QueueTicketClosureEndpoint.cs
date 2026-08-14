using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.QueueTicketClosure;

public sealed class QueueTicketClosureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/tickets/{id:guid}/queue-closure", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new QueueTicketClosureCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .WithName("QueueTicketClosure")
            .WithTags("Admin", "Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
