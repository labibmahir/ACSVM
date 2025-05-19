using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    public class CapturedFingerPrintDto
    {
        /// <summary>
        /// dep, xs:string, fingerprint data, which is between 1 and 768, and it should be encoded by Base64
        /// </summary>
        public string? FingerData { get; set; }

        /// <summary>
        /// req, xs:integer, finger No., which is between 1 and 10
        /// </summary>
        public int? FingerNumber { get; set; }

        /// <summary>
        /// req, xs:integer, fingerprint quality, which is between 1 and 100
        /// </summary>
        public int? FingerPrintQuality { get; set; }
    }
}
