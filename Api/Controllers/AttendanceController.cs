using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Contracts;
using Utilities.Constants;
using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClosedXML.Excel;
using Infrastructure;

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
        [HttpDelete]
        [Route(RouteConstants.DeletePersonAttendance)]
        public async Task<IActionResult> DeletePersonAttendance(AttendanceDeleteDto attendanceDeleteDto)
        {
            try
            {
                var personAttendances = await context.AttendanceRepository.GetPersonAttendancesBetweenDates(attendanceDeleteDto.FromDate, attendanceDeleteDto.ToDate);

                if (personAttendances == null || personAttendances.Count() == 0)
                    return StatusCode(StatusCodes.Status404NotFound);

                var attendances = personAttendances.ToList();


                // Generate Excel in memory
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Deleted Attendance");
                worksheet.Cell(1, 1).Value = "Device Ip";
                worksheet.Cell(1, 2).Value = "Person Number";
                worksheet.Cell(1, 3).Value = "Full Name";
                worksheet.Cell(1, 4).Value = "Authentication Date And Time";

                for (int i = 0; i < attendances.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = attendances[i].Device.DeviceIP.ToString();
                    worksheet.Cell(i + 2, 2).Value = attendances[i].Person.PersonNumber;
                    worksheet.Cell(i + 2, 3).Value = attendances[i].Person.FirstName + " " + attendances[i].Person.Surname;
                    worksheet.Cell(i + 2, 4).Value = attendances[i].AuthenticationDate?.ToString("yyyy-MM-dd HH:mm:ss");
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                // Delete records
                foreach (var item in attendances)
                {
                    context.AttendanceRepository.Delete(item);
                }
                await context.SaveChangesAsync();

                var fileName = $"DeletedAttendance_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        [HttpDelete]
        [Route(RouteConstants.DeleteVisitorAttendance)]
        public async Task<IActionResult> DeleteVisitorAttendance(AttendanceDeleteDto attendanceDeleteDto)
        {
            try
            {
                var visitorAttendances = await context.VisitorLogRepository.GetVisitorAttendancesBetweenDates(attendanceDeleteDto.FromDate, attendanceDeleteDto.ToDate);
          
                if (visitorAttendances == null || visitorAttendances.Count() == 0)
                    return StatusCode(StatusCodes.Status404NotFound);
                
                var attendances = visitorAttendances.ToList();

           

                // Generate Excel in memory
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Deleted Attendance");
                worksheet.Cell(1, 1).Value = "Device Ip";
                worksheet.Cell(1, 2).Value = "Visitor Number";
                worksheet.Cell(1, 3).Value = "Full Name";
                worksheet.Cell(1, 4).Value = "Authentication Date And Time";

                for (int i = 0; i < attendances.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = attendances[i].Device.DeviceIP.ToString();
                    worksheet.Cell(i + 2, 2).Value = attendances[i].Visitor.VisitorNumber;
                    worksheet.Cell(i + 2, 3).Value = attendances[i].Visitor.FirstName + " " + attendances[i].Visitor.Surname;
                    worksheet.Cell(i + 2, 4).Value = attendances[i].AuthenticationDate?.ToString("yyyy-MM-dd HH:mm:ss");
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                // Delete records
                foreach (var item in attendances)
                {
                    context.VisitorLogRepository.Delete(item);
                }
                await context.SaveChangesAsync();

                var fileName = $"DeletedAttendance_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

    }
}
