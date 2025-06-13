using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class DeviceLogFilterByDeviceDto : PaginationDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool? IsSync { get; set; } 
        public Guid? PersonId { get; set; }
        public Guid? VisitorId { get; set; }
    }
}
