using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class EmployeeCardCount
    {
        public string EmployeeNo { get; set; }
        public int CardCount { get; set; }

        public EmployeeCardCount()
        {

        }
        public EmployeeCardCount(VMCardInfoCountResponse vm, string employeeNo)
        {
            this.EmployeeNo = employeeNo;
            this.CardCount = vm.CardInfoCount.cardNumber;
        }
    }
}
