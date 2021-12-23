using Microsoft.EntityFrameworkCore;
using RestApi.Entities;

namespace RestApi.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
