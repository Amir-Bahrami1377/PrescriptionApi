using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.CloseTicket;

public sealed class CloseTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tickets/{id:guid}/close", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var command = new CloseTicketCommand(id, customerId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("CloseTicket")
            .WithTags("Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
