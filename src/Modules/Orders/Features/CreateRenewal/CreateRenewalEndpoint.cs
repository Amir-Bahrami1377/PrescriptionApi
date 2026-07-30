using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.CreateRenewal;

public sealed record CreateRenewalRequest(
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance);

public sealed class CreateRenewalEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/renewals", async (
                CreateRenewalRequest body,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var command = new CreateRenewalCommand(
                    customerId,
                    body.CurrentPrescriptionReferenceNumber,
                    body.NationalCode,
                    body.BasicInsurance);

                var response = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/renewals/{response.RenewalId}", response);
            })
            .WithName("CreateRenewal")
            .WithTags("Renewals")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
