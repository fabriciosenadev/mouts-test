using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public class UserFilter
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserStatus? Status { get; set; }
    public UserRole? Role { get; set; }
}
