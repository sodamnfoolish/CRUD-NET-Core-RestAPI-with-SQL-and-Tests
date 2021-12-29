using Newtonsoft.Json;
using RestApi.Entities;
using RestApi.Dtos;
using RestApi.DbContexts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestApi.Interfaces;
using RestApi.Services;

namespace IntegrationTests
{
    public class UserControllerIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly List<User> _dbUserList;



        public UserControllerIntegrationTests()
        {
            _dbUserList = new List<User>();

            var webApp = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));

                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<UserDbContext>(options => options.UseInMemoryDatabase("DB"));

                    var dbContext = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<UserDbContext>();

                    dbContext.Database.EnsureDeleted();

                    for (int i = 0; i < 5; i++)
                    {
                        var userForDbContext = new User
                        {
                            id = Guid.NewGuid(),
                            name = $"TestName{i}",
                            password = $"TestPassword{i}",
                        };

                        var userForDbUserList = new User
                        {
                            id = userForDbContext.id,
                            name = userForDbContext.name,
                            password = userForDbContext.password,
                        };

                        dbContext.Add(userForDbContext);
                        _dbUserList.Add(userForDbUserList);
                    }

                    _dbUserList.Sort(delegate (User firstUser, User secondUser)
                    {
                        return firstUser.id.CompareTo(secondUser.id);
                    });

                    dbContext.SaveChanges();
                });
            });

            _client = webApp.CreateClient();
        }



        [Fact]
        public async void GetAll_Ok()
        {
            var response = await _client.GetAsync("/api/User");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            var userList = JsonConvert.DeserializeObject<List<UserDto>>(responseContent);

            Assert.NotNull(userList);

            Assert.NotEmpty(userList);

            Assert.True(Equal(_dbUserList, userList));
        }

        [Fact]
        public async void GetById_Ok()
        {
            foreach (var dbUser in _dbUserList)
            {
                var response = await _client.GetAsync($"/api/User/{dbUser.id}");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();

                Assert.NotNull(responseContent);

                var user = JsonConvert.DeserializeObject<UserDto>(responseContent);

                Assert.NotNull(user);

                Assert.True(Equal(dbUser, user));
            }

            GetAll_Ok();
        }

        [Fact]
        public async void GetById_InvalidId_Incorrect()
        {
            var userIdForGet = "incorrect";

            var response = await _client.GetAsync($"/api/User/{userIdForGet}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"The value '{userIdForGet}' is not valid.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void GetById_InvalidId_NonExistent()
        {
            var userIdForGet = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/User/{userIdForGet}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"Cannot find User with {userIdForGet} id.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_Created()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName1",
                password = "CreatedPassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            var user = JsonConvert.DeserializeObject<UserDto>(responseContent);

            Assert.NotNull(user);

            Assert.Equal(userForCreate.name, user.name);

            Assert.Equal(userForCreate.password, user.password);

            _dbUserList.Add(new User
            {
                id = user.id,
                name = user.name,
                password = user.password,
            });

            _dbUserList.Sort(delegate (User firstUser, User secondUser)
            {
                return firstUser.id.CompareTo(secondUser.id);
            });

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidName_IsEmpty()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "",
                password = "CreatedPassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidPassword_IsEmpty()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName",
                password = "",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidPassword_LessThanRequiredLength()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidPassword_NoUpperCaseLetters()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidPassword_NoLowerCaseLetters()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Create_InvalidPassword_NoDigits()
        {
            var userForCreate = new UserForCreateDto
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/User", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Delete_Deleted()
        {
            var userForDelete = _dbUserList.First();

            var response = await _client.DeleteAsync($"/api/User/{userForDelete.id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            var user = JsonConvert.DeserializeObject<UserDto>(responseContent);

            Assert.NotNull(user);

            Assert.True(Equal(userForDelete, user));

            _dbUserList.Remove(userForDelete);

            _dbUserList.Sort(delegate (User firstUser, User secondUser)
            {
                return firstUser.id.CompareTo(secondUser.id);
            });

            GetAll_Ok();
        }

        [Fact]
        public async void Delete_InvalidId_Incorrect()
        {
            var userIdForDelete = "incorrect";

            var response = await _client.DeleteAsync($"/api/User/{userIdForDelete}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"The value '{userIdForDelete}' is not valid.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Delete_InvalidId_NonExistent()
        {
            var userIdForDelete = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/User/{userIdForDelete}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"Cannot find User with {userIdForDelete} id.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_Updated()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForCreateDto
            {
                name = "UpdatedName1",
                password = "UpdatedPassword1",
            };

            var userShouldBe = new User
            {
                id = dbUser.id,
                name = userForUpdate.name,
                password = userForUpdate.password,
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            var user = JsonConvert.DeserializeObject<UserDto>(responseContent);

            Assert.NotNull(user);

            Assert.True(Equal(userShouldBe, user));

            dbUser.name = userForUpdate.name;
            dbUser.password = userForUpdate.password;

            _dbUserList.Sort(delegate (User firstUser, User secondUser)
            {
                return firstUser.id.CompareTo(secondUser.id);
            });

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidId_Incorrect()
        {
            var userIdForUpdated = "incorrect";

            var userForUpdate = new UserForUpdateDto
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{userIdForUpdated}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"The value '{userIdForUpdated}' is not valid.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidId_NonExistent()
        {
            var userIdForUpdated = Guid.NewGuid();

            var userForUpdate = new UserForUpdateDto
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{userIdForUpdated}", requestContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains($"Cannot find User with {userIdForUpdated} id.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidName_IsEmpty()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "",
                password = "CreatedPassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidPassword_IsEmpty()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "CreatedName",
                password = "",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidPassword_LessThanRequiredLength()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidPassword_NoUpperCaseLetters()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidPassword_NoLowerCaseLetters()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter.", responseContent);

            GetAll_Ok();
        }

        [Fact]
        public async void Update_InvalidPassword_NoDigits()
        {
            var dbUser = _dbUserList.First();

            var userForUpdate = new UserForUpdateDto
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/User/{dbUser.id}", requestContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit.", responseContent);

            GetAll_Ok();
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
    }
}