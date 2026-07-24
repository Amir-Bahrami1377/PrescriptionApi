using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.GetMyConsultationFee;

public sealed class GetMyConsultationFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/doctors/me/consultation-fee", async (ICurrentUserService currentUser, ISender sender, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new GetMyConsultationFeeQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetMyConsultationFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
