using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintSetUpResponse
    {
        /// <summary>
        /// optional, string, status: "success", "failed". This node will be returned only when editing fingerprint parameters or
        /// deleting fingerprints; for applying fingerprint data to the fingerprint module, this node will not be returned
        /// </summary>
        public string? status { get; set; }

        /// <summary>
        /// optional, status list. This node will be returned only when applying fingerprint data to the fingerprint module; for
        /// editing fingerprint parameters or deleting fingerprints, this node will not be returned
        /// </summary>
        public List<VMFingerPrintSetUpStatusListItem>? StatusList { get; set; }

        /// <summary>
        /// required, integer, applying status: 0-applying, 1-applied
        /// </summary>
        public int totalStatus { get; set; }
    }
}
