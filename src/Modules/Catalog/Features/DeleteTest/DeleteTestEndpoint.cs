using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Features.DeleteTest;

public sealed class DeleteTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/catalog/tests/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteTestCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteTest")
            .WithTags("Catalog")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
