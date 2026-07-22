using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity.Features.VerifyOtpAndLogin;

public sealed class VerifyOtpAndLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/otp/verify", async (VerifyOtpAndLoginCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("VerifyOtpAndLogin")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
