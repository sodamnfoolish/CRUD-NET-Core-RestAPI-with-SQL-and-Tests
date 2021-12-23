using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ProjectRestApi.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace INTEGRATION_TESTS
{
    public class TestingWebApplicationFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(Services =>
            {
                var descriptor = Services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<UserDbContext>));

                if (descriptor != null)
                    Services.Remove(descriptor);

                Services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DB");
                });

                Services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<UserDbContext>().Database.EnsureCreated();
            });
        }
    }
}
