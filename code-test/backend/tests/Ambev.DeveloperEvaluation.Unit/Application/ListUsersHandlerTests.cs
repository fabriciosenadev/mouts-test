using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListUsersHandlerTests
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly ListUsersHandler _handler;

    public ListUsersHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new ListUsersHandler(_mapper, _userRepository);
    }

    [Fact(DisplayName = "Given valid filters When listing users Then returns paginated result")]
    public async Task Handle_ValidRequest_ReturnsPaginatedResult()
    {
        var command = new ListUsersCommand
        {
            Page = 1,
            Size = 10,
            Username = "user"
        };

        var filter = new UserFilter
        {
            Page = command.Page,
            Size = command.Size,
            Username = command.Username
        };

        var users = new List<Ambev.DeveloperEvaluation.Domain.Entities.User>
        {
            UserTestData.GenerateValidUser()
        };
        users[0].Id = Guid.NewGuid();

        var mappedItems = new List<ListUsersItemResult>
        {
            new()
            {
                Id = users[0].Id,
                Name = users[0].Username,
                Email = users[0].Email,
                Phone = users[0].Phone,
                Role = users[0].Role,
                Status = users[0].Status
            }
        };

        _mapper.Map<UserFilter>(command).Returns(filter);
        _userRepository.SearchAsync(filter, Arg.Any<CancellationToken>()).Returns((users, 1));
        _mapper.Map<List<ListUsersItemResult>>(users).Returns(mappedItems);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.CurrentPage.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalCount.Should().Be(1);
        response.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Given invalid paging When listing users Then throws validation exception")]
    public async Task Handle_InvalidPaging_ThrowsValidationException()
    {
        var command = new ListUsersCommand
        {
            Page = 0,
            Size = 10
        };

        var action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
