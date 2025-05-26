using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Entities;

public class AccessLevel : BaseModel
{
    [Key]
    public int Oid { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Description { get; set; }
    
    [JsonIgnore]
    public virtual IEnumerable<Device> Devices { get; set; }
}