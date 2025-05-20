using Domain.Dto;
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
    public class VisitorRepository : Repository<Visitor>, IVisitorRepository
    {
        public VisitorRepository(DataContext context) : base(context)
        {
        }
        public async Task<Visitor> GetVisitorByKey(Guid key)
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
        public async Task<IEnumerable<Visitor>> GetVisitors()
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

        public async Task<IEnumerable<VisitorReadDto>> GetVisitors(VisitorFilterDto visitorFilterDto)
        {
            try
            {
                var query = context.Visitors.AsNoTracking().Include(pi => pi.PersonImages).Include(f => f.FingerPrints).Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(visitorFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(visitorFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(visitorFilterDto.search.ToLower().Trim())
                    || x.VisitorNumber.ToLower().Contains(visitorFilterDto.search.ToLower().Trim()) ||
                    (x.PhoneNumber.ToLower().Contains(visitorFilterDto.search.ToLower().Trim())) || (x.Email.ToLower().Contains(visitorFilterDto.search))
                       );

                if (!string.IsNullOrEmpty(visitorFilterDto.FirstName))
                {
                    query = query.Where(x => x.FirstName.ToLower().Contains(visitorFilterDto.FirstName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.Surname))
                {
                    query = query.Where(x => x.Surname.ToLower().Contains(visitorFilterDto.Surname.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.PhoneNumber))
                {
                    query = query.Where(x => x.PhoneNumber.ToLower().Contains(visitorFilterDto.PhoneNumber.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.Email))
                {
                    query = query.Where(x => x.Email.ToLower().Contains(visitorFilterDto.Email.ToLower().Trim()));
                }

                if (visitorFilterDto.Gender.HasValue)
                {
                    query = query.Where(x => x.Gender == visitorFilterDto.Gender);
                }


                if (visitorFilterDto.UserVerifyMode.HasValue)
                {
                    query = query.Where(x => x.UserVerifyMode == visitorFilterDto.UserVerifyMode);
                }


                if (visitorFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Select(x => new VisitorReadDto()
                {
                    Email = x.Email,
                    FirstName = x.FirstName,
                    Surname = x.Surname,
                    Gender = x.Gender,
                    imageBase64 = x.PersonImages.Any() ? x.PersonImages.FirstOrDefault().ImageBase64 : null,
                    FingerPrints = x.FingerPrints.ToList(),
                    Oid = x.Oid,
                    OrganizationId = x.OrganizationId,
                    VisitorNumber = x.VisitorNumber,
                    PhoneNumber = x.PhoneNumber,
                    UserVerifyMode = x.UserVerifyMode,
                    ValidateEndPeriod = x.ValidateEndPeriod,
                    ValidateStartPeriod = x.ValidateStartPeriod,
                }).Skip(visitorFilterDto.Page).Take(visitorFilterDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> GetVisitorsCount(VisitorFilterDto visitorFilterDto)
        {
            try
            {
                var query = context.Visitors.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(visitorFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(visitorFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(visitorFilterDto.search.ToLower().Trim())
                    || x.VisitorNumber.ToLower().Contains(visitorFilterDto.search.ToLower().Trim()) ||
                    (x.PhoneNumber.ToLower().Contains(visitorFilterDto.search.ToLower().Trim())) || (x.Email.ToLower().Contains(visitorFilterDto.search))
                       );

                if (!string.IsNullOrEmpty(visitorFilterDto.FirstName))
                {
                    query = query.Where(x => x.FirstName.ToLower().Contains(visitorFilterDto.FirstName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.Surname))
                {
                    query = query.Where(x => x.Surname.ToLower().Contains(visitorFilterDto.Surname.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.PhoneNumber))
                {
                    query = query.Where(x => x.PhoneNumber.ToLower().Contains(visitorFilterDto.PhoneNumber.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(visitorFilterDto.Email))
                {
                    query = query.Where(x => x.Email.ToLower().Contains(visitorFilterDto.Email.ToLower().Trim()));
                }

                if (visitorFilterDto.Gender.HasValue)
                {
                    query = query.Where(x => x.Gender == visitorFilterDto.Gender);
                }


                if (visitorFilterDto.UserVerifyMode.HasValue)
                {
                    query = query.Where(x => x.UserVerifyMode == visitorFilterDto.UserVerifyMode);
                }


                if (visitorFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);


                var result = await query.Skip(visitorFilterDto.Page).Take(visitorFilterDto.PageSize)
                  .CountAsync();

                return result;

            }
            catch
            {
                throw;
            }

        }
        public async Task<Visitor> GetVisitorByVisitorNumber(string VisitorNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.VisitorNumber.ToLower().Trim() == VisitorNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }
        }

        public async Task<Visitor> GetVisitorByphoneNumber(string phoneNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PhoneNumber.ToLower().Trim() == phoneNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }

        }


    }
}
