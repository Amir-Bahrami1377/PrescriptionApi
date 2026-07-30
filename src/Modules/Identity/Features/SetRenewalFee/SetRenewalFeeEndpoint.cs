using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.SetRenewalFee;

public sealed record SetRenewalFeeRequest(long FeeInRials);

public sealed class SetRenewalFeeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/doctors/me/renewal-fee", async (
                SetRenewalFeeRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new SetRenewalFeeCommand(doctorId, body.FeeInRials), cancellationToken);
                return Results.NoContent();
            })
            .WithName("SetRenewalFee")
            .WithTags("Doctor")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
