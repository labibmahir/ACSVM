using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class DeviceFilterDto : PaginationDto
    {

        public string? DeviceName { get; set; }

        public string? ModelName { get; set; }
        public string? MacAddress { get; set; }
        public string? FirmwareVersion { get; set; }
        public DateTime? FirmwareReleasedDate { get; set; }
        public string? SerialNumber { get; set; }

        public string? DeviceIP { get; set; }
        public string? DeviceLicence { get; set; }

        public bool? IsActive { get; set; }
    }
}
