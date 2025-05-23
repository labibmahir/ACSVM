using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class IdentifiedAssignedAppointmentRepository : Repository<IdentifiedAssignedAppointment>, IIdentifiedAssignedAppointmentRepository
    {
        public IdentifiedAssignedAppointmentRepository(DataContext context) : base(context)
        {
        }
        public async Task<IEnumerable<IdentifiedAssignedAppointment>> GetIdentifiedAssignedAppointmentByAppointment(Guid AppointmentId)
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false && x.AppointmentId == AppointmentId);
            }
            catch
            {
                throw;
            }
        }
    }
}
