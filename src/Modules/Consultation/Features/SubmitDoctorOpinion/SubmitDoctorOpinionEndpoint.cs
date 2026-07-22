using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Consultation.Features.SubmitDoctorOpinion;

public sealed record SubmitDoctorOpinionRequest(string Opinion);

public sealed class SubmitDoctorOpinionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/consultations/{id:guid}/opinion", async (
                Guid id,
                SubmitDoctorOpinionRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var command = new SubmitDoctorOpinionCommand(id, doctorId, body.Opinion);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("SubmitDoctorOpinion")
            .WithTags("Consultation")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
