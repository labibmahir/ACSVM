using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;
using ClosedXML.Excel;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceLogController : ControllerBase
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<DeviceLogController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public DeviceLogController(IUnitOfWork context, ILogger<DeviceLogController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// URL:api/device-logs
        /// </summary>
        /// <returns>A list of accessLevel.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadDeviceLogs)]
        public async Task<IActionResult> ReadDeviceLogs([FromQuery] DeviceLogFilterDto deviceLogFilterDto)
        {
            try
            {
                if (deviceLogFilterDto.PageSize == 0)
                {
                    var devicelogs = await context.DeviceLogRepository.GetDeviceLogsWithOutPagintion(deviceLogFilterDto);

                    PagedResultDto<DeviceLog> resultDto = new PagedResultDto<DeviceLog>()
                    {
                        Data = devicelogs.ToList(),
                        PageNumber = 0,
                        PageSize = 0,
                        TotalItems = await context.DeviceLogRepository.GetDeviceLogCount(deviceLogFilterDto)
                    };

                    return Ok(resultDto);
                }
                else
                {
                    int currentPage = deviceLogFilterDto.Page;
                    deviceLogFilterDto.Page = ((deviceLogFilterDto.Page - 1) * (deviceLogFilterDto.PageSize));
                    var devicelogs = await context.DeviceLogRepository.GetDeviceLogsWithOutPagintion(deviceLogFilterDto);

                    PagedResultDto<DeviceLog> resultDto = new PagedResultDto<DeviceLog>()
                    {
                        Data = devicelogs.ToList(),
                        PageNumber = currentPage,
                        PageSize = deviceLogFilterDto.PageSize,
                        TotalItems = await context.DeviceLogRepository.GetDeviceLogCount(deviceLogFilterDto)
                    };
                    resultDto.TotalPages = (int)Math.Ceiling((double)resultDto.TotalItems / resultDto.PageSize);


                    return Ok(resultDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/device-logs/{deviceId}
        /// </summary>
        /// <returns>A list of accessLevel.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadDeviceLogsByDeviceId)]
        public async Task<IActionResult> ReadDeviceLogsByDeviceId(int deviceId, [FromQuery] DeviceLogFilterByDeviceDto deviceLogFilterByDeviceDto)
        {
            try
            {
                if (deviceLogFilterByDeviceDto.PageSize == 0)
                {

                    var devicelogs = await context.DeviceLogRepository.GetDeviceLogsByDeviceId(deviceId, deviceLogFilterByDeviceDto);

                    PagedResultDto<DeviceLog> resultDto = new PagedResultDto<DeviceLog>()
                    {
                        Data = devicelogs.ToList(),
                        PageNumber = 0,
                        PageSize = 0,
                        TotalItems = await context.DeviceLogRepository.GetDeviceLogsByDeviceIdCount(deviceId, deviceLogFilterByDeviceDto)
                    };

                    return Ok(resultDto);
                }
                else
                {
                    int currentPage = deviceLogFilterByDeviceDto.Page;
                    deviceLogFilterByDeviceDto.Page = ((deviceLogFilterByDeviceDto.Page - 1) * (deviceLogFilterByDeviceDto.PageSize));

                    var devicelogs = await context.DeviceLogRepository.GetDeviceLogsByDeviceId(deviceId, deviceLogFilterByDeviceDto);

                    PagedResultDto<DeviceLog> resultDto = new PagedResultDto<DeviceLog>()
                    {
                        Data = devicelogs.ToList(),
                        PageNumber = currentPage,
                        PageSize = deviceLogFilterByDeviceDto.PageSize,
                        TotalItems = await context.DeviceLogRepository.GetDeviceLogsByDeviceIdCount(deviceId, deviceLogFilterByDeviceDto)
                    };

                    return Ok(resultDto);

                }


            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        [HttpDelete]
        [Route(RouteConstants.DeleteDeviceLogs)]
        public async Task<IActionResult> DeleteDeviceLogs(DeviceLogDeleteDto deviceLogDelete)
        {
            try
            {
                var deviceLog = await context.DeviceLogRepository.GetDeviceLogsBetweenDates(deviceLogDelete.StartDate, deviceLogDelete.EndDate, deviceLogDelete.DeviceId);

                foreach (var item in deviceLog)
                {
                    context.DeviceLogRepository.Delete(item);
                }

                await context.SaveChangesAsync();

                return Ok(deviceLog);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

    }
}
