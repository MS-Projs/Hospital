using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;

namespace DataAccess.Schemas.Public;

[Table("document_categories", Schema = "public")]
public class DocumentCategory : EntityWithState
{
    [Column("keyword", TypeName = "varchar(40)")]
    public string KeyWord { get; set; } = default!;
}