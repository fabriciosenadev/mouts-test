using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersHandler : IRequestHandler<ListUsersCommand, ListUsersResult>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public ListUsersHandler(IMapper mapper, IUserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<ListUsersResult> Handle(ListUsersCommand request, CancellationToken cancellationToken)
    {
        var validator = new ListUsersValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var filter = _mapper.Map<UserFilter>(request);
        var (items, totalCount) = await _userRepository.SearchAsync(filter, cancellationToken);

        return new ListUsersResult
        {
            CurrentPage = request.Page,
            PageSize = request.Size,
            TotalCount = totalCount,
            Items = _mapper.Map<List<ListUsersItemResult>>(items)
        };
    }
}
