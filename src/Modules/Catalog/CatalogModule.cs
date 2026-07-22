using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Catalog.Infrastructure;
using Prescription.Modules.Catalog.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICatalogLookup, CatalogLookup>();

        return services;
    }
}
