using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Appointment : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime AppointmentDate { get; set; }
    
    [Required]
    public int SlotId { get; set; }
    
    [Required]
    public Guid VisitorId { get; set; }
    
    [ForeignKey("SlotId")]
    public virtual Slot Slot { get; set; }
    
    [ForeignKey("VisitorId")]
    public virtual Visitor Vistor { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignedAppointment> IdentifiedAssignedAppointments { get; set; }
}