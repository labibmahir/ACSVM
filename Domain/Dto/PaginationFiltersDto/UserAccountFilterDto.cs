using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class UserAccountFilterDto : PaginationDto
    {
        public string? Firstname { get; set; }

        public string? Surname { get; set; }

        public string? Email { get; set; }

        public string? CountryCode { get; set; }

        public string? CellPhone { get; set; }

        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }


    }
}
