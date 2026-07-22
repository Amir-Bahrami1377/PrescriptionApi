using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Prescription.Modules.Payments.Infrastructure;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ZarinPalOptions>(configuration.GetSection("ZarinPal"));

        services.AddHttpClient<IPaymentGateway, ZarinPalPaymentGateway>((HttpClient client) =>
            {
                var options = configuration.GetSection("ZarinPal").Get<ZarinPalOptions>()
                    ?? throw new InvalidOperationException("ZarinPal configuration section is missing.");
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}
