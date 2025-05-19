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
        public async Task<PersonImage> GetPersonImageByKey(Guid key)
        {
            try
            {
                return await FirstOrDefaultAsync(i => i.IsDeleted == false && i.Oid == key);
            }
            catch
            {
                throw;
            }

        }
        public async Task<PersonImage> GetImageByPersonId(Guid personId)
        {
            try
            {
                return await FirstOrDefaultAsync(i => i.IsDeleted == false && i.PersonId == personId);
            }
            catch
            {
                throw;
            }
        }
        public async Task<PersonImage> GetImageByVisitorId(Guid visitorId)
        {
            try
            {
                return await FirstOrDefaultAsync(i => i.IsDeleted == false && i.VisitorId == visitorId);
            }
            catch
            {
                throw;
            }
        }
    }
}
