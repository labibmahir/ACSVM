using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class AlarmInfo
    {
        public string ipAddress { get; set; }

        public string ipv6Address { get; set; }

        public int portNo { get; set; }

        public string protocol { get; set; }

        public string macAddress { get; set; }

        public string channelID { get; set; }

        public string dateTime { get; set; }

        public int activePostCount { get; set; }

        public string eventType { get; set; }

        public string eventState { get; set; }

        public string eventDescription { get; set; }

        public AccessControllerEvent AccessControllerEvent { get; set; }
        public Device? Device { get; set; }

    }
}
