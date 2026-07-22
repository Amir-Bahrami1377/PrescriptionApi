using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Features.CreateTicket;

public sealed record CreateTicketRequest(string Subject, string Message);

public sealed class CreateTicketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tickets", async (
                CreateTicketRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var command = new CreateTicketCommand(customerId, body.Subject, body.Message);
                var response = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/tickets/{response.TicketId}", response);
            })
            .WithName("CreateTicket")
            .WithTags("Ticketing")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
