using Api.NotificationHub;
using Domain.Dto.HIKVision;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.SignalR;
using SurveillanceDevice.Integration.HIKVision;
using System.Net.NetworkInformation;

namespace Api.BackGroundServices
{
    public class AttendanceSyncronizer : BackgroundService
    {
        private readonly ILogger<AttendanceSyncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private static AttendanceNotificationAggrigator _attendanceAggrigator;
        public AttendanceSyncronizer(
            ILogger<AttendanceSyncronizer> logger,
            IServiceProvider serviceProvider
            , IHubContext<NotificationsHub> hubContext
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _attendanceAggrigator = new AttendanceNotificationAggrigator(_hubContext);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Device> devices = new List<Device>();
            Dictionary<string, Task> monitoringTasks = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var visionMachineService = scope.ServiceProvider.GetRequiredService<IHikVisionMachineService>();
                    var devicesList = await context.DeviceRepository.QueryAsync(x => x.IsDeleted == false);
                    devices = devicesList.ToList();
                    #region ACSEvent
                    foreach (var device in devices)
                    {
                        if (!await IsDeviceActive(device.DeviceIP))
                        {
                            continue;
                        }
                        // Processing ACS Event Data
                        DateTime startTime = DateTime.Now.AddMinutes(-1);
                        DateTime endTime = startTime.AddMinutes(1);

                        while (startTime < DateTime.Now) // Continue until today
                        {
                            try
                            {
                                //  WriteLogToFile($"{device.IP}");
                                int acsEventCount = await visionMachineService.GetAcsEventCount(device, startTime, endTime);
                                var acsEventInfo = new List<Info>();

                                int acsEventLastIndex = 0;

                                while (acsEventInfo.Count < acsEventCount)
                                {
                                    try
                                    {
                                        var result = await visionMachineService.GetAcsEvent(device, startTime, endTime, acsEventLastIndex, 20);

                                        if (result?.AcsEvent != null && result.AcsEvent.InfoList.Any())
                                        {
                                            acsEventInfo.AddRange(result.AcsEvent.InfoList);
                                            acsEventLastIndex += result.AcsEvent.InfoList.Count;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error fetching ACS events for device {DeviceId} on {StartTime}",
                                            device.Oid, startTime);
                                    }
                                }

                                try
                                {
                                    int authDateTimeFormat = 0;
                                    int authDateFormat = 0;
                                    int authTimeFormat = 0;
                                    foreach (var item in acsEventInfo)
                                    {
                                        //  WriteLogToFile($"check employee No Null");
                                        if (!string.IsNullOrEmpty(item.EmployeeNo))
                                        {

                                            try
                                            {
                                                var oneMinuteAgo = DateTime.Now.AddMinutes(-1);
                                                var now = DateTime.Now;

                                                Person? getPersonByPersonNumber = new Person();
                                                Visitor? getVisitorByVisitorNumber = new Visitor();
                                                getPersonByPersonNumber = await context.PersonRepository.GetPersonByPersonNumber(item.EmployeeNo);
                                                if (getPersonByPersonNumber == null)
                                                {
                                                    getVisitorByVisitorNumber = await context.VisitorRepository.GetVisitorByVisitorNumber(item.EmployeeNo);
                                                }
                                                Attendance recentAttendance = null; ;
                                                if (getPersonByPersonNumber != null)
                                                {
                                                    recentAttendance = await context.AttendanceRepository.GetAttendanceBetweenDateAndTimeByPersonNumber(oneMinuteAgo, now, getPersonByPersonNumber.PersonNumber);
                                                    //else if (getVisitorByPersonNumber != null)
                                                    //            recentAttendance = await context.AttendanceRepository.GetAttendanceBetweenDateAndTimeByVisitorNumber(oneMinuteAgo, now, getVisitorByPersonNumber.VisitorNumber);

                                                    if (recentAttendance == null)
                                                    {
                                                        AlarmInfo alarmInfo = new AlarmInfo()
                                                        {
                                                            dateTime = item.EventTime.ToString(),
                                                            eventType = "AccessControllEvent",
                                                            ipAddress = device.DeviceIP,
                                                            eventDescription = "Access Controll Event",
                                                            eventState = "",
                                                            ipv6Address = "",
                                                            activePostCount = 0,
                                                            channelID = "",
                                                            AccessControllerEvent = new AccessControllerEvent()
                                                            {
                                                                employeeNoString = item.EmployeeNo,
                                                                name = "",
                                                                deviceName = "",
                                                                userType = item.UserType,
                                                                serialNo = 0,
                                                                cardType = int.TryParse(item.CardType.ToString(), out int result) ? result : 0,
                                                                accessChannel = 0
                                                            }
                                                        };
                                                        alarmInfo.Device = device;
                                                        await _attendanceAggrigator.SendAccessControllEventNotification(alarmInfo);

                                                        Attendance accessControllEvent = new Attendance()
                                                        {
                                                            CardNo = item.CardReaderNo.ToString(),
                                                            // VisitorId = getVisitorByPersonNumber == null ? null : getVisitorByPersonNumber.Oid,
                                                            PersonId = getPersonByPersonNumber.Oid,
                                                            AuthenticationDateAndTime = item.EventTime.DateTime,
                                                            AuthenticationDate = item.EventTime.DateTime,
                                                            AuthenticationTime = item.EventTime.TimeOfDay,
                                                            DeviceId = device.Oid,
                                                            DateCreated = DateTime.Now

                                                        };
                                                        var added = context.AttendanceRepository.Add(accessControllEvent);

                                                        await context.SaveChangesAsync();
                                                    }
                                                }
                                                else if (getVisitorByVisitorNumber != null)
                                                {
                                                    var checkAppointment = await context.AppointmentRepository.GetActiveAppointmentByVisitorAppointmentDateAndTime(getVisitorByVisitorNumber.Oid, item.EventTime.DateTime, item.EventTime.TimeOfDay);
                                                    if (checkAppointment != null)
                                                    {
                                                        var assignAppointment = await context.IdentifiedAssignedAppointmentRepository.GetIdentifiedAssignedAppointmentByAppointment(checkAppointment.Oid);
                                                        await _attendanceAggrigator.SendAppointmentNotification(getVisitorByVisitorNumber, assignAppointment.Select(x => x.PersonId).ToList());

                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                            }

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // WriteLogToFile($"Exception  : {ex.Message}");
                                }
                                // Move to the next day
                                startTime = startTime.AddDays(1);
                                endTime = startTime.AddDays(1);

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
                #endregion




                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

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
