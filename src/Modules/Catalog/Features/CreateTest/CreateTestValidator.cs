using FluentValidation;

namespace Prescription.Modules.Catalog.Features.CreateTest;

public sealed class CreateTestValidator : AbstractValidator<CreateTestCommand>
{
    public CreateTestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PriceInRials).GreaterThan(0);
    }
}
