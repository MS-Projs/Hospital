using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public partial class EntityContext(
    DbContextOptions<EntityContext> options) : DbContext(options)
{
}