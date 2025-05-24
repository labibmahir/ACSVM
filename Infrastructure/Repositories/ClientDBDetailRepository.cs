using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ClientDBDetailRepository : Repository<ClientDBDetail>, IClientDBDetailRepository
    {
        public ClientDBDetailRepository(DataContext context) : base(context)
        {
        }
    }
}
