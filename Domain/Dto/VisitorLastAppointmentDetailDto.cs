using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class VisitorLastAppointmentDetailDto
    {
        public Guid Oid { get; set; }
        public int? OrganizationId { get; set; }
        public string FullName { get; set; }
        public string VisitorNumber { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public DateTime AppointmentDate { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsCancelled { get; set; }
        public Visitor Vistor { get; set; }
        public Guid VisitorId { get; set; }
        public List<Person> Persons { get; set; }
        public List<Device> AssignedDevicesToVisitor { get; set; }
     //   public List<int> AssignedDevicesIdToVisitor { get; set; }
        public List<AccessLevel> AssignedAccessLevelToVisitor { get; set; }
       // public List<int> AssignedAccessLevelIdToVisitor { get; set; }

    }
}
