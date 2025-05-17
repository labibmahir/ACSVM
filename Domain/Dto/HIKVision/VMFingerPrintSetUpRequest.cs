using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintSetUpRequest
    {
        /// <summary>
        /// required, string, employee No. (person ID) linked with the fingerprint
        /// </summary>
        public string employeeNo { get; set; } = string.Empty;

        /// <summary>
        /// required, array, fingerprint modules to apply fingerprint data to, e.g., [1,3,5] indicates applying fingerprint data to
        /// fingerprint modules No.1, No.3, and No.5
        /// </summary>
        public int[] enableCardReader { get; set; } = new int[0];

        /// <summary>
        /// required, integer, fingerprint No., which is between 1 and 10
        /// </summary>
        public int fingerPrintID { get; set; }

        /// <summary>
        /// optional, boolean, whether to delete the fingerprint: "true"-yes. This node is required only when the fingerprint
        /// needs to be deleted; for adding or editing fingerprint information, this node can be set to NULL
        /// </summary>
        public bool deleteFingerPrint { get; set; } = false;

        /// <summary>
        /// required, string, fingerprint type: "normalFP"-normal fingerprint, "hijackFP"-duress fingerprint, "patrolFP"-patrol
        /// fingerprint, "superFP"-super fingerprint, "dismissingFP"-dismiss fingerprint
        /// </summary>
        public string fingerType { get; set; } = "normalFP";

        /// <summary>
        /// required, string, fingerprint data encoded by Base64
        /// </summary>
        public string fingerData { get; set; } = string.Empty;

        /// <summary>
        /// optional, array, whether the access control points support first fingerprint authentication function, e.g., [1,3,5]
        /// indicates that access control points No.1, No.3, and No.5 support first fingerprint authentication function
        /// </summary>
        public int[] leaderFP { get; set; } = new int[0]; // TODO: What is this?

        /// <summary>
        /// optional, boolean, whether to check the existence of the employee No. (person ID): "false"-no, "true"-yes. If this
        /// node is not configured, the device will check the existence of the employee No. (person ID) by default. If this node is
        /// set to "false", the device will not check the existence of the employee No. (person ID) to speed up data applying; if this
        /// node is set to "true" or NULL, the device will check the existence of the employee No. (person ID), and it is
        /// recommended to set this node to "true" or NULL if there is no need to speed up data applying
        /// </summary>
        public bool checkEmployeeNo { get; set; } = true;
    }
}
