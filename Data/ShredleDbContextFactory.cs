using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShredleApi.Data
{
    // This factory is used by EF Core tools for migrations
    public class ShredleDbContextFactory : IDesignTimeDbContextFactory<ShredleDbContext>
    {
        public ShredleDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShredleDbContext>();
            
            // Default connection string for migrations
            var connectionString = "Host=localhost;Database=shredle;Username=postgres;Password=postgres";
            
            optionsBuilder.UseNpgsql(connectionString);

            return new ShredleDbContext(optionsBuilder.Options);
        }
    }
}
