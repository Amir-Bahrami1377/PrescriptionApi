using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.SetSpecialPatient;

public sealed record SetSpecialPatientRequest(bool IsSpecialPatient);

public sealed class SetSpecialPatientEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/users/{id:guid}/special-patient", async (
                Guid id,
                SetSpecialPatientRequest body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new SetSpecialPatientCommand(id, body.IsSpecialPatient), cancellationToken);
                return Results.NoContent();
            })
            .WithName("SetSpecialPatient")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
