using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
   public class PersonDeviceAssignAndUnAssignDto
    {
        public int[] DeviceIdList { get; set; }
        public Guid PersonId { get; set; }
    }
}
