using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;
using System.Net.NetworkInformation;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DeviceController : ApiBaseController
    {

        private readonly IUnitOfWork context;
        private readonly ILogger<DeviceController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public DeviceController(IUnitOfWork context, ILogger<DeviceController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// URL: api/device
        /// </summary>
        /// <param name="device">device object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateDevice)]
        public async Task<ActionResult<Device>> CreateDevice(Device device)
        {
            try
            {
                var deviceWithSameDeviceName = await context.DeviceRepository.GetDeviceByDeviceName(device.DeviceName);

                if (deviceWithSameDeviceName != null && deviceWithSameDeviceName.OrganizationId == device.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNameTaken);

                var deviceWithSameIP = await context.DeviceRepository.GetDeviceByIP(device.DeviceIP);

                if (deviceWithSameIP != null && deviceWithSameIP.OrganizationId == device.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateIPError);

                device.CurrentActiveStatus = await IsDeviceActive(device.DeviceIP);

                device.DateCreated = DateTime.Now;
                device.IsDeleted = false;
                device.CreatedBy = GetLoggedInUserId();

                context.DeviceRepository.Add(device);
                await context.SaveChangesAsync();

                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/device/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadDeviceByKey)]
        [Authorize]
        public async Task<IActionResult> ReadDeviceByKey(int key)
        {
            try
            {
                if (key <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var device = await context.DeviceRepository.GetDeviceByKey(key);

                if (device == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                //  device.DeviceCurrectActiveStatus = await IsDeviceActive(device.DeviceIP);

                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/devices
        /// </summary>
        /// <returns>A list of devices.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadDevices)]
        public async Task<IActionResult> ReadDevices([FromQuery] DeviceFilterDto deviceFilterDto)
        {
            try
            {
                if (deviceFilterDto.PageSize == 0)
                {
                    var devices = await context.DeviceRepository.GetDevices();//add accessLevelName

                    //foreach (var device in devices)
                    //    device.DeviceCurrectActiveStatus = await IsDeviceActive(device.DeviceIP);

                    return Ok(devices);
                }
                else
                {
                    int currentPage = deviceFilterDto.Page;
                    deviceFilterDto.Page = ((deviceFilterDto.Page - 1) * (deviceFilterDto.PageSize));
                    var devices = await context.DeviceRepository.GetDevices(deviceFilterDto);//add accessLevelName

                    //foreach (var device in devices)
                    //    device.DeviceCurrectActiveStatus = await IsDeviceActive(device.DeviceIP);

                    PagedResultDto<DeviceReadDto> devicesDto = new PagedResultDto<DeviceReadDto>()
                    {
                        Data = devices.ToList(),
                        PageNumber = currentPage,
                        PageSize = deviceFilterDto.PageSize,
                        TotalItems = await context.DeviceRepository.GetDeviceCount(deviceFilterDto)
                    };
                    devicesDto.TotalPages = (int)Math.Ceiling((double)devicesDto.TotalItems / devicesDto.PageSize);
                    return Ok(devicesDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/device/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <param name="device">Device to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateDevice)]
        //[AllowAnonymous]
        public async Task<IActionResult> UpdateDevice(int key, Device device)
        {
            try
            {
                if (key != device.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var deviceWithSameDeviceName = await context.DeviceRepository.GetDeviceByDeviceName(device.DeviceName);

                if (deviceWithSameDeviceName != null && deviceWithSameDeviceName.OrganizationId == device.OrganizationId && deviceWithSameDeviceName.Oid != device.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNameTaken);

                var deviceWithSameIP = await context.DeviceRepository.GetDeviceByIP(device.DeviceIP);

                if (deviceWithSameIP != null && deviceWithSameIP.OrganizationId == device.OrganizationId && deviceWithSameIP.Oid != device.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateCellphoneError);

                var deviceInDb = await context.DeviceRepository.GetDeviceByKey(device.Oid);

                deviceInDb.DeviceIP = device.DeviceIP;
                deviceInDb.DeviceName = device.DeviceName;
                deviceInDb.SerialNumber = device.SerialNumber;
                deviceInDb.DeviceLicence = device.DeviceLicence;
                deviceInDb.FirmwareReleasedDate = device.FirmwareReleasedDate;
                deviceInDb.FirmwareVersion = device.FirmwareVersion;
                deviceInDb.IsActive = device.IsActive;
                deviceInDb.MacAddress = device.MacAddress;
                deviceInDb.ModelName = device.ModelName;
                deviceInDb.Port = device.Port;
                deviceInDb.Username = device.Username;
                deviceInDb.Password = device.Password;
                deviceInDb.AccessLevelId = device.AccessLevelId;
                deviceInDb.IsDeleted = false;
                deviceInDb.OrganizationId = device.OrganizationId;
                deviceInDb.DateModified = DateTime.Now;
                deviceInDb.ModifiedBy = GetLoggedInUserId();

                context.DeviceRepository.Update(deviceInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/device/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <param name="device">Device to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.ActivateDevice)]
        //[AllowAnonymous]
        public async Task<IActionResult> ActivateDevice(int key, ActivateDeviceDto activateDeviceDto)
        {
            try
            {
                if (key != activateDeviceDto.DeviceId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var deviceInDb = await context.DeviceRepository.GetDeviceByKey(activateDeviceDto.DeviceId);

                if (deviceInDb == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                deviceInDb.IsActive = true;
                deviceInDb.IsDeleted = false;
                deviceInDb.DateModified = DateTime.Now;
                deviceInDb.ModifiedBy = GetLoggedInUserId();

                context.DeviceRepository.Update(deviceInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/device/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <param name="device">Device to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.DeActivateDevice)]
        //[AllowAnonymous]
        public async Task<IActionResult> DeActivateDevice(int key, ActivateDeviceDto activateDeviceDto)
        {
            try
            {
                if (key != activateDeviceDto.DeviceId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var deviceInDb = await context.DeviceRepository.GetDeviceByKey(activateDeviceDto.DeviceId);

                if (deviceInDb == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                deviceInDb.IsActive = false;
                deviceInDb.IsDeleted = false;
                deviceInDb.DateModified = DateTime.Now;
                deviceInDb.ModifiedBy = GetLoggedInUserId();

                context.DeviceRepository.Update(deviceInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/Device/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteDevice)]
        public async Task<IActionResult> DeleteDevice(int key)
        {
            try
            {
                if (key <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var devicetInDb = await context.DeviceRepository.GetDeviceByKey(key);

                if (devicetInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                //var checkIfDeviceIsAssigned = await context.IdentifiedAssignDeviceRepository.FirstOrDefaultAsync(x => x.IsDeleted == false && x.DeviceId == key);

                //if (checkIfDeviceIsAssigned == null)
                //    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceCannotBeDeleted);

                devicetInDb.DateModified = DateTime.Now;
                devicetInDb.IsDeleted = true;
                devicetInDb.IsActive = false;
                devicetInDb.ModifiedBy = GetLoggedInUserId();

                context.DeviceRepository.Update(devicetInDb);
                await context.SaveChangesAsync();

                return Ok(devicetInDb);
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
