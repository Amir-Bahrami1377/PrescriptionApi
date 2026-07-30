using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.CompleteRenewal;

public sealed record CompleteRenewalRequest(string NewPrescriptionReferenceNumber);

public sealed class CompleteRenewalEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/renewals/{id:guid}/complete", async (
                Guid id,
                CompleteRenewalRequest body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new CompleteRenewalCommand(id, body.NewPrescriptionReferenceNumber), cancellationToken);
                return Results.NoContent();
            })
            .WithName("CompleteRenewal")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
