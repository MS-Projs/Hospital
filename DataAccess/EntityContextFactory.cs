using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess;

public class EntityContextFactory : IDesignTimeDbContextFactory<EntityContext>
{
    public EntityContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EntityContext>();
        optionsBuilder.UseNpgsql( "Host=localhost;Port=5432;Database=MyMd_db;Username=postgres;Password=postgres");

        return new EntityContext(optionsBuilder.Options);
    }
}
