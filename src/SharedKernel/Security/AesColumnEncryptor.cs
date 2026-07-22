using System.Security.Cryptography;

namespace Prescription.SharedKernel.Security;

public sealed class ColumnEncryptionOptions
{
    /// <summary>Base64-encoded 256-bit AES key. Must come from a secret store (env var / user-secrets), never source control.</summary>
    public required string Key { get; init; }
}

/// <summary>AES-256-GCM column encryptor. Output is Base64(nonce || tag || ciphertext).</summary>
public sealed class AesColumnEncryptor(ColumnEncryptionOptions options) : IColumnEncryptor
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    private readonly byte[] _key = Convert.FromBase64String(options.Key);

    public string Encrypt(string plainText)
    {
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSizeBytes + TagSizeBytes + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, result, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSizeBytes + TagSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var input = Convert.FromBase64String(cipherText);

        var nonce = input[..NonceSizeBytes];
        var tag = input[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
        var cipherBytes = input[(NonceSizeBytes + TagSizeBytes)..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
