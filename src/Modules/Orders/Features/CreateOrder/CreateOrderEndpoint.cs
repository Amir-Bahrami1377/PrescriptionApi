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
                var file = form.Files.GetFile("file");
                if (file is null || file.Length == 0)
                {
                    return Results.BadRequest("بارگذاری فایل الزامی است.");
                }

                if (!Guid.TryParse(form["labTestId"], out var labTestId))
                {
                    return Results.BadRequest("شناسه آزمایش نامعتبر است.");
                }

                var note = form["note"].ToString();

                await using var stream = file.OpenReadStream();
                var command = new CreateOrderCommand(customerId, labTestId, note, stream, file.FileName, file.ContentType);
                var response = await sender.Send(command, cancellationToken);

                return Results.Created($"/api/orders/{response.OrderId}", response);
            })
            .DisableAntiforgery()
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization(policy => policy.RequireRole("Customer"));
    }
}
