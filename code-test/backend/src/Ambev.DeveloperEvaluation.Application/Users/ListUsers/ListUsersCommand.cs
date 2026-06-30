using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersCommand : IRequest<ListUsersResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserStatus? Status { get; set; }
    public UserRole? Role { get; set; }
}
