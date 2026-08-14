using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Orders.Features.NotifyDoctorsOfUnclaimedOrders;
using Prescription.Modules.Orders.Infrastructure.Notifications;
using Prescription.Modules.Orders.Infrastructure.Persistence;

namespace Prescription.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IOrderStatusNotifier, OrderStatusNotifier>();
        services.AddScoped<INotifyDoctorsOfUnclaimedOrdersJob, NotifyDoctorsOfUnclaimedOrdersJob>();

        return services;
    }
}
