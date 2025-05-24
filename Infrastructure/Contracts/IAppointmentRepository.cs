using Domain.Dto;
using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        /// <summary>
        /// The method is used to get a Appointment by key.
        /// </summary>
        /// <param name="key">Primary key of the table Appointment.</param>
        /// <returns>Returns a Visitor if the key is matched.</returns>
        public Task<Appointment> GetAppointmentByKey(Guid key);
        public Task<AppointmentReadDto> GetAppointmentByKeyInDto(Guid key);
        public Task<Appointment> GetActiveAppointmentByVisitor(Guid VisitorId);
        public Task<Appointment> GetActiveAppointmentByVisitorAppointmentDateAndTime(Guid VisitorId, DateTime authenticationDate, TimeSpan authenticationTime);

        /// <summary>
        /// The method is used to get the list of Appointments.
        /// </summary>
        /// <returns>Returns a list of all Appointment.</returns>
        public Task<IEnumerable<Appointment>> GetAppointments();
        public Task<IEnumerable<AppointmentReadDto>> GetAppointments(AppointmentFilterDto appointmentFilterDto);
        public Task<int> GetAppointmentsCount(AppointmentFilterDto appointmentFilterDto);
    }
}
