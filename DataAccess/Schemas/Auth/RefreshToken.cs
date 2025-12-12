using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;

namespace DataAccess.Schemas.Auth;

[Table("refresh_tokens", Schema = "auth")]
public class RefreshToken : Entity
{
    [Column("user_id")]
    public long UserId { get; set; }
    
    [Column("token")]
    public string Token { get; set; } = null!;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }


    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}




