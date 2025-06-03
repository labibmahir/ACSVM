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
    public class VisitorLogRepository : Repository<VisitorLog>, IVisitorLogRepository
    {
        public VisitorLogRepository(DataContext context) : base(context)
        {
        }
        public async Task<IEnumerable<VisitorLog>> GetVisitorAttendances()
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
        public async Task<IEnumerable<VisitorLog>> GetAttendanceByVisitorId(Guid visitorId)
        {
            try
            {
                var query = context.VisitorLogs.Include(v => v.Visitor).Where(i => i.IsDeleted == false).AsQueryable();
                var result = await query
                       .ToListAsync();

                return result;

            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<VisitorLog>> GetVisitorAttendances(VisitorAttendanceFilterDto visitorAttendanceFilterDto)
        {
            try
            {
                var query = context.VisitorLogs.Include(v => v.Visitor).Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.search))
                    query = query.Where(x => x.VisitorId != null && x.Visitor.FirstName.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim())
                    || x.Visitor.Surname.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim()) || x.Visitor.VisitorNumber.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim()));

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.VisitorNumber))
                    query = query.Where(x => x.Visitor.VisitorNumber.Contains(visitorAttendanceFilterDto.VisitorNumber));

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.FullName))
                    query = query.Where(x => x.Visitor.FirstName.Contains(visitorAttendanceFilterDto.FullName) || x.Visitor.Surname.Contains(visitorAttendanceFilterDto.FullName));

                if (visitorAttendanceFilterDto.AttendanceDate.HasValue && visitorAttendanceFilterDto.AttendanceDate.Value.Date != DateTime.MinValue.Date)
                    query = query.Where(x => x.AuthenticationDate != null && x.AuthenticationDate.Value.Date == visitorAttendanceFilterDto.AttendanceDate.Value.Date);

                if (visitorAttendanceFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(visitorAttendanceFilterDto.Page).Take(visitorAttendanceFilterDto.PageSize)
                              .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetVisitorAttendancesCount(VisitorAttendanceFilterDto visitorAttendanceFilterDto)
        {
            try
            {
                var query = context.VisitorLogs.Include(v => v.Visitor).Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.search))
                    query = query.Where(x => x.VisitorId != null && x.Visitor.FirstName.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim())
                    || x.Visitor.Surname.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim()) || x.Visitor.VisitorNumber.ToLower().Contains(visitorAttendanceFilterDto.search.ToLower().Trim()));

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.VisitorNumber))
                    query = query.Where(x => x.Visitor.VisitorNumber.Contains(visitorAttendanceFilterDto.VisitorNumber));

                if (!string.IsNullOrEmpty(visitorAttendanceFilterDto.FullName))
                    query = query.Where(x => x.Visitor.FirstName.Contains(visitorAttendanceFilterDto.FullName) || x.Visitor.Surname.Contains(visitorAttendanceFilterDto.FullName));

                if (visitorAttendanceFilterDto.AttendanceDate.HasValue && visitorAttendanceFilterDto.AttendanceDate.Value.Date != DateTime.MinValue.Date)
                    query = query.Where(x => x.AuthenticationDate != null && x.AuthenticationDate.Value.Date == visitorAttendanceFilterDto.AttendanceDate.Value.Date);

                if (visitorAttendanceFilterDto.orderBy.ToLower().Trim() == "desc")
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
