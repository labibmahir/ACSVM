﻿using Domain.Dto;
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
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(DataContext context) : base(context)
        {
        }
        public async Task<Appointment> GetAppointmentByKey(Guid key)
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
        public async Task<AppointmentReadDto> GetAppointmentByKeyInDto(Guid key)
        {
            try
            {
                return await context.Appointments.Where(x => x.IsDeleted == false && x.Oid == key).Select(a => new AppointmentReadDto()
                {
                    AppointmentDate = a.AppointmentDate,
                    EndTime = a.EndTime,
                    IsCancelled = a.IsCancelled,
                    IsCompleted = a.IsCompleted,
                    Oid = a.Oid,
                    Vistor = a.Vistor,
                    StartTime = a.StartTime,
                    VisitorId = a.VisitorId,
                    Persons = a.IdentifiedAssignedAppointments.Select(p => new Person()
                    {
                        FirstName = p.Person.FirstName,
                        Surname = p.Person.Surname,
                        Gender = p.Person.Gender
                    }).ToList(),
                }).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }

        }
        public async Task<Appointment> GetActiveAppointmentByVisitor(Guid VisitorId)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == false && x.AppointmentDate <= DateTime.Now);
            }
            catch
            {
                throw;
            }

        }
        public async Task<Appointment> GetActiveAppointmentByVisitorAppointmentDateAndTime(Guid VisitorId, DateTime authenticationDate, TimeSpan authenticationTime)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == false && x.AppointmentDate.Date == authenticationDate.Date && x.StartTime >= authenticationTime && x.EndTime <= authenticationTime);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<Appointment>> GetAppointments()
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

        public async Task<IEnumerable<AppointmentReadDto>> GetAppointments(AppointmentFilterDto appointmentFilterDto)
        {
            try
            {

                var query = context.Appointments.AsNoTracking().Include(v => v.Vistor).ThenInclude(i => i.PersonImages)
                    .Include(i => i.IdentifiedAssignedAppointments).Include(v => v.Vistor).ThenInclude(id => id.IdentifiedAssignDevices).AsQueryable();


                if (!string.IsNullOrEmpty(appointmentFilterDto.search))
                    query = query.Where(x => x.Vistor.FirstName.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim()) || x.Vistor.Surname.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim())
                    || x.Vistor.PhoneNumber.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim())
                    || (x.Vistor.Email != null && x.Vistor.Email.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim()))
                       );

                if (!string.IsNullOrEmpty(appointmentFilterDto.VisitorPhone))
                    query = query.Where(x => x.Vistor.PhoneNumber.ToLower().Contains(appointmentFilterDto.VisitorPhone.ToLower().Trim()));

                if (appointmentFilterDto.AppointmentDate.HasValue && appointmentFilterDto.AppointmentDate != DateTime.MinValue)
                    query = query.Where(x => x.AppointmentDate == appointmentFilterDto.AppointmentDate);

                if (appointmentFilterDto.IsCancelled.HasValue)
                    query = query.Where(x => x.IsCancelled == appointmentFilterDto.IsCancelled);

                if (appointmentFilterDto.IsCompleted.HasValue)
                    query = query.Where(x => x.IsCompleted == appointmentFilterDto.IsCompleted);


                var result = await query.Select(x => new AppointmentReadDto()
                {
                    AppointmentDate = x.AppointmentDate,
                    EndTime = x.EndTime,
                    IsCancelled = x.IsCancelled,
                    IsCompleted = x.IsCompleted,
                    Oid = x.Oid,
                    StartTime = x.StartTime,
                    Vistor = x.Vistor,
                    VisitorId = x.VisitorId,
                    VisitorImageBase64 = x.Vistor.PersonImages.FirstOrDefault() == null ? "" : x.Vistor.PersonImages.FirstOrDefault().ImageBase64,
                    Persons = x.IdentifiedAssignedAppointments.Select(p => new Person()
                    {
                        FirstName = p.Person.FirstName,
                        Surname = p.Person.Surname,
                        Gender = p.Person.Gender,
                        Department = p.Person.Department,
                    }).ToList(),
                    AssignedDevicesToVisitor = x.Vistor.IdentifiedAssignDevices.Select(d => d.Device).ToList(),

                }).Skip(appointmentFilterDto.Page).Take(appointmentFilterDto.PageSize)
                  .ToListAsync();

                foreach (var item in result)
                {
                    var accessLevels = await context.IdentifiedAssignDevices.Include(d => d.Device).ThenInclude(a => a.AccessLevel).Where(x => x.IsDeleted == false && x.VisitorId == item.VisitorId && x.Device.AccessLevelId != null).ToListAsync();
                    if (result != null)
                    {
                        //  result.AssignedAccessLevelIdToVisitor = accessLevels.Select(x => x.Device.AccessLevelId.Value).Distinct().ToList(); 
                        item.AssignedAccessLevelToVisitor = accessLevels.Select(x => new AccessLevel()
                        {
                            Oid = x.Device.AccessLevel.Oid,
                            Description = x.Device.AccessLevel.Description
                        }).Distinct().ToList();
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetAppointmentsCount(AppointmentFilterDto appointmentFilterDto)
        {
            try
            {

                var query = context.Appointments.AsNoTracking().Include(v => v.Vistor).Include(i => i.IdentifiedAssignedAppointments).AsQueryable();


                if (!string.IsNullOrEmpty(appointmentFilterDto.search))
                    query = query.Where(x => x.Vistor.FirstName.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim()) || x.Vistor.Surname.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim())
                    || x.Vistor.PhoneNumber.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim())
                    || (x.Vistor.Email != null && x.Vistor.Email.ToLower().Contains(appointmentFilterDto.search.ToLower().Trim()))
                       );

                if (!string.IsNullOrEmpty(appointmentFilterDto.VisitorPhone))
                    query = query.Where(x => x.Vistor.PhoneNumber.ToLower().Contains(appointmentFilterDto.VisitorPhone.ToLower().Trim()));

                if (appointmentFilterDto.AppointmentDate.HasValue && appointmentFilterDto.AppointmentDate != DateTime.MinValue)
                    query = query.Where(x => x.AppointmentDate == appointmentFilterDto.AppointmentDate);

                if (appointmentFilterDto.IsCancelled.HasValue)
                    query = query.Where(x => x.IsCancelled == appointmentFilterDto.IsCancelled);

                if (appointmentFilterDto.IsCompleted.HasValue)
                    query = query.Where(x => x.IsCompleted == appointmentFilterDto.IsCompleted);


                var result = await query.CountAsync();

                return result;
            }
            catch
            {
                throw;
            }

        }
        public async Task<VisitorLastAppointmentDetailDto> GetLastAppointmentByVisitorNo(string visitorNo)
        {

            try
            {
                var query = context.Appointments.AsNoTracking().Include(v => v.Vistor).Include(i => i.IdentifiedAssignedAppointments).Include(v => v.Vistor)
                    .ThenInclude(pr => pr.PersonImages).Where(x => x.Vistor.VisitorNumber.ToLower().Trim() == visitorNo && x.IsDeleted == false).AsQueryable();

                var result = await query.OrderByDescending(x => x.AppointmentDate).Select(x => new VisitorLastAppointmentDetailDto()
                {
                    Oid = x.Oid,
                    Address = x.Vistor.Address,
                    Firstname = x.Vistor.FirstName,
                    Surname = x.Vistor.Surname,
                    IsCancelled = x.IsCancelled,
                    AppointmentDate = x.AppointmentDate,
                    Email = x.Vistor.Email,
                    PhoneNumber = x.Vistor.PhoneNumber,
                    Persons = x.IdentifiedAssignedAppointments.Select(p => new Person()
                    {
                        FirstName = p.Person.FirstName,
                        Surname = p.Person.Surname,
                        Gender = p.Person.Gender,
                        Department = p.Person.Department,
                        Email = p.Person.Email,
                        PhoneNumber = p.Person.PhoneNumber,
                        Oid = p.Person.Oid
                    }).ToList(),
                    VisitorNumber = x.Vistor.VisitorNumber,
                    VisitorId = x.VisitorId,
                    VisitorImageBase64 = x.Vistor.PersonImages.Where(x => x.IsDeleted == false).FirstOrDefault() == null ? "" : x.Vistor.PersonImages.Where(x => x.IsDeleted == false).FirstOrDefault().ImageBase64,
                    //AssignedDevicesIdToVisitor = x.Vistor.IdentifiedAssignDevices.Select(d => d.Device.Oid).ToList(),
                    AssignedDevicesToVisitor = x.Vistor.IdentifiedAssignDevices.Select(d => d.Device).ToList(),

                }).FirstOrDefaultAsync();
                if (result != null)
                {
                    var accessLevels = await context.IdentifiedAssignDevices.Include(d => d.Device).ThenInclude(a => a.AccessLevel).Where(x => x.IsDeleted == false && x.VisitorId == result.VisitorId && x.Device.AccessLevelId != null).ToListAsync();

                    //  result.AssignedAccessLevelIdToVisitor = accessLevels.Select(x => x.Device.AccessLevelId.Value).Distinct().ToList();
                    result.AssignedAccessLevelToVisitor = accessLevels.Select(x => new AccessLevel()
                    {
                        Oid = x.Device.AccessLevel.Oid,
                        Description = x.Device.AccessLevel.Description
                    }).Distinct().ToList();
                }
                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<VisitorLastAppointmentDetailDto> GetLastAppointmentByPhoneNo(string phoneNo)
        {

            try
            {
                var query = context.Appointments.AsNoTracking().Include(v => v.Vistor).Include(i => i.IdentifiedAssignedAppointments).Include(v => v.Vistor)
                    .ThenInclude(pr => pr.PersonImages).Where(x => x.Vistor.PhoneNumber.ToLower().Trim() == phoneNo && x.IsDeleted == false).AsQueryable();

                var result = await query.OrderByDescending(x => x.AppointmentDate).Select(x => new VisitorLastAppointmentDetailDto()
                {
                    Oid = x.Oid,
                    Address = x.Vistor.Address,
                    Firstname = x.Vistor.FirstName,
                    Surname = x.Vistor.Surname,
                    IsCancelled = x.IsCancelled,
                    AppointmentDate = x.AppointmentDate,
                    Email = x.Vistor.Email,
                    PhoneNumber = x.Vistor.PhoneNumber,
                    Persons = x.IdentifiedAssignedAppointments.Select(p => new Person()
                    {
                        FirstName = p.Person.FirstName,
                        Surname = p.Person.Surname,
                        Gender = p.Person.Gender,
                        Department = p.Person.Department,
                        Email = p.Person.Email,
                        PhoneNumber = p.Person.PhoneNumber,
                        Oid = p.Person.Oid
                    }).ToList(),
                    VisitorNumber = x.Vistor.VisitorNumber,
                    VisitorId = x.VisitorId,
                    VisitorImageBase64 = x.Vistor.PersonImages.Where(x => x.IsDeleted == false).FirstOrDefault() == null ? "" : x.Vistor.PersonImages.Where(x => x.IsDeleted == false).FirstOrDefault().ImageBase64,
                    //AssignedDevicesIdToVisitor = x.Vistor.IdentifiedAssignDevices.Select(d => d.Device.Oid).ToList(),
                    AssignedDevicesToVisitor = x.Vistor.IdentifiedAssignDevices.Select(d => d.Device).ToList(),

                }).FirstOrDefaultAsync();
                if (result != null)
                {
                    var accessLevels = await context.IdentifiedAssignDevices.Include(d => d.Device).ThenInclude(a => a.AccessLevel).Where(x => x.IsDeleted == false && x.VisitorId == result.VisitorId && x.Device.AccessLevelId != null).ToListAsync();

                    //  result.AssignedAccessLevelIdToVisitor = accessLevels.Select(x => x.Device.AccessLevelId.Value).Distinct().ToList();
                    result.AssignedAccessLevelToVisitor = accessLevels.Select(x =>new AccessLevel() {
                     Oid = x.Device.AccessLevel.Oid,
                     Description = x.Device.AccessLevel.Description
                    }).Distinct().ToList();
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

    }
}
