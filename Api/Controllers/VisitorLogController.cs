using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitorLogController : ControllerBase
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<VisitorLogController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public VisitorLogController(IUnitOfWork context, ILogger<VisitorLogController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// URL:api/visit-history-visitor/{ReadVisitHistoryByVisitorId}
        /// </summary>
        /// <returns>A list of visitorLog.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadVisitHistoryByVisitorId)]
        public async Task<IActionResult> ReadVisitHistoryByVisitorId(Guid visitorId)
        {
            try
            {
                if (visitorId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitHistory = await context.VisitorLogRepository.GetAttendanceByVisitorId(visitorId);
                return Ok(visitHistory);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        [HttpDelete]
        [Route(RouteConstants.DeleteVisitorLog)]
        public async Task<IActionResult> DeleteVisitorLog(VisitorLogDeleteDto visitorLogDeleteDto)
        {
            try
            {

                var visitorLogs = await context.VisitorLogRepository.GetVisitorAttendancesBetweenDates(visitorLogDeleteDto.StartDate, visitorLogDeleteDto.EndDate, visitorLogDeleteDto.VisitorId, visitorLogDeleteDto.DeviceId);

                foreach (var item in visitorLogs)
                {
                    context.VisitorLogRepository.Delete(item);
                }

                await context.SaveChangesAsync();

                return Ok(visitorLogs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


    }
}
