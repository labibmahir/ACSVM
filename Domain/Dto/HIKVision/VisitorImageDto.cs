﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VisitorImageDto
    {
        [Required]
        public Guid VisitorId { get; set; }
        public IFormFile Image { get; set; }
    }
}
