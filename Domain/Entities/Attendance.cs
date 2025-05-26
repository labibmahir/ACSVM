using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Attendance : BaseModel
    {
        [Key]
        public Guid Oid { get; set; }

        [Required]
        [StringLength(50)]
        public Guid PersonId { get; set; }

        public DateTime? AuthenticationDateAndTime { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "smalldatetime")]
        public DateTime? AuthenticationDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? AuthenticationTime { get; set; }

        [StringLength(50)]
        public string? Direction { get; set; }

        public int DeviceId { get; set; }
        [StringLength(100)]
        public string? DeviceName { get; set; }

        [StringLength(50)]
        public string? DeviceSerialNo { get; set; }
        
        [StringLength(50)]
        public string? CardNo { get; set; }

        [ForeignKey("PersonId")]
        public virtual Person Person { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; }
    }
}
