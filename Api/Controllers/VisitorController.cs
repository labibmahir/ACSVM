using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;
using System.Net.NetworkInformation;
using Domain.Dto.PaginationFiltersDto;
using Api.BackGroundServices.ProccessContract;
using Api.BackGroundServices.ProcessImplimentations;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitorController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<VisitorController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;
        private readonly IProgressManager progressManager;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public VisitorController(IUnitOfWork context, ILogger<VisitorController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService, IProgressManager progressManager)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
            this.progressManager = progressManager;
        }
        /// <summary>
        /// URL: api/visitor
        /// </summary>
        /// <param name="visitor">visitor object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateVisitor)]
        public async Task<ActionResult<Person>> CreateVisitor(VisitorDto visitorDto)
        {
            try
            {
                if (!visitorDto.AccessLevelId.HasValue && visitorDto.DeviceIdList != null && visitorDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                //var visitorWithSameVisitornNumer = await context.VisitorRepository.GetVisitorByVisitorNumber(visitorDto.VisitorNumber);

                //if (visitorWithSameVisitornNumer != null && visitorWithSameVisitornNumer.OrganizationId == visitorDto.OrganizationId)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNumberTaken);

                //var checkPersonNoWithSameVisitornNumer = await context.PersonRepository.GetPersonByPersonNumber(visitorDto.VisitorNumber);

                //if (checkPersonNoWithSameVisitornNumer != null && checkPersonNoWithSameVisitornNumer.OrganizationId == visitorDto.OrganizationId)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNumberTaken);

                var visitorWithSamePhone = await context.VisitorRepository.GetVisitorByphoneNumber(visitorDto.PhoneNumber);

                if (visitorWithSamePhone != null && visitorWithSamePhone.OrganizationId == visitorDto.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateIPError);

                var devices = new List<Device>();
                if (visitorDto.AccessLevelId.HasValue && visitorDto.AccessLevelId.Value > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(visitorDto.AccessLevelId.Value);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(visitorDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                var currentlyInActiveDevices = new List<Device>();
                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);


                Visitor visitor = new Visitor()
                {
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    CreatedBy = GetLoggedInUserId(),
                    Email = visitorDto.Email,
                    FirstName = visitorDto.FirstName,
                    Gender = visitorDto.Gender,
                    OrganizationId = visitorDto.OrganizationId,
                    VisitorNumber = GenerateVisitorNo(),//visitorDto.VisitorNumber,
                    PhoneNumber = visitorDto.PhoneNumber,
                    Surname = visitorDto.Surname,
                    UserVerifyMode = visitorDto.UserVerifyMode,
                    ValidateEndPeriod = visitorDto.ValidateEndPeriod,
                    ValidateStartPeriod = visitorDto.ValidateStartPeriod,

                };


                foreach (var device in devices)
                {
                    if (!await IsDeviceActive(device.DeviceIP))
                    {
                        devices = devices.Where(x => x.Oid != device.Oid).ToList();
                    }

                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);


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
                        beginTime = visitorDto.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = visitorDto.ValidateEndPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = false,
                    userVerifyMode = visitorDto.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    checkUser = true,
                    addUser = true,
                    callNumbers = new List<string> { " 1-1-1-401" },
                    floorNumbers = new List<FloorNumber> { new FloorNumber() { min = 1, max = 100 } },
                    gender = visitorDto.Gender switch
                    {
                        Enums.Gender.Male => "male",
                        Enums.Gender.Female => "female",
                        _ => "other"
                    },
                };

                foreach (var device in devices)
                {
                    var vService = await _visionMachineService.AddUser(device, vMUserInfo);
                    var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(vService);
                    if (res.StatusCode != 1)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, $"Device Error , statusString: {res.ErrorCode} ErrorMessage: {res.ErrorMsg}");
                    }
                }


                context.VisitorRepository.Add(visitor);
                await context.SaveChangesAsync();
                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices.Concat(currentlyInActiveDevices))
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = visitor.OrganizationId,
                        VisitorId = visitor.Oid,
                        IsDeleted = false

                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();
                return Ok(visitor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/visitor/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table visitor.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadVisitorByKey)]
        [Authorize]
        public async Task<IActionResult> ReadVisitorByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitor = await context.VisitorRepository.GetVisitorByKey(key);

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
        /// URL: api/visitor/phone-number/{phoneNo}
        /// </summary>
        /// <param name="key">Primary key of the table visitor.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadVisitorByPhone)]

        public async Task<IActionResult> ReadVisitorByPhone(string phoneNo)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNo))
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitor = await context.VisitorRepository.GetVisitorByphoneNumber(phoneNo);

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
        /// URL:api/visitors
        /// </summary>
        /// <returns>A list of visitors.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadVisitors)]
        public async Task<IActionResult> ReadVisitors([FromQuery] VisitorFilterDto visitorFilterDto)
        {
            try
            {
                if (visitorFilterDto.PageSize == 0)
                {
                    var visitors = await context.VisitorRepository.GetVisitors();

                    return Ok(visitors);
                }
                else
                {
                    int currentPage = visitorFilterDto.Page;
                    visitorFilterDto.Page = ((visitorFilterDto.Page - 1) * (visitorFilterDto.PageSize));
                    var visitors = await context.VisitorRepository.GetVisitors(visitorFilterDto);

                    PagedResultDto<VisitorReadDto> visitorDto = new PagedResultDto<VisitorReadDto>()
                    {
                        Data = visitors.ToList(),
                        PageNumber = currentPage,
                        PageSize = visitorFilterDto.PageSize,
                        TotalItems = await context.VisitorRepository.GetVisitorsCount(visitorFilterDto)
                    };
                    visitorDto.TotalPages = (int)Math.Ceiling((double)visitorDto.TotalItems / visitorDto.PageSize);
                    return Ok(visitorDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/visitor/{key}
        /// </summary>
        /// <param name="key">Primary key of the table person.</param>
        /// <param name="visitorDto">person to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateVisitor)]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateVisitor(Guid key, VisitorDto visitorDto)
        {
            try
            {
                if (key != visitorDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                if (!visitorDto.AccessLevelId.HasValue && visitorDto.DeviceIdList != null && visitorDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var visitorInDb = await context.VisitorRepository.GetVisitorByKey(visitorDto.Oid);
                if (visitorInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                //var visitorWithSameVisitorNo = await context.VisitorRepository.GetVisitorByVisitorNumber(visitorDto.VisitorNumber);

                //if (visitorWithSameVisitorNo != null && visitorWithSameVisitorNo.OrganizationId == visitorDto.OrganizationId && visitorWithSameVisitorNo.Oid != visitorWithSameVisitorNo.Oid)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.PersonNumberTaken);

                //var checkPersonNoWithSameVisitornNumer = await context.PersonRepository.GetPersonByPersonNumber(visitorDto.VisitorNumber);

                //if (checkPersonNoWithSameVisitornNumer != null && checkPersonNoWithSameVisitornNumer.OrganizationId == visitorDto.OrganizationId)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNumberTaken);



                var devices = new List<Device>();
                if (visitorDto.AccessLevelId.HasValue && visitorDto.AccessLevelId.Value > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevel(visitorDto.AccessLevelId.Value);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(visitorDto.DeviceIdList);
                    devices = devicelist.ToList();
                }


                var currentlyInActiveDevices = new List<Device>();

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);

                foreach (var device in devices)
                {
                    if (!await IsDeviceActive(device.DeviceIP))
                    {
                        devices = devices.Where(x => x.Oid != device.Oid).ToList();
                    }

                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);


                visitorInDb.FirstName = visitorDto.FirstName;
                visitorInDb.Surname = visitorDto.Surname;
                visitorInDb.PhoneNumber = visitorDto.PhoneNumber;
                visitorInDb.Gender = visitorDto.Gender;
                visitorInDb.Email = visitorDto.Email;
                visitorInDb.OrganizationId = visitorDto.OrganizationId;
                visitorInDb.UserVerifyMode = visitorDto.UserVerifyMode;
                visitorInDb.ValidateStartPeriod = visitorDto.ValidateStartPeriod;
                visitorInDb.ValidateEndPeriod = visitorDto.ValidateEndPeriod;
                visitorInDb.IsDeleted = false;
                visitorInDb.OrganizationId = visitorDto.OrganizationId;
                visitorInDb.DateModified = DateTime.Now;
                visitorInDb.ModifiedBy = GetLoggedInUserId();

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
                    name = visitorDto.FirstName + " " + visitorDto.Surname,
                    userType = "normal",
                    closeDelayEnabled = true,
                    Valid = new VMEffectivePeriod()
                    {
                        enable = true,
                        beginTime = visitorDto.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = visitorDto.ValidateEndPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = false,
                    userVerifyMode = visitorDto.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    gender = visitorDto.Gender switch
                    {
                        Enums.Gender.Male => "male",
                        Enums.Gender.Female => "female",
                        _ => "other"
                    }
                };
                List<int> visitorToBeUpdateInDevices = new List<int>();
                List<int> visitorToBeAddedInDevices = new List<int>();
                var identifiedAssignedDeviceByVisitor = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.VisitorId == visitorDto.Oid);

                var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByVisitor.Select(x => x.DeviceId).ToList();

                visitorToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                visitorToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();

                foreach (var item in identifiedAssignedDeviceByVisitor)
                    context.IdentifiedAssignDeviceRepository.Delete(new IdentifiedAssignDevice() { Oid = item.Oid });


                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices.Concat(currentlyInActiveDevices))
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = visitorDto.OrganizationId,
                        PersonId = visitorDto.Oid,
                        IsDeleted = false
                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }
                if (visitorToBeUpdateInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeUpdateInDevices.Contains(x.Oid)).ToList())
                    {
                        var updated = await _visionMachineService.UpdateUser(device, vMUserInfo);
                        var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(updated);
                        if (res.StatusCode != 1)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, $"Device Error , statusString: {res.ErrorCode} ErrorMessage: {res.ErrorMsg}");
                        }
                    }
                }

                if (visitorToBeAddedInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => visitorToBeAddedInDevices.Contains(x.Oid)).ToList())
                    {
                        var updated = await _visionMachineService.AddUser(device, vMUserInfo);
                        var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(updated);
                        if (res.StatusCode != 1)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, $"Device Error , statusString: {res.ErrorCode} ErrorMessage: {res.ErrorMsg}");
                        }
                    }
                }

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                context.VisitorRepository.Update(visitorInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }



        /// <summary>
        /// URL: api/assign-device-to-visitor
        /// </summary>
        /// <param name="visitorDeviceAssignAndUnAssignDto">person object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.AssignedDeviceToVisitor)]
        public async Task<ActionResult<Person>> AssignedDeviceToVisitor(VisitorDeviceAssignAndUnAssignDto visitorDeviceAssignAndUnAssignDto)
        {
            try
            {
                if (visitorDeviceAssignAndUnAssignDto.DeviceIdList == null || visitorDeviceAssignAndUnAssignDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var visitor = await context.VisitorRepository.GetVisitorByKey(visitorDeviceAssignAndUnAssignDto.VisitorId);
                if (visitor == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNotFound);


                var devices = new List<Device>();

                if (visitorDeviceAssignAndUnAssignDto.DeviceIdList != null && visitorDeviceAssignAndUnAssignDto.DeviceIdList.Count() > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(visitorDeviceAssignAndUnAssignDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);

                var assignedDevice = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(visitorDeviceAssignAndUnAssignDto.VisitorId);

                List<int> assignedDevicesId = new List<int>();
                if (assignedDevice != null && assignedDevice.Any())
                    assignedDevicesId = assignedDevice.Select(x => x.DeviceId).ToList();

                var devicesToBeAssigned = devices.Where(x => !assignedDevicesId.Contains(x.Oid)).ToList();

                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devicesToBeAssigned)
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = visitor.OrganizationId,
                        VisitorId = visitor.Oid,
                        IsDeleted = false

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

                foreach (var item in devices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Add,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
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

                IProcess importPeople = new DeviceActionProcess(_configuration, deviceSynchronizer, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);
                return Ok(visitor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/assign-device-to-visitor
        /// </summary>
        /// <param name="visitorDeviceAssignAndUnAssignDto">person object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.UnAssignedDeviceToVisitor)]
        public async Task<ActionResult<Person>> UnAssignedDeviceToVisitor(VisitorDeviceAssignAndUnAssignDto visitorDeviceAssignAndUnAssignDto)
        {
            try
            {
                if (visitorDeviceAssignAndUnAssignDto.DeviceIdList == null || visitorDeviceAssignAndUnAssignDto.DeviceIdList.Length == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var visitor = await context.VisitorRepository.GetVisitorByKey(visitorDeviceAssignAndUnAssignDto.VisitorId);
                if (visitor == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.VisitorNotFound);


                var devices = new List<Device>();

                if (visitorDeviceAssignAndUnAssignDto.DeviceIdList != null && visitorDeviceAssignAndUnAssignDto.DeviceIdList.Count() > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(visitorDeviceAssignAndUnAssignDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);

                var assignedDevice = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(visitorDeviceAssignAndUnAssignDto.VisitorId);

                List<int> assignedDevicesId = new List<int>();
                if (assignedDevice != null && assignedDevice.Any())
                    assignedDevicesId = assignedDevice.Select(x => x.DeviceId).ToList();

                var devicesToBeUnAssigned = devices.Where(x => assignedDevicesId.Contains(x.Oid)).ToList();

                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devicesToBeUnAssigned)
                {
                    var identifiedAssignedDevice = await context.IdentifiedAssignDeviceRepository.FirstOrDefaultAsync(x => x.VisitorId == visitor.Oid && x.DeviceId == item.Oid);

                    identifiedAssignedDevice.IsDeleted = true;
                    context.IdentifiedAssignDeviceRepository.Update(identifiedAssignedDevice);

                }

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

                foreach (var item in devices)
                {
                    IdentifiedSyncDevice identifiedSyncDevice = new IdentifiedSyncDevice()
                    {
                        Oid = Guid.NewGuid(),
                        Action = Utilities.Constants.Enums.DeviceAction.Delete,
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
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

                IProcess importPeople = new DeviceActionProcess(_configuration, deviceSynchronizer, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);
                return Ok(visitor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/visitor/{key}
        /// </summary>
        /// <param name="key">Primary key of the table person.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteVisitor)]
        public async Task<IActionResult> DeleteVisitor(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitorInDb = await context.VisitorRepository.GetVisitorByKey(key);

                if (visitorInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var visitorAssignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(key);

                foreach (var item in visitorAssignedDevices)
                {
                    item.IsDeleted = true;
                    item.DateModified = DateTime.Now;
                    item.ModifiedBy = GetLoggedInUserId();

                    context.IdentifiedAssignDeviceRepository.Update(item);
                }

                var personsCard = await context.IdentifiedAssignCardRepository.QueryAsync(x => x.IsDeleted == false && x.PersonId == key);

                foreach (var item in personsCard)
                {
                    item.IsDeleted = true;
                    item.DateModified = DateTime.Now;
                    item.ModifiedBy = GetLoggedInUserId();

                    context.IdentifiedAssignCardRepository.Update(item);
                }

                var personsImage = await context.PersonImageRepository.QueryAsync(x => x.IsDeleted == false && x.PersonId == key);

                foreach (var item in personsImage)
                {
                    item.IsDeleted = true;
                    item.DateModified = DateTime.Now;
                    item.ModifiedBy = GetLoggedInUserId();

                    context.PersonImageRepository.Update(item);
                }

                var personsFingerPrints = await context.FingerPrintRepository.QueryAsync(x => x.IsDeleted == false && x.PersonId == key);

                foreach (var item in personsFingerPrints)
                {
                    item.IsDeleted = true;
                    item.DateModified = DateTime.Now;
                    item.ModifiedBy = GetLoggedInUserId();

                    context.FingerPrintRepository.Update(item);
                }

                visitorInDb.DateModified = DateTime.Now;
                visitorInDb.ModifiedBy = GetLoggedInUserId();
                visitorInDb.IsDeleted = true;

                VMUserInfoDetailsDeleteRequest vMCardInfoDeleteRequest = new VMUserInfoDetailsDeleteRequest()
                {
                    EmployeeNoList = new List<VMEmployeeNoListItem?>() { new() { employeeNo = visitorInDb.VisitorNumber } }
                };

                foreach (var assignedDevice in visitorAssignedDevices)
                {

                    var result = await _visionMachineService.DeleteUserWithDetails(assignedDevice.Device, vMCardInfoDeleteRequest);
                }

                var fingerPrintInDb = await context.FingerPrintRepository.GetAllFingerPrintOfVisitorByVisitorId(visitorInDb.Oid);
                if (fingerPrintInDb != null && fingerPrintInDb.Count() > 0)
                {

                    foreach (var item in fingerPrintInDb)
                    {

                        item.IsDeleted = true;
                        item.ModifiedBy = GetLoggedInUserId();
                        item.DateModified = DateTime.Now;
                        context.FingerPrintRepository.Update(item);

                    }
                }
                var assignCardInDb = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByVisitor(visitorInDb.Oid);
                if (assignCardInDb != null)
                {
                    assignCardInDb.IsDeleted = true;
                    assignCardInDb.DateModified = DateTime.Now;
                    assignCardInDb.ModifiedBy = GetLoggedInUserId();
                    context.IdentifiedAssignCardRepository.Update(assignCardInDb);

                }

                context.VisitorRepository.Update(visitorInDb);
                await context.SaveChangesAsync();

                return Ok(visitorInDb);
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