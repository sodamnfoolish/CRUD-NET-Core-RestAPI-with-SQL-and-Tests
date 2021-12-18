using Microsoft.EntityFrameworkCore;
using ProjectRestApi.Entities;

namespace ProjectRestApi.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
