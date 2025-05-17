using Domain.Dto.PaginationFiltersDto;
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
    public class CardRepository : Repository<Card>, ICardRepository
    {
        public CardRepository(DataContext context) : base(context)
        {
        }
        public async Task<Card> GetCardByKey(Guid key)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.Oid == key);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Card>> GetCards()
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Card>> GetCards(PaginationDto paginationDto)
        {
            try
            {
                var query = context.Cards.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(paginationDto.search))
                    query = query.Where(x => x.CardNumber.ToLower().Contains(paginationDto.search.ToLower().Trim()));

                if (paginationDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(paginationDto.Page).Take(paginationDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> GetCardCount(PaginationDto paginationDto)
        {
            try
            {
                var query = context.Cards.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(paginationDto.search))
                    query = query.Where(x => x.CardNumber.ToLower().Contains(paginationDto.search.ToLower().Trim()));

                if (paginationDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(paginationDto.Page).Take(paginationDto.PageSize)
                  .CountAsync();

                return result;
            }
            catch
            {
                throw;
            }

        }
        public async Task<Card> GetCardByCardNumber(string cardNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.CardNumber.ToLower() == cardNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }
        }
    }
}
