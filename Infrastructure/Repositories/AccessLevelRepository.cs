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
    public class AccessLevelRepository : Repository<AccessLevel>, IAccessLevelRepository
    {
        public AccessLevelRepository(DataContext context) : base(context)
        {
        }
        public async Task<AccessLevel> GetAccessLevelByKey(int key)
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
        public async Task<AccessLevel> GetAccessLevelByDescription(string description)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.Description.ToLower().Trim() == description.ToLower().Trim());
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<AccessLevel>> GetAccessLevels()
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

        public async Task<IEnumerable<AccessLevel>> GetAccessLevels(PaginationDto paginationDto)
        {
            try
            {
                var query = context.AccessLevels.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(paginationDto.search))
                    query = query.Where(x => x.Description.ToLower().Contains(paginationDto.search.ToLower().Trim()));

                var result = await query.Skip(paginationDto.Page).Take(paginationDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<int> GetAccessLevelsCount(PaginationDto paginationDto)
        {
            try
            {
                var query = context.AccessLevels.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(paginationDto.search))
                    query = query.Where(x => x.Description.ToLower().Contains(paginationDto.search.ToLower().Trim()));


                var result = await query.Skip(paginationDto.Page).Take(paginationDto.PageSize)
                  .CountAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
