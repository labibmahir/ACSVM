using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Card : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public string CardNumber { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }
}