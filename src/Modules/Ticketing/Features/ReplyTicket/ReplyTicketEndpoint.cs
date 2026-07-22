using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.ReplyTicket;

public sealed record ReplyTicketRequest(string Message);

public sealed class ReplyTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tickets/{id:guid}/reply", async (
                Guid id,
                ReplyTicketRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } senderId || currentUser.Role is not { } role)
                {
                    return Results.Unauthorized();
                }

                var command = new ReplyTicketCommand(id, senderId, role, body.Message);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("ReplyTicket")
            .WithTags("Ticketing")
            .RequireAuthorization();
    }
}
