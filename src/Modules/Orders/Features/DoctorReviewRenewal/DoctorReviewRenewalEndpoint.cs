using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.DoctorReviewRenewal;

public sealed record DoctorReviewRenewalRequest(bool Approve, string? RejectionReason);

public sealed class DoctorReviewRenewalEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/renewals/{id:guid}/review", async (
                Guid id,
                DoctorReviewRenewalRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var command = new DoctorReviewRenewalCommand(id, doctorId, body.Approve, body.RejectionReason);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DoctorReviewRenewal")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
