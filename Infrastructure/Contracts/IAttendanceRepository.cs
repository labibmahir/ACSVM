using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        public Task<Attendance> GetAttendanceBetweenDateAndTimeByPersonNumber(DateTime StartAuthenticationDateAndTime, DateTime EndAuthenticationDateAndTime, string PersonNo);
        /// <summary>
        /// The method is used to get the list of Attendance.
        /// </summary>
        /// <returns>Returns a list of all Attendance.</returns>
        public Task<IEnumerable<Attendance>> GetPersonAttendances();
        public Task<IEnumerable<Attendance>> GetAttendanceByPersonId(Guid personId);
        public Task<IEnumerable<Attendance>> GetPersonAttendances(PersonAttendanceFilterDto personAttendanceFilterDto);
        public Task<int> GetPersonAttendancesCount(PersonAttendanceFilterDto personAttendanceFilterDto);
    }
}
