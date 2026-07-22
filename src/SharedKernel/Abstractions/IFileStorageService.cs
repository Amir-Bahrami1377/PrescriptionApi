namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Abstraction over MinIO/S3 object storage. Implemented by Prescription.Modules.FileStorage
/// (MinioFileStorageService). Only object keys/URLs are ever persisted in Postgres.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);

    Task<string> GetPresignedUrlAsync(string bucket, string objectKey, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task DeleteAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);
}
