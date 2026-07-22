using Minio;
using Minio.DataModel.Args;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.FileStorage.Infrastructure;

public sealed class MinioFileStorageService(IMinioClient minioClient) : IFileStorageService
{
    public async Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucket, cancellationToken);

        var putArgs = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(putArgs, cancellationToken);

        return objectKey;
    }

    public async Task<Stream> DownloadAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        var getArgs = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await minioClient.GetObjectAsync(getArgs, cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds);

        return await minioClient.PresignedGetObjectAsync(presignedArgs);
    }

    public async Task DeleteAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey);

        await minioClient.RemoveObjectAsync(removeArgs, cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucket);
        var exists = await minioClient.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken);
        }
    }
}
