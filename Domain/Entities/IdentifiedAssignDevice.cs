using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class IdentifiedAssignDevice : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public int DeviceId { get; set; }
    
    public Guid? PersonId { get; set; }
    
    public Guid? VisitorId { get; set; }
    
    [ForeignKey("VisitorId")]
    public virtual Visitor Visitor { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
    
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; }
}