using Api.NotificationHub;
using Dapper;
using Domain.Dto.HIKVision;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Contracts;
using Infrastructure.Helper;
using Microsoft.AspNetCore.SignalR;
using SurveillanceDevice.Integration.HIKVision;
using System.Data;
using System.Net.NetworkInformation;
using static Utilities.Constants.Enums;



namespace Api.BackGroundServices
{
    public class DataExchangeSyncronizer : BackgroundService
    {
        private readonly ILogger<AttendanceSyncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        //   private readonly IHubContext<NotificationsHub> _hubContext;
        private static AttendanceNotificationAggrigator _attendanceAggrigator;
        public DataExchangeSyncronizer(
            ILogger<AttendanceSyncronizer> logger,
            IServiceProvider serviceProvider
            // , IHubContext<NotificationsHub> hubContext
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            // _hubContext = hubContext;
            ///_attendanceAggrigator = new AttendanceNotificationAggrigator(_hubContext);

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
                    var _dbContextFactory = scope.ServiceProvider.GetRequiredService<DynamicDbContextFactory>();
                    var devicesList = await context.DeviceRepository.QueryAsync(x => x.IsDeleted == false);
                    devices = devicesList.ToList();
                    #region ACSEvent
                    foreach (var device in devices)
                    {
                        if (!await IsDeviceActive(device.DeviceIP))
                        {
                            continue;
                        }
                        #region ACSEvent
                        // Processing ACS Event Data
                        DateTime startTime = device.DateCreated ?? DateTime.Now; // Start from Jan 1, 2022
                                                                                 //   DateTime startTime = DateTime.Now.AddDays(-2); // Start from Jan 1, 2022
                                                                                 //DateTime startTime = DateTime.Now;
                                                                                 // DateTime endTime = startTime.AddDays(1); // Process 1 day at a time
                        DateTime endTime = startTime.AddDays(1); // Process 1 day at a time

                        while (startTime < DateTime.Now) // Continue until today
                        {
                            try
                            {
                                //_logger.LogInformation("Fetching ACS events from {StartTime} to {EndTime} for Device {DeviceId}",
                                //    startTime, endTime, device.DeviceID);
                                //WriteLogToFile($"{device.IP}");
                                int acsEventCount = await visionMachineService.GetAcsEventCount(device, startTime, endTime);
                                var acsEventInfo = new List<Info>();

                                int acsEventLastIndex = 0;
                                List<Task<AcsEventResponse>> acsEventTasks = new List<Task<AcsEventResponse>>();
                                // WriteLogToFile($"{acsEventInfo.Count}");
                                while (acsEventInfo.Count < acsEventCount)
                                {
                                    try
                                    {
                                        var task = visionMachineService.GetAcsEvent(device, startTime, endTime, acsEventLastIndex, 20);
                                        acsEventTasks.Add(task);

                                        if (acsEventTasks.Count >= 1) // Process in batches
                                        {
                                            var completedTasks = await Task.WhenAll(acsEventTasks);
                                            acsEventTasks.Clear();

                                            foreach (var result in completedTasks)
                                            {
                                                if (result?.AcsEvent != null && result.AcsEvent.InfoList.Any())
                                                {
                                                    acsEventInfo.AddRange(result.AcsEvent.InfoList);
                                                    acsEventLastIndex += result.AcsEvent.InfoList.Count;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //_logger.LogError(ex, "Error fetching ACS events for device {DeviceId} on {StartTime}",
                                        //    device.DeviceID, startTime);
                                    }
                                }

                                try
                                {
                                    int authDateTimeFormat = 0;
                                    int authDateFormat = 0;
                                    int authTimeFormat = 0;
                                    //  WriteLogToFile($"dbCOnection");
                                    IDbConnection? dbConnection = null;
                                    var config = await context.ClientDBDetailRepository.FirstOrDefaultAsync(c => c.IsConnectionActive == true);
                                    dbConnection = await _dbContextFactory.CreateDbContextAsync(config == null ? "Default" : config.DatabaseType.ToString());
                                    dbConnection.Open();
                                    //  WriteLogToFile($"config");
                                    var (insertQuery, dbTypes) = await GenerateInsertQuery(config, _dbContextFactory);
                                    foreach (var item in acsEventInfo)
                                    {
                                        //  WriteLogToFile($"check employee No Null");
                                        if (!string.IsNullOrEmpty(item.EmployeeNo))
                                        {
                                            if (string.IsNullOrEmpty(insertQuery))
                                            {
                                                var personByEmployeeNo = await context.PersonRepository.FirstOrDefaultAsync(x => x.PersonNumber == item.EmployeeNo);
                                                var tabAttendance = await context.AttendanceRepository.FirstOrDefaultAsync(i =>
                                            i.PersonId == personByEmployeeNo.Oid && i.AuthenticationDateAndTime == item.EventTime.DateTime);

                                                if (tabAttendance == null)
                                                {

                                                    Attendance accessControllEvent = new Attendance()
                                                    {
                                                        CardNo = item.CardReaderNo.ToString(),
                                                        PersonId = personByEmployeeNo.Oid,
                                                        AuthenticationDateAndTime = item.EventTime.DateTime,
                                                        AuthenticationDate = item.EventTime.DateTime,
                                                        AuthenticationTime = item.EventTime.TimeOfDay,
                                                        Direction = null,
                                                        DeviceName = device.DeviceName,
                                                        DeviceSerialNo = device.DeviceIP,
                                                    };
                                                    var added = context.AttendanceRepository.Add(accessControllEvent);

                                                    await context.SaveChangesAsync();
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    // WriteLogToFile("Inserting in to Client DB");

                                                    authDateTimeFormat = dbTypes.ContainsKey("AuthenticationDateTime") && dbTypes["AuthenticationDateTime"] != 0
                                                        ? dbTypes["AuthenticationDateTime"]
                                                        : 0;

                                                    authDateFormat = dbTypes.ContainsKey("AuthenticationDate") && dbTypes["AuthenticationDate"] != 0
                                                        ? dbTypes["AuthenticationDate"]
                                                        : 0;

                                                    authTimeFormat = dbTypes.ContainsKey("AuthenticationTime") && dbTypes["AuthenticationTime"] != 0
                                                        ? dbTypes["AuthenticationTime"]
                                                    : 0;

                                                    var person = context.PersonRepository.GetAll().FirstOrDefault(x => x.PersonNumber == item.EmployeeNo && x.IsDeleted == false);

                                                    // var deviceFromDb = unitOfWork.AccessControllDeviceRepository.FirstOrDefault(x => x.IP == dto.ipAddress && x.IsRowDeleted == false);

                                                    var rowsAffected = await dbConnection.ExecuteAsync(insertQuery, new
                                                    {
                                                        EmployeeId = item.EmployeeNo,
                                                        AuthenticationDateTime = item.EventTime.UtcDateTime, //DateTimeOffset.Parse(dto.EventTime).ToOffset(TimeSpan.FromHours(6)).DateTime,
                                                                                                             // AuthenticationDateTime = DBMappingDateTimeConverter.ConvertDateTimeFormat(DateTimeOffset.Parse(dto.dateTime).ToOffset(TimeSpan.FromHours(6)).DateTime, authDateTimeFormat, config == null ? DatabaseType.SQLServer : config.DatabaseType),
                                                        AuthenticationDate = item.EventTime.UtcDateTime.Date,
                                                        //AuthenticationDate = Convert.ToDateTime(dto.dateTime).Date, // DBMappingDateTimeConverter.ConvertDateFormat(Convert.ToDateTime(dto.dateTime), authDateFormat, config == null ? DatabaseType.SQLServer : config.DatabaseType),
                                                        AuthenticationTime = item.EventTime.UtcDateTime.TimeOfDay, //DateTimeOffset.Parse(dto.dateTime).ToOffset(TimeSpan.FromHours(6)).TimeOfDay,
                                                                                                                   // AuthenticationTime = DBMappingDateTimeConverter.ConvertTimeFormat(DateTimeOffset.Parse(dto.dateTime).ToOffset(TimeSpan.FromHours(6)).DateTime, authDateFormat, config == null ? DatabaseType.SQLServer : config.DatabaseType),
                                                        DeviceName = device.DeviceName,// dto?.Name ?? dto.AccessControllerEvent.deviceName,
                                                        DeviceSerialNo = device.SerialNumber,
                                                        PersonName = person == null ? "No Person Found" : (person.FirstName + ' ' + person.Surname),
                                                        CardNo = item.CardNo,// dto.AccessControllerEvent.GetFormattedCardNo(),
                                                        DepartmentName = "No Department Found",
                                                        FirstName = person?.FirstName ?? "No First Name Found",
                                                        LastName = person?.Surname ?? "No Last Name Found"
                                                    });

                                                }
                                                catch (Exception ex)
                                                {
                                                    //                                                  WriteLogToFile($"Exception  : {ex.Message}");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //                                    WriteLogToFile($"Exception  : {ex.Message}");
                                }
                                // Move to the next day
                                startTime = startTime.AddDays(1);
                                endTime = startTime.AddDays(1);
                            }
                            catch (Exception ex)
                            {
                                //WriteLogToFile($"Exception  : {ex.Message}");
                                //_logger.LogError(ex, "Error processing ACS events for Device {DeviceId} from {StartTime} to {EndTime}",
                                //    device.DeviceID, startTime, endTime);
                            }
                        }
                        #endregion


                    }
                }
                #endregion




                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

            }


        }


        private async Task<(string?, Dictionary<string, int> dbTypes)> GenerateInsertQuery(ClientDBDetail clientDBDetail, DynamicDbContextFactory dynamicDbContextFactory)
        {
            if (clientDBDetail == null)
            {
                return (null, new Dictionary<string, int>());
            }
            //   WriteLogToFile("Calling GetClientFieldMappings");
            var fieldMappings = await GetClientFieldMappings(dynamicDbContextFactory, clientDBDetail.Oid);

            if (fieldMappings == null || !fieldMappings.Any())
            {
                return (null, new Dictionary<string, int>());
            }

            // Filter out mappings where ClientField is null or empty
            var validMappings = fieldMappings.Values.Where(f => !string.IsNullOrEmpty(f.ClientField)).ToList();

            if (!validMappings.Any())
            {
                return (null, new Dictionary<string, int>());
            }

            var tableName = validMappings.First().TableName ?? "TabEmployeeAttendances";

            var columnNames = string.Join(",", validMappings.Select(f => f.ClientField));

            // Prepare parameters based on database type
            string parameters = clientDBDetail.DatabaseType switch
            {
                DatabaseType.MySQL => string.Join(",", validMappings.Select(f => "?" + f.StandardField)),
                DatabaseType.Oracle => string.Join(",", validMappings.Select(f => ":" + f.StandardField)),
                _ => string.Join(",", validMappings.Select(f => "@" + f.StandardField))
            };


            var dbTypes = validMappings.Where(f => f.StandardField != null && (f.StandardField.Contains("DateTime") ||
                                                                                    f.StandardField.Contains("Date") ||
                                                                                        f.StandardField.Contains("Time")))
                                                                    .ToDictionary(f => f.StandardField!, f => f.FormatType ?? 1);

            // Ensure required keys exist with a default value of 1
            foreach (var key in new[] { "AuthenticationDateTime", "AuthenticationDate", "AuthenticationTime" })
            {
                dbTypes.TryAdd(key, 1);
            }

            var insertQuery = clientDBDetail.DatabaseType switch
            {
                DatabaseType.SQLServer => $@"INSERT INTO {tableName} ({columnNames}) 
                             VALUES ({parameters})",

                DatabaseType.CustomSQLServer => $@"INSERT INTO {tableName} 
                                 (DeviceNo, CardNo, EmpId, dtPunchDate, dtPunchTime, pTime, IsNew, dtCreate) 
                                 VALUES (@DeviceSerialNo, @CardNo, @EmployeeIdInt, @AuthenticationDateAndTime,
                                 @AuthenticationDateAndTime, CONVERT(TIME(7), @AuthenticationDateAndTime, 126), 1, GETDATE())",

                DatabaseType.PostgreSQL => $@"INSERT INTO {tableName} ({columnNames}) 
                                 VALUES ({parameters})",

                DatabaseType.Oracle => $@"INSERT INTO {tableName} ({columnNames}) 
                                 VALUES ({parameters})",

                DatabaseType.MySQL => $@"INSERT INTO {tableName} ({columnNames}) 
                                 VALUES ({parameters})",

                _ => throw new Exception("Unsupported database type")
            };

            return (insertQuery, dbTypes);
        }
        private async Task<Dictionary<string, ClientFieldMapping>> GetClientFieldMappings(DynamicDbContextFactory _dbContextFactory, int clientDBDetailId = 0)
        {
            IDbConnection? dbConnection = null;

            // var config = await unitOfWork.ClientDBDetailRepository.GetAll().Where(c => c.IsConnectionActive == true).FirstOrDefaultAsync();
            using (var scope = _serviceProvider.CreateScope())
            {
                // WriteLogToFile("Service Started");
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var s = unitOfWork.ClientFieldMappingRepository.GetAll().Where(x => x.ClientDBDetailId == clientDBDetailId);

                // dbConnection = await _dbContextFactory.CreateDbContextAsync("Default");

                string query = @"SELECT * FROM ClientFieldMappings 
                      WHERE ClientDBDetailId = @ClientDBDetailId";

                try
                {
                    var mappings = unitOfWork.ClientFieldMappingRepository.GetAll().Where(x => x.ClientDBDetailId == clientDBDetailId);

                    return mappings.ToDictionary(m => m.StandardField, m => m);

                }
                catch (Exception ex)
                {
                    //  WriteLogToFile($"Exception  : {ex.Message}");
                    throw ex;
                }
                finally
                {
                    if (dbConnection != null && dbConnection.State == ConnectionState.Open)
                    {
                        dbConnection.Close();
                    }
                }
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
