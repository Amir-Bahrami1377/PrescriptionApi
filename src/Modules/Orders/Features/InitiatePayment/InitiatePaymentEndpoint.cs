using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.InitiatePayment;

public sealed class InitiatePaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/payment/initiate", async (
                Guid id,
                HttpRequest httpRequest,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var callbackUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/api/orders/{id}/payment/callback";
                var command = new InitiatePaymentCommand(id, customerId, callbackUrl, currentUser.PhoneNumber);
                var response = await sender.Send(command, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("InitiatePayment")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
