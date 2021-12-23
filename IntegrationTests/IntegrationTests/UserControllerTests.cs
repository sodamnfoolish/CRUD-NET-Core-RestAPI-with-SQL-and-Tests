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

namespace ItegrationTests.IntegrationTests
{
    public class UserControllerTests
    {
        private HttpClient client;
        private List<User> dbUserList = new List<User>();



        public UserControllerTests()
        {
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
                        User user = new User(Guid.NewGuid(), $"TestName{i}", $"TestPassword{i}");
                        dbContext.Add(user);
                        dbUserList.Add(user);
                    }

                    dbUserList.Sort(delegate (User _firstUser, User _secondUser)
                    {
                        return _firstUser.id.CompareTo(_secondUser.id);
                    });

                    dbContext.SaveChanges();
                });
            });

            client = webApp.CreateClient();
        }



        [Fact]
        public async void GetAll_Ok()
        {
            var response = await client.GetAsync($"/api/Users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            List<UserDto> userList = JsonConvert.DeserializeObject<List<UserDto>>(responseContent);

            Assert.NotNull(userList);
            Assert.NotEmpty(userList);
            Assert.True(Equal(dbUserList, userList));
        }

        [Fact]
        public async void GetById_Ok()
        {
            foreach (var dbUser in dbUserList)
            {
                var response = await client.GetAsync($"/api/Users/{dbUser.id}");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();

                Assert.NotNull(responseContent);

                UserDto user = JsonConvert.DeserializeObject<UserDto>(responseContent);

                Assert.NotNull(user);

                Assert.True(Equal(dbUser, user));
            }
        }

        [Fact]
        public async void GetById_InvalidId_Incorrect()
        {
            var response = await client.GetAsync($"/api/Users/incorrect");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void GetById_InvalidId_NonExistent()
        {
            var response = await client.GetAsync($"/api/Users/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void Create_Created()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName1",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            UserDto user = JsonConvert.DeserializeObject<UserDto>(responseContent);

            Assert.NotNull(user);
            Assert.Equal(userForCreate.name, user.name);
            Assert.Equal(userForCreate.password, user.password);

            dbUserList.Add(
                new User()
                {
                    id = user.id,
                    name = user.name,
                    password = user.password,
                });

            dbUserList.Sort(delegate (User _firstUser, User _secondUser)
            {
                return _firstUser.id.CompareTo(_secondUser.id);
            });
        }

        [Fact]
        public async void Create_InvalidName_IsEmpty()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_IsEmpty()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_LessThanRequiredLength()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoUpperCaseLetters()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoLowerCaseLetters()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoDigits()
        {
            UserForCreateDto userForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForCreate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Users/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit", responseContent);
        }

        [Fact]
        public async void Delete_Deleted()
        {
            User userForDelete = dbUserList.First();

            var response = await client.DeleteAsync($"/api/Users/{userForDelete.id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dbUserList.Remove(userForDelete);

            dbUserList.Sort(delegate (User _firstUser, User _secondUser)
            {
                return _firstUser.id.CompareTo(_secondUser.id);
            });
        }

        [Fact]
        public async void Delete_InvalidId_Incorrect()
        {
            var response = await client.DeleteAsync($"/api/Users/incorrect");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void Delete_InvalidId_NonExistent()
        {
            var response = await client.DeleteAsync($"/api/Users/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void Update_Updated()
        {
            User dbUser = dbUserList.First();

            UserForCreateDto userForUpdate = new UserForCreateDto()
            {
                name = "UpdatedName1",
                password = "UpdatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            dbUser.name = userForUpdate.name;
            dbUser.password = userForUpdate.password;

            dbUserList.Sort(delegate (User _firstUser, User _secondUser)
            {
                return _firstUser.id.CompareTo(_secondUser.id);
            });
        }

        [Fact]
        public async void Update_InvalidId_Incorrect()
        {

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/incorrect", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async void Update_InvalidId_NonExistent()
        {
            Guid userId = Guid.NewGuid();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{userId}", stringContent);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async void Update_InvalidName_IsEmpty()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_IsEmpty()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_LessThanRequiredLength()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoUpperCaseLetters()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoLowerCaseLetters()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoDigits()
        {
            User dbUser = dbUserList.First();

            UserForUpdateDto userForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(userForUpdate), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Users/{dbUser.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit", responseContent);
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
    }
}