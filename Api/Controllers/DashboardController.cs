using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;
using DocumentFormat.OpenXml.InkML;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DashboardController : ApiBaseController
    {

        private readonly IUnitOfWork context;
        private readonly ILogger<DashboardController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public DashboardController(IUnitOfWork context, ILogger<DashboardController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// URL:api/dashboard-data
        /// </summary>
        /// <returns>A list of dashboard.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadDashboardData)]
        public async Task<IActionResult> ReadDashboardData([FromQuery] DashboardFilterDto dashboardFilterDto)
        {
            try
            {
                var totalEmployee = await context.PersonRepository.CountAsync(x => x.IsDeleted == false);

                var totalDevices = await context.DeviceRepository.CountAsync(x => x.IsDeleted == false);
                var totalActiveDevices = await context.DeviceRepository.CountAsync(x => x.IsDeleted == false && x.IsActive == true);
                var totalInActiveDevices = await context.DeviceRepository.CountAsync(x => x.IsDeleted == false && x.IsActive == false);
                var totalOnlineDevices = await context.DeviceRepository.CountAsync(x => x.IsDeleted == false && x.IsActive == true && x.CurrentActiveStatus == true);
                var totalOfflineDevices = await context.DeviceRepository.CountAsync(x => x.IsDeleted == false && x.IsActive == true && x.CurrentActiveStatus == false);
                
                var totalAppointment = 0;
                var activeAppointment = 0;
                var cancelledAppointment = 0;

                if (dashboardFilterDto.appointmentDate.HasValue && dashboardFilterDto.appointmentDate != DateTime.MinValue)
                {
                    var todaysTotalAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == false
                    && x.AppointmentDate.Date == dashboardFilterDto.appointmentDate.Value);

                    var todayActiveAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == true
                    && x.AppointmentDate.Date == dashboardFilterDto.appointmentDate.Value);

                    var todayCancelledAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == true
                    && x.AppointmentDate.Date == dashboardFilterDto.appointmentDate.Value);

                }
                else
                {
                    var todaysTotalAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == false
                    && x.AppointmentDate.Date == DateTime.Now.Date);

                    var todayActiveAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == false && x.IsCompleted == true
                    && x.AppointmentDate.Date == DateTime.Now.Date);

                    var todayCancelledAppointment = await context.AppointmentRepository.CountAsync(x => x.IsDeleted == false && x.IsCancelled == true
                    && x.AppointmentDate.Date == DateTime.Now.Date);


                }

                DashboardDto dashboardDto = new DashboardDto()
                {
                    ActiveAppointment = activeAppointment,
                    TotalAppointment=totalAppointment,
                    CancelledAppointment=cancelledAppointment,
                    TotalDevices=totalDevices,
                    TotalActiveDevices=totalActiveDevices,
                    TotalInActiveDevices=totalInActiveDevices,
                    TotalOnlineDevices=totalOnlineDevices,
                    TotalOfflineDevices=totalOfflineDevices,
                    TotalEmployee=totalEmployee,
                    
                    

                };

                return Ok(dashboardDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
    }
}
