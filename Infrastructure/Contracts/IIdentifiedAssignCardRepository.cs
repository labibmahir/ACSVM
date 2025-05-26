using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IIdentifiedAssignCardRepository : IRepository<IdentifiedAssignCard>
    {
        public Task<IdentifiedAssignCard> GetIdentifiedAssignCardByPerson(Guid PersonId);
        public Task<IdentifiedAssignCard> GetIdentifiedAssignCardByVisitor(Guid VisitorId);
        public Task<IEnumerable<IdentifiedAssignCard>> GetAllInActiveVisitorIdentifiedAssignCards();
        public Task<IEnumerable<IdentifiedAssignCard>> GetAllActiveVisitorIdentifiedAssignCards();
    }
}
