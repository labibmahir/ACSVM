﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class PersonNoAndPernameNameFilterDto : PaginationDto
    {
        public string? FullName { get; set; }

        public string? PersonNumber { get; set; }
    }
}
