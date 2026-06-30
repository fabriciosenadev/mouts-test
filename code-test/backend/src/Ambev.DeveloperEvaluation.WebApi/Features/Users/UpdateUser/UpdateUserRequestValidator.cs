using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty().Length(3, 50);
        RuleFor(request => request.Email).SetValidator(new EmailValidator());
        RuleFor(request => request.Phone).SetValidator(new PhoneValidator());
        RuleFor(request => request.Status).NotEqual(UserStatus.Unknown);
        RuleFor(request => request.Role).NotEqual(UserRole.None);
    }
}
