using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class PersonImageDto
    {
        [Required]
        public Guid PersonId { get; set; }
        public IFormFile Image { get; set; }
    }
}
