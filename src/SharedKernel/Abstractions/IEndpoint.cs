using Microsoft.AspNetCore.Routing;

namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Implemented once per Feature slice, next to its Command/Query and Handler, so the endpoint
/// travels with the feature instead of living in a shared Controller. Prescription.Api discovers
/// every implementation via assembly scanning and calls MapEndpoint at startup.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
