
using System.ComponentModel.DataAnnotations.Schema;

namespace BoltonCup.Core;

public abstract class EntityBase
{
    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("last_modified")]
    public DateTime? LastModified { get; set; }

    [Column("last_modified_by")]
    public string? LastModifiedBy { get; set; }
}