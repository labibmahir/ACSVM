using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMCardInfo
    {
        /// <summary>
        /// required, string, employee No. (person ID)
        /// </summary>
        public string employeeNo { get; set; }

        /// <summary>
        /// required, string, card No.
        /// </summary>
        public string cardNo { get; set; }

        /// <summary>
        /// optional, boolean, whether to delete the card: "true"-yes. This node is required only when the card needs to be
        /// deleted; for adding or editing card information, this node can be set to NULL
        /// </summary>
        [JsonIgnore]
        public bool? deleteCard { get; set; } = null;

        /// <summary>
        /// optional, string, card type: "normalCard"-normal card, "patrolCard"-patrol card, "hijackCard"-duress card,
        /// "superCard"-super card, "dismissingCard"-dismiss card, "emergencyCard"-emergency card(it is used to assign
        /// permission to a temporary card, but it cannot open the door)
        /// </summary>
        public string? cardType { get; set; } = "normalCard";

        /// <summary>
        /// optional, string, whether to support first card authentication function, e.g., the value "1,3,5" indicates that the
        /// access control points No.1, No.3, and No.5 support first card authentication function
        /// </summary>
        [JsonIgnore]
        public string? leaderCard { get; set; } = string.Empty;

        /// <summary>
        /// optional, boolean, whether to enable duplicated card verification: "false"-disable, "true"-enable. If this node is not
        /// configured, the device will verify the duplicated card by default. When there is no card information, you can set
        /// checkCardNo to "false" to speed up data applying; otherwise, it is not recommended to configure this node
        /// </summary>
        [JsonIgnore]
        public bool? checkCardNo { get; set; } = true;

        /// <summary>
        /// optional, boolean, whether to check the existence of the employee No. (person ID): "false"-no, "true"-yes. If this
        /// node is not configured, the device will check the existence of the employee No. (person ID) by default. If this node is
        /// set to "false", the device will not check the existence of the employee No. (person ID) to speed up data applying; if this
        /// node is set to "true" or NULL, the device will check the existence of the employee No. (person ID), and it is
        /// recommended to set this node to "true" or NULL if there is no need to speed up data applying
        /// </summary>
        [JsonIgnore]
        public bool? checkEmployeeNo { get; set; } = true;

        /// <summary>
        /// optional, boolean type, whether to add the card if the card information being edited does not exist: "false"-no (if
        /// * the device has checked that the card information being edited does not exist, the failure response message will be
        /// returned along with the error code), "true"-yes(if the device has checked that the card information being edited does
        /// not exist, the success response message will be returned, and the card will be added). If this node is not configured,
        /// the card will not be added by default
        /// </summary>
        public bool? addCard { get; set; } = true;
    }
}
