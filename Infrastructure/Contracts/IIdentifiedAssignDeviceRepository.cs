using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IIdentifiedAssignDeviceRepository : IRepository<IdentifiedAssignDevice>
    {
        /// <summary>
        /// The method is used to get a IdentifiedAssignDevice by PersonId.
        /// </summary>
        /// <param name="PersonId">deviceName of a user.</param>
        /// <returns>Returns a IdentifiedAssignDevice if the PersonId is matched.
        public Task<IEnumerable<IdentifiedAssignDevice>> GetIdentifiedAssignDeviceByPerson(Guid PersonId);
        public Task<IEnumerable<Device>> GetUnAssignDeviceByPerson(Guid PersonId);
        public Task<IEnumerable<IdentifiedAssignDevice>> GetIdentifiedAssignDeviceByVisitor(Guid VisitorId);
        public Task<IEnumerable<Person>> GetPersonsByIdentifiedAssignDevice(int DeviceId);
    }
}
