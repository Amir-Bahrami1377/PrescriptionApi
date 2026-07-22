using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog.Features.ListTests;

public sealed class ListTestsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog/tests", async (string? search, ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListTestsQuery(search), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListTests")
            .WithTags("Catalog")
            .AllowAnonymous();
    }
}
