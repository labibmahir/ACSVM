using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto
{
   public class VisitorFingerPrintDto
    {
        public Guid Oid { get; set; }

        [Required]
        public string Data { get; set; }

        [Required]
        public Enums.FingerNumber FingerNumber { get; set; }

        [Required]
        public Guid VisitorId { get; set; }


        public int? OrganizationId { get; set; }
    }
}
