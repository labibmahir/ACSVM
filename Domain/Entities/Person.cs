using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

    [MaxLength(50)]
    public string Email { get; set; }

    [MaxLength(15)]
    public string PhoneNumber { get; set; }

    [Required]
    public Enums.Gender Gender { get; set; }
    public string? Department { get; set; }

    [Required]
    public bool IsDeviceAdministrator { get; set; }

    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ValidateStartPeriod { get; set; }

    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime? ValidateEndPeriod { get; set; }

    public Enums.UserVerifyMode? UserVerifyMode { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignCard> IdentifiedAssignCards { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<FingerPrint> FingerPrints { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<PersonImage> PeopleImages { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignedAppointment> IdentifiedAssignedAppointments { get; set; }
}