using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.UploadConsultationTestResult;

/// <summary>Documents the multipart/form-data body for OpenAPI/Scalar — see CreateOrderFormDto for why this is needed alongside manual HttpRequest parsing.</summary>
public sealed class UploadConsultationTestResultFormDto
{
    public required IFormFile File { get; init; }
}

public sealed class UploadConsultationTestResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/consultation-result", async (
                Guid id,
                HttpRequest httpRequest,
                ICurrentUserService currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is not { } customerId)
                {
                    return Results.Unauthorized();
                }

                var form = await httpRequest.ReadFormAsync(cancellationToken);
                var file = form.Files.GetFile("file");
                if (file is null || file.Length == 0)
                {
                    return Results.BadRequest("بارگذاری فایل جواب آزمایش الزامی است.");
                }

                await using var stream = file.OpenReadStream();
                var command = new UploadConsultationTestResultCommand(id, customerId, stream, file.FileName, file.ContentType);
                await sender.Send(command, cancellationToken);

                return Results.NoContent();
            })
            .DisableAntiforgery()
            .Accepts<UploadConsultationTestResultFormDto>("multipart/form-data")
            .WithName("UploadConsultationTestResult")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
