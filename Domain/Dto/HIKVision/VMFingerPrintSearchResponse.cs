using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintSearchResponse
    {
        /// <summary>
        /// required, string, search ID, which is used to confirm the upper-level platform or system. If the platform or the
        /// system is the same one during two searching, the search history will be saved in the memory to speed up next
        /// searching
        /// </summary>
        public string searchID { get; set; } = string.Empty;

        /// <summary>
        /// required, string, status: "OK"-the fingerprint exists, "NoFP"-the fingerprint does not exist
        /// </summary>
        public string status { get; set; } = string.Empty;

        public List<VMFingerPrintListItem>? FingerPrintList { get; set; } = new List<VMFingerPrintListItem>();
    }
}
