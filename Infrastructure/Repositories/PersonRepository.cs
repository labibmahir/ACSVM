using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    class PersonRepository : Repository<Person>, IPersonRepository
    {
        public PersonRepository(DataContext context) : base(context)
        {
        }

        public async Task<Person> GetPersonByPersonNumber(string personNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PersonNumber.ToLower().Trim() == personNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }
        }

        public async Task<Person> GetPersonByphoneNumber(string phoneNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PhoneNumber.ToLower().Trim() == phoneNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }

        }
    }
}
