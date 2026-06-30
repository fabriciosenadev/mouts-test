using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new CreateUserHandler(_userRepository, _mapper, _passwordHasher);
    }

    [Fact(DisplayName = "Given valid user data When creating user Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        var persistedUser = CreateUserHandlerTestData.GenerateValidUserFrom(command, "hashedPassword");
        persistedUser.Id = Guid.NewGuid();

        var result = new CreateUserResult
        {
            Id = persistedUser.Id,
            Name = persistedUser.Username,
            Email = persistedUser.Email,
            Phone = persistedUser.Phone,
            Role = persistedUser.Role,
            Status = persistedUser.Status
        };

        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(persistedUser);
        _mapper.Map<CreateUserResult>(persistedUser).Returns(result);
        _passwordHasher.HashPassword(command.Password).Returns("hashedPassword");

        var createUserResult = await _handler.Handle(command, CancellationToken.None);

        createUserResult.Should().NotBeNull();
        createUserResult.Id.Should().Be(persistedUser.Id);
        createUserResult.Name.Should().Be(persistedUser.Username);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid user data When creating user Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateUserCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given user creation request When handling Then password is hashed")]
    public async Task Handle_ValidRequest_HashesPassword()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        const string hashedPassword = "h@shedPassw0rd";
        var persistedUser = CreateUserHandlerTestData.GenerateValidUserFrom(command, hashedPassword);

        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(persistedUser);
        _mapper.Map<CreateUserResult>(persistedUser).Returns(new CreateUserResult { Id = persistedUser.Id });
        _passwordHasher.HashPassword(command.Password).Returns(hashedPassword);

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(command.Password);
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(user =>
                user.Password == hashedPassword &&
                user.Email == command.Email.ToLowerInvariant().Trim()),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicated email When creating user Then throws invalid operation exception")]
    public async Task Handle_DuplicatedEmail_ThrowsInvalidOperationException()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(CreateUserHandlerTestData.GenerateValidUserFrom(command, "existing"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
