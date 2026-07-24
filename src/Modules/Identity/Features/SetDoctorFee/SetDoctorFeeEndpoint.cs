using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.SetDoctorFee;

public sealed record SetDoctorFeeRequest(long FeeInRials);

public sealed class SetDoctorFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/doctors/me/fee", async (
                SetDoctorFeeRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new SetDoctorFeeCommand(doctorId, body.FeeInRials), cancellationToken);
                return Results.NoContent();
            })
            .WithName("SetDoctorFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
