using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Enums;

namespace DataAccess.Models;

public class Entity
{
    [Column("id")]
    public long Id { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("updated_date")]
    public DateTime? UpdatedDate { get; set; }

    [Column("status")]
    public EntityStatus Status { get; set; }
}