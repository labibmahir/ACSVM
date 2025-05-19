using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public record EmployeeDetailDto
    {
        public string EmployeeNo { get; set; }

        public int FingerPrintId { get; set; }

        public int FingerPrintNumber { get; set; }

       // public int[] DeviceIdList { get; set; }
    }
}
