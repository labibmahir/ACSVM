using Api.NotificationHub;
using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.SignalR;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;
using System.Net.NetworkInformation;

namespace Api.BackGroundServices
{
    public class PersonAndVisitorSyncronizer : BackgroundService
    {
        private readonly ILogger<PersonAndVisitorSyncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly string? _logDirectory;
        public PersonAndVisitorSyncronizer(IConfiguration configuration,
            ILogger<PersonAndVisitorSyncronizer> logger,
            IServiceProvider serviceProvider
            , IHubContext<NotificationsHub> hubContext
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logDirectory = configuration["ServiceLogFilePath:FileLogPath"];

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Device> devices = new List<Device>();
            Dictionary<string, Task> monitoringTasks = new();
            WriteLogToFile("PersonAndVisitor Syncronizer BackGround Service Started");
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




                        #region Vistor Syn To Device
                        WriteLogToFile("PersonAndVisitor Syncronizer BackGround Service Vistor Syn To Device Section");
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
                                                            DateCreated = DateTime.Now,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                            DateCreated=DateTime.Now,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                            DateCreated = DateTime.Now,
                                                            IsDeleted = false,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                    DateCreated = DateTime.Now,
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
                        #region Person Syn To Device
                        WriteLogToFile("PersonAndVisitor Syncronizer BackGround Service Person Syn To Device Section");
                        try
                        {
                            var deviceSync = await context.DeviceSynchronizerRepository.QueryAsync(x => x.IsDeleted == false && x.IsSync != true && x.PersonId != null);
                            foreach (var deviceSynchronizer in deviceSync)
                            {
                                var person = await context.PersonRepository.GetPersonByKey(deviceSynchronizer.PersonId.Value);
                                if (person != null)
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
                                        employeeNo = person.PersonNumber,
                                        deleteUser = false,
                                        name = person.FirstName + " " + person.Surname,
                                        userType = "normal",
                                        closeDelayEnabled = true,
                                        Valid = new VMEffectivePeriod()
                                        {
                                            enable = true,
                                            beginTime = person.ValidateStartPeriod.ToString("yyyy-MM-ddTHH:mm:ss"),
                                            endTime = person.ValidateEndPeriod?.ToString("yyyy-MM-ddTHH:mm:ss"),
                                            timeType = "local"
                                        },
                                        doorRight = "1",
                                        RightPlan = vMDoorPermissionSchedules,
                                        localUIRight = false,
                                        userVerifyMode = person.UserVerifyMode switch
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
                                        gender = person.Gender switch
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
                                                            Message = "Failed Upadting person Record",
                                                            Oid = Guid.NewGuid(),
                                                            PersonId = deviceSynchronizer.PersonId,
                                                            DateCreated = DateTime.Now,
                                                            IsDeleted = false,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                            PersonId = deviceSynchronizer.PersonId,
                                                            IsDeleted = false,
                                                            DateCreated = DateTime.Now,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                        EmployeeNoList = new List<VMEmployeeNoListItem?>() { new() { employeeNo = person.PersonNumber } }
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
                                                            PersonId = deviceSynchronizer.PersonId,
                                                            IsDeleted = false,
                                                            DateCreated = DateTime.Now,
                                                            IsSync = false
                                                        };
                                                        synDevice.IsSync = false;
                                                        context.IdentifiedSyncDeviceRepository.Update(synDevice);
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
                                                    PersonId = deviceSynchronizer.PersonId,
                                                    DateCreated = DateTime.Now,
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

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    WriteLogToFile($"Exception in PersonAndVisitor Syncronizer:{ex.Message} ");
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
