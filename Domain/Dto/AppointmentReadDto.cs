using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class AppointmentReadDto
    {
        public Guid Oid { get; set; }

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsCancelled { get; set; }
        public Visitor Vistor { get; set; }
        public Guid VisitorId { get; set; }
        public string VisitorImageBase64 { get; set; }
        public List<Device> AssignedDevicesToVisitor { get; set; }
        public List<AccessLevel> AssignedAccessLevelToVisitor { get; set; }
        public List<Person> Persons { get; set; }


    }
}
