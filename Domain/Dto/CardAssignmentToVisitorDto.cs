using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class CardAssignmentToVisitorDto
    {
        public Guid VisitorId { get; set; }
        public Guid CardId { get; set; }
        [Required]
        public bool IsPermanent { get; set; }
    }
}
