using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;

namespace DataAccess.Schemas.Public;

public class CertificateType : EntityWithState
{
    [Column("keyword", TypeName = "varchar(40)")]
    public string KeyWord { get; set; }
    
}