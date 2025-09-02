using DataAccess.Schemas.Auth;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public partial class EntityContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Session> Sessions { get; set; }

}