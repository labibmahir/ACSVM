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
        private readonly ILogger<DataExchangeSyncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private static AttendanceNotificationAggrigator _attendanceAggrigator;
        private readonly string? _logDirectory;
        public DataExchangeSyncronizer(IConfiguration configuration,
            ILogger<DataExchangeSyncronizer> logger,
            IServiceProvider serviceProvider
             , IHubContext<NotificationsHub> hubContext
            )
        {
            _logger = logger;
            _logDirectory = configuration["ServiceLogFilePath:FileLogPath"];
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _attendanceAggrigator = new AttendanceNotificationAggrigator(_hubContext);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            WriteLogToFile($"DataExchange BackGround Service Started");
            List<Device> devices = new List<Device>();
            Dictionary<string, Task> monitoringTasks = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
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
                            // DateTime startTime = device.DateCreated ?? DateTime.Now; // Start from Jan 1, 2022
                            DateTime startTime = DateTime.Now.AddHours(-48); // Start from Jan 1, 2022
                                                                             //   DateTime startTime = DateTime.Now.AddDays(-2); // Start from Jan 1, 2022
                                                                             //DateTime startTime = DateTime.Now;
                                                                             // DateTime endTime = startTime.AddDays(1); // Process 1 day at a time
                            DateTime endTime = DateTime.Now; // Process 1 day at a time

                            while (startTime < DateTime.Now) // Continue until today
                            {
                                try
                                {
                                    //_logger.LogInformation("Fetching ACS events from {StartTime} to {EndTime} for Device {DeviceId}",
                                    //    startTime, endTime, device.DeviceID);
                                    WriteLogToFile($"{device.DeviceIP}");
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
                                        string insertQuery = "";
                                        Dictionary<string, int> dbTypes = null;
                                        if (config != null && config.UseClientDb)
                                        {
                                            dbConnection = await _dbContextFactory.CreateDbContextAsync(config == null ? "Default" : config.DatabaseType.ToString());
                                            dbConnection.Open();
                                            //  WriteLogToFile($"config");
                                            var (query, databaseTypes) = await GenerateInsertQuery(config, _dbContextFactory);
                                            insertQuery = query;
                                            dbTypes = databaseTypes;
                                        }
                                        foreach (var item in acsEventInfo)
                                        {
                                            //  WriteLogToFile($"check employee No Null");
                                            if (!string.IsNullOrEmpty(item.EmployeeNo))
                                            {
                                                var getVisitorByVisitorNumber = await context.VisitorRepository.GetVisitorByVisitorNumber(item.EmployeeNo);
                                                if (getVisitorByVisitorNumber == null)
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
                                                            string duplicateCheckQuery = await GenerateSelectDuplicateQuery(config, _dbContextFactory);
                                                            // var deviceFromDb = unitOfWork.AccessControllDeviceRepository.FirstOrDefault(x => x.IP == dto.ipAddress && x.IsRowDeleted == false);
                                                            int checkRecord = 0;
                                                            var paramValues = new DynamicParameters();
                                                            foreach (var mapping in await GetClientFieldMappings(_dbContextFactory, config.Oid))
                                                            {
                                                                if (!string.IsNullOrEmpty(mapping.Value.ClientField) &&
                                                                    (mapping.Value.StandardField == "employee" || mapping.Value.StandardField == "AuthenticationDateAndTime"))
                                                                {
                                                                    var standardField = mapping.Value.StandardField;
                                                                    object value = standardField switch
                                                                    {
                                                                        "EmployeeNo" => item.EmployeeNo,
                                                                        "AuthenticationDateAndTime" => item.EventTime.UtcDateTime,
                                                                        _ => null
                                                                    };

                                                                    paramValues.Add(standardField, value);
                                                                }
                                                            }
                                                            try
                                                            {
                                                                checkRecord = await dbConnection.QueryFirstAsync<int>(duplicateCheckQuery, paramValues);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                // Log ex.Message
                                                            }
                                                            if (checkRecord == 0)
                                                            {
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
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            //                                                  WriteLogToFile($"Exception  : {ex.Message}");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //this will handle visitor visits logs (visitor attendance)
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
                catch (Exception ex)
                {
                    WriteLogToFile($"Exception in DataExchange:{ex.Message} ");
                }
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
        private async Task<string?> GenerateSelectDuplicateQuery(ClientDBDetail clientDBDetail, DynamicDbContextFactory dynamicDbContextFactory)
        {
            if (clientDBDetail == null)
                return null;

            var fieldMappings = await GetClientFieldMappings(dynamicDbContextFactory, clientDBDetail.Oid);
            if (fieldMappings == null || !fieldMappings.Any())
                return null;

            var duplicateCheckFields = new[] { "EmployeeNo", "AuthenticationDateTime" };

            var validMappings = fieldMappings.Values
                .Where(f => !string.IsNullOrEmpty(f.ClientField) && duplicateCheckFields.Contains(f.StandardField))
                .ToList();


            if (!validMappings.Any())
                return null;

            var tableName = validMappings.First().TableName ?? "TabEmployeeAttendances";

            string parameterPrefix = clientDBDetail.DatabaseType switch
            {
                DatabaseType.MySQL => "?",
                DatabaseType.Oracle => ":",
                _ => "@" // SQL Server, PostgreSQL.
            };

            var whereClause = string.Join(" AND ", validMappings.Select(f =>
                $"{f.ClientField} = {parameterPrefix}{f.StandardField}"));

            var selectQuery = $@"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";

            return selectQuery;
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
