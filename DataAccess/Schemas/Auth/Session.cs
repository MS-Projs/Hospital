using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Auth;

[Table("sessions", Schema = "auth")]
[Index(nameof(UserId))]
public class Session : Entity
{
    [Column("user_id")]
    public long? UserId { get; set; }
    
    [Column("otp_code")]
    public string OtpCode { get; set; } = null!;

    [Column("otp_expire_date")]
    public DateTime OtpExpireDate { get; set; }

    [Column("is_verified")]
    public bool IsVerified { get; set; }
    
    [Column("session_expire_date")]
    public DateTime? SessionExpireDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}