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
                return await context.IdentifiedAssignCards.AsNoTracking().Include(c => c.Card).Where(x => x.VisitorId == VisitorId && x.IsDeleted == false).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<IdentifiedAssignCard>> GetAllInActiveVisitorIdentifiedAssignCards()
        {
            try
            {
                return await context.IdentifiedAssignCards.Include(x => x.Card).Include(v => v.Visitor)
                     .Where(x => x.IsDeleted == false && x.VisitorId != null && x.Card.Status == Utilities.Constants.Enums.Status.Inactive).ToListAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<IdentifiedAssignCard>> GetAllActiveVisitorIdentifiedAssignCards()
        {
            try
            {
                return await context.IdentifiedAssignCards.Include(x => x.Card).Include(v => v.Visitor)
                     .Where(x => x.IsDeleted == false && x.VisitorId != null && x.Card.Status == Utilities.Constants.Enums.Status.Active).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

    }
}
