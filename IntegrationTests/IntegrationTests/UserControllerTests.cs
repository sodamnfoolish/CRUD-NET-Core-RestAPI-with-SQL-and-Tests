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

namespace IntegrationTests
{
    public class UserControllerTests
    {
        private HttpClient Client;
        private List<User> DB_UserList = new List<User>();



        public UserControllerTests()
        {
            var WebApp = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<UserDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<UserDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("DB");
                    });

                    var DbContext = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<UserDbContext>();

                    DbContext.Database.EnsureDeleted();

                    for (int i = 0; i < 5; i++)
                    {
                        User NewUser = new User(Guid.NewGuid(), $"TestName{i}", $"TestPassword{i}");
                        DbContext.Add(NewUser);
                        DB_UserList.Add(NewUser);
                    }

                    DB_UserList.Sort(delegate (User F, User S)
                    {
                        return F.id.CompareTo(S.id);
                    });

                    DbContext.SaveChanges();
                });
            });

            Client = WebApp.CreateClient();
        }



        [Fact]
        public async void GetAll_Ok()
        {
            var response = await Client.GetAsync($"/api/User");

            Assert.True(response.StatusCode == HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            List<UserDto> UserList = JsonConvert.DeserializeObject<List<UserDto>>(responseContent);

            Assert.NotNull(UserList);
            Assert.NotEmpty(UserList);
            Assert.True(Equal(UserList, DB_UserList));
        }

        [Fact]
        public async void GetById_Ok()
        {
            foreach (var DB_User in DB_UserList)
            {
                var response = await Client.GetAsync($"/api/User/{DB_User.id}");

                Assert.True(response.StatusCode == HttpStatusCode.OK);

                var responseContent = await response.Content.ReadAsStringAsync();

                Assert.NotNull(responseContent);

                UserDto User = JsonConvert.DeserializeObject<UserDto>(responseContent);

                Assert.NotNull(User);

                Assert.True(Equal(User, DB_User));
            }
        }

        [Fact]
        public async void GetById_InvalidId_Incorrect()
        {
            var response = await Client.GetAsync($"/api/User/incorrect");

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void GetById_InvalidId_NonExistent()
        {
            var response = await Client.GetAsync($"/api/User/{Guid.NewGuid()}");

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Create_Created()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName1",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.NotNull(responseContent);

            UserDto User = JsonConvert.DeserializeObject<UserDto>(responseContent);

            Assert.NotNull(User);
            Assert.True(User.name == "CreatedName1");
            Assert.True(User.password == "CreatedPassword1");

            DB_UserList.Add(
                new User() {
                    id = User.id,
                    name = User.name,
                    password = User.password,
                });

            DB_UserList.Sort(delegate (User F, User S)
            {
                return F.id.CompareTo(S.id);
            });
        }

        [Fact]
        public async void Create_InvalidName_IsEmpty()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_IsEmpty()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_LessThanRequiredLength()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoUpperCaseLetters()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoLowerCaseLetters()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter", responseContent);
        }

        [Fact]
        public async void Create_InvalidPassword_NoDigits()
        {
            UserForCreateDto UserForCreate = new UserForCreateDto()
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForCreate), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"/api/User/", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit", responseContent);
        }

        [Fact]
        public async void Delete_Deleted()
        {
            User UserForDelete = DB_UserList.First();
            var response = await Client.DeleteAsync($"/api/User/{UserForDelete.id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            DB_UserList.Remove(UserForDelete);

            DB_UserList.Sort(delegate (User F, User S)
            {
                return F.id.CompareTo(S.id);
            });
        }

        [Fact]
        public async void Delete_InvalidId_Incorrect()
        {
            var response = await Client.DeleteAsync($"/api/User/incorrect");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void Delete_InvalidId_NonExistent()
        {
            var response = await Client.DeleteAsync($"/api/User/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void Update_Updated()
        {
            User User = DB_UserList.First();

            UserForCreateDto UserForUpdate = new UserForCreateDto()
            {
                name = "UpdatedName1",
                password = "UpdatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            User.name = UserForUpdate.name;
            User.password = UserForUpdate.password;

            DB_UserList.Sort(delegate (User F, User S)
            {
                return F.id.CompareTo(S.id);
            });
        }

        [Fact]
        public async void Update_InvalidId_Incorrect()
        {

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/incorrect", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async void Update_InvalidId_NonExistent()
        {
            Guid UserId = Guid.NewGuid();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "UpdateName1",
                password = "UpdatePassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{UserId}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async void Update_InvalidName_IsEmpty()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "",
                password = "CreatedPassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Name is required", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_IsEmpty()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password is required", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_LessThanRequiredLength()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TestP1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must be at least 8 characters long", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoUpperCaseLetters()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "testpassword1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an uppercase letter", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoLowerCaseLetters()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TESTPASSWORD1",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain an lowercase letter", responseContent);
        }

        [Fact]
        public async void Update_InvalidPassword_NoDigits()
        {
            User User = DB_UserList.First();

            UserForUpdateDto UserForUpdate = new UserForUpdateDto()
            {
                name = "CreatedName",
                password = "TestPassword",
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(UserForUpdate), Encoding.UTF8, "application/json");

            var response = await Client.PutAsync($"/api/User/{User.id}", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password must contain a digit", responseContent);
        }



        private bool Equal(UserDto F, User S)
        {
            return (F.id == S.id && F.name == S.name && F.password == S.password);
        }

        private bool Equal(List<UserDto> F, List<User> S)
        {
            if (F.Count != S.Count) return false;

            for (int i = 0; i < F.Count; i++)
                if (!Equal(F[i], S[i])) return false;

            return true;
        }
    }
}