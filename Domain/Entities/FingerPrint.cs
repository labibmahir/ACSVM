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
    
    [Required]
    public Guid PersonId { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
}