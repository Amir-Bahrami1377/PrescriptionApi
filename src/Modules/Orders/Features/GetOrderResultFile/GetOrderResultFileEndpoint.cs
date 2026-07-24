using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.GetOrderResultFile;

public sealed class GetOrderResultFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}/result-file", async (
                Guid id,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } userId || currentUser.Role is not { } role)
                {
                    return Results.Unauthorized();
                }

                var response = await sender.Send(new GetOrderResultFileQuery(id, userId, role), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetOrderResultFile")
            .WithTags("Orders")
            .RequireAuthorization();
    }
}
