using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Username).NotEmpty().Length(3, 50);
        RuleFor(command => command.Email).SetValidator(new EmailValidator());
        RuleFor(command => command.Phone).SetValidator(new PhoneValidator());
        RuleFor(command => command.Status).NotEqual(UserStatus.Unknown);
        RuleFor(command => command.Role).NotEqual(UserRole.None);
    }
}
