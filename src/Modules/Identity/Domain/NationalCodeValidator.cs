namespace Prescription.Modules.Identity.Domain;

public static class NationalCodeValidator
{
    public static bool IsValid(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10 || !nationalCode.All(char.IsDigit))
        {
            return false;
        }

        // Reject sequences like "0000000000" / "1111111111" which pass the checksum but are not real codes.
        if (nationalCode.Distinct().Count() == 1)
        {
            return false;
        }

        var digits = nationalCode.Select(c => c - '0').ToArray();
        var checkDigit = digits[9];

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += digits[i] * (10 - i);
        }

        var remainder = sum % 11;

        return remainder < 2 ? checkDigit == remainder : checkDigit == 11 - remainder;
    }
}
