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

    }
}
