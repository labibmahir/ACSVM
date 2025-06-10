using Api.NotificationHub;
using Domain.Dto;
using Domain.Dto.HIKVision;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.SignalR;
using SurveillanceDevice.Integration.HIKVision;
using System.Net.NetworkInformation;
using Utilities.Constants;

namespace Api.BackGroundServices
{
    public class AttendanceSyncronizer : BackgroundService
    {
        private readonly ILogger<AttendanceSyncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private static AttendanceNotificationAggrigator _attendanceAggrigator;
        private readonly string? _logDirectory;
        public AttendanceSyncronizer(IConfiguration configuration,
            ILogger<AttendanceSyncronizer> logger,
            IServiceProvider serviceProvider
            , IHubContext<NotificationsHub> hubContext
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logDirectory = configuration["ServiceLogFilePath:FileLogPath"];
            _attendanceAggrigator = new AttendanceNotificationAggrigator(_hubContext);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Device> devices = new List<Device>();
            Dictionary<string, Task> monitoringTasks = new();
            WriteLogToFile("AttenDance Syncronizer BackGround Service Started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var visionMachineService = scope.ServiceProvider.GetRequiredService<IHikVisionMachineService>();
                        var devicesList = await context.DeviceRepository.QueryAsync(x => x.IsDeleted == false);
                        devices = devicesList.ToList();

                        foreach (var device in devices)
                        {
                            if (!await IsDeviceActive(device.DeviceIP))
                            {
                                device.CurrentActiveStatus = false;
                                context.DeviceRepository.Update(device);
                                await context.SaveChangesAsync();
                                continue;
                            }
                            device.CurrentActiveStatus = true;
                            context.DeviceRepository.Update(device);
                            await context.SaveChangesAsync();

                            #region ACSEvent

                            // Processing ACS Event Data
                            DateTime startTime = DateTime.Now.AddHours(-48);
                            DateTime endTime = DateTime.Now;// startTime.AddMinutes(1);

                            while (startTime.TimeOfDay < DateTime.Now.TimeOfDay) // Continue until today
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
                                                        //    recentAttendance = await context.AttendanceRepository.GetAttendanceBetweenDateAndTimeByPersonNumber(oneMinuteAgo, now, getPersonByPersonNumber.PersonNumber);
                                                        recentAttendance = await context.AttendanceRepository.FirstOrDefaultAsync(x => x.PersonId == getPersonByPersonNumber.Oid
                                          && x.AuthenticationDateAndTime == item.EventTime.DateTime && x.AuthenticationDate == item.EventTime.DateTime && x.AuthenticationTime == item.EventTime.TimeOfDay
                                          );

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

                                                            var accessControllEventInDb = await context.AttendanceRepository.FirstOrDefaultAsync(x => x.PersonId == getPersonByPersonNumber.Oid
                                                            && x.AuthenticationDateAndTime == item.EventTime.DateTime && x.AuthenticationDate == item.EventTime.DateTime && x.AuthenticationTime == item.EventTime.TimeOfDay
                                                            );

                                                            if (accessControllEventInDb == null)
                                                            {
                                                                Attendance accessControllEvent = new Attendance()
                                                                {
                                                                    CardNo = item.CardReaderNo.ToString(),
                                                                    // VisitorId = getVisitorByPersonNumber == null ? null : getVisitorByPersonNumber.Oid,
                                                                    PersonId = getPersonByPersonNumber.Oid,
                                                                    AuthenticationDateAndTime = item.EventTime.DateTime,
                                                                    AuthenticationDate = item.EventTime.DateTime,
                                                                    AuthenticationTime = item.EventTime.TimeOfDay,
                                                                    DeviceId = device.Oid,
                                                                    DateCreated = DateTime.Now,
                                                                    IsDeleted = false

                                                                };
                                                                var added = context.AttendanceRepository.Add(accessControllEvent);

                                                                await context.SaveChangesAsync();
                                                            }
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
                                                        var visitorLogInDb = await context.VisitorLogRepository.FirstOrDefaultAsync(x => x.VisitorId == getVisitorByVisitorNumber.Oid
                                                        && x.AuthenticationDateAndTime == item.EventTime.DateTime && x.AuthenticationDate == item.EventTime.DateTime && x.AuthenticationTime == item.EventTime.TimeOfDay
                                                        );

                                                        if (visitorLogInDb == null)
                                                        {
                                                            VisitorLog visitorLog = new VisitorLog()
                                                            {
                                                                CardNo = item.CardReaderNo.ToString(),
                                                                // VisitorId = getVisitorByPersonNumber == null ? null : getVisitorByPersonNumber.Oid,
                                                                VisitorId = getVisitorByVisitorNumber.Oid,
                                                                AuthenticationDateAndTime = item.EventTime.DateTime,
                                                                AuthenticationDate = item.EventTime.DateTime,
                                                                AuthenticationTime = item.EventTime.TimeOfDay,
                                                                DeviceId = device.Oid,
                                                                DateCreated = DateTime.Now,
                                                                IsDeleted = false,


                                                            };

                                                            context.VisitorLogRepository.Add(visitorLog);
                                                            await context.SaveChangesAsync();
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
                                    //startTime = startTime.AddDays(1);
                                    //endTime = startTime.AddDays(1);



                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            #endregion

                        }
                        #region Visitor CardAssignment
                        WriteLogToFile("AttenDance Syncronizer BackGround Service Card Assignment Section");
                        var assignedCards = await context.IdentifiedAssignCardRepository.GetAllInActiveVisitorIdentifiedAssignCards();
                        foreach (var item in assignedCards)
                        {
                            var checkAppointment = await context.AppointmentRepository.GetActiveAppointmentByVisitorAppointmentDateAndTime(item.VisitorId.Value, DateTime.Now.Date, DateTime.Now.TimeOfDay);

                            if (checkAppointment != null)
                            {
                                VMCardInfo vMCardInfo = new VMCardInfo()
                                {
                                    addCard = true,
                                    cardNo = item.Card.CardNumber,
                                    cardType = "normalCard",
                                    employeeNo = item.Visitor.VisitorNumber
                                };
                                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(item.VisitorId.Value);
                                bool atleastIn1Device = false;
                                bool synInAllDevice = true;
                                foreach (var assignDevice in assignedDevices)
                                {
                                    var result = await visionMachineService.AddCard(assignDevice.Device, vMCardInfo);

                                    if (result.Success)
                                    {
                                        atleastIn1Device = true;
                                    }
                                    else
                                    {
                                        synInAllDevice = false;
                                    }
                                }
                                if (atleastIn1Device)
                                {
                                    Card card = item.Card;
                                    card.Status = Utilities.Constants.Enums.Status.Active;
                                    if (synInAllDevice)
                                        card.IsSync = true;
                                    else
                                        card.IsSync = false;

                                    context.CardRepository.Update(card);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        var assignedActiveCards = await context.IdentifiedAssignCardRepository.GetAllActiveVisitorIdentifiedAssignCards();
                        foreach (var item in assignedActiveCards)
                        {
                            var checkAppointment = await context.AppointmentRepository.GetActiveAppointmentByVisitorAppointmentDateAndTime(item.VisitorId.Value, DateTime.Now.Date, DateTime.Now.TimeOfDay);
                            if (checkAppointment == null)
                            {
                                VMCardInfoDeleteRequest vMCardInfoDeleteRequest = new VMCardInfoDeleteRequest()
                                {
                                    CardNoList = new List<VMCardNoListItem?>() { new VMCardNoListItem() { cardNo = item.Card.CardNumber } }
                                };
                                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(item.VisitorId.Value);
                                bool atlestin1Device = false;
                                bool syynInAllDevice = true;
                                foreach (var assignDevice in assignedDevices)
                                {
                                    var result = await visionMachineService.DeleteCard(assignDevice.Device, vMCardInfoDeleteRequest);
                                    if (result.Success)
                                        atlestin1Device = true;
                                    else
                                        syynInAllDevice = false;
                                }
                                if (atlestin1Device)
                                {
                                    Card card = item.Card;
                                    card.Status = Utilities.Constants.Enums.Status.Inactive;
                                    if (syynInAllDevice)
                                        card.IsSync = true;
                                    else
                                        card.IsSync = false;

                                    context.CardRepository.Update(card);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        #endregion

                        #region Vistor Syn To Device
                        WriteLogToFile("AttenDance Syncronizer BackGround Service Vistor Syn To Device Section");
                        try
                        {
                            var deviceSync = await context.DeviceSynchronizerRepository.QueryAsync(x => x.IsDeleted == false && x.IsSync != true && x.VisitorId != null);
                            foreach (var deviceSynchronizer in deviceSync)
                            {
                                var visitor = await context.VisitorRepository.GetVisitorByKey(deviceSynchronizer.VisitorId.Value);
                                if (visitor != null)
                                {
                                    List<VMDoorPermissionSchedule> vMDoorPermissionSchedules = new List<VMDoorPermissionSchedule>();

                                    VMDoorPermissionSchedule vMDoorPermissionSchedule = new VMDoorPermissionSchedule()
                                    {
                                        doorNo = 1,
                                        planTemplateNo = "1",
                                    };

                                    vMDoorPermissionSchedules.Add(vMDoorPermissionSchedule);
                                    VMUserInfo vMUserInfo = new VMUserInfo()
                                    {
                                        employeeNo = visitor.VisitorNumber,
                                        deleteUser = false,
                                        name = visitor.FirstName + " " + visitor.Surname,
                                        userType = "normal",
                                        closeDelayEnabled = true,
                                        Valid = new VMEffectivePeriod()
                                        {
                                            enable = true,
                                            beginTime = visitor.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                                            endTime = visitor.ValidateEndPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                                            timeType = "local"
                                        },
                                        doorRight = "1",
                                        RightPlan = vMDoorPermissionSchedules,
                                        localUIRight = false,
                                        userVerifyMode = visitor.UserVerifyMode switch
                                        {
                                            Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                                            Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                                            Enums.UserVerifyMode.card => "card",
                                            _ => "faceAndFpAndCard"//this is default
                                        },
                                        checkUser = true,
                                        addUser = true,
                                        callNumbers = new List<string> { " 1-1-1-401" },
                                        floorNumbers = new List<FloorNumber> { new FloorNumber() { min = 1, max = 100 } },
                                        gender = visitor.Gender switch
                                        {
                                            Enums.Gender.Male => "male",
                                            Enums.Gender.Female => "female",
                                            _ => "other"
                                        },
                                    };


                                    var identifiedSynDevices = await context.IdentifiedSyncDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.IsSync != true && x.DeviceSynchronizerId == deviceSynchronizer.Oid && x.TryCount <= 50);
                                    bool isSynceAllDevice = true;
                                    foreach (var synDevice in identifiedSynDevices)
                                    {
                                        synDevice.TryCount = synDevice.TryCount + 1;
                                        var device = await context.DeviceRepository.GetDeviceByKey(synDevice.DeviceId);
                                        if (device != null)
                                        {
                                            if (await IsDeviceActive(device.DeviceIP))
                                            {
                                                if (synDevice.Action == Enums.DeviceAction.Add)
                                                {
                                                    var vService = await visionMachineService.AddUser(device, vMUserInfo);
                                                    var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(vService);
                                                    if (res.StatusCode != 1)
                                                    {
                                                        DeviceLog deviceLog = new DeviceLog()
                                                        {
                                                            DeviceId = synDevice.DeviceId,
                                                            DeviceResponse = vService,
                                                            Message = "Failed Upadting Visitor Record",
                                                            Oid = Guid.NewGuid(),
                                                            VisitorId = deviceSynchronizer.VisitorId,
                                                            IsDeleted = false,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Add(synDevice);
                                                        context.DeviceLogRepository.Add(deviceLog);
                                                        isSynceAllDevice = false;
                                                    }
                                                    else
                                                    {
                                                        synDevice.IsSync = true;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
                                                    }
                                                }
                                                else if (synDevice.Action == Enums.DeviceAction.Update)
                                                {
                                                    var vService = await visionMachineService.UpdateUser(device, vMUserInfo);
                                                    var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(vService);
                                                    if (res.StatusCode != 1)
                                                    {
                                                        DeviceLog deviceLog = new DeviceLog()
                                                        {
                                                            DeviceId = synDevice.DeviceId,
                                                            DeviceResponse = vService,
                                                            Message = "Failed Upadting Visitor Record",
                                                            Oid = Guid.NewGuid(),
                                                            VisitorId = deviceSynchronizer.VisitorId,
                                                            IsDeleted = false,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Add(synDevice);
                                                        context.DeviceLogRepository.Add(deviceLog);
                                                        isSynceAllDevice = false;

                                                    }
                                                    else
                                                    {
                                                        synDevice.IsSync = true;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
                                                    }
                                                }
                                                else if (synDevice.Action == Enums.DeviceAction.Delete)
                                                {
                                                    VMUserInfoDetailsDeleteRequest vMCardInfoDeleteRequest = new VMUserInfoDetailsDeleteRequest()
                                                    {
                                                        EmployeeNoList = new List<VMEmployeeNoListItem?>() { new() { employeeNo = visitor.VisitorNumber } }
                                                    };

                                                    var vService = await visionMachineService.DeleteUserWithDetails(device, vMCardInfoDeleteRequest);
                                                    var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(vService);
                                                    if (res.StatusCode != 1)
                                                    {
                                                        DeviceLog deviceLog = new DeviceLog()
                                                        {
                                                            DeviceId = synDevice.DeviceId,
                                                            DeviceResponse = vService,
                                                            Message = "Failed Delete Visitor Record",
                                                            Oid = Guid.NewGuid(),
                                                            VisitorId = deviceSynchronizer.VisitorId,
                                                            IsDeleted = false,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Add(synDevice);
                                                        context.DeviceLogRepository.Add(deviceLog);
                                                        isSynceAllDevice = false;

                                                    }
                                                    else
                                                    {
                                                        synDevice.IsSync = true;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
                                                    }
                                                }
                                                await context.SaveChangesAsync();
                                            }
                                            else
                                            {
                                                DeviceLog deviceLog = new DeviceLog()
                                                {
                                                    DeviceId = synDevice.DeviceId,
                                                    DeviceResponse = "",
                                                    Message = "Device In Active",
                                                    Oid = Guid.NewGuid(),
                                                    VisitorId = deviceSynchronizer.VisitorId,
                                                    IsDeleted = false,
                                                    IsSync = false
                                                };
                                                synDevice.IsSync = false;
                                                isSynceAllDevice = false;
                                                context.DeviceLogRepository.Add(deviceLog);
                                                context.IdentifiedSyncDeviceRepository.Update(synDevice);
                                            }
                                        }
                                        deviceSynchronizer.IsSync = isSynceAllDevice;
                                        context.DeviceSynchronizerRepository.Update(deviceSynchronizer);
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        #endregion

                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    WriteLogToFile($"Exception in Attendance Syncronizer:{ex.Message} ");
                }
            }
        }
        private void WriteLogToFile(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_logDirectory))
                    return;

                string logFileName = $"Log_{DateTime.Now:yyyy-MM-dd}.txt";
                string fullLogPath = Path.Combine(_logDirectory, logFileName);

                // Ensure the directory exists
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
                File.AppendAllText(fullLogPath, logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write log to file");
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
