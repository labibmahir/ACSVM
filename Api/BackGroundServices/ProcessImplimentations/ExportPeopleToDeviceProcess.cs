using Api.BackGroundServices.ProccessContract;
using Domain.Dto;
using Domain.Dto.HIKVision;
using Domain.Entities;
using Infrastructure.Contracts;
using static Utilities.Constants.Enums;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;

namespace Api.BackGroundServices.ProcessImplimentations
{
    public class ExportPeopleToDeviceProcess : IProcess
    {
        public string ProcessId { get; }

        public float progress { get; set; }

        public ProcessType ProcessType { get; }

        public ProcessState ProcessState { get; set; }
        public string ProcessName { get; set; }
        public string ProcessDescription { get; set; }
        //  public Device Device { get; set; }
        public ExportToDeviceDto exportToDevice { get; set; }
        public List<ErrorMessage> Errors { get; set; } = new List<ErrorMessage>();
        public ProcessPriority ProcessPriority { get; private set; }
        private readonly string? _logDirectory;
        public ExportPeopleToDeviceProcess(IConfiguration configuration, ExportToDeviceDto exportToDeviceDto, ProcessPriority processPriority = ProcessPriority.Normal)
        {
            ProcessId = Guid.NewGuid().ToString();
            ProcessType = ProcessType.ExportDataToDevice;
            ProcessState = ProcessState.Waiting;
            progress = 0;
            //  this.Device = device;
            this.exportToDevice = exportToDeviceDto;
            ProcessPriority = processPriority;
            this.ProcessName = $"Export People to device Id: '{this.exportToDevice.ToDeviceId}'";
            this.ProcessDescription = "Waiting...";
            _logDirectory = configuration["ServiceLogFilePath:FileLogPath"];
        }
        public async Task Execute(ILogger<ExportPeopleToDeviceProcess> logger, IUnitOfWork context, IHikVisionMachineService _visionMachineService)
        {
            try
            {
                WriteLogToFile("Export People To Device Process started");
                this.ProcessState = ProcessState.Running;

                var personsInFromDevice = await context.IdentifiedAssignDeviceRepository.GetPersonsByIdentifiedAssignDevice(exportToDevice.FromDeviceId);
                foreach (var person in personsInFromDevice)
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
                        localUIRight = person.IsDeviceAdministrator,
                        userVerifyMode = person.UserVerifyMode switch
                        {
                            Enums.UserVerifyMode.faceAndFpAndCard => "faceAndFpAndCard",
                            Enums.UserVerifyMode.faceOrFpOrCardOrPw => "faceOrFpOrCardOrPw",
                            Enums.UserVerifyMode.card => "card",
                            _ => "faceAndFpAndCard"//this is default
                        },
                        checkUser = true,
                        addUser = true,
                        //callNumbers = new List<string> { $"{person.PhoneNumber}" },
                        callNumbers = new List<string> { " 1-1-1-401" },
                        floorNumbers = new List<FloorNumber> { new FloorNumber() { min = 1, max = 100 } },
                        gender = person.Gender switch
                        {
                            Enums.Gender.Male => "male",
                            Enums.Gender.Female => "female",
                            _ => "other"
                        },
                    };
                    var toDevice = await context.DeviceRepository.GetDeviceByKey(exportToDevice.ToDeviceId);

                    var vService = await _visionMachineService.AddUser(toDevice, vMUserInfo);
                    var res = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(vService);
                    if (res.StatusCode == 1)
                    {
                        var getFingerPrintByPerson = await context.FingerPrintRepository.GetAllFingerPrintOfPeronByPersonId(person.Oid);

                        foreach (var fingerPrint in getFingerPrintByPerson)
                        {
                            VMFingerPrintSetUpRequest vMFingerPrintSetUpRequest = new VMFingerPrintSetUpRequest()
                            {
                                employeeNo = person.PersonNumber,
                                fingerData = fingerPrint.Data,
                                fingerPrintID = (int)fingerPrint.FingerNumber,
                                fingerType = "normalFP",//personFingerPrintDto.FingerprintType,
                                enableCardReader = new int[] { 1 },
                            };
                            var restult = await _visionMachineService.SetFingerprint(toDevice, vMFingerPrintSetUpRequest);

                        }
                        var getAssignedCardToPerson = await context.IdentifiedAssignCardRepository.GetIdentifiedAssignCardByPerson(person.Oid);
                        if (getAssignedCardToPerson != null)
                        {
                            VMCardInfo vMCardInfo = new VMCardInfo()
                            {
                                addCard = true,
                                cardNo = getAssignedCardToPerson.Card.CardNumber,
                                cardType = "normalCard",
                                employeeNo = person.PersonNumber
                            };
                            var result = await _visionMachineService.AddCard(toDevice, vMCardInfo);
                        }

                        var getPersonImage = await context.PersonImageRepository.GetImageByPersonId(person.Oid);
                        if (getPersonImage != null)
                        {
                            FacePictureUploadDto vMPersonImageSetUpRequest = new FacePictureUploadDto()
                            {
                                faceLibType = "blackFD",
                                FDID = "1",
                                FPID = person.PersonNumber
                            };

                            var (IsSuccess, Message) = await _visionMachineService.PostFaceRecordToLibrary(toDevice.DeviceIP, Convert.ToInt16(toDevice.Port), toDevice.Username, toDevice.Password, vMPersonImageSetUpRequest, getPersonImage.ImageData);

                        }
                    }
                }
                this.ProcessState = ProcessState.Finished;
                this.progress = 100;
            }
            catch (Exception ex)
            {
                WriteLogToFile($"Exception in Export People to Device Process:{ex.Message} ");
                //     FileLogger.Log($"Exception : {ex.Message}");
                this.ProcessState = ProcessState.Failed;
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
    }
}
