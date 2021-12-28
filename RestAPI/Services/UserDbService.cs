using Microsoft.EntityFrameworkCore;
using RestApi.DbContexts;
using RestApi.Dtos;
using RestApi.Entities;
using RestApi.Interfaces;

namespace RestApi.Services
{
    public class UserDbService : IUserDbService
    {
        private readonly UserDbContext _userDbContext;



        public UserDbService(UserDbContext userDbContext)
        {
            this._userDbContext = userDbContext;
        }



        public async Task<List<User>> GetAll()
        {
            var userList = await _userDbContext.Users.ToListAsync();

            userList.Sort(delegate(User firstUser, User secondUser)
            {
                return firstUser.id.CompareTo(secondUser.id);
            });

            return userList;
        }



        public async Task<User> Create(User user)
        {
            _userDbContext.Users.Add(user);

            var saved = await _userDbContext.SaveChangesAsync();

            if (saved == 0) return null;

            return user;
        }



        public async Task<User> GetById(Guid id)
        {
            var user = await _userDbContext.Users.FindAsync(id);

            return user;
        }



        public async Task<User> Delete(User user)
        {
            var deletedUser = new User
            {
                id = user.id,
                name = user.name,
                password = user.password,
            };

            _userDbContext.Users.Remove(user);

            var saved = await _userDbContext.SaveChangesAsync();

            if (saved == 0) return null;

            return deletedUser;
        }



        public async Task<User> Update(User user)
        {
            _userDbContext.Users.Update(user);

            var saved = await _userDbContext.SaveChangesAsync();

            if (saved == 0) return null;

            return user;
        }
    }
}
