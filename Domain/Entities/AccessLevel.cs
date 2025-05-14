using System.ComponentModel.DataAnnotations;
using Utilities.Constants;

namespace Domain.Entities;

public class AccessLevel : BaseModel
{
    [Key]
    public int Oid { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Description { get; set; }
    
    public virtual IEnumerable<Device> Devices { get; set; }
}