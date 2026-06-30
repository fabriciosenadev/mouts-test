using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersValidator : AbstractValidator<ListUsersCommand>
{
    public ListUsersValidator()
    {
        RuleFor(command => command.Page).GreaterThan(0);
        RuleFor(command => command.Size).InclusiveBetween(1, 100);
    }
}
