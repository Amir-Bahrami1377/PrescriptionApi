using FluentAssertions;
using Prescription.Modules.Identity.Domain;
using Prescription.Modules.Identity.Features.CompleteProfile;

namespace Prescription.Modules.Identity.Tests;

public class CompleteProfileValidatorTests
{
    private readonly CompleteProfileValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CompleteProfileCommand(Guid.NewGuid(), "0499370899", "علی رضایی", 30, Gender.Male);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidNationalCode_HasError()
    {
        var command = new CompleteProfileCommand(Guid.NewGuid(), "1234567890", "علی رضایی", 30, Gender.Male);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CompleteProfileCommand.NationalCode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(121)]
    public void Validate_AgeOutOfRange_HasError(int age)
    {
        var command = new CompleteProfileCommand(Guid.NewGuid(), "0499370899", "علی رضایی", age, Gender.Male);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CompleteProfileCommand.Age));
    }
}
