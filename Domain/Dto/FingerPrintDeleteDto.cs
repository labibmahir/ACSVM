using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto
{
    public class FingerPrintDeleteDto
    {
        public Guid Oid { get; set; }

        [Required]
        public Enums.FingerNumber FingerNumber { get; set; }
    }
}
