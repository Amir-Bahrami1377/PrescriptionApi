using Minio;
using Minio.DataModel.Args;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.FileStorage.Infrastructure;

/// <summary>Uses two MinIO clients: one pointed at the internal endpoint for server-side upload/download/delete
/// calls, and one pointed at a publicly reachable endpoint so presigned URLs actually resolve for an external
/// browser instead of embedding an internal-only Docker service name.</summary>
public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly IMinioClient _publicMinioClient;

    public MinioFileStorageService(MinioOptions options)
    {
        _minioClient = new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.UseSsl)
            .Build();

        // Falls back to the internal endpoint only when no public one is configured, which is the
        // local-development case where they are the same host.
        var publicEndpoint = string.IsNullOrWhiteSpace(options.PublicEndpoint) ? options.Endpoint : options.PublicEndpoint;
        var publicUseSsl = string.IsNullOrWhiteSpace(options.PublicEndpoint) ? options.UseSsl : options.PublicUseSsl;

        _publicMinioClient = new MinioClient()
            .WithEndpoint(publicEndpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(publicUseSsl)
            .Build();
    }

    public async Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucket, cancellationToken);

        var putArgs = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putArgs, cancellationToken);

        return objectKey;
    }

    public async Task<Stream> DownloadAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        var getArgs = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getArgs, cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds);

        return await _publicMinioClient.PresignedGetObjectAsync(presignedArgs);
    }

    public async Task DeleteAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(removeArgs, cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucket);
        var exists = await _minioClient.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken);
        }
    }
}
