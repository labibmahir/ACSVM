using Api.BackGroundServices.ProccessContract;
using Api.BackGroundServices.ProcessImplimentations;
using Domain.Dto;
using Domain.Dto.HIKVision;
using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveillanceDevice.Integration.HIKVision;
using System.Net.NetworkInformation;
using Utilities.Constants;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PersonController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<PersonController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;
        private readonly IProgressManager progressManager;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public PersonController(IUnitOfWork context, ILogger<PersonController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService, IProgressManager progressManager)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
            this.progressManager = progressManager;
        }

        /// <summary>
        /// URL: api/person
        /// </summary>
        /// <param name="person">person object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreatePerson)]
        public async Task<ActionResult<Person>> CreatePerson(PersonDto personDto)
        {
            try
            {
                if ((personDto.AccessLevelIds == null || personDto.AccessLevelIds.Length == 0) && (personDto.DeviceIdList == null || personDto.DeviceIdList.Length == 0))
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var personWithSamePhone = await context.PersonRepository.GetPersonByphoneNumber(personDto.PhoneNumber);

                if (personWithSamePhone != null && personWithSamePhone.OrganizationId == personDto.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateIPError);

                var devices = new List<Device>();
                if (personDto.AccessLevelIds != null && personDto.AccessLevelIds.Count() > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevels(personDto.AccessLevelIds);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(personDto.DeviceIdList);
                    devices = devicelist.ToList();
                }

                var currentlyInActiveDevices = new List<Device>();
                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotFoundAccessLevelError);


                Person person = new Person()
                {
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    CreatedBy = GetLoggedInUserId(),
                    Email = personDto.Email,
                    FirstName = personDto.FirstName,
                    Gender = personDto.Gender,
                    OrganizationId = personDto.OrganizationId,
                    PersonNumber = GeneratePersonNo(),
                    PhoneNumber = personDto.PhoneNumber,
                    Surname = personDto.Surname,
                    IsDeviceAdministrator = personDto.IsDeviceAdministrator,
                    UserVerifyMode = personDto.UserVerifyMode,
                    ValidateEndPeriod = personDto.ValidateEndPeriod,
                    ValidateStartPeriod = personDto.ValidateStartPeriod,
                    Department = personDto.Department

                };


                foreach (var device in devices)
                {
                    if (!await IsDeviceActive(device.DeviceIP))
                    {
                        devices = devices.Where(x => x.Oid != device.Oid).ToList();
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);
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
                    employeeNo = person.PersonNumber,
                    deleteUser = false,
                    name = person.FirstName + " " + person.Surname,
                    userType = "normal",
                    closeDelayEnabled = true,
                    Valid = new VMEffectivePeriod()
                    {
                        enable = true,
                        beginTime = person.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = person.ValidateEndPeriod?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = person.IsDeviceAdministrator,
                    userVerifyMode = personDto.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    checkUser = true,
                    addUser = true,
                    //callNumbers = new List<string> { $"{person.PhoneNumber}" },
                    callNumbers = new List<string> { " 1-1-1-401" },
                    floorNumbers = new List<FloorNumber> { new FloorNumber() { min = 1, max = 100 } },
                    gender = person.Gender switch
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


                context.PersonRepository.Add(person);
                await context.SaveChangesAsync();
                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices.Concat(currentlyInActiveDevices))
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = person.OrganizationId,
                        PersonId = person.Oid,
                        IsDeleted = false

                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }

                context.IdentifiedAssignDeviceRepository.AddRange(identifiedAssignDevices);
                await context.SaveChangesAsync();
                return Ok(person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonByKey)]
        [Authorize]
        public async Task<IActionResult> ReadPersonByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var person = await context.PersonRepository.GetPersonByKey(key);

                if (person == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                return Ok(person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/persons
        /// </summary>
        /// <returns>A list of persons.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersons)]
        [AllowAnonymous]
        public async Task<IActionResult> ReadPersons([FromQuery] PersonFilterDto personFilterDto)
        {
            try
            {
                if (personFilterDto.PageSize == 0)
                {
                    var persons = await context.PersonRepository.GetPersons();

                    return Ok(persons);
                }
                else
                {
                    int currentPage = personFilterDto.Page;
                    personFilterDto.Page = ((personFilterDto.Page - 1) * (personFilterDto.PageSize));
                    var persons = await context.PersonRepository.GetPersons(personFilterDto);

                    PagedResultDto<PersonReadDto> personDto = new PagedResultDto<PersonReadDto>()
                    {
                        Data = persons.ToList(),
                        PageNumber = currentPage,
                        PageSize = personFilterDto.PageSize,
                        TotalItems = await context.PersonRepository.GetPersonsCount(personFilterDto)
                    };
                    personDto.TotalPages = (int)Math.Ceiling((double)personDto.TotalItems / personDto.PageSize);
                    return Ok(personDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/persons
        /// </summary>
        /// <returns>A list of persons.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonsMinifiedData)]
        [AllowAnonymous]
        public async Task<IActionResult> ReadPersonsMinifiedData([FromQuery] PersonNoAndPernameNameFilterDto personFilterDto)
        {
            try
            {
                if (personFilterDto.PageSize == 0)
                {
                    var persons = await context.PersonRepository.GetPersons(personFilterDto);
                    PagedResultDto<PersonMinifiedDataDto> personDto = new PagedResultDto<PersonMinifiedDataDto>()
                    {
                        Data = persons.ToList(),
                        TotalItems = await context.PersonRepository.GetPersonsCount(personFilterDto),
                    };
                    return Ok(personDto);
                }
                else
                {
                    int currentPage = personFilterDto.Page;
                    personFilterDto.Page = ((personFilterDto.Page - 1) * (personFilterDto.PageSize));
                    var persons = await context.PersonRepository.GetPersons(personFilterDto);

                    PagedResultDto<PersonMinifiedDataDto> personDto = new PagedResultDto<PersonMinifiedDataDto>()
                    {
                        Data = persons.ToList(),
                        PageNumber = currentPage,
                        PageSize = personFilterDto.PageSize,
                        TotalItems = await context.PersonRepository.GetPersonsCount(personFilterDto),

                    };
                    personDto.TotalPages = (int)Math.Ceiling((double)personDto.TotalItems / personDto.PageSize);

                    return Ok(personDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person/{key}
        /// </summary>
        /// <param name="key">Primary key of the table person.</param>
        /// <param name="personDto">person to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdatePerson)]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePerson(Guid key, PersonDto personDto)
        {
            try
            {
                if (key != personDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                if (personDto.AccessLevelIds != null && personDto.AccessLevelIds.Count() == 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidDeviceId);

                var personInDb = await context.PersonRepository.GetPersonByKey(personDto.Oid);
                if (personInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var devices = new List<Device>();
                if (personDto.AccessLevelIds != null && personDto.AccessLevelIds.Count() > 0)
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByAccessLevels(personDto.AccessLevelIds);
                    devices = devicelist.ToList();
                }
                else
                {
                    var devicelist = await context.DeviceRepository.GetDevicesByDeviceIds(personDto.DeviceIdList);
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
                        return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);
                    }

                }

                if (devices.Count() <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNotActive);


                personInDb.FirstName = personDto.FirstName;
                personInDb.Surname = personDto.Surname;
                personInDb.PhoneNumber = personDto.PhoneNumber;
                personInDb.Gender = personDto.Gender;
                personInDb.Email = personDto.Email;
                personInDb.IsDeviceAdministrator = personDto.IsDeviceAdministrator;
                personInDb.OrganizationId = personDto.OrganizationId;
                personInDb.UserVerifyMode = personDto.UserVerifyMode;
                personInDb.ValidateStartPeriod = personDto.ValidateStartPeriod;
                personInDb.ValidateEndPeriod = personDto.ValidateEndPeriod;
                personInDb.IsDeleted = false;
                personInDb.OrganizationId = personDto.OrganizationId;
                personInDb.DateModified = DateTime.Now;
                personInDb.ModifiedBy = GetLoggedInUserId();
                personInDb.Department = personDto.Department;

                List<VMDoorPermissionSchedule> vMDoorPermissionSchedules = new List<VMDoorPermissionSchedule>();

                VMDoorPermissionSchedule vMDoorPermissionSchedule = new VMDoorPermissionSchedule()
                {
                    doorNo = 1,
                    planTemplateNo = "1",
                };

                vMDoorPermissionSchedules.Add(vMDoorPermissionSchedule);

                VMUserInfo vMUserInfo = new VMUserInfo()
                {
                    employeeNo = personInDb.PersonNumber,
                    deleteUser = false,
                    name = personDto.FirstName + " " + personDto.Surname,
                    userType = "normal",
                    closeDelayEnabled = true,
                    Valid = new VMEffectivePeriod()
                    {
                        enable = true,
                        beginTime = personDto.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                        endTime = personDto.ValidateEndPeriod?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        timeType = "local"
                    },
                    doorRight = "1",
                    RightPlan = vMDoorPermissionSchedules,
                    localUIRight = personDto.IsDeviceAdministrator,
                    callNumbers = new List<string> { " 1-1-1-401" },
                    floorNumbers = new List<FloorNumber> { new FloorNumber() { min = 1, max = 100 } },
                    userVerifyMode = personDto.UserVerifyMode switch
                    {
                        Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                        Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                        Enums.UserVerifyMode.card => "card",
                        _ => "faceAndFpAndCard"//this is default
                    },
                    gender = personDto.Gender switch
                    {
                        Enums.Gender.Male => "male",
                        Enums.Gender.Female => "female",
                        _ => "other"
                    }
                };

                List<int> personToBeUpdateInDevices = new List<int>();
                List<int> personToBeAddedInDevices = new List<int>();
                var identifiedAssignedDeviceByPerson = await context.IdentifiedAssignDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.PersonId == personDto.Oid);

                var newAssignedDevice = devices.Select(x => x.Oid).ToList();
                var identifiedAlreadyAssignedDevice = identifiedAssignedDeviceByPerson.Select(x => x.DeviceId).ToList();
                personToBeUpdateInDevices = newAssignedDevice.Intersect(identifiedAlreadyAssignedDevice).ToList();
                personToBeAddedInDevices = newAssignedDevice.Except(identifiedAlreadyAssignedDevice).ToList();

                foreach (var item in identifiedAssignedDeviceByPerson)
                    context.IdentifiedAssignDeviceRepository.Delete(new IdentifiedAssignDevice() { Oid = item.Oid });


                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                foreach (var item in devices.Concat(currentlyInActiveDevices))
                {
                    IdentifiedAssignDevice identifiedAssignDevice = new IdentifiedAssignDevice()
                    {
                        CreatedBy = GetLoggedInUserId(),
                        DateCreated = DateTime.Now,
                        DeviceId = item.Oid,
                        OrganizationId = personDto.OrganizationId,
                        PersonId = personDto.Oid,
                        IsDeleted = false

                    };
                    identifiedAssignDevices.Add(identifiedAssignDevice);
                }

                if (personToBeUpdateInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => personToBeAddedInDevices.Contains(x.Oid)).ToList())
                    {
                        var updated = await _visionMachineService.UpdateUser(device, vMUserInfo);
                        var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(updated);
                        if (res.StatusCode != 1)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, $"Device Error , statusString: {res.ErrorCode} ErrorMessage: {res.ErrorMsg}");
                        }
                    }
                }
                if (personToBeAddedInDevices.Count() > 0)
                {
                    foreach (var device in devices.Where(x => personToBeAddedInDevices.Contains(x.Oid)).ToList())
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
                context.PersonRepository.Update(personInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/person/{key}
        /// </summary>
        /// <param name="key">Primary key of the table person.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeletePerson)]
        public async Task<IActionResult> DeletePerson(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var personInDb = await context.PersonRepository.GetPersonByKey(key);

                if (personInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var personAssignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(key);

                foreach (var item in personAssignedDevices)
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

                personInDb.DateModified = DateTime.Now;
                personInDb.ModifiedBy = GetLoggedInUserId();
                personInDb.IsDeleted = true;

                VMUserInfoDetailsDeleteRequest vMCardInfoDeleteRequest = new VMUserInfoDetailsDeleteRequest()
                {
                    EmployeeNoList = new List<VMEmployeeNoListItem?>() { new() { employeeNo = personInDb.PersonNumber } }
                };

                foreach (var assignedDevice in personAssignedDevices)
                {

                    var result = await _visionMachineService.DeleteUserWithDetails(assignedDevice.Device, vMCardInfoDeleteRequest);
                }
                var fingerPrintInDb = await context.FingerPrintRepository.GetAllFingerPrintOfPeronByPersonId(personInDb.Oid);
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
                var assignCardInDb = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByPerson(personInDb.Oid);
                if (assignCardInDb != null)
                {
                    assignCardInDb.IsDeleted = true;
                    assignCardInDb.DateModified = DateTime.Now;
                    assignCardInDb.ModifiedBy = GetLoggedInUserId();
                    context.IdentifiedAssignCardRepository.Update(assignCardInDb);

                }

                context.PersonRepository.Update(personInDb);
                await context.SaveChangesAsync();


                return Ok(personInDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        [HttpGet]
        [Route(RouteConstants.ImportPersonFromDevice)]
        public async Task<IActionResult> ImportPersonFromDevice(int id)
        {
            try
            {
                var device = await context.DeviceRepository.GetDeviceByKey(id);
                if (device == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);
                }

                if (!await IsDeviceActive(device.DeviceIP))
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.SelectedDeviceNotActive);
                }

                IProcess importPeople = new ImportPeopleFromDeviceProcess(device, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);
                return Ok(importPeople.ToProcessDto());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person
        /// </summary>
        /// <param name="ExportPeopleToDevice">ExportPeopleToDevice object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.ExportPeopleToDevice)]
        public async Task<ActionResult<Person>> ExportToDevice(ExportToDeviceDto exportToDeviceDto)
        {
            try
            {
                var fromDevice = await context.DeviceRepository.GetDeviceByKey(exportToDeviceDto.FromDeviceId);
                if (fromDevice == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);
                }
                var toDevice = await context.DeviceRepository.GetDeviceByKey(exportToDeviceDto.FromDeviceId);
                if (toDevice == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);
                }
                if (!await IsDeviceActive(toDevice.DeviceIP))
                {
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.SelectedDeviceNotActive);
                }

                IProcess importPeople = new ExportPeopleToDeviceProcess(exportToDeviceDto, ProcessPriority.Urgent);
                await progressManager.AddProcess(importPeople);
                return Ok(exportToDeviceDto);
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

        private string GeneratePersonNo()
        {
            // 1. Use shorter timestamp (Unix seconds instead of milliseconds)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(); // 10 digits

            // 2. Generate optimized random portion
            var random = new Random();
            var randomPart = random.Next(100000, 999999).ToString(); // 6 digits

            // 3. Construct ID with ideal length (16-20 chars)
            var id = $"Person{timestamp}{randomPart}";

            // 4. Ensure length is between 16-20 characters
            id = id.Length > 20 ? id.Substring(0, 20) : id;

            return id;
        }
    }
}
