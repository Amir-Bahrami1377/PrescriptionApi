using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Features.UpdateTest;

public sealed record UpdateTestRequest(string Name, string? Description, long PriceInRials, bool IsActive);

public sealed class UpdateTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/catalog/tests/{id:guid}", async (Guid id, UpdateTestRequest body, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new UpdateTestCommand(id, body.Name, body.Description, body.PriceInRials, body.IsActive);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("UpdateTest")
            .WithTags("Catalog")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
