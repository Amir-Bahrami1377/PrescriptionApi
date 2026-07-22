namespace Prescription.Modules.Identity.Domain;

public static class PhoneNumberNormalizer
{
    /// <summary>Normalizes +98/98/0-prefixed Iranian mobile numbers to the canonical 09xxxxxxxxx form.</summary>
    public static string Normalize(string phoneNumber)
    {
        var digits = phoneNumber.Trim();

        if (digits.StartsWith("+98", StringComparison.Ordinal))
        {
            digits = "0" + digits[3..];
        }
        else if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length == 12)
        {
            digits = "0" + digits[2..];
        }
        else if (!digits.StartsWith('0'))
        {
            digits = "0" + digits;
        }

        return digits;
    }
}
