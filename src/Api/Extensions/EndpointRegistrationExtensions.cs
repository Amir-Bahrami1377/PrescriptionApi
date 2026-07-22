using Prescription.SharedKernel.Abstractions;

namespace Prescription.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static IServiceCollection AddEndpointsFromAssemblies(this IServiceCollection services, params IEnumerable<System.Reflection.Assembly> assemblies)
    {
        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            services.AddSingleton(typeof(IEndpoint), type);
        }

        return services;
    }

    public static IApplicationBuilder MapDiscoveredEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
