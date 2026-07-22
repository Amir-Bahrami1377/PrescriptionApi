using FluentAssertions;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Tests;

public class NationalCodeValidatorTests
{
    [Theory]
    [InlineData("0000000000")]
    [InlineData("1111111111")]
    public void IsValid_RepeatedDigitCodes_ReturnsFalse(string code)
    {
        // These pass the raw checksum but are not real national codes.
        NationalCodeValidator.IsValid(code).Should().BeFalse();
    }

    [Theory]
    [InlineData("0499370899")]
    [InlineData("0084575948")]
    public void IsValid_ValidChecksum_ReturnsTrue(string code)
    {
        NationalCodeValidator.IsValid(code).Should().BeTrue();
    }

    [Theory]
    [InlineData("0499370890")] // bad checksum
    [InlineData("123456789")] // too short
    [InlineData("12345678901")] // too long
    [InlineData("12345abcde")] // non-digit
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_InvalidCodes_ReturnsFalse(string? code)
    {
        NationalCodeValidator.IsValid(code!).Should().BeFalse();
    }
}
