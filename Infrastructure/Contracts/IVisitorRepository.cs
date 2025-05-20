using Domain.Dto;
using Domain.Dto.PaginationFiltersDto;
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

        /// <summary>
        /// The method is used to get a Visitor by VisitorNumber.
        /// </summary>
        /// <param name="VisitorNumber">VisitorNumber of a Visitor.</param>
        /// <returns>Returns a Visitor if the VisitorNumber is matched.
        /// 
        /// <summary>
        /// The method is used to get the list of Visitors.
        /// </summary>
        /// <returns>Returns a list of all Visitors.</returns>
        public Task<IEnumerable<Visitor>> GetVisitors();
        public Task<IEnumerable<VisitorReadDto>> GetVisitors(VisitorFilterDto visitorFilterDto);
        public Task<int> GetVisitorsCount(VisitorFilterDto visitorFilterDto);
        public Task<Visitor> GetVisitorByVisitorNumber(string VisitorNumber);

        /// <summary>
        /// The method is used to get a Visitor by phoneNumber.
        /// </summary>
        /// <param name="phoneNumber">phoneNumber of a Visitor.</param>
        /// <returns>Returns a Visitor if the phoneNumber is matched.
        public Task<Visitor> GetVisitorByphoneNumber(string phoneNumber);
    }
}
