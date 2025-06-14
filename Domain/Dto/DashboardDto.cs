using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class DashboardDto
    {
        public int TotalDevices { get; set; }
        public int TotalActiveDevices { get; set; }
        public int TotalInActiveDevices { get; set; }
        public int TotalOnlineDevices { get; set; }
        public int TotalOfflineDevices { get; set; }
        public int TotalEmployee { get; set; }
        public int TotalAppointment { get; set; }
        public int ActiveAppointment { get; set; }
        public int CancelledAppointment { get; set; }
    }
}
