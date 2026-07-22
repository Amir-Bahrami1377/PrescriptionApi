using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Prescription.Modules.Identity.Infrastructure.Jwt;
using Prescription.Modules.Identity.Infrastructure.Otp;
using Prescription.Modules.Identity.Infrastructure.Persistence;
using Prescription.Modules.Identity.Infrastructure.Sms;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IOtpCodeStore, RedisOtpCodeStore>();

        services.Configure<MeliPayamakOptions>(configuration.GetSection("MeliPayamak"));
        services.AddHttpClient<IOtpProvider, MeliPayamakOtpProvider>((HttpClient client) =>
            {
                var options = configuration.GetSection("MeliPayamak").Get<MeliPayamakOptions>()
                    ?? throw new InvalidOperationException("MeliPayamak configuration section is missing.");
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}
