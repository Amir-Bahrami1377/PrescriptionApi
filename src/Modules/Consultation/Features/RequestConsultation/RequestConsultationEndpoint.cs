using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Consultation.Features.RequestConsultation;

/// <summary>Documents the multipart/form-data body for OpenAPI/Scalar — see CreateOrderFormDto for why this is needed alongside manual HttpRequest parsing.</summary>
public sealed class RequestConsultationFormDto
{
    public required IFormFile File { get; init; }
    public string? Note { get; init; }
}

public sealed class RequestConsultationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/consultations", async (
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
                    return Results.BadRequest("بارگذاری تصویر الزامی است.");
                }

                var note = form["note"].ToString();

                await using var stream = file.OpenReadStream();
                var command = new RequestConsultationCommand(customerId, note, stream, file.FileName, file.ContentType);
                var response = await sender.Send(command, cancellationToken);

                return Results.Created($"/api/consultations/{response.ConsultationId}", response);
            })
            .DisableAntiforgery()
            .Accepts<RequestConsultationFormDto>("multipart/form-data")
            .WithName("RequestConsultation")
            .WithTags("Consultation")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
