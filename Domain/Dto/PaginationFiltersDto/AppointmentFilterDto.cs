using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class AppointmentFilterDto : PaginationDto
    {
        public DateTime? AppointmentDate { get; set; }

        public bool? IsCompleted { get; set; }

        public string? VisitorPhone { get; set; }

        public bool? IsCancelled { get; set; }
        public Guid? VisitorId { get; set; }

    }
}
