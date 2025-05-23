using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IIdentifiedAssignedAppointmentRepository : IRepository<IdentifiedAssignedAppointment>
    {
        public Task<IEnumerable<IdentifiedAssignedAppointment>> GetIdentifiedAssignedAppointmentByAppointment(Guid AppointmentId);
    }
}
