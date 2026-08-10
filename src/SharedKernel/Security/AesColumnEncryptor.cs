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

    private readonly byte[] _key = DecodeKey(options.Key);

    /// <summary>
    /// Validated here rather than left to AesGcm, which is only reached on the first encrypt. A key
    /// of the wrong length is still valid Base64, so the application would start happily and then
    /// fail much later in the middle of a user action, with nothing pointing at the configuration.
    /// </summary>
    private static byte[] DecodeKey(string configuredKey)
    {
        byte[] key;
        try
        {
            key = Convert.FromBase64String(configuredKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "ColumnEncryption:Key is not valid Base64. Generate one with: openssl rand -base64 32",
                exception);
        }

        if (key.Length is not (16 or 24 or 32))
        {
            // The length is safe to report; the key itself never is.
            throw new InvalidOperationException(
                $"ColumnEncryption:Key must decode to 16, 24 or 32 bytes for AES-128/192/256, but decodes to {key.Length}. " +
                "Generate one with: openssl rand -base64 32");
        }

        return key;
    }

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
