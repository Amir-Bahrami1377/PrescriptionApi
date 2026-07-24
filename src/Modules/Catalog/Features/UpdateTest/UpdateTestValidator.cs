using FluentValidation;

namespace Prescription.Modules.Catalog.Features.UpdateTest;

public sealed class UpdateTestValidator : AbstractValidator<UpdateTestCommand>
{
    public UpdateTestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
