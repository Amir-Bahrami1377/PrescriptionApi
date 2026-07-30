using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.GetRenewalStatus;

public sealed class GetRenewalStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/renewals/{id:guid}", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } userId || currentUser.Role is not { } role)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new GetRenewalStatusQuery(id, userId, role), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetRenewalStatus")
            .WithTags("Renewals")
            .RequireAuthorization();
    }
}
