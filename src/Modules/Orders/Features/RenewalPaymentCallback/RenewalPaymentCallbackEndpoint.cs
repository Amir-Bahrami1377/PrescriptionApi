using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.RenewalPaymentCallback;

public sealed class RenewalPaymentCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/renewals/{id:guid}/payment/callback", async (
                Guid id,
                string Authority,
                string Status,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new RenewalPaymentCallbackCommand(id, Authority, Status);
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("RenewalPaymentCallback")
            .WithTags("Renewals")
            .AllowAnonymous();
    }
}
