using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Slot : BaseModel
{
    [Key]
    public int Oid { get; set; }
    
    [Required]
    [Column(TypeName = "time(7)")]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    [Column(TypeName = "time(7)")]
    public TimeSpan EndTime { get; set; }
    
    public virtual IEnumerable<Appointment> Appointments { get; set; }
}