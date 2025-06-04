using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class CardDto
    {
        public Guid Oid { get; set; }
        
        public string Name { get; set; }

        [Required]
        public string CardNumber { get; set; }
    
    }
}
