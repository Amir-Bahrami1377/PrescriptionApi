using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.UploadTestResult;

public sealed class UploadTestResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/result", async (
                Guid id,
                HttpRequest httpRequest,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var form = await httpRequest.ReadFormAsync(cancellationToken);
                var file = form.Files.GetFile("file");
                if (file is null || file.Length == 0)
                {
                    return Results.BadRequest("بارگذاری فایل جواب آزمایش الزامی است.");
                }

                await using var stream = file.OpenReadStream();
                var command = new UploadTestResultCommand(id, stream, file.FileName, file.ContentType);
                await sender.Send(command, cancellationToken);

                return Results.NoContent();
            })
            .DisableAntiforgery()
            .WithName("UploadTestResult")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Doctor"));
    }
}
