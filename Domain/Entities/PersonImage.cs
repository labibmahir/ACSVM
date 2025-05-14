using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class PersonImage : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public string ImageBase64 { get; set; }
    
    [Required]
    public string ImageData { get; set; }
    
    [ForeignKey("Oid")]
    public virtual Person Person { get; set; }
}