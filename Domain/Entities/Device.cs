using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Device : BaseModel
{
    [Key]
    public int Oid { get; set; }

    [Required]
    [StringLength(50)]
    public string DeviceName { get; set; }

    [Required]
    [StringLength(50)]
    public string ModelName { get; set; }

    [StringLength(250)]
    public string MacAddress { get; set; }

    [StringLength(250)]
    public string FirmwareVersion { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? FirmwareReleasedDate { get; set; }

    [StringLength(250)]
    public string SerialNumber { get; set; }

    [StringLength(20)]
    public string DeviceIP { get; set; }

    [StringLength(6)]
    public string Port { get; set; }

    [StringLength(30)]
    public string Username { get; set; }

    [StringLength(30)]
    public string Password { get; set; }

    [StringLength(50)]
    public string DeviceLicence { get; set; }

    public bool IsActive { get; set; }

    public int? AccessLevelId { get; set; }

    [ForeignKey("AccessLevelId")]
    public virtual AccessLevel AccessLevel { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<IdentifiedAssignDevice> IdentifiedAssignDevices { get; set; }

    [NotMapped]
    public bool? DeviceCurrectActiveStatus { get; set; }
}