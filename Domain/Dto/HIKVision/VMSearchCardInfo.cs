using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMSearchCardInfo
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
        /// required, string, card type: "normalCard"-normal card, "patrolCard"-patrol card, "hijackCard"-duress card,
        ///  * "superCard"-super card, "dismissingCard"-dismiss card, "emergencyCard"-emergency card(it is used to assign
        /// permission to a temporary card, but it cannot open the door)
        /// </summary>
        public string cardType { get; set; }

        /// <summary>
        /// optional, string, whether to support first card authentication function, e.g., the value "1,3,5" indicates that the
        /// access control points No.1, No.3, and No.5 support first card authentication function
        /// </summary>
        public string leaderCard { get; set; }
    }
}
