namespace Prescription.Modules.FileStorage.Infrastructure;

public sealed class MinioOptions
{
    /// <summary>Host:port the API server itself uses to reach MinIO (e.g. the "minio" Docker service name).</summary>
    public required string Endpoint { get; init; }

    /// <summary>Host:port to embed in presigned URLs handed back to browsers/clients outside the API's own
    /// network (e.g. "localhost:9000" for the published Docker Compose port). Falls back to Endpoint when unset,
    /// which only works if the client happens to share the API's network.</summary>
    public string? PublicEndpoint { get; init; }

    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }

    /// <summary>TLS for the server's own connection to MinIO. Behind a reverse proxy that is a plain
    /// hop inside the container network, so this stays off while presigned URLs are still HTTPS.</summary>
    public bool UseSsl { get; init; }

    /// <summary>Scheme used for presigned URLs. Separate from <see cref="UseSsl"/> because the two
    /// genuinely differ once TLS is terminated at the proxy: turning them on together would send the
    /// internal client at https://minio:9000, where nothing is listening for TLS, and break uploads.</summary>
    public bool PublicUseSsl { get; init; } = true;
}
