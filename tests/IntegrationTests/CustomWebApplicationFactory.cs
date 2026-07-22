using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prescription.IntegrationTests.Fakes;
using Prescription.SharedKernel.Abstractions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Prescription.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("prescription_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder().Build();

    public FakeOtpProvider FakeOtpProvider { get; } = new();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                ["Jwt:SigningKey"] = "AZLkvtSOfz2ng8oE0jHBx2eE30RAoNlCs1k2ST/zfHU=",
                ["ColumnEncryption:Key"] = "SdgFjsUfDwN7byuQOJumTSTfS5cpS3nEnVGy7lihJ4Y=",
                ["MeliPayamak:Username"] = "test",
                ["MeliPayamak:Password"] = "test",
                ["MeliPayamak:BodyId"] = "0",
                ["MeliPayamak:SenderNumber"] = "50002000",
                ["ZarinPal:MerchantId"] = "00000000-0000-0000-0000-000000000000",
                ["MinIO:Endpoint"] = "localhost:9000",
                ["MinIO:AccessKey"] = "minioadmin",
                ["MinIO:SecretKey"] = "minioadmin",
                ["MinIO:UseSsl"] = "false",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // OTP delivery is the only external dependency this test suite actually exercises,
            // so it's the only one that needs a fake — everything else (ZarinPal, MinIO, generic SMS)
            // is registered against dummy config above but never invoked by the Identity-only flow.
            services.RemoveAll<IOtpProvider>();
            services.AddSingleton<IOtpProvider>(FakeOtpProvider);
        });
    }
}
