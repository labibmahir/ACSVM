using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IVisitorLogRepository : IRepository<VisitorLog>
    {  /// <summary>
       /// The method is used to get the list of VisitorLog.
       /// </summary>
       /// <returns>Returns a list of all VisitorLog.</returns>
        public Task<IEnumerable<VisitorLog>> GetVisitorAttendances();
        public Task<IEnumerable<VisitorLog>> GetVisitorAttendances(VisitorAttendanceFilterDto visitorAttendanceFilterDto);
        public Task<int> GetVisitorAttendancesCount(VisitorAttendanceFilterDto visitorAttendanceFilterDto);

        public Task<IEnumerable<VisitorLog>> GetAttendanceByVisitorId(Guid visitorId);
        public Task<IEnumerable<VisitorLog>> GetVisitorAttendancesBetweenDates(DateTime StartDate, DateTime EndDate);
    }
}
