using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class VisitorAttendanceFilterDto : PaginationDto
    {
        public DateTime? AttendanceDate { get; set; }
        public string? FullName { get; set; }
        public string? VisitorNumber { get; set; }
    }
}
