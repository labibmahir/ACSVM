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
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(DataContext context) : base(context)
        {
        }

        public async Task<Attendance> GetAttendanceBetweenDateAndTimeByPersonNumber(DateTime StartAuthenticationDateAndTime, DateTime EndAuthenticationDateAndTime, string PersonNo)
        {
            try
            {
                return await context.Attendances.AsNoTracking().Include(p => p.Person).Where(
                                                        x => x.Person.PersonNumber == PersonNo.Trim()
                                                             && x.AuthenticationDateAndTime >= StartAuthenticationDateAndTime
                                                             && x.AuthenticationDateAndTime <= EndAuthenticationDateAndTime
                                                    ).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Attendance>> GetPersonAttendances()
        {
            try
            {
                return await QueryAsync(a => a.IsDeleted == false);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Attendance>> GetAttendanceByPersonId(Guid personId)
        {
            try
            {
                var query = context.Attendances.Include(p => p.Person).Where(i => i.IsDeleted == false && i.PersonId == personId).AsQueryable();

                var result = await query
                  .ToListAsync();
                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Attendance>> GetPersonAttendances(PersonAttendanceFilterDto personAttendanceFilterDto)
        {
            try
            {
                var query = context.Attendances.Include(p => p.Person).Include(d => d.Device).Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.search))
                    query = query.Where(x => x.Person.FirstName.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim())
                    || x.Person.Surname.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim()) || x.Person.PersonNumber.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim()));

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.PersonNumber))
                    query = query.Where(x => x.Person.PersonNumber.Contains(personAttendanceFilterDto.PersonNumber));

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.FullName))
                    query = query.Where(x => x.Person.FirstName.Contains(personAttendanceFilterDto.FullName) || x.Person.Surname.Contains(personAttendanceFilterDto.FullName));

                if (personAttendanceFilterDto.AttendanceDate.HasValue && personAttendanceFilterDto.AttendanceDate.Value.Date != DateTime.MinValue.Date)
                    query = query.Where(x => x.AuthenticationDate != null && x.AuthenticationDate.Value.Date == personAttendanceFilterDto.AttendanceDate.Value.Date);
                
                if (personAttendanceFilterDto.ToDate.HasValue && personAttendanceFilterDto.FromDate.Value.Date != DateTime.MinValue.Date)
                    query = query.Where(x => x.AuthenticationDate != null && x.AuthenticationDate.Value.Date == personAttendanceFilterDto.FromDate.Value.Date && x.AuthenticationDate.Value.Date == personAttendanceFilterDto.ToDate.Value.Date);

                if (personAttendanceFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(personAttendanceFilterDto.Page).Take(personAttendanceFilterDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Attendance>> GetPersonAttendancesBetweenDates(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                var query = context.Attendances.Include(p => p.Person).Include(d => d.Device).Where(x => x.IsDeleted == false
                && (x.AuthenticationDate.HasValue && x.AuthenticationDate.Value.Date >= StartDate.Date && x.AuthenticationDate.Value.Date <= EndDate.Date)
                ).AsQueryable();

                return await query.ToListAsync();
            }
            catch
            {
                throw;

            }
        }
        public async Task<int> GetPersonAttendancesCount(PersonAttendanceFilterDto personAttendanceFilterDto)
        {
            try
            {
                var query = context.Attendances.Include(p => p.Person).Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.search))
                    query = query.Where(x => x.Person.FirstName.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim())
                    || x.Person.Surname.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim()) || x.Person.PersonNumber.ToLower().Contains(personAttendanceFilterDto.search.ToLower().Trim()));

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.PersonNumber))
                    query = query.Where(x => x.Person.PersonNumber.Contains(personAttendanceFilterDto.PersonNumber));

                if (!string.IsNullOrEmpty(personAttendanceFilterDto.FullName))
                    query = query.Where(x => x.Person.FirstName.Contains(personAttendanceFilterDto.FullName) || x.Person.Surname.Contains(personAttendanceFilterDto.FullName));

                if (personAttendanceFilterDto.AttendanceDate.HasValue && personAttendanceFilterDto.AttendanceDate.Value.Date != DateTime.MinValue.Date)
                    query = query.Where(x => x.AuthenticationDate != null && x.AuthenticationDate.Value.Date == personAttendanceFilterDto.AttendanceDate.Value.Date);

                if (personAttendanceFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.CountAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
    }
}
