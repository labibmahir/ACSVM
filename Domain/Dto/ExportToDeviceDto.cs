using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ExportToDeviceDto
    {
        [Required]
        public int FromDeviceId { get; set; }
        [Required]
        public int ToDeviceId { get; set; }
    }
}
