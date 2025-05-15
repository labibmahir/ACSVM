using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities.Constants;

namespace Domain.Entities;

public class FingerPrint : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public string Data { get; set; }
    
    [Required]
    public Enums.FingerNumber FingerNumber { get; set; }
    
    public Guid? PersonId { get; set; }
    
    public Guid? VisitorId { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
    
    [ForeignKey("VisitorId")]
    public virtual Visitor Visitor { get; set; }
}