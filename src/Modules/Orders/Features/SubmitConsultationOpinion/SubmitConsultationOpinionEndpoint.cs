using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.SubmitConsultationOpinion;

public sealed record SubmitConsultationOpinionRequest(string Opinion);

public sealed class SubmitConsultationOpinionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/consultation-opinion", async (
                Guid id,
                SubmitConsultationOpinionRequest body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new SubmitConsultationOpinionCommand(id, body.Opinion), cancellationToken);
                return Results.NoContent();
            })
            .WithName("SubmitConsultationOpinion")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
