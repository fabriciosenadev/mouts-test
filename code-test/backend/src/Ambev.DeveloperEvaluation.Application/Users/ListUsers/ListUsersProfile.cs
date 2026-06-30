using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersProfile : Profile
{
    public ListUsersProfile()
    {
        CreateMap<ListUsersCommand, UserFilter>();
        CreateMap<User, ListUsersItemResult>()
            .ForMember(destination => destination.Name, configuration => configuration.MapFrom(source => source.Username));
    }
}
