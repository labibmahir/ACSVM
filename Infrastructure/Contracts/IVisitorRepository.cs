using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IVisitorRepository : IRepository<Visitor>
    {   /// <summary>
        /// The method is used to get a Visitor by key.
        /// </summary>
        /// <param name="key">Primary key of the table Visitor.</param>
        /// <returns>Returns a Visitor if the key is matched.</returns>
        public Task<Visitor> GetVisitorByKey(Guid key);
    }
}
