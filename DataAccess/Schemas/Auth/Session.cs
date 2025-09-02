using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Auth;

[Table("sessions", Schema = "auth")]
[Index(nameof(UserId))]
[Index(nameof(Code), IsUnique = true)]
public class Session : Entity
{
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("code")]
    public string Code { get; set; }

    [Column("expire_date")]
    public DateTime ExpireDate { get; set; }
}