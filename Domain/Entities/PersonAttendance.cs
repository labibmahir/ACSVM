using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PersonAttendance : BaseModel
    {
        [Key]
        public Guid Oid { get; set; }

        [StringLength(50)]
        public Guid? PersonId { get; set; }
        public Guid? VisitorId { get; set; }

        public DateTime? AuthenticationDateAndTime { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "smalldatetime")]
        public DateTime? AuthenticationDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? AuthenticationTime { get; set; }

        [StringLength(50)]
        public string? Direction { get; set; }

        [StringLength(100)]
        public string? DeviceName { get; set; }

        [StringLength(50)]
        public string? DeviceSerialNo { get; set; }
        [StringLength(50)]
        public string? CardNo { get; set; }

        public bool? Sync { get; set; }

        public DateTime? CreationTime { get; set; }
    }
}
