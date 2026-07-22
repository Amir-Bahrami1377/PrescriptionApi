using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.DoctorReviewOrder;

public sealed record DoctorReviewOrderRequest(bool Approve, string? RejectionReason);

public sealed class DoctorReviewOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/review", async (
                Guid id,
                DoctorReviewOrderRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } doctorId)
                {
                    return Results.Unauthorized();
                }

                var command = new DoctorReviewOrderCommand(id, doctorId, body.Approve, body.RejectionReason);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DoctorReviewOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
