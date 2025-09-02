using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;

namespace DataAccess.Schemas.Auth;

[Table("users", Schema = "auth")]
public class User : Entity
{
    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("password")]
    public string? Password { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }
}