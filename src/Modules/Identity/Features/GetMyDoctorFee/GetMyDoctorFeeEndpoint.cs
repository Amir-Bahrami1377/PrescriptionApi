using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.GetMyDoctorFee;

public sealed class GetMyDoctorFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/doctors/me/fee", async (ICurrentUserService currentUser, ISender sender, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new GetMyDoctorFeeQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetMyDoctorFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
