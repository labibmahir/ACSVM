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
    public class IdentifiedAssignCardRepository : Repository<IdentifiedAssignCard>, IIdentifiedAssignCardRepository
    {
        public IdentifiedAssignCardRepository(DataContext context) : base(context)
        {
        }

        public async Task<IdentifiedAssignCard> GetIdentifiedAssignCardByPerson(Guid PersonId)
        {
            try
            {
                return await context.IdentifiedAssignCards.AsNoTracking().Include(c => c.Card).Where(x => x.PersonId == PersonId).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }

        }
        public async Task<IdentifiedAssignCard> GetIdentifiedAssignCardByVisitor(Guid VisitorId)
        {
            try
            {
                return await context.IdentifiedAssignCards.AsNoTracking().Include(c => c.Card).Where(x => x.VisitorId == VisitorId).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

    }
}
