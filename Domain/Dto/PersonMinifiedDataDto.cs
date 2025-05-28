using Domain.Dto.PaginationFiltersDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class PersonMinifiedDataDto
    {
        public Guid Oid { get; set; }
        public string FullName { get; set; }
        public string PersonNumber { get; set; }
        public string? Department { get; set; }
    }
}
