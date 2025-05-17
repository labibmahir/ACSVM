using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PersonImageRepository : Repository<PersonImage>, IPersonImageRepository
    {
        public PersonImageRepository(DataContext context) : base(context)
        {
        }
    }
}
