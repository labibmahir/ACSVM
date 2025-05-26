using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Utilities.Constants;

namespace Domain.Entities;

public class Visitor : BaseModel
{
    [Key]
    public Guid Oid { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string Surname { get; set; }
    [MaxLength(50)]
    public string VisitorNumber { get; set; }
    public string Email { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public Enums.Gender Gender { get; set; }
    public string Address { get; set; }

    public string CompanyName { get; set; }
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ValidateStartPeriod { get; set; }

    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ValidateEndPeriod { get; set; }

    public Enums.UserVerifyMode? UserVerifyMode { get; set; }
    
    [JsonIgnore]
    public virtual IEnumerable<Appointment> Appointments { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<FingerPrint> FingerPrints { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<PersonImage> PersonImages { get; set; }
}