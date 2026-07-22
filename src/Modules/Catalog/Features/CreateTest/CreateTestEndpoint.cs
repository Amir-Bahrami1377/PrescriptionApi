using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed class CreateTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/catalog/tests", async (CreateTestCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/catalog/tests/{response.Id}", response);
            })
            .WithName("CreateTest")
            .WithTags("Catalog")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
