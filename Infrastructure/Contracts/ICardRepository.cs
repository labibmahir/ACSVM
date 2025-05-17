using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface ICardRepository : IRepository<Card>
    {

        /// <summary>
        /// The method is used to get a Card by key.
        /// </summary>
        /// <param name="key">Primary key of the table Card.</param>
        /// <returns>Returns a Card if the key is matched.</returns>
        public Task<Card> GetCardByKey(Guid key);


        /// <summary>
        /// The method is used to get the list of Cards.
        /// </summary>
        /// <returns>Returns a list of all Cards.</returns>
        public Task<IEnumerable<Card>> GetCards();
        public Task<IEnumerable<Card>> GetCards(PaginationDto paginationDto);
        public Task<int> GetCardCount(PaginationDto paginationDto);

        /// <summary>
        /// The method is used to get a Card by cardNumber.
        /// </summary>
        /// <param name="cardNumber">deviceName of a user.</param>
        /// <returns>Returns a Card if the cardNumber is matched.
        public Task<Card> GetCardByCardNumber(string cardNumber);
    }
}
