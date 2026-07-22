using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Prescription.Modules.FileStorage.Infrastructure;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.FileStorage;

public static class FileStorageModule
{
    public static IServiceCollection AddFileStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("MinIO").Get<MinioOptions>()
            ?? throw new InvalidOperationException("MinIO configuration section is missing.");

        services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build());

        services.AddSingleton<IFileStorageService, MinioFileStorageService>();

        return services;
    }
}
