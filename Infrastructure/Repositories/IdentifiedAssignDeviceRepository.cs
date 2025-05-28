using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class IdentifiedAssignDeviceRepository : Repository<IdentifiedAssignDevice>, IIdentifiedAssignDeviceRepository
    {
        public IdentifiedAssignDeviceRepository(DataContext context) : base(context)
        {
        }
        public async Task<IEnumerable<IdentifiedAssignDevice>> GetIdentifiedAssignDeviceByPerson(Guid PersonId)
        {
            try
            {
                return await LoadListWithChildAsync<IdentifiedAssignDevice>(x => x.IsDeleted == false && x.PersonId == PersonId, d => d.Device);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<IdentifiedAssignDevice>> GetIdentifiedAssignDeviceByVisitor(Guid VisitorId)
        {
            try
            {
                return await LoadListWithChildAsync<IdentifiedAssignDevice>(x => x.IsDeleted == false && x.VisitorId == VisitorId, d => d.Device);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Person>> GetPersonsByIdentifiedAssignDevice(int DeviceId)
        {
            try
            {
                return await context.IdentifiedAssignDevices.AsNoTracking().Include(p => p.Person).Where(x => x.IsDeleted == false && x.DeviceId == DeviceId)
                    .Select(x => x.Person).ToListAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
