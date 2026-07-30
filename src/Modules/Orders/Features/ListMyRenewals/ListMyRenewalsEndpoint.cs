using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListMyRenewals;

public sealed class ListMyRenewalsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/renewals/mine", async (
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new ListMyRenewalsQuery(customerId), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListMyRenewals")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
