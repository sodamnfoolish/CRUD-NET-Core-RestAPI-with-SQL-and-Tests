using Microsoft.EntityFrameworkCore;
using RestApi.DbContexts;
using RestApi.Entities;
using RestApi.Interfaces;

namespace RestApi.Services
{
    public class UserDbService : IUserDbService
    {
        public UserDbContext userDbContext;



        public UserDbService(UserDbContext _userDbContext)
        {
            this.userDbContext = _userDbContext;
        }



        public async Task<List<User>> GetAll()
        {
            List<User> userList = await userDbContext.Users.ToListAsync();

            userList.Sort(delegate(User _firstUser, User _secondUser)
            {
                return _firstUser.id.CompareTo(_secondUser.id);
            });

            return userList;
        }



        public async Task<User> GetById(Guid _id)
        {
            User user = await userDbContext.Users.FindAsync(_id);

            return user;
        }



        public async Task<User> Create(User _userForCreate)
        {
            _userForCreate.id = Guid.NewGuid();

            userDbContext.Users.Add(_userForCreate);

            int isSaved = await userDbContext.SaveChangesAsync();

            return isSaved == 0 ? null : _userForCreate;
        }



        public async Task<bool> Delete(User _userForDelete)
        {
            userDbContext.Users.Remove(_userForDelete);

            var isDeleted = await userDbContext.SaveChangesAsync();

            return isDeleted == 0 ? false : true;
        }



        public async Task<bool> Update(Guid _id, User _userForUpdate)
        {
            var user = await userDbContext.Users.FindAsync(_id);

            user.name = _userForUpdate.name;
            user.password = _userForUpdate.password;

            userDbContext.Entry(user).State = EntityState.Modified; 

            var isUpdated = await userDbContext.SaveChangesAsync();

            return isUpdated == 0 ? false : true;
        }
    }
}
