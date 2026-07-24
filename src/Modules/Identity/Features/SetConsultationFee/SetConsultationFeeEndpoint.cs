using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.SetConsultationFee;

public sealed record SetConsultationFeeRequest(long FeeInRials);

public sealed class SetConsultationFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/doctors/me/consultation-fee", async (
                SetConsultationFeeRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new SetConsultationFeeCommand(doctorId, body.FeeInRials), cancellationToken);
                return Results.NoContent();
            })
            .WithName("SetConsultationFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
