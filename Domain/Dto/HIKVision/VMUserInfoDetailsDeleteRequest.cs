using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public record VMUserInfoDetailsDeleteRequest
    {
        public string mode { get; set; }

        /// <summary>
        /// optional, person ID list (if this node does not exist or is set to NULL, it indicates deleting all person information)
        /// </summary>
        [Display(Name = "Employees numbers list")]
        public List<VMEmployeeNoListItem>? EmployeeNoList { get; set; } = new List<VMEmployeeNoListItem>();
    }
    public class VMEmployeeNoListItem
    {
        /// <summary>
        /// optional, string type, employee No. (person ID)
        /// </summary>
        public string? employeeNo { get; set; }
    }
}
