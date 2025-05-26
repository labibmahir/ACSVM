using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Appointment : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime AppointmentDate { get; set; }
    
    [Required]
    [Column(TypeName = "time(7)")]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    [Column(TypeName = "time(7)")]
    public TimeSpan EndTime { get; set; }
    
    [Required]
    public bool IsCompleted { get; set; }
    
    [Required]
    public bool IsCancelled { get; set; }
    
    [Required]
    public Guid VisitorId { get; set; }
    
    [ForeignKey("VisitorId")]
    public virtual Visitor Vistor { get; set; }
    
    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignedAppointment> IdentifiedAssignedAppointments { get; set; }
}