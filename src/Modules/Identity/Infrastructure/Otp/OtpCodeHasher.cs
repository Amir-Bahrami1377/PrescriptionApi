using System.Security.Cryptography;
using System.Text;

namespace Prescription.Modules.Identity.Infrastructure.Otp;

internal static class OtpCodeHasher
{
    public static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
