using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(email => email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The email address cannot be empty.")
            .MaximumLength(100)
            .WithMessage("The email address cannot be longer than 100 characters.")
            .Must(BeValidEmail)
            .WithMessage("The provided email address is not valid.");
    }

    private bool BeValidEmail(string email)
    {
        return EmailAddress.TryCreate(email, out _);
    }
}
