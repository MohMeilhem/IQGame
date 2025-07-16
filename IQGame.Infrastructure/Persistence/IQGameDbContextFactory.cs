using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IQGame.Infrastructure.Persistence
{
    public class IQGameDbContextFactory : IDesignTimeDbContextFactory<IQGameDbContext>
    {
        public IQGameDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<IQGameDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new IQGameDbContext(optionsBuilder.Options);
        }
    }
} 