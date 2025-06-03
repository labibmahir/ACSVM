using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto.PaginationFiltersDto
{
    public class UserAccountAdvanceFilterDto : PaginationDto
    {
        public string? FullName { get; set; }


        public string? Email { get; set; }

        public string? CellPhoneAndCountryCode { get; set; }

        public Enums.Role? RoleId { get; set; }
    }
}
