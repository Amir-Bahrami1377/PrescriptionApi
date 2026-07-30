using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.InitiateRenewalPayment;

public sealed class InitiateRenewalPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/renewals/{id:guid}/payment/initiate", async (
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

                var callbackUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/api/renewals/{id}/payment/callback";
                var command = new InitiateRenewalPaymentCommand(id, customerId, callbackUrl);
                var response = await sender.Send(command, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("InitiateRenewalPayment")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
