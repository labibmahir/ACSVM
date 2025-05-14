using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AccessLevelRepository : Repository<AccessLevel>, IAccessLevelRepository
    {
        public AccessLevelRepository(DataContext context) : base(context)
        {
        }
    }
}
