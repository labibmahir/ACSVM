using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Infrastructure.Contracts
{
    public interface IFingerPrintRepository : IRepository<FingerPrint>
    {
        public Task<FingerPrint> GetFingerPrintByPersonAndFingerNumber(Guid personId, Enums.FingerNumber fingerNumber);
        public Task<FingerPrint> GetFingerPrintByVisitorAndFingerNumber(Guid visitorId, Enums.FingerNumber fingerNumber);
    }
}
