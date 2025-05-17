using Domain.Entities;
using Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Infrastructure.Repositories
{
    public class FingerPrintRepository : Repository<FingerPrint>, IFingerPrintRepository
    {
        public FingerPrintRepository(DataContext context) : base(context)
        {
        }

        public async Task<FingerPrint> GetFingerPrintByPersonAndFingerNumber(Guid personId, Enums.FingerNumber fingerNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PersonId == personId && x.FingerNumber == fingerNumber);
            }
            catch
            {
                throw;
            }

        }
        public async Task<FingerPrint> GetFingerPrintByVisitorAndFingerNumber(Guid visitorId, Enums.FingerNumber fingerNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.VisitorId == visitorId && x.FingerNumber == fingerNumber);
            }
            catch
            {
                throw;
            }
        }

    }
}
