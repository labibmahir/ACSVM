using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;
using Infrastructure.Contracts;
using SurveillanceDevice.Integration.HIKVision;
using System.Net.NetworkInformation;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using static Utilities.Constants.Enums;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Cryptography;
using Domain.Dto.PaginationFiltersDto;
using System.Drawing.Imaging;
using System.Drawing;
using System;
using Api.BackGroundServices.ProccessContract;
using Api.BackGroundServices.ProcessImplimentations;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ApiBaseController
    {

        private readonly IUnitOfWork context;
        private readonly ILogger<AppointmentController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;
        private readonly IProgressManager progressManager;
        public AppointmentController(IUnitOfWork context, ILogger<AppointmentController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService, IProgressManager progressManager)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
            this.progressManager = progressManager;
        }
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>
        /// <summary>
        /// URL: api/visitor
        /// </summary>
        /// <param name="Appointment">AppointmentDto object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateAppointment)]
        [AllowAnonymous]
        public async Task<ActionResult<Person>> CreateAppointment(AppointmentDto appointmentDto)
        {
            try
            {
                if (appointmentDto.AccessLevelList.Length == 0 && appointmentDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var visitorByPhone = await context.VisitorRepository.GetVisitorByphoneNumber(appointmentDto.PhoneNumber);

                //if (visitorByPhone != null && visitorByPhone.OrganizationId == appointmentDto.OrganizationId)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNumberTaken);
                if (visitorByPhone != null)
                {
                    var checkActiveAppointment = await context.AppointmentRepository.GetActiveAppointmentByVisitor(visitorByPhone.Oid);

                    if (checkActiveAppointment != null && checkActiveAppointment.OrganizationId == appointmentDto.OrganizationId)
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.AlreadyHaveAppointment);
                }

                foreach (var personid in appointmentDto.PersonIds)
                {
                    var checkPersonId = await context.PersonRepository.FirstOrDefaultAsync(x => x.Oid == personid);
                    if (checkPersonId == null)
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.PersonNotFound);
                }
                Card visitorCard = new Card();
                if (appointmentDto.CardId != Guid.Empty)
                {
                    var card = await context.CardRepository.GetCardByKey(appointmentDto.CardId);

                    if (card.Status == Enums.Status.NotInService)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardNotInService);

                    if (card.Status == Enums.Status.Allocated)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardAlreadyAllocated);

                    if (card.Status == Enums.Status.Active)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardCurrenlyActive);

                    visitorCard = card;
                }
                
                
                var devices = new List<Device>();
                if (appointmentDto.AccessLevelList.Length > 0)
                {
                    var devicesbyAccess = new List<Device>();
                    foreach (var item in appointmentDto.AccessLevelList)
                    {
                        var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(item);
                        devicesbyAccess.AddRange(devicelist);
                    }

                    devices = devicesbyAccess.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(appointmentDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                // var currentlyInActiveDevices = new List<Device>();
                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);
                

                List<int> visitorToBeUpdateInDevices = new List<int>();
                List<int> visitorToBeAddedInDevices = new List<int>();
                List<int> visitorToBeDeletedFromDevices = new List<int>();

                Visitor visitor = new Visitor();
                if (visitorByPhone == null)
                {
                    visitor = new Visitor()
                    {
                        DateCreated = DateTime.Now,
                        IsDeleted = false,
                        CreatedBy = GetLoggedInUserId(),
                        Email = appointmentDto.Email,
                        FirstName = appointmentDto.FirstName,
                        Gender = appointmentDto.Gender,
                        OrganizationId = appointmentDto.OrganizationId,
                        VisitorNumber = GenerateVisitorNo(),
                        PhoneNumber = appointmentDto.PhoneNumber,
                        Surname = appointmentDto.Surname,
                        UserVerifyMode = appointmentDto.VisitorVerifyMode,
                        ValidateEndPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.EndTime,
                        ValidateStartPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.StartTime,
                        Oid = Guid.NewGuid(),
                        Address = appointmentDto.Address,
                        CompanyName = appointmentDto.CompanyName,

                    };
                    context.VisitorRepository.Add(visitor);
                    #region assigning Card to visitor
                    if (appointmentDto.CardId != Guid.Empty)
                    {
                        IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                        {
                            IsDeleted = false,
                            CardId = visitorCard.Oid,
                            VisitorId = visitor.Oid,
                            IsPermanent = false,
                            Oid = Guid.NewGuid(),
                            DateCreated = DateTime.Now,
                            CreatedBy = GetLoggedInUserId()
                        };
                        context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                    }
                    #endregion
                    visitorToBeAddedInDevices = devices.Select(x => x.Oid).ToList();
                }
                else
                {
                    visitor = new Visitor()
                    {
                        IsDeleted = false,
                        ModifiedBy = GetLoggedInUserId(),
                        DateModified = DateTime.Now,
                        Email = appointmentDto.Email,
                        FirstName = appointmentDto.FirstName,
                        Gender = appointmentDto.Gender,
                        OrganizationId = appointmentDto.OrganizationId,
                        PhoneNumber = appointmentDto.PhoneNumber,
                        Surname = appointmentDto.Surname,
                        UserVerifyMode = appointmentDto.VisitorVerifyMode,
                        ValidateEndPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.EndTime,
                        ValidateStartPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.StartTime,
                        Address = appointmentDto.Address,
                        CompanyName = appointmentDto.CompanyName,

                    };
                    context.VisitorRepository.Update(visitor);
                    #region assigning Card to visitor
                    if (appointmentDto.CardId != Guid.Empty)
                    {
                        var identifiedAssignedCard = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(visitor.Oid);
                        if (identifiedAssignedCard != null)
                        {
                            if (identifiedAssignedCard.CardId != visitorCard.Oid)
                            {
                                identifiedAssignedCard.IsDeleted = true;
                                identifiedAssignedCard.ModifiedBy = GetLoggedInUserId();
                                identifiedAssignedCard.DateModified = DateTime.Now;
                                context.IdentifiedAssignCardRepository.Update(identifiedAssignedCard);

                                IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                                {
                                    IsDeleted = false,
                                    CardId = visitorCard.Oid,
                                    VisitorId = visitor.Oid,
                                    IsPermanent = false,
                                    Oid = Guid.NewGuid(),
                                    DateCreated = DateTime.Now,
                                    CreatedBy = GetLoggedInUserId()
                                };
                                context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                            }
                        }
                        else
                        {
                            IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                            {
                                IsDeleted = false,
                                CardId = visitorCard.Oid,
                                VisitorId = visitor.Oid,
                                IsPermanent = false,
                                Oid = Guid.NewGuid(),
                                DateCreated = DateTime.Now,
                                CreatedBy = GetLoggedInUserId()
                            };
                            context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                        }
                    }
                    #endregion

                    var identifiedAssignedDeviceByVisitor = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitor.Oid);
                    var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                    var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByVisitor.Select(x => x.DeviceId).ToList();

                    visitorToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                    visitorToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();
                    visitorToBeDeletedFromDevices = identifiedAlreadyAssignedDevice.Except(newAssignedDevice).ToList();

                    foreach (var item in identifiedAssignedDeviceByVisitor)
                        context.IdentifiedAssignDeviceRepository.Delete(new IdentifiedAssignDevice() { Oid = item.Oid });
                }
                
                await context.SaveChangesAsync();
                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices)
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = visitor.OrganizationId,
                        VisitorId = visitor.Oid,
                        IsDeleted = false,

                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();
                #region DeviceSynchronizer
                DeviceSynchronizer deviceSynchronizer = new DeviceSynchronizer()
                {
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    IsSync = false,
                    VisitorId = visitor.Oid,
                    OrganizationId = visitor.OrganizationId,
                    Oid = Guid.NewGuid(),
                };
                context.DeviceSynchronizerRepository.Add(deviceSynchronizer);
                List<IdentifiedSyncDevice> identifiedSyncDevices = new List<IdentifiedSyncDevice>();

                foreach (var item in visitorToBeAddedInDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Add,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }
                foreach (var item in visitorToBeUpdateInDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Update,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }

                foreach (var item in visitorToBeDeletedFromDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Delete,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }
                context.IdentifiedSyncDeviceRepository.AddRange(identifiedSyncDevices);
                #endregion
                Appointment appointment = new Appointment()
                {
                    IsCompleted = false,
                    IsDeleted = false,
                    Oid = Guid.NewGuid(),
                    DateCreated = DateTime.Now,
                    CreatedBy = GetLoggedInUserId(),
                    AppointmentDate = appointmentDto.AppointmentDate,
                    EndTime = appointmentDto.EndTime,
                    StartTime = appointmentDto.StartTime,
                    IsCancelled = false,
                    OrganizationId = appointmentDto.OrganizationId,
                    VisitorId = visitor.Oid,


                };
                context.AppointmentRepository.Add(appointment);
                await context.SaveChangesAsync();
                List<IdentifiedAssignedAppointment> identifiedAssignedAppointments = new List<IdentifiedAssignedAppointment>();
                foreach (var personId in appointmentDto.PersonIds)
                {
                    IdentifiedAssignedAppointment identifiedAssignedAppointment = new IdentifiedAssignedAppointment()
                    {
                        AppointmentId = appointment.Oid,
                        OrganizationId = appointment.OrganizationId,
                        Oid = Guid.NewGuid(),
                        DateCreated = DateTime.Now,
                        CreatedBy = GetLoggedInUserId(),
                        IsDeleted = false,
                        PersonId = personId,

                    };
                    identifiedAssignedAppointments.Add(identifiedAssignedAppointment);
                }
                context.IdentifiedAssignedAppointmentRepository.AddRange(identifiedAssignedAppointments);
                await context.SaveChangesAsync();
                //#endregion
                IProcess importPeople = new DeviceActionProcess(deviceSynchronizer, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL:api/appointments
        /// </summary>
        /// <returns>A list of appointments.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAppointment)]
        public async Task<IActionResult> ReadAppointment([FromQuery] AppointmentFilterDto appointmentFilterDto)
        {
            try
            {
                if (appointmentFilterDto.PageSize == 0)
                {
                    var appointments = await context.AppointmentRepository.GetAppointments();

                    return Ok(appointments);
                }
                else
                {
                    int currentPage = appointmentFilterDto.Page;
                    appointmentFilterDto.Page = ((appointmentFilterDto.Page - 1) * (appointmentFilterDto.PageSize));
                    var appointments = await context.AppointmentRepository.GetAppointments(appointmentFilterDto);

                    PagedResultDto<AppointmentReadDto> appointmentDto = new PagedResultDto<AppointmentReadDto>()
                    {
                        Data = appointments.ToList(),
                        PageNumber = currentPage,
                        PageSize = appointmentFilterDto.PageSize,
                        TotalItems = await context.AppointmentRepository.GetAppointmentsCount(appointmentFilterDto)
                    };

                    appointmentDto.TotalPages = (int)Math.Ceiling((double)appointmentDto.TotalItems / appointmentDto.PageSize);


                    return Ok(appointmentDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/appointments/last-appointment-by-visitor-number/{VisitorNumber}
        /// </summary>
        /// <returns>A list of appointments.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadLastAppointmentByVisitorNo)]
        public async Task<IActionResult> ReadLastAppointmentByVisitorNo(string visitorNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(visitorNumber))
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var appointments = await context.AppointmentRepository.GetLastAppointmentByVisitorNo(visitorNumber);

                return Ok(appointments);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/appointments/last-appointment-by-visitor-number/{VisitorNumber}
        /// </summary>
        /// <returns>A list of appointments.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadLastAppointmentByPhoneNo)]
        public async Task<IActionResult> ReadLastAppointmentByPhoneNo(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var appointments = await context.AppointmentRepository.GetLastAppointmentByPhoneNo(phoneNumber);

                return Ok(appointments);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/appointment/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table appointment.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAppointmentByKey)]
        [Authorize]
        public async Task<IActionResult> ReadAppointmentByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitor = await context.AppointmentRepository.GetAppointmentByKeyInDto(key);

                if (visitor == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                return Ok(visitor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
        /// <summary>
        /// URL: api/appointment/{key}
        /// </summary>
        /// <param name="key">Primary key of the table appointment.</param>
        /// <param name="AppointmentDto">appointment to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateAppointment)]
        public async Task<IActionResult> UpdateAppointment(Guid key, AppointmentDto appointmentDto)
        {
            try
            {
                if (key != appointmentDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                if (appointmentDto.AccessLevelList.Length == 0 && appointmentDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var appointmentInDb = await context.AppointmentRepository.GetAppointmentByKey(appointmentDto.Oid);
                if (appointmentInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var visitorInDb = await context.VisitorRepository.GetVisitorByKey(appointmentInDb.VisitorId);
                if (visitorInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var visitorByPhone = await context.VisitorRepository.GetVisitorByphoneNumber(appointmentDto.PhoneNumber);
                if (visitorByPhone != null && visitorInDb.Oid != visitorByPhone.Oid)
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.DuplicateCellphoneError);

                }

                foreach (var personid in appointmentDto.PersonIds)
                {
                    var checkPersonId = await context.PersonRepository.FirstOrDefaultAsync(x => x.Oid == personid);
                    if (checkPersonId == null)
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.PersonNotFound);
                }

                Card visitorCard = new Card();
                if (appointmentDto.CardId == Guid.Empty)
                {
                    var card = await context.CardRepository.GetCardByKey(appointmentDto.CardId);

                    if (card.Status == Enums.Status.NotInService)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardNotInService);

                    if (card.Status == Enums.Status.Allocated)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardAlreadyAllocated);

                    if (card.Status == Enums.Status.Active)
                        return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardCurrenlyActive);

                    visitorCard = card;
                }
                var devices = new List<Device>();
                if (appointmentDto.AccessLevelList.Length > 0)
                {
                    var devicesbyAccess = new List<Device>();
                    foreach (var item in appointmentDto.AccessLevelList)
                    {
                        var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(item);
                        devicesbyAccess.AddRange(devicelist);
                    }

                    devices = devicesbyAccess.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(appointmentDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);


                appointmentInDb.IsCompleted = false;
                appointmentInDb.IsDeleted = false;
                appointmentInDb.DateCreated = DateTime.Now;
                appointmentInDb.CreatedBy = GetLoggedInUserId();
                appointmentInDb.AppointmentDate = appointmentDto.AppointmentDate;
                appointmentInDb.EndTime = appointmentDto.EndTime;
                appointmentInDb.StartTime = appointmentDto.StartTime;
                appointmentInDb.IsCancelled = false;
                appointmentInDb.OrganizationId = appointmentDto.OrganizationId;
                appointmentInDb.VisitorId = visitorInDb.Oid;

                context.AppointmentRepository.Update(appointmentInDb);
                List<IdentifiedAssignedAppointment> identifiedAssignedAppointments = new List<IdentifiedAssignedAppointment>();
                foreach (var personId in appointmentDto.PersonIds)
                {
                    IdentifiedAssignedAppointment identifiedAssignedAppointment = new IdentifiedAssignedAppointment()
                    {
                        AppointmentId = appointmentInDb.Oid,
                        OrganizationId = appointmentInDb.OrganizationId,
                        Oid = Guid.NewGuid(),
                        DateCreated = DateTime.Now,
                        CreatedBy = GetLoggedInUserId(),
                        IsDeleted = false,
                        PersonId = personId,

                    };
                    identifiedAssignedAppointments.Add(identifiedAssignedAppointment);
                }
                context.IdentifiedAssignedAppointmentRepository.AddRange(identifiedAssignedAppointments);
                var identifiedAssignedAppointmentInDb = await context.IdentifiedAssignedAppointmentRepository.GetIdentifiedAssignedAppointmentByAppointment(appointmentDto.Oid);
                if (identifiedAssignedAppointmentInDb != null && identifiedAssignedAppointmentInDb.Count() > 0)
                {
                    foreach (var item in identifiedAssignedAppointmentInDb)
                    {
                        context.IdentifiedAssignedAppointmentRepository.Delete(item);
                    }
                }

                List<int> visitorToBeUpdateInDevices = new List<int>();
                List<int> visitorToBeAddedInDevices = new List<int>();
                List<int> visitorToBeDeletedFromDevices = new List<int>();

                visitorInDb.IsDeleted = false;
                visitorInDb.ModifiedBy = GetLoggedInUserId();
                visitorInDb.DateModified = DateTime.Now;
                visitorInDb.Email = appointmentDto.Email;
                visitorInDb.FirstName = appointmentDto.FirstName;
                visitorInDb.Gender = appointmentDto.Gender;
                visitorInDb.OrganizationId = appointmentDto.OrganizationId;
                visitorInDb.PhoneNumber = appointmentDto.PhoneNumber;
                visitorInDb.Surname = appointmentDto.Surname;
                visitorInDb.UserVerifyMode = appointmentDto.VisitorVerifyMode;
                visitorInDb.ValidateEndPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.EndTime;
                visitorInDb.ValidateStartPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.StartTime;
                visitorInDb.Address = appointmentDto.Address;
                visitorInDb.CompanyName = appointmentDto.CompanyName;

                context.VisitorRepository.Update(visitorInDb);
                #region Card Assignment
                if (appointmentDto.CardId == Guid.Empty)
                {
                    var identifiedAssignedCard = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(visitorInDb.Oid);
                    if (identifiedAssignedCard != null)
                    {
                        if (identifiedAssignedCard.CardId != visitorCard.Oid)
                        {
                            identifiedAssignedCard.IsDeleted = true;
                            identifiedAssignedCard.ModifiedBy = GetLoggedInUserId();
                            identifiedAssignedCard.DateModified = DateTime.Now;
                            context.IdentifiedAssignCardRepository.Update(identifiedAssignedCard);

                            IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                            {
                                IsDeleted = false,
                                CardId = visitorCard.Oid,
                                VisitorId = visitorInDb.Oid,
                                IsPermanent = false,
                                Oid = Guid.NewGuid(),
                                DateCreated = DateTime.Now,
                                CreatedBy = GetLoggedInUserId()
                            };
                            context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                        }
                    }
                    else
                    {
                        IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                        {
                            IsDeleted = false,
                            CardId = visitorCard.Oid,
                            VisitorId = visitorInDb.Oid,
                            IsPermanent = false,
                            Oid = Guid.NewGuid(),
                            DateCreated = DateTime.Now,
                            CreatedBy = GetLoggedInUserId()
                        };
                        context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                    }
                }
                #endregion
                var identifiedAssignedDeviceByVisitor = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitorInDb.Oid);
                var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByVisitor.Select(x => x.DeviceId).ToList();

                visitorToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                visitorToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();
                visitorToBeDeletedFromDevices = identifiedAlreadyAssignedDevice.Except(newAssignedDevice).ToList();

                foreach (var item in identifiedAssignedDeviceByVisitor)
                    context.IdentifiedAssignDeviceRepository.Delete(new IdentifiedAssignDevice() { Oid = item.Oid });


                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices)
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = visitorInDb.OrganizationId,
                        VisitorId = visitorInDb.Oid,
                        IsDeleted = false,
                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }


                List<VMDoorPermissionSchedule> vMDoorPermissionSchedules = new List<VMDoorPermissionSchedule>();

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();

                #region DeviceSynchronizer
                DeviceSynchronizer deviceSynchronizer = new DeviceSynchronizer()
                {
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    IsSync = false,
                    VisitorId = visitorInDb.Oid,
                    OrganizationId = visitorInDb.OrganizationId,
                    Oid = Guid.NewGuid(),
                };
                context.DeviceSynchronizerRepository.Add(deviceSynchronizer);
                List<IdentifiedSyncDevice> identifiedSyncDevices = new List<IdentifiedSyncDevice>();

                foreach (var item in visitorToBeAddedInDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Add,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }
                foreach (var item in visitorToBeUpdateInDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Update,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }

                foreach (var item in visitorToBeDeletedFromDevices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Delete,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item,
                        DeviceSynchronizerId = deviceSynchronizer.Oid,
                        IsDeleted = false,
                        IsSync = false,
                        TryCount = 0,

                    };
                    identifiedSyncDevices.Add(identifiedSyncDevice);
                }
                context.IdentifiedSyncDeviceRepository.AddRange(identifiedSyncDevices);
                await context.SaveChangesAsync();
                #endregion

                IProcess importPeople = new DeviceActionProcess(deviceSynchronizer, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/complete-appointment/{key}
        /// </summary>
        /// <param name="key">Primary key of the table appointment.</param> 
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.CompleteAppointment)]
        public async Task<IActionResult> CompleteAppointment(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);


                var appointmentInDb = await context.AppointmentRepository.GetAppointmentByKey(key);
                if (appointmentInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var visitorInDb = await context.VisitorRepository.GetVisitorByKey(appointmentInDb.VisitorId);
                if (visitorInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);


                var identifiedCardAssign = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(appointmentInDb.VisitorId);

                Card visitorCard = new Card();
                if (identifiedCardAssign != null)
                {

                    identifiedCardAssign.IsDeleted = true;
                    context.IdentifiedAssignCardRepository.Update(identifiedCardAssign);


                    if (identifiedCardAssign.Card != null)
                    {
                        visitorCard = identifiedCardAssign.Card;
                        visitorCard.Status = Status.Inactive;
                        context.CardRepository.Update(visitorCard);
                    }


                    var assignedDevices = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitorInDb.Oid);

                    VMCardInfoDeleteRequest vMCardInfoDeleteRequest = new VMCardInfoDeleteRequest()
                    {
                        CardNoList = new List<VMCardNoListItem?>() { new VMCardNoListItem() { cardNo = visitorCard.CardNumber } }
                    };

                    foreach (var item in assignedDevices)
                    {
                        var result = await _visionMachineService.DeleteCard(item.Device, vMCardInfoDeleteRequest);
                    }
                    await context.SaveChangesAsync();

                }

                var devices = new List<Device>();
                
                devices = context.DeviceRepository.GetAll().ToList();
                
                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);


                appointmentInDb.IsCompleted = true;
                appointmentInDb.IsCancelled = false;
                appointmentInDb.IsDeleted = false;
                appointmentInDb.DateCreated = DateTime.Now;
                appointmentInDb.CreatedBy = GetLoggedInUserId();

                context.AppointmentRepository.Update(appointmentInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
        /// <summary>
        /// URL: api/cancel-appointment/{key}
        /// </summary>
        /// <param name="key">Primary key of the table appointment.</param> 
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.CancelAppointment)]
        public async Task<IActionResult> CancelAppointment(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);


                var appointmentInDb = await context.AppointmentRepository.GetAppointmentByKey(key);
                if (appointmentInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var visitorInDb = await context.VisitorRepository.GetVisitorByKey(appointmentInDb.VisitorId);
                if (visitorInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);


                var identifiedCardAssign = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(appointmentInDb.VisitorId);

                Card visitorCard = new Card();
                if (identifiedCardAssign != null)
                {

                    identifiedCardAssign.IsDeleted = true;
                    context.IdentifiedAssignCardRepository.Update(identifiedCardAssign);


                    if (identifiedCardAssign.Card != null)
                    {
                        visitorCard = identifiedCardAssign.Card;
                        visitorCard.Status = Status.Inactive;
                        context.CardRepository.Update(visitorCard);
                    }


                    var assignedDevices = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitorInDb.Oid);

                    VMCardInfoDeleteRequest vMCardInfoDeleteRequest = new VMCardInfoDeleteRequest()
                    {
                        CardNoList = new List<VMCardNoListItem?>() { new VMCardNoListItem() { cardNo = visitorCard.CardNumber } }
                    };

                    foreach (var item in assignedDevices)
                    {
                        var result = await _visionMachineService.DeleteCard(item.Device, vMCardInfoDeleteRequest);

                        if (!result.Success)
                        {

                        }
                    }
                    await context.SaveChangesAsync();

                }

                var devices = new List<Device>();

                devices = context.DeviceRepository.GetAll().ToList();

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);


                appointmentInDb.IsCompleted = false;
                appointmentInDb.IsCancelled = true;
                appointmentInDb.IsDeleted = false;
                appointmentInDb.DateCreated = DateTime.Now;
                appointmentInDb.CreatedBy = GetLoggedInUserId();

                context.AppointmentRepository.Update(appointmentInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/appointment/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Appointment.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteAppointment)]
        public async Task<IActionResult> DeleteAppointment(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var appointmentInDb = await context.AppointmentRepository.GetAppointmentByKey(key);


                var checkIfCardIsAssigned = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(appointmentInDb.VisitorId);

                if (checkIfCardIsAssigned != null)
                {
                    context.IdentifiedAssignCardRepository.Delete(checkIfCardIsAssigned);
                    var cardInDb = await context.CardRepository.GetCardByKey(checkIfCardIsAssigned.CardId);
                    cardInDb.DateModified = DateTime.Now;
                    cardInDb.ModifiedBy = GetLoggedInUserId();
                    cardInDb.IsDeleted = false;
                    cardInDb.Status = Enums.Status.Inactive;
                    context.CardRepository.Update(cardInDb);

                }

                context.AppointmentRepository.Delete(appointmentInDb);

                await context.SaveChangesAsync();

                return Ok(appointmentInDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
        private async Task<bool> IsDeviceActive(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ipAddress, 2000); // 2 seconds timeout
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private string GenerateVisitorNo()
        {
            // 1. Use shorter timestamp (Unix seconds instead of milliseconds)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(); // 10 digits

            // 2. Generate optimized random portion
            var random = new Random();
            var randomPart = random.Next(100000, 999999).ToString(); // 6 digits

            // 3. Construct ID with ideal length (16-20 chars)
            var id = $"Visit{timestamp}{randomPart}";

            // 4. Ensure length is between 16-20 characters
            id = id.Length > 20 ? id.Substring(0, 20) : id;

            return id;
        }
    }
}
