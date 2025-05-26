using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMCardInfoDeleteRequest
    {
        /// <summary>
        /// optional, person ID list, if this node does not exist or is set to NULL, it indicates deleting all cards
        /// </summary>
        //[Display(Name = "Employee numbers list")]
        //public List<EmployeeNoListItem?> EmployeeNoList { get; set; } = new List<EmployeeNoListItem?>();

        /// <summary>
        /// optional, card No. list (this node cannot exist together with the EmployeeNoList, and if this node does not exist or is
        /// set to NULL, it indicates deleting all cards)
        /// </summary>
        public List<VMCardNoListItem?> CardNoList { get; set; } = new List<VMCardNoListItem?>();
    }
}
