using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintDeleteProccess
    {
        /// <summary>
        /// required, string, deleting status: "processing"-deleting, "success"-deleted, "failed"-deleting failed
        /// </summary>
        public string? status { get; set; }
    }
}
