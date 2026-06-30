using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserProfile : Profile
{
    public CreateUserProfile()
    {
        CreateMap<User, CreateUserResult>()
            .ForMember(destination => destination.Name, configuration => configuration.MapFrom(source => source.Username));
    }
}
