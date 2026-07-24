using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

                var command = new CreateOrderCommand(customerId, labTestIds, note, stream, file?.FileName, file?.ContentType);
                var response = await sender.Send(command, cancellationToken);

                return Results.Created($"/api/orders/{response.OrderId}", response);
            })
            .DisableAntiforgery()
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
