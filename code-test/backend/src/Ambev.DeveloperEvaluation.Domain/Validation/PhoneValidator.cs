using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class PhoneValidator : AbstractValidator<string>
{
    public PhoneValidator()
    {
        RuleFor(phone => phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The phone cannot be empty.")
            .Must(BeValidPhone)
            .WithMessage("The phone format is not valid.");
    }

    private bool BeValidPhone(string phone)
    {
        return PhoneNumber.TryCreate(phone, out _);
    }
}
