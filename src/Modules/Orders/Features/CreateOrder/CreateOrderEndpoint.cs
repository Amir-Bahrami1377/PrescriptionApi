using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.CreateOrder;

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
                    thirdPartyPhoneNumber);
                var response = await sender.Send(command, cancellationToken);

                return Results.Created($"/api/orders/{response.OrderId}", response);
            })
            .DisableAntiforgery()
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
