using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintSearchRequest
    {
        /// <summary>
        /// required, string, search ID, which is used to confirm the upper-level platform or system. If the platform or the
        /// system is the same one during two searching, the search history will be saved in the memory to speed up next
        /// searching
        /// </summary>
        public string searchID { get; set; } = string.Empty;

        /// <summary>
        /// required, string, employee No. (person ID) linked with the fingerprint
        /// </summary>
        public string employeeNo { get; set; } = string.Empty;

        /// <summary>
        /// optional, integer, fingerprint module No.
        /// </summary>
        public int? cardReaderNo { get; set; }

        /// <summary>
        /// optional, integer, fingerprint No., which is between 1 and 10
        /// </summary>
        public int? fingerPrintID { get; set; }
    }
}
