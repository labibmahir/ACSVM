using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintEmployeeNumberDetail
    {
        /// <summary>
        /// optional, string, employee No. (person ID) linked with the fingerprint
        /// </summary>
        public string employeeNo { get; set; } = string.Empty;

        /// <summary>
        /// optional, array, fingerprint module whose fingerprints should be deleted, e.g., [1,3,5] indicates that the fingerprints
        /// of fingerprint modules No.1, No.3, and No.5 are deleted
        /// </summary>
        //[JsonIgnore]
        //public int[] enableCardReader { get; set; } = new int[0];

        /// <summary>
        /// optional, array, No. of fingerprint to be deleted, e.g., [1,3,5] indicates deleting fingerprint No.1, No.3, and No.5
        /// </summary>
        public int[] fingerPrintID { get; set; } = new int[0];
    }
}
