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
        /// <summary>
        /// The method is used to get a FingerPrint by key.
        /// </summary>
        /// <param name="key">Primary key of the table FingerPrint.</param>
        /// <returns>Returns a FingerPrint if the key is matched.</returns>
        public Task<FingerPrint> GetFingerPrintByKey(Guid key);
        public Task<FingerPrint> GetFingerPrintByKeyAndFingerNo(Guid key,Enums.FingerNumber fingerNumber);
        public Task<FingerPrint> GetFingerPrintByPersonAndFingerNumber(Guid personId, Enums.FingerNumber fingerNumber);
        public Task<FingerPrint> GetFingerPrintByVisitorAndFingerNumber(Guid visitorId, Enums.FingerNumber fingerNumber);

        public Task<IEnumerable<FingerPrint>> GetAllFingerPrintOfPeronByPersonId(Guid personId);
        public Task<IEnumerable<FingerPrint>> GetAllFingerPrintOfVisitorByVisitorId(Guid visitorId);
    }
}
