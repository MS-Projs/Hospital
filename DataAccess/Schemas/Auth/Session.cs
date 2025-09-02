using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;

namespace DataAccess.Schemas.Auth;

[Table("sessions", Schema = "auth")]
public class Session : Entity
{
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("code")]
    public string Code { get; set; }

    [Column("expire_date")]
    public DateTime ExpireDate { get; set; }
}