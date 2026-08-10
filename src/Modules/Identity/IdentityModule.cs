using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Modules.Identity.Infrastructure;
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
        services.AddScoped<IIdentityLookup, IdentityLookup>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IOtpCodeStore, RedisOtpCodeStore>();

        // Delivery rides on the Notifications module's SMS provider, so there is a single gateway
        // client, one set of credentials and one retry policy for every message sent.
        services.AddScoped<IOtpProvider, SmsOtpProvider>();

        return services;
    }
}
