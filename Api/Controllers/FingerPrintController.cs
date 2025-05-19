using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;
using System.Net.NetworkInformation;
using Infrastructure;
using System.Reflection;
using Domain.Dto.PaginationFiltersDto;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FingerPrintController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<FingerPrintController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public FingerPrintController(IUnitOfWork context, ILogger<FingerPrintController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
        }

        /// <summary>
        /// URL: api/capturefingerprint
        /// </summary>
        /// <param name="captureFingerPrintDto">CaptureFingerPrintDto object.</param>
        /// <returns>Http status code: Ok.</returns>

        [HttpPost]
        [Route(RouteConstants.CaptureFingerPrints)]
        public async Task<IActionResult> CaptureFingerPrint([FromBody] CaptureFingerPrintDto captureFingerPrintDto)
        {
            try
            {
                var device = await context.DeviceRepository.FirstOrDefaultAsync(x => x.Oid == captureFingerPrintDto.DeviceId);
                if (device == null) return NotFound("The specified device does not exist");

                VMCaptureFingerPrintResponse finger = await _visionMachineService.CaptureFingerPrint(device,
                    new VMCaptureFingerPrintRequest()
                    {
                        fingerNo = captureFingerPrintDto.FingerNumber
                    });

                CapturedFingerPrintDto capturedFingerPrintDto = new CapturedFingerPrintDto()
                {
                    FingerData = finger.fingerData,
                    FingerNumber = finger.fingerNo,
                    FingerPrintQuality = finger.fingerPrintQuality,
                };
                return Ok(capturedFingerPrintDto);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not delete the finger print");
            }
        }


        /// <summary>
        /// URL: api/fingerprint-person
        /// </summary>
        /// <param name="fingerPrint">fingerPrint object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreatePersonFingerPrint)]
        public async Task<ActionResult<Person>> CreatePersonFingerPrint(PersonFingerPrintDto personFingerPrintDto)
        {
            try
            {
                var person = await context.PersonRepository.GetPersonByKey(personFingerPrintDto.PersonId);
                if (person == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                var fingerPrintWithSameFinger = await context.FingerPrintRepository.GetFingerPrintByPersonAndFingerNumber(personFingerPrintDto.PersonId, personFingerPrintDto.FingerNumber);

                if (fingerPrintWithSameFinger != null && fingerPrintWithSameFinger.OrganizationId == fingerPrintWithSameFinger.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateFingerError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(person.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                FingerPrint fingerPrint = new FingerPrint()
                {
                    Oid = Guid.NewGuid(),
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    Data = personFingerPrintDto.Data,
                    FingerNumber = personFingerPrintDto.FingerNumber,
                    OrganizationId = personFingerPrintDto.OrganizationId
                };

                VMFingerPrintSetUpRequest vMFingerPrintSetUpRequest = new VMFingerPrintSetUpRequest()
                {
                    employeeNo = person.PersonNumber,
                    fingerData = personFingerPrintDto.Data,
                    fingerPrintID = (int)personFingerPrintDto.FingerNumber,
                    fingerType = "normalFP",//personFingerPrintDto.FingerprintType,
                    enableCardReader = new int[] { 1 },
                };


                foreach (var item in assignedDevices)
                {
                    var restult = await _visionMachineService.SetFingerprint(item.Device, vMFingerPrintSetUpRequest);

                    if (restult.IsSuccess == false)
                    {

                    }
                }

                context.FingerPrintRepository.Add(fingerPrint);
                return Ok(fingerPrint);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/fingerprint-person
        /// </summary>
        /// <param name="fingerPrint">fingerPrint object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateVisitorFingerPrint)]
        public async Task<ActionResult<Person>> CreateVisitorFingerPrint(VisitorFingerPrintDto visitorFingerPrintDto)
        {
            try
            {
                var visitor = await context.VisitorRepository.GetVisitorByKey(visitorFingerPrintDto.VisitorId);
                if (visitor == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                var fingerPrintWithSameFinger = await context.FingerPrintRepository.GetFingerPrintByVisitorAndFingerNumber(visitorFingerPrintDto.VisitorId, visitorFingerPrintDto.FingerNumber);

                if (fingerPrintWithSameFinger != null && fingerPrintWithSameFinger.OrganizationId == fingerPrintWithSameFinger.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateFingerError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(visitor.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                FingerPrint fingerPrint = new FingerPrint()
                {
                    Oid = Guid.NewGuid(),
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    Data = visitorFingerPrintDto.Data,
                    FingerNumber = visitorFingerPrintDto.FingerNumber,
                    OrganizationId = visitorFingerPrintDto.OrganizationId
                };

                VMFingerPrintSetUpRequest vMFingerPrintSetUpRequest = new VMFingerPrintSetUpRequest()
                {
                    employeeNo = visitor.VisitorNumber,
                    fingerData = visitorFingerPrintDto.Data,
                    fingerPrintID = (int)visitorFingerPrintDto.FingerNumber,
                    fingerType = "normalFP",//personFingerPrintDto.FingerprintType,
                    enableCardReader = new int[] { 1 },
                };


                foreach (var item in assignedDevices)
                {
                    var restult = await _visionMachineService.SetFingerprint(item.Device, vMFingerPrintSetUpRequest);

                    if (restult.IsSuccess == false)
                    {

                    }
                }

                context.FingerPrintRepository.Add(fingerPrint);
                return Ok(fingerPrint);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/fingerprint/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Fingerprint.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadFingerPrintByKey)]
        public async Task<IActionResult> ReadFingerPrintByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var fingerPrint = await context.FingerPrintRepository.GetFingerPrintByKey(key);


                return Ok(fingerPrint);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/fingerPrint/person/{PersonId}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadFingerPrintByPerson)]
        public async Task<IActionResult> ReadFingerPrintByPerson(Guid PersonId)
        {
            try
            {
                if (PersonId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var fingerPrints = await context.FingerPrintRepository.GetAllFingerPrintOfPeronByPersonId(PersonId);

                return Ok(fingerPrints);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/fingerPrint/person/{PersonId}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadFingerPrintByVisitor)]
        public async Task<IActionResult> ReadFingerPrintByVisitor(Guid VisitorId)
        {
            try
            {
                if (VisitorId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var fingerPrints = await context.FingerPrintRepository.GetAllFingerPrintOfVisitorByVisitorId(VisitorId);

                return Ok(fingerPrints);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/fingerPrint/{key}
        /// </summary>
        /// <param name="key">Primary key of the table FingerPrint.</param>
        /// <param name="fingerPrintUpdateDto">FingerPrint to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateFingerPrint)]

        public async Task<IActionResult> UpdateFingerPrint(Guid key, FingerPrintUpdateDto fingerPrintUpdateDto)
        {
            try
            {
                if (key != fingerPrintUpdateDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);


                var fingerPrintInDb = await context.FingerPrintRepository.GetFingerPrintByKeyAndFingerNo(fingerPrintUpdateDto.Oid, fingerPrintUpdateDto.FingerNumber);

                if (fingerPrintInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);



                Person person = new Person();
                Visitor visitor = new Visitor();
                if (fingerPrintInDb.PersonId.HasValue)
                {
                    person = await context.PersonRepository.GetPersonByKey(fingerPrintInDb.PersonId.Value);
                }
                else if (fingerPrintInDb.VisitorId.HasValue)
                {
                    visitor = await context.VisitorRepository.GetVisitorByKey(fingerPrintInDb.VisitorId.Value);
                }
                VMFingerPrintSetUpRequest vMFingerPrintSetUpRequest = new VMFingerPrintSetUpRequest();

                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                if (fingerPrintInDb.VisitorId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(fingerPrintInDb.VisitorId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                    vMFingerPrintSetUpRequest.employeeNo = visitor.VisitorNumber;
                }
                else if (fingerPrintInDb.PersonId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(fingerPrintInDb.PersonId.Value);
                    identifiedAssignDevices = assignDevices.ToList();

                }
                foreach (var item in identifiedAssignDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }


                vMFingerPrintSetUpRequest.fingerData = fingerPrintUpdateDto.Data;
                vMFingerPrintSetUpRequest.fingerPrintID = (int)fingerPrintUpdateDto.FingerNumber;
                vMFingerPrintSetUpRequest.fingerType = "normalFP";


                foreach (var device in identifiedAssignDevices)
                {
                    var (isSuccess, Message) = await _visionMachineService.SetFingerprint(device.Device, vMFingerPrintSetUpRequest);

                }

                fingerPrintInDb.Data = fingerPrintUpdateDto.Data;
                fingerPrintInDb.IsDeleted = false;
                fingerPrintInDb.ModifiedBy = GetLoggedInUserId();
                fingerPrintInDb.DateModified = DateTime.Now;

                context.FingerPrintRepository.Update(fingerPrintInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/fingerPrint/{key}
        /// </summary>
        /// <param name="key">Primary key of the table FingerPrint.</param>
        /// <param name="fingerPrintUpdateDto">FingerPrint to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteFingerPrint)]

        public async Task<IActionResult> DeleteFingerPrint(FingerPrintDeleteDto fingerPrintDeleteDto)
        {
            try
            {

                var fingerPrintInDb = await context.FingerPrintRepository.GetFingerPrintByKeyAndFingerNo(fingerPrintDeleteDto.Oid, fingerPrintDeleteDto.FingerNumber);

                if (fingerPrintInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);



                Person person = new Person();
                Visitor visitor = new Visitor();


                if (fingerPrintInDb.PersonId.HasValue)
                {
                    person = await context.PersonRepository.GetPersonByKey(fingerPrintInDb.PersonId.Value);
                }
                else if (fingerPrintInDb.VisitorId.HasValue)
                {
                    visitor = await context.VisitorRepository.GetVisitorByKey(fingerPrintInDb.VisitorId.Value);
                }

                EmployeeDetailDto employeeDetailDto = new EmployeeDetailDto();
                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                if (fingerPrintInDb.VisitorId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(fingerPrintInDb.VisitorId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                    employeeDetailDto.EmployeeNo = visitor.VisitorNumber;
                    employeeDetailDto.FingerPrintId = (int)fingerPrintDeleteDto.FingerNumber;
                }
                else if (fingerPrintInDb.PersonId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(fingerPrintInDb.PersonId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                    employeeDetailDto.EmployeeNo = person.PersonNumber;
                    employeeDetailDto.FingerPrintId = (int)fingerPrintDeleteDto.FingerNumber;
                }
                foreach (var item in identifiedAssignDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                 

                foreach (var device in identifiedAssignDevices)
                {
                    var result = await _visionMachineService.DeleteFingerprint(device.Device, employeeDetailDto);
                }
                fingerPrintInDb.IsDeleted = true;
                fingerPrintInDb.ModifiedBy = GetLoggedInUserId();
                fingerPrintInDb.DateModified = DateTime.Now;

                context.FingerPrintRepository.Update(fingerPrintInDb);
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
    }
}
