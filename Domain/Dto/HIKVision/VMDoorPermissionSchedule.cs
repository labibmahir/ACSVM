using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMDoorPermissionSchedule
    {
        /// <summary>
        /// optional, integer, door No. (lock ID)
        /// </summary>
        public int? doorNo { get; set; } = null;

        public string? planTemplateNo { get; set; } = null;
    }
}
