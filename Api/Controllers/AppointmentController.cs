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
        public AppointmentController(IUnitOfWork context, ILogger<AppointmentController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
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
        public async Task<ActionResult<Person>> CreateAppointment(AppointmentDto appointmentDto)
        {
            try
            {
                if (!appointmentDto.AccessLevelId.HasValue && appointmentDto.DeviceIdList.Length == 0)
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
                    var checkPersonId = await context.PersonImageRepository.FirstOrDefaultAsync(x => x.Oid == personid);
                    if (checkPersonId == null)
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.PersonNotFound);
                }

                var devices = new List<Device>();
                if (appointmentDto.AccessLevelId.HasValue && appointmentDto.AccessLevelId.Value > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(appointmentDto.AccessLevelId.Value);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(appointmentDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                // var currentlyInActiveDevices = new List<Device>();
                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);

                foreach (var device in devices)
                {
                    if (!await IsDeviceActive(device.DeviceIP))
                    {
                        // devices = devices.Where(x => x.Oid != device.Oid).ToList();
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);


                    }

                }

                //if (devices.Count() <= 0)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);


                List<int> visitorToBeUpdateInDevices = new List<int>();
                List<int> visitorToBeAddedInDevices = new List<int>();

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
                        VisitorNumber = GenerateVisitorNumber(),
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
                        VisitorNumber = GenerateVisitorNumber(),
                        PhoneNumber = appointmentDto.PhoneNumber,
                        Surname = appointmentDto.Surname,
                        UserVerifyMode = appointmentDto.VisitorVerifyMode,
                        ValidateEndPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.EndTime,
                        ValidateStartPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.StartTime,
                        Address = appointmentDto.Address,
                        CompanyName = appointmentDto.CompanyName,

                    };
                    context.VisitorRepository.Update(visitor);

                    var identifiedAssignedDeviceByVisitor = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitor.Oid);
                    var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                    var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByVisitor.Select(x => x.DeviceId).ToList();

                    visitorToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                    visitorToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();
                    foreach (var item in identifiedAssignedDeviceByVisitor)
                        context.IdentifiedAssignDeviceRepository.Delete(new IdentifiedAssignDevice() { Oid = item.Oid });

                }
                List<VMDoorPermissionSchedule> vMDoorPermissionSchedules = new List<VMDoorPermissionSchedule>();

                VMDoorPermissionSchedule vMDoorPermissionSchedule = new VMDoorPermissionSchedule()
                {
                    doorNo = 1,
                    planTemplateNo = "1",
                };

                vMDoorPermissionSchedules.Add(vMDoorPermissionSchedule);


                VMUserInfo vMUserInfo = new VMUserInfo()
                {
                    employeeNo = visitor.VisitorNumber,
                    deleteUser = false,
                    name = visitor.FirstName + " " + visitor.Surname,
                    userType = "normal",
                    closeDelayEnabled = true,
                    Valid = new VMEffectivePeriod()
                    {
                        enable = true,
                        beginTime = visitor.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = visitor.ValidateEndPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = false,
                    userVerifyMode = visitor.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    checkUser = true,
                    addUser = true,
                    gender = visitor.Gender switch
                    {
                        Enums.Gender.Male => "male",
                        Enums.Gender.Female => "female",
                        _ => "other"
                    },
                };
                if (visitorToBeAddedInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeAddedInDevices.Contains(x.Oid)).ToList())
                    {
                        var vService = await _visionMachineService.AddUser(device, vMUserInfo);
                    }
                }
                if (visitorToBeUpdateInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeUpdateInDevices.Contains(x.Oid)).ToList())
                    {
                        var vService = await _visionMachineService.UpdateUser(device, vMUserInfo);
                    }
                }


                // context.VisitorRepository.Add(visitor);
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

                    };
                    identifiedAssignDevices.AddRange(identifiedAssignDevices);
                }

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();


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

                    return Ok(appointmentDto);

                }
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

                if (!appointmentDto.AccessLevelId.HasValue && appointmentDto.DeviceIdList.Length == 0)
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
                    var checkPersonId = await context.PersonImageRepository.FirstOrDefaultAsync(x => x.Oid == personid);
                    if (checkPersonId == null)
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.PersonNotFound);
                }

                var devices = new List<Device>();
                if (appointmentDto.AccessLevelId.HasValue && appointmentDto.AccessLevelId.Value > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(appointmentDto.AccessLevelId.Value);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(appointmentDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);

                foreach (var device in devices)
                {
                    if (!await IsDeviceActive(device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);

                    }
                }

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

                visitorInDb.IsDeleted = false;
                visitorInDb.ModifiedBy = GetLoggedInUserId();
                visitorInDb.DateModified = DateTime.Now;
                visitorInDb.Email = appointmentDto.Email;
                visitorInDb.FirstName = appointmentDto.FirstName;
                visitorInDb.Gender = appointmentDto.Gender;
                visitorInDb.OrganizationId = appointmentDto.OrganizationId;
                visitorInDb.VisitorNumber = GenerateVisitorNumber();
                visitorInDb.PhoneNumber = appointmentDto.PhoneNumber;
                visitorInDb.Surname = appointmentDto.Surname;
                visitorInDb.UserVerifyMode = appointmentDto.VisitorVerifyMode;
                visitorInDb.ValidateEndPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.EndTime;
                visitorInDb.ValidateStartPeriod = appointmentDto.AppointmentDate.Date + appointmentDto.StartTime;
                visitorInDb.Address = appointmentDto.Address;
                visitorInDb.CompanyName = appointmentDto.CompanyName;

                context.VisitorRepository.Update(visitorInDb);
                var identifiedAssignedDeviceByVisitor = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitorInDb.Oid);
                var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByVisitor.Select(x => x.DeviceId).ToList();

                visitorToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                visitorToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();
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

                    };
                    identifiedAssignDevices.AddRange(identifiedAssignDevices);
                }


                List<VMDoorPermissionSchedule> vMDoorPermissionSchedules = new List<VMDoorPermissionSchedule>();

                VMDoorPermissionSchedule vMDoorPermissionSchedule = new VMDoorPermissionSchedule()
                {
                    doorNo = 1,
                    planTemplateNo = "1",
                };

                vMDoorPermissionSchedules.Add(vMDoorPermissionSchedule);

                VMUserInfo vMUserInfo = new VMUserInfo()
                {
                    employeeNo = visitorInDb.VisitorNumber,
                    deleteUser = false,
                    name = visitorInDb.FirstName + " " + visitorInDb.Surname,
                    userType = "normal",
                    closeDelayEnabled = true,
                    Valid = new VMEffectivePeriod()
                    {
                        enable = true,
                        beginTime = visitorInDb.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = visitorInDb.ValidateEndPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = false,
                    userVerifyMode = visitorInDb.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    gender = visitorInDb.Gender switch
                    {
                        Enums.Gender.Male => "male",
                        Enums.Gender.Female => "female",
                        _ => "other"
                    }
                };
                if (visitorToBeAddedInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeAddedInDevices.Contains(x.Oid)).ToList())
                    {
                        var vService = await _visionMachineService.AddUser(device, vMUserInfo);
                    }
                }
                if (visitorToBeUpdateInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeUpdateInDevices.Contains(x.Oid)).ToList())
                    {
                        var vService = await _visionMachineService.UpdateUser(device, vMUserInfo);
                    }
                }
                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status204NoContent);
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
        private string GenerateVisitorNumber()
        {
            // Use a timestamp and a short GUID segment for uniqueness
            string prefix = "Visitor-";
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // 14 characters
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 18); // 18 characters

            string visitorNumber = prefix + timestamp + randomPart;

            // Ensure total length is not more than 40
            return visitorNumber.Length > 40 ? visitorNumber.Substring(0, 40) : visitorNumber;
        }
    }
}
