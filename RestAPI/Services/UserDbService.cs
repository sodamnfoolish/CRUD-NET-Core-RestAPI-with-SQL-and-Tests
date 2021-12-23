using Microsoft.EntityFrameworkCore;
using RestApi.DbContexts;
using RestApi.Entities;
using RestApi.Interfaces;

namespace RestApi.Services
{
    public class UserDbService : IUserDbService
    {
        public UserDbContext UserDbContext;



        public UserDbService(UserDbContext UserDbContext)
        {
            this.UserDbContext = UserDbContext;
        }



        public async Task<List<User>> GetAll()
        {
            List <User> UserList = await UserDbContext.Users.ToListAsync();

            UserList.Sort(delegate(User First, User Second)
            {
                return First.id.CompareTo(Second.id);
            });

            return UserList;
        }



        public async Task<User> GetById(Guid id)
        {
            User User = await UserDbContext.Users.FindAsync(id);

            return User;
        }



        public async Task<User> Create(User User)
        {
            User.id = Guid.NewGuid();

            UserDbContext.Users.Add(User);

            int IsSaved = await UserDbContext.SaveChangesAsync();

            return IsSaved == 0 ? null : User;
        }



        public async Task<bool> Delete(User User)
        {
            UserDbContext.Users.Remove(User);

            var IsDeleted = await UserDbContext.SaveChangesAsync();

            return IsDeleted == 0 ? false : true;
        }



        public async Task<bool> Update(Guid id, User NewUser)
        {
            if (await this.GetById(id) == null) return false;

            var OldUser = await UserDbContext.Users.FindAsync(id);

            OldUser.name = NewUser.name;
            OldUser.password = NewUser.password;

            UserDbContext.Entry(OldUser).State = EntityState.Modified; 

            var IsUpdated = await UserDbContext.SaveChangesAsync();

            return IsUpdated == 0 ? false : true;
        }
    }
}
