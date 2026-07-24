using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.CreateOrder;

/// <summary>
/// Documents the multipart/form-data body for OpenAPI/Scalar only — the endpoint below still
/// parses HttpRequest by hand (repeated "labTestIds" keys, optional file, custom per-field error
/// messages don't fit plain [FromForm] binding), so without this the request body showed up
/// undocumented and every field — especially the two insurance enums — had to be discovered by
/// probing.
/// </summary>
public sealed class CreateOrderFormDto
{
    public required List<Guid> LabTestIds { get; init; }
    public string? Note { get; init; }
    public IFormFile? File { get; init; }
    public BasicInsuranceType BasicInsurance { get; init; }
    public SupplementaryInsuranceType SupplementaryInsurance { get; init; }
    public bool IsForThirdParty { get; init; }
    public string? ThirdPartyNationalCode { get; init; }
    public string? ThirdPartyPhoneNumber { get; init; }

    /// <summary>When true, a doctor-configured consultation fee is added to the price and the order
    /// routes through customer self-upload + doctor opinion after payment instead of finishing once the doctor is done.</summary>
    public bool RequestsConsultation { get; init; }
}

public sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
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

                // Client sends one or more "labTestIds" fields (repeated multipart key) for multi-select.
                var labTestIds = new List<Guid>();
                foreach (var value in form["labTestIds"])
                {
                    if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var id))
                    {
                        return Results.BadRequest("شناسه آزمایش نامعتبر است.");
                    }

                    labTestIds.Add(id);
                }

                if (labTestIds.Count == 0)
                {
                    return Results.BadRequest("انتخاب حداقل یک آزمایش الزامی است.");
                }

                var note = form["note"].ToString();

                // File attachment is optional.
                var file = form.Files.GetFile("file");
                await using var stream = file?.OpenReadStream();

                var basicInsurance = Enum.TryParse<BasicInsuranceType>(form["basicInsurance"], ignoreCase: true, out var basic)
                    ? basic
                    : BasicInsuranceType.None;
                var supplementaryInsurance = Enum.TryParse<SupplementaryInsuranceType>(form["supplementaryInsurance"], ignoreCase: true, out var supplementary)
                    ? supplementary
                    : SupplementaryInsuranceType.None;

                var isForThirdParty = bool.TryParse(form["isForThirdParty"], out var forThirdParty) && forThirdParty;
                var thirdPartyNationalCode = form["thirdPartyNationalCode"].ToString() is { Length: > 0 } nationalCode ? nationalCode : null;
                var thirdPartyPhoneNumber = form["thirdPartyPhoneNumber"].ToString() is { Length: > 0 } phoneNumber ? phoneNumber : null;
                var requestsConsultation = bool.TryParse(form["requestsConsultation"], out var consultation) && consultation;

                var command = new CreateOrderCommand(
                    customerId,
                    labTestIds,
                    note,
                    stream,
                    file?.FileName,
                    file?.ContentType,
                    basicInsurance,
                    supplementaryInsurance,
                    isForThirdParty,
                    thirdPartyNationalCode,
                    thirdPartyPhoneNumber,
                    requestsConsultation);
                var response = await sender.Send(command, cancellationToken);

                return Results.Created($"/api/orders/{response.OrderId}", response);
            })
            .DisableAntiforgery()
            .Accepts<CreateOrderFormDto>("multipart/form-data")
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
