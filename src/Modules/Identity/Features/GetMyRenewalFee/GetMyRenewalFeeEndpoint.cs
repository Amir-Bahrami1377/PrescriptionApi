using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.GetMyRenewalFee;

public sealed class GetMyRenewalFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/doctors/me/renewal-fee", async (ICurrentUserService currentUser, ISender sender, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new GetMyRenewalFeeQuery(doctorId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetMyRenewalFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
