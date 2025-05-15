using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Visitor : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Address { get; set; }
    
    public string CompanyName { get; set; }
    
    public virtual IEnumerable<Appointment> Appointments { get; set; }
    
    public virtual IEnumerable<FingerPrint> FingerPrints { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }
    
    public virtual IEnumerable<PersonImage> PersonImages { get; set; }
}