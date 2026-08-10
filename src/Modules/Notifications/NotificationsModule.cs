using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Prescription.Modules.Notifications.Features.SendSms;
using Prescription.Modules.Notifications.Infrastructure;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MeliPayamakOptions>(configuration.GetSection("MeliPayamak"));

        services.AddHttpClient<ISmsProvider, MeliPayamakSmsProvider>((HttpClient client) =>
            {
                var options = configuration.GetSection("MeliPayamak").Get<MeliPayamakOptions>()
                    ?? throw new InvalidOperationException("MeliPayamak configuration section is missing.");
                client.BaseAddress = new Uri(options.SmsBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddScoped<ISmsJob, SmsJob>();
        services.AddScoped<INotificationQueue, HangfireNotificationQueue>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}
