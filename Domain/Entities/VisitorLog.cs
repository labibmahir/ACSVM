using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class VisitorLog : BaseModel
    {
        [Key]
        public Guid Oid { get; set; }

        //[Required]
        //[StringLength(50)]
        public Guid? VisitorId { get; set; }

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

        [ForeignKey("VisitorId")]
        public virtual Visitor Visitor { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; }
    }
}
