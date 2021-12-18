using AutoMapper;
using ProjectRestApi.Entities;
using ProjectRestApi.Dtos;


namespace ProjectRestApi.Profiles
{
    public class UserControllerMapperProfile : Profile
    {
        public UserControllerMapperProfile()
        {
            CreateMap<UserForCreateDto, User>();
            CreateMap<UserForDeleteDto, User>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<User, UserDto>();
        }
    }
}
