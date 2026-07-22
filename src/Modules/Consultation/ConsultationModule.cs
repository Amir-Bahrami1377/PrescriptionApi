using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Consultation.Infrastructure.Persistence;

namespace Prescription.Modules.Consultation;

public static class ConsultationModule
{
    public static IServiceCollection AddConsultationModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ConsultationDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
