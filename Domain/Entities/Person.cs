using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities.Constants;

namespace Domain.Entities;

public class Person : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Surname { get; set; }
    
    [MaxLength(50)]
    public string PersonNumber { get; set; }
    
    [MaxLength(20)]
    public string Email { get; set; }
    
    [MaxLength(15)]
    public string PhoneNumber { get; set; }
    
    [Required]
    public Enums.Gender Gender { get; set; }
    
    [Required]
    public bool IsDeviceAdministrator { get; set; }
    
    [Required]
    public Enums.PersonType PersonType{ get; set; }
    
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ValidateStartPeriod { get; set; }
    
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime? ValidateEndPeriod { get; set; }
    
    public Enums.UserVerifyMode? UserVerifyMode { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }
    
    public virtual IEnumerable<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }
    
    public virtual IEnumerable<FingerPrint> FingerPrints { get; set; }
    
    public virtual IEnumerable<PersonImage> PeopleImages { get; set; }
}