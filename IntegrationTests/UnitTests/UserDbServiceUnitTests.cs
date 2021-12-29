/*
 *                  NOT IMPLEMENTED
 *                  
using Xunit;
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
using RestApi.Services;
using RestApi.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Infrastructure;
using MockQueryable.Moq;
using System.Threading.Tasks;

namespace UnitTests
{
    public class UserDbServiceUnitTests
    {
        private List<User> _dbUserList;

        public UserDbServiceUnitTests()
        {
            _dbUserList = new List<User>();

            for (int i = 0; i < 5; i++)
            {
                var user = new User
                {
                    id = Guid.NewGuid(),
                    name = $"TestName{i}",
                    password = $"TestPassword{i}"
                };
                _dbUserList.Add(user);
            }

            _dbUserList.Sort(delegate (User firstUser, User secondUser)
            {
                return firstUser.id.CompareTo(secondUser.id);
            });
        }

        [Fact]
        public async void GetAll_Ok()
        {
            var mockedUserDbContext = await CreateUserDbContextWithMockedDbSet();

            var service = new UserDbService(mockedUserDbContext);

            var response = await service.GetAll();

            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.True(Equal(_dbUserList, response));
        }

        [Fact]
        public async void GetById_Ok()
        {
            var mockedUserDbContext = await CreateUserDbContextWithMockedDbSet();

            var service = new UserDbService(mockedUserDbContext);

            foreach (var user in _dbUserList)
            {
                var response = await service.GetById(user.id);

                Assert.NotNull(response);
                Assert.True(Equal(user, response));
            }
        }

        private bool Equal(User firstUser, User secondUser)
        {
            return firstUser.id == secondUser.id && firstUser.name == secondUser.name && firstUser.password == secondUser.password;
        }

        private bool Equal(List<User> firstUserList, List<User> secondUserList)
        {
            if (firstUserList.Count != secondUserList.Count) return false;

            for (int i = 0; i < firstUserList.Count; i++)
                if (!Equal(firstUserList[i], secondUserList[i])) return false;

            return true;
        }

        private async Task<UserDbContext> CreateUserDbContextWithMockedDbSet()
        {
            var clonedDbUserList = new List<User>();
            _dbUserList.ForEach(user => clonedDbUserList.Add(new User { id = user.id, name = user.name, password = user.password }));

            var mockDbSet = clonedDbUserList.AsQueryable().BuildMockDbSet();

            var options = new DbContextOptionsBuilder<UserDbContext>().UseInMemoryDatabase(databaseName: "DB_UserDbServiceUnitTests").Options;
            var DbContext = new UserDbContext(options);

            DbContext.Users = mockDbSet.Object;

            await DbContext.SaveChangesAsync();

            return DbContext;
        }
    }
}*/