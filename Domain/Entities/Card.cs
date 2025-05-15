using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities.Constants;

namespace Domain.Entities;

public class Card : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public string CardNumber { get; set; }
    
    [Required]
    public Enums.Status Status { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }
}