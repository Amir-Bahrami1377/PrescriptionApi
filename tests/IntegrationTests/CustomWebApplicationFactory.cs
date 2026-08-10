using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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

        // Program.cs reads configuration into local variables before builder.Build(), which is
        // earlier than WebApplicationFactory's ConfigureAppConfiguration override gets spliced in
        // for the minimal-hosting Program.cs shape — so an in-memory config source here is too
        // late. Environment variables aren't: WebApplicationBuilder.CreateBuilder(args) reads them
        // immediately via AddEnvironmentVariables(), before any of Program.cs's own code runs, as
        // long as they're set (as here) before the host is ever created.
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", _redis.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "AZLkvtSOfz2ng8oE0jHBx2eE30RAoNlCs1k2ST/zfHU=");
        Environment.SetEnvironmentVariable("ColumnEncryption__Key", "SdgFjsUfDwN7byuQOJumTSTfS5cpS3nEnVGy7lihJ4Y=");
        Environment.SetEnvironmentVariable("MeliPayamak__Username", "test");
        Environment.SetEnvironmentVariable("MeliPayamak__Password", "test");
        Environment.SetEnvironmentVariable("MeliPayamak__SenderNumber", "50002000");
        Environment.SetEnvironmentVariable("ZarinPal__MerchantId", "00000000-0000-0000-0000-000000000000");
        Environment.SetEnvironmentVariable("MinIO__Endpoint", "localhost:9000");
        Environment.SetEnvironmentVariable("MinIO__AccessKey", "minioadmin");
        Environment.SetEnvironmentVariable("MinIO__SecretKey", "minioadmin");
        Environment.SetEnvironmentVariable("MinIO__UseSsl", "false");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

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
