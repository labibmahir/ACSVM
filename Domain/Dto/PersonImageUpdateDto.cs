using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class PersonImageUpdateDto
    {
        [Required]
        public Guid Oid { get; set; }
        public IFormFile Image { get; set; }
    }
}
