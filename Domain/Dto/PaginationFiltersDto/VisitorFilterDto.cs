using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto.PaginationFiltersDto
{
    public class VisitorFilterDto : PaginationDto
    {
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? VisitorNumber { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public Enums.Gender? Gender { get; set; }
        public Enums.UserVerifyMode? UserVerifyMode { get; set; }
    }
}
