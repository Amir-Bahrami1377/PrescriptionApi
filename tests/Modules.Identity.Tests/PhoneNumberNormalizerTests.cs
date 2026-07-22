using FluentAssertions;
using Prescription.Modules.Identity.Domain;

namespace Prescription.Modules.Identity.Tests;

public class PhoneNumberNormalizerTests
{
    [Theory]
    [InlineData("09123456789", "09123456789")]
    [InlineData("+989123456789", "09123456789")]
    [InlineData("989123456789", "09123456789")]
    [InlineData("9123456789", "09123456789")]
    public void Normalize_VariousFormats_ReturnsCanonicalForm(string input, string expected)
    {
        PhoneNumberNormalizer.Normalize(input).Should().Be(expected);
    }
}
