using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class IdentifiedAssignDevice : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public int DeviceId { get; set; }
    
    [Required]
    public Guid PersonId { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
    
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; }
}