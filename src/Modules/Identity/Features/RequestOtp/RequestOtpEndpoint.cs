using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.RequestOtp;

public sealed class RequestOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/otp/request", async (RequestOtpCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("RequestOtp")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
