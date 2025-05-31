using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.Constants.Enums;

namespace Domain.Entities
{
    public class IdentifiedSyncDevice : BaseModel
    {
        [Key]
        public Guid Oid { get; set; }
        [Required]
        public Guid DeviceSynchronizerId { get; set; }
        public int DeviceId { get; set; }
        public int TryCount { get; set; }
        public DeviceAction Action { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; }
    }
}
