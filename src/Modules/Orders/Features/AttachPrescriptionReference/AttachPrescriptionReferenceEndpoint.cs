using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.AttachPrescriptionReference;

public sealed record AttachPrescriptionReferenceRequest(string PrescriptionReferenceNumber);

public sealed class AttachPrescriptionReferenceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/prescription-reference", async (
                Guid id,
                AttachPrescriptionReferenceRequest body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new AttachPrescriptionReferenceCommand(id, body.PrescriptionReferenceNumber);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("AttachPrescriptionReference")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
