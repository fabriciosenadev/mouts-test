using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid user data When updating user Then returns updated response")]
    public async Task Handle_ValidRequest_ReturnsUpdatedUser()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "updated.user",
            Email = "updated.user@example.com",
            Phone = "+5511999999999",
            Status = UserStatus.Active,
            Role = UserRole.Admin
        };

        var result = new UpdateUserResult
        {
            Id = user.Id,
            Name = command.Username,
            Email = command.Email,
            Phone = command.Phone,
            Status = command.Status,
            Role = command.Role
        };

        _userRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((Ambev.DeveloperEvaluation.Domain.Entities.User?)null);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UpdateUserResult>(user).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Name.Should().Be(command.Username);
        user.Email.Should().Be(command.Email);
        user.Phone.Should().Be(command.Phone);
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact(DisplayName = "Given duplicated email from another user When updating user Then throws invalid operation exception")]
    public async Task Handle_DuplicatedEmail_ThrowsInvalidOperationException()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        var existingUser = UserTestData.GenerateValidUser();
        existingUser.Id = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "updated.user",
            Email = existingUser.Email,
            Phone = "+5511999999999",
            Status = UserStatus.Active,
            Role = UserRole.Admin
        };

        _userRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(existingUser);

        var action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
