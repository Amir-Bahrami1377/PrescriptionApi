using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Ticketing.Features.CloseExpiredTickets;
using Prescription.Modules.Ticketing.Infrastructure.Notifications;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.Modules.Ticketing;

public static class TicketingModule
{
    public static IServiceCollection AddTicketingModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TicketingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITicketNotificationService, TicketNotificationService>();
        services.AddScoped<ICloseExpiredTicketsJob, CloseExpiredTicketsJob>();

        return services;
    }
}
