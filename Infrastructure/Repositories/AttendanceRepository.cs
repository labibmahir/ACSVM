using Domain.Dto;
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
    }
}
