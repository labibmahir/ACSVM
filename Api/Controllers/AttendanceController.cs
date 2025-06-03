using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Contracts;
using Utilities.Constants;
using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class AttendanceController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<AttendanceController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public AttendanceController(IUnitOfWork context, ILogger<AttendanceController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }
        /// <summary>
        /// URL: api/attendance/person-attendances
        /// </summary>
        /// <param name="key">Primary key of the table Attendance.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonAttendances)]

        public async Task<IActionResult> ReadPersonAttendances([FromQuery] PersonAttendanceFilterDto personAttendanceFilterDto)
        {
            try
            {
                if (personAttendanceFilterDto.PageSize == 0)
                {
                    var attendances = await context.AttendanceRepository.GetPersonAttendances();

                    return Ok(attendances);
                }
                else
                {
                    int currentPage = personAttendanceFilterDto.Page;
                    personAttendanceFilterDto.Page = ((personAttendanceFilterDto.Page - 1) * (personAttendanceFilterDto.PageSize));
                    var accessLevels = await context.AttendanceRepository.GetPersonAttendances(personAttendanceFilterDto);

                    PagedResultDto<Attendance> accessLevelsDto = new PagedResultDto<Attendance>()
                    {
                        Data = accessLevels.ToList(),
                        PageNumber = currentPage,
                        PageSize = personAttendanceFilterDto.PageSize,
                        TotalItems = await context.AttendanceRepository.GetPersonAttendancesCount(personAttendanceFilterDto)
                    };
                    accessLevelsDto.TotalPages = (int)Math.Ceiling((double)accessLevelsDto.TotalItems / accessLevelsDto.PageSize);
                    return Ok(accessLevelsDto);


                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/attendance/person-attendances
        /// </summary>
        /// <param name="key">Primary key of the table Attendance.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAttendanceByPersonId)]

        public async Task<IActionResult> ReadAttendanceByPersonId(Guid personId)
        {
            try
            {
                if (personId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var attendances = await context.AttendanceRepository.GetAttendanceByPersonId(personId);
                return Ok(attendances);



            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/attendance/visitor-attendances
        /// </summary>
        /// <param name="key">Primary key of the table Attendance.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadVisitorAttendances)]

        public async Task<IActionResult> ReadVisitorAttendances([FromQuery] VisitorAttendanceFilterDto visitorAttendanceFilterDto)
        {
            try
            {
                if (visitorAttendanceFilterDto.PageSize == 0)
                {
                    var attendances = await context.VisitorLogRepository.GetVisitorAttendances();

                    return Ok(attendances);
                }
                else
                {
                    int currentPage = visitorAttendanceFilterDto.Page;
                    visitorAttendanceFilterDto.Page = ((visitorAttendanceFilterDto.Page - 1) * (visitorAttendanceFilterDto.PageSize));
                    var accessLevels = await context.VisitorLogRepository.GetVisitorAttendances(visitorAttendanceFilterDto);

                    PagedResultDto<VisitorLog> accessLevelsDto = new PagedResultDto<VisitorLog>()
                    {
                        Data = accessLevels.ToList(),
                        PageNumber = currentPage,
                        PageSize = visitorAttendanceFilterDto.PageSize,
                        TotalItems = await context.VisitorLogRepository.GetVisitorAttendancesCount(visitorAttendanceFilterDto)
                    };
                    accessLevelsDto.TotalPages = (int)Math.Ceiling((double)accessLevelsDto.TotalItems / accessLevelsDto.PageSize);
                    return Ok(accessLevelsDto);


                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/attendance/person-attendances
        /// </summary>
        /// <param name="key">Primary key of the table Attendance.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAttendanceByVisitorId)]

        public async Task<IActionResult> ReadAttendanceByVisitorId(Guid visitorId)
        {
            try
            {
                if (visitorId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var attendances = await context.VisitorLogRepository.GetAttendanceByVisitorId(visitorId);
                return Ok(attendances);



            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

    }
}
