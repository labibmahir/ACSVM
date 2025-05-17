using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class CardAssignmentDto
    {
        public Guid PersonId { get; set; }
        public Guid CardId { get; set; }
        [Required]
        public bool IsPermanent { get; set; }
    }
}
