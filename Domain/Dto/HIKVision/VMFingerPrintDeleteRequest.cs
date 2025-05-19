using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintDeleteRequest
    {
        /// <summary>
        /// required, string, deleting mode: "byEmployeeNo"-delete by employee No. (person ID), "byCardReader"-delete by
        /// fingerprint module
        /// </summary>
        public string mode { get; set; } = "byEmployeeNo";

        /// <summary>
        /// optional, delete by employee No. (person ID), this node is valid when mode is "byEmployeeNo"
        /// </summary>
        public VMFingerPrintEmployeeNumberDetail EmployeeNoDetail { get; set; }
    }
}
