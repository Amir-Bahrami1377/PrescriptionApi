using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.ListBasicInsuranceTypes;

public sealed class ListBasicInsuranceTypesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/basic-insurance-types", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(new ListBasicInsuranceTypesQuery(), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListBasicInsuranceTypes")
            .WithTags("Orders")
            .AllowAnonymous();
    }
}
