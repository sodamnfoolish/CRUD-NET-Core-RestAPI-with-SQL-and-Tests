using AutoMapper;
using RestApi.Entities;
using RestApi.Dtos;


namespace RestApi.Profiles
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
