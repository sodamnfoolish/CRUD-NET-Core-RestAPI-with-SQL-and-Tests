/*using Xunit;
using RestApi.Controllers;
using AutoMapper;
using RestApi.Interfaces;
using RestApi.Entities;
using RestApi.Profiles;
using Moq;
using System.Collections.Generic;
using System;
using RestApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                User user = new User()
                {
                    id = Guid.NewGuid(),
                    name = $"TestName{i}",
                    password = $"TestPassword{i}"
                };
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

            Assert.True(response is CreatedAtActionResult);

            var result = response as CreatedResult;

            Assert.True(result.Value is UserDto);

            var resultUser = result.Value as UserDto;

            Assert.Equal(userForCreate.name, resultUser.name);
            Assert.Equal(userForCreate.password, resultUser.password);
        }

        [Fact]
        public async void Delete_Deleted()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var userIdForDelete = dbUserList.First().id;

            var response = await controller.Delete(userIdForDelete);

            Assert.True(response is OkResult);
        }

        [Fact]
        public async void Delete_InvalidId_NonExistent()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var userIdForDelete = Guid.NewGuid();

            var response = await controller.Delete(userIdForDelete);

            Assert.True(response is NotFoundResult);
        }

        [Fact]
        public async void Update_Updated()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var userIdForUpdate = dbUserList.First().id;

            var userForUpdate = new UserForUpdateDto()
            {
                name = "UpdatedName1",
                password = "UpdatedPassword",
            };

            var response = await controller.Update(userIdForUpdate, userForUpdate);

            Assert.True(response is OkResult);
        }

        [Fact]
        public async void Update_InvalidId_NonExistent()
        {
            var mockedUserDbService = CreateMockedUserDbService(dbUserList);

            var controller = new UserController(mockedUserDbService, mapper);

            var userIdForUpdate = Guid.NewGuid();

            var userForUpdate = new UserForUpdateDto()
            {
                name = "UpdatedName1",
                password = "UpdatedPassword",
            };

            var response = await controller.Update(userIdForUpdate, userForUpdate);

            Assert.True(response is NotFoundResult);
        }

        private bool Equal(User user, UserDto userDto)
        {
            return user.id == userDto.id && user.name == userDto.name && user.password == userDto.password;
        }

        private bool Equal(List<User> userList, List<UserDto> userDtoList)
        {
            if (userList.Count != userDtoList.Count) return false;

            for (int i = 0; i < userList.Count; i++)
                if (!Equal(userList[i], userDtoList[i])) return false;

            return true;
        }

        private IUserDbService CreateMockedUserDbService(List<User> userList)
        {
            var mockUserDbService = new Mock<IUserDbService>();

            mockUserDbService.Setup(service => service.GetAll()).ReturnsAsync(userList);

            foreach (var user in userList)
            {
                mockUserDbService.Setup(service => service.GetById(user.id)).ReturnsAsync(user);
            }
            mockUserDbService.Setup(service => service.Create(It.IsAny<User>())).ReturnsAsync((User user) =>
            {
                user.id = Guid.NewGuid();
                return user;
            });

            mockUserDbService.Setup(service => service.Delete(It.IsAny<User>())).ReturnsAsync((User user) => user);

            mockUserDbService.Setup(service => service.Update(It.IsAny<User>(), It.IsAny<UserForUpdateDto>())).ReturnsAsync((User user, UserForUpdateDto userForUpdateDto) =>
            {
                user.name = userForUpdateDto.name;
                user.password = userForUpdateDto.password;
                return user;
            });

            return mockUserDbService.Object;
        }
    }
}
*/