using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.ReopenTicket;

public sealed class ReopenTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tickets/{id:guid}/reopen", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new ReopenTicketCommand(id, customerId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("ReopenTicket")
            .WithTags("Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
