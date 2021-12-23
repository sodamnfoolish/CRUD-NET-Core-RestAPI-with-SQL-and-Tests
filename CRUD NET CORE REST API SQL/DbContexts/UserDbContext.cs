using Microsoft.EntityFrameworkCore;
using ProjectRestApi.Entities;

namespace ProjectRestApi.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User[]
                {
                    new User(new Guid("44305c10-797f-436a-b725-07f3e9859da5"), "TestName1", "TestPassword1"),
                    new User(new Guid("b8cf1b6b-9659-4d6d-94c1-27472d76f457"), "TestName2", "TestPassword2"),
                    new User(new Guid("46c61a55-072e-4aae-8ab9-2b4e9a9f6f71"), "TestName3", "TestPassword3"),
                    new User(new Guid("55176675-bd33-419d-95cd-44959818c52e"), "TestName4", "TestPassword4"),
                    new User(new Guid("aa164c2e-8afc-4f96-9cb9-45d2502a6303"), "TestName5", "TestPassword5"),
                });
        }
    }
}
