using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListOrdersAwaitingConsultationOpinion;

public sealed class ListOrdersAwaitingConsultationOpinionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/awaiting-consultation-opinion", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListOrdersAwaitingConsultationOpinionQuery(), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListOrdersAwaitingConsultationOpinion")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
