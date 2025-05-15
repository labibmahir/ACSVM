using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class IdentifiedAssignedAppointment : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public Guid AppointmentId { get; set; }
    
    [Required]
    public Guid PersonId { get; set; }
    
    [ForeignKey("PersonId")]
    public virtual Person Person { get; set; }
    
    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; }
}