using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintListItem
    {
        /// <summary>
        /// required, integer, fingerprint module No.
        /// </summary>
        public int cardReaderNo { get; set; }

        /// <summary>
        /// required, integer, fingerprint No., which is between 1 and 10
        /// </summary>
        public int fingerPrintID { get; set; }

        /// <summary>
        /// required, string, fingerprint type: "normalFP"-normal fingerprint, "hijackFP"-duress fingerprint, "patrolFP"-patrol
        /// fingerprint, "superFP"-super fingerprint, "dismissingFP"-dismiss fingerprint
        /// </summary>
        public string fingerType { get; set; } = string.Empty;

        /// <summary>
        /// required, string, fingerprint data encoded by Base64
        /// </summary>
        public string fingerData { get; set; } = string.Empty;

        /// <summary>
        /// optional, array, whether the access control points support first fingerprint authentication function, e.g., [1,3,5]
        /// indicates that access control points No.1, No.3, and No.5 support first fingerprint authentication function
        /// </summary>
        public int[] leaderFP { get; set; } = new int[0];
    }
}
