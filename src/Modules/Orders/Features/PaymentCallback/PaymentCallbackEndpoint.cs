using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.PaymentCallback;

public sealed class PaymentCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}/payment/callback", async (
                Guid id,
                string Authority,
                string Status,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new PaymentCallbackCommand(id, Authority, Status);
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("PaymentCallback")
            .WithTags("Orders")
            .AllowAnonymous();
    }
}
