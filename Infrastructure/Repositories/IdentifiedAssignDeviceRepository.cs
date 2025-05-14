using Domain.Entities;
using Infrastructure.Contracts;
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
    }
}
