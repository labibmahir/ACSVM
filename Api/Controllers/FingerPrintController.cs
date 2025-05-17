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
