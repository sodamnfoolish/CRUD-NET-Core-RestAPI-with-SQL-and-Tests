using Xunit;
using RestApi.Controllers;
using AutoMapper;
using RestApi.Interfaces;
using RestApi.Entities;
using RestApi.Profiles;
using Moq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using RestApi.Services;
using RestApi.Dtos;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests
{
    public class UsersContollerUnitTests
    {
        private IMapper mapper;
        private List<User> dbUserList;

        public UsersContollerUnitTests()
        {
            mapper = new Mapper(new MapperConfiguration(config => config.AddProfile(new UserControllerMapperProfile())));

            dbUserList = new List<User>();

            for (int i = 0; i < 5; i++)
            {
                User user = new User(Guid.NewGuid(), $"TestName{i}", $"TestPassword{i}");
                dbUserList.Add(user);
            }

            dbUserList.Sort(delegate (User _firstUser, User _secondUser)
            {
                return _firstUser.id.CompareTo(_secondUser.id);
            });
        }



        [Fact]
        public async void GetAll_Ok()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var response = await controller.GetAll();

            Assert.True(response is OkObjectResult);

            var result = response as OkObjectResult;

            Assert.True(result.Value is List<UserDto>);

            var resultUserList = result.Value as List<UserDto>;

            Assert.NotNull(resultUserList);
            Assert.NotEmpty(resultUserList);
            Assert.True(Equal(dbUserList, resultUserList));
        }

        [Fact]
        public async void GetById_Ok()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            foreach(var dbUser in dbUserList)
            {
                var response = await controller.GetById(dbUser.id);

                Assert.True(response is OkObjectResult);

                var result = response as OkObjectResult;

                Assert.True(result.Value is UserDto);

                var resultUser = result.Value as UserDto;

                Assert.NotNull(resultUser);
                Assert.True(Equal(dbUser, resultUser));
            }
        }

        [Fact]
        public async void GetById_InvalidId_NonExistent()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var response = await controller.GetById(Guid.NewGuid());

            Assert.True(response is NotFoundResult);
        }

        [Fact]
        public async void Create_Created()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName1",
                password = "CreatedPassword1",
            };

            var response = await controller.Create(userForCreate);

            Assert.True(response is CreatedResult);

            var result = response as CreatedResult;

            Assert.True(result.Value is UserDto);


        }

        private bool Equal(User _firstUser, UserDto _secondUser)
        {
            return _firstUser.id == _secondUser.id && _firstUser.name == _secondUser.name && _firstUser.password == _secondUser.password;
        }

        private bool Equal(List<User> _firstUserList, List<UserDto> _secondUserList)
        {
            if (_firstUserList.Count != _secondUserList.Count) return false;

            for (int i = 0; i < _firstUserList.Count; i++)
                if (!Equal(_firstUserList[i], _secondUserList[i])) return false;

            return true;
        }

        private IUserDbService CreateMockedUserDbService(List<User> _userList)
        {
            var mockUserDbService = new Mock<IUserDbService>();

            mockUserDbService.Setup(service => service.GetAll()).ReturnsAsync(_userList);

            foreach (var user in _userList)
            {
                mockUserDbService.Setup(service => service.GetById(user.id)).ReturnsAsync(user);
            }
            mockUserDbService.Setup(service => service.Create(It.IsAny<User>())).ReturnsAsync((User user) =>
            {
                user.id = Guid.NewGuid(); 
                return user;
            });

            mockUserDbService.Setup(service => service.Delete(It.IsAny<User>())).ReturnsAsync(true);

            mockUserDbService.Setup(service => service.Update(It.IsAny<Guid>(), It.IsAny<User>())).ReturnsAsync(true);

            return mockUserDbService.Object;
        }
    }
}
