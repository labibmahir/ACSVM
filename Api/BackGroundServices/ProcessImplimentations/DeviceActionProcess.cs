using Api.BackGroundServices.ProccessContract;
using Domain.Dto;
using Domain.Dto.HIKVision;
using Domain.Entities;
using Infrastructure.Contracts;
using SurveillanceDevice.Integration.HIKVision;
using System.Net.NetworkInformation;
using Utilities.Constants;

namespace Api.BackGroundServices.ProcessImplimentations
{
    public class DeviceActionProcess : IProcess
    {
        public string ProcessId { get; }

        public float progress { get; set; }

        public ProcessType ProcessType { get; }

        public ProcessState ProcessState { get; set; }
        public string ProcessName { get; set; }
        public string ProcessDescription { get; set; }
        //  public Device Device { get; set; }
        public DeviceSynchronizer deviceSynchronizer { get; set; }
        public List<ErrorMessage> Errors { get; set; } = new List<ErrorMessage>();
        public ProcessPriority ProcessPriority { get; private set; }
        public DeviceActionProcess(DeviceSynchronizer deviceSynchronizer, ProcessPriority processPriority = ProcessPriority.Normal)
        {
            ProcessId = Guid.NewGuid().ToString();
            ProcessType = ProcessType.DeviceAction;
            ProcessState = ProcessState.Waiting;
            progress = 0;
            //  this.Device = device;
            this.deviceSynchronizer = deviceSynchronizer;
            ProcessPriority = processPriority;
            this.ProcessName = $"Export People to device Id: '{this.deviceSynchronizer.Oid}'";
            this.ProcessDescription = "Waiting...";
        }
        public async Task Execute(ILogger<DeviceActionProcess> logger, IUnitOfWork context, IHikVisionMachineService _visionMachineService)
        {
            try
            {
                this.ProcessState = ProcessState.Running;
                if (deviceSynchronizer.VisitorId != null)
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
                        var identifiedSynDevices = await context.IdentifiedSyncDeviceRepository.QueryAsync(x => x.IsDeleted == false && x.DeviceSynchronizerId == deviceSynchronizer.Oid);
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
                                        var vService = await _visionMachineService.AddUser(device, vMUserInfo);
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
                                        var vService = await _visionMachineService.UpdateUser(device, vMUserInfo);
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

                                        var vService = await _visionMachineService.DeleteUserWithDetails(device, vMCardInfoDeleteRequest);
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
                                    context.IdentifiedSyncDeviceRepository.Update(synDevice);

                                }
                            }
                        }
                        deviceSynchronizer.IsSync = isSynceAllDevice;
                        context.DeviceSynchronizerRepository.Update(deviceSynchronizer);
                        await context.SaveChangesAsync();

                    }
                }
                this.ProcessState = ProcessState.Finished;
                this.progress = 100;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                //     FileLogger.Log($"Exception : {ex.Message}");
                this.ProcessState = ProcessState.Failed;
            }
        }

        public ProcessDto ToProcessDto()
        {
            ProcessDto dto = new ProcessDto();
            dto.Name = this.ProcessName;
            dto.Progress = this.progress;
            dto.Description = this.ProcessDescription;
            dto.Id = this.ProcessId;
            return dto;
        }
        private async Task<bool> IsDeviceActive(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ipAddress, 4000); // 2 seconds timeout
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
