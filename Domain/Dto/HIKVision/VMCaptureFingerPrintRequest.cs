using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMCaptureFingerPrintRequest
    {
        /// <summary>
        /// req, xs: integer, finger No., which is between 1 and 10
        /// </summary>
        public int? fingerNo { get; set; }
    }
}
