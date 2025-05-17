using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMEffectivePeriod
    {
        /// <summary>
        /// required, boolean, whether to enable the effective period: "false"-disable, "true"-enable. If this node is set to
        /// "false", the effective period is permanent
        /// </summary>
        public bool enable { get; set; } = false;

        /// <summary>
        /// required, start time of the effective period (if timeType does not exist or is "local", the beginTime is the device local
        /// time, e.g., 2017-08-01T17:30:08; if timeType is "UTC", the beginTime is UTC time, e.g., 2017-08-01T17:30:08+08:00)
        /// </summary>
        public string? beginTime { get; set; }

        /// <summary>
        /// required, end time of the effective period (if timeType does not exist or is "local", the endTime is the device local
        /// time, e.g., 2017-08-01T17:30:08; if timeType is "UTC", the endTime is UTC time, e.g., 2017-08-01T17:30:08+08:00)
        /// </summary>
        public string? endTime { get; set; }

        /// <summary>
        /// optional, string, time type: "local"- device local time, "UTC"- UTC time
        /// </summary>
        public string? timeType { get; set; } = null;
    }
}
