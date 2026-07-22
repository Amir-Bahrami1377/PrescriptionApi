namespace Prescription.SharedKernel.Security;

/// <summary>Column-level encryption for sensitive fields (e.g. national code) stored in Postgres.</summary>
public interface IColumnEncryptor
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
