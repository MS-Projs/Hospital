using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models;

public class EntityWithState : Entity
{
    [Column("value_ru", TypeName = "varchar(200)")]
    public string ValueRu { get; set; } = default!;

    [Column("value_uz", TypeName = "varchar(200)")]
    public string ValueUz { get; set; } = default!;

    [Column("value_uzl", TypeName = "varchar(200)")]
    public string ValueUzl { get; set; } = default!;

    [Column("value_en", TypeName = "varchar(200)")]
    public string ValueEn { get; set; }

    [NotMapped]
    public string this[string language]
        => language switch
        {
            "en" => ValueEn,
            "uz" => ValueUz,
            "uzl" => ValueUzl,
            _ => ValueRu
        };
}