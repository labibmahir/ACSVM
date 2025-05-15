using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities.Constants;

namespace Domain.Entities;

public class IdentifiedAssignCard : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    public Guid? PersonId { get; set; }
    
    public Guid? VisitorId { get; set; }
    
    [Required]
    public Guid CardId { get; set; }
    
    [Required]
    public bool IsPermanent { get; set; }
    
    [ForeignKey("VisitorId")]
    public virtual Visitor Visitor { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
    
    [ForeignKey("CardId")]
    public virtual Card Card { get; set; }
}