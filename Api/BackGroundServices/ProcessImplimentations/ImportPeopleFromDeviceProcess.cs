using Api.BackGroundServices.ProccessContract;
using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;
using Infrastructure;
using static Utilities.Constants.Enums;

namespace Api.BackGroundServices.ProcessImplimentations
{
    public class ImportPeopleFromDeviceProcess : IProcess
    {
        public string ProcessId { get; }

        public float progress { get; set; }

        public ProcessType ProcessType { get; }

        public ProcessState ProcessState { get; set; }
        public string ProcessName { get; set; }
        public string ProcessDescription { get; set; }
        public Device Device { get; set; }
        public List<ErrorMessage> Errors { get; set; } = new List<ErrorMessage>();
        public ProcessPriority ProcessPriority { get; private set; }
        private readonly string? _logDirectory;
        public ImportPeopleFromDeviceProcess(IConfiguration configuration, Device device, ProcessPriority processPriority = ProcessPriority.Normal)
        {
            ProcessId = Guid.NewGuid().ToString();
            ProcessType = ProcessType.ImportingPeopleFromDevice;
            ProcessState = ProcessState.Waiting;
            progress = 0;
            this.Device = device;
            ProcessPriority = processPriority;
            this.ProcessName = $"Importing People from device: '{this.Device.DeviceName}'";
            this.ProcessDescription = "Waiting...";
            _logDirectory = configuration["ServiceLogFilePath:FileLogPath"];
        }

        public async Task Execute(ILogger<ImportPeopleFromDeviceProcess> logger, IUnitOfWork context, IHikVisionMachineService _visionMachineService)
        {
            try
            {
                WriteLogToFile("Import People From Device Process");
                this.ProcessState = ProcessState.Running;
                //FileLogger.Log($"{this.ProcessState} = {ProcessState.Running}");
                int count = await _visionMachineService.GetUserCount(this.Device);
                var userInfo = new List<VMUserInfo>();
                int LastIndex = 0;
                List<Task<VMUserInfoSearchResponse>> tasks = new List<Task<VMUserInfoSearchResponse>>();
                // FileLogger.Log($"UserCount from Device :{count}");
                while (userInfo.Count < count) // Stop only when we have ALL users
                {
                    try
                    {
                        // Fetch users from the device
                        var task = _visionMachineService.GetAllUsers(this.Device, LastIndex, 20);
                        tasks.Add(task);

                        //LastIndex += 20;

                        if (tasks.Count >= 1) // Process batch when 5 tasks are queued
                        {
                            var completedTasks = await Task.WhenAll(tasks);
                            tasks.Clear(); // Clear processed tasks

                            foreach (var result in completedTasks)
                            {
                                if (result?.UserInfo != null && result.UserInfo.Count > 0)
                                {
                                    userInfo.AddRange(result.UserInfo);
                                    LastIndex += result.UserInfo.Count; // MOVE INDEX BASED ON ACTUAL RECORDS RECEIVED
                                }
                            }

                            // Update progress
                            this.progress = (float)(((float)userInfo.Count * 100.0f) / (float)count);
                            this.ProcessDescription = $"Importing person info: {userInfo.Count}/{count} Person";
                        }
                    }
                    catch (Exception ex)
                    {
                        //   FileLogger.Log($"Exception : {ex.Message}");
                    }
                }

                // Process remaining tasks
                if (tasks.Count > 0)
                {
                    var completedTasks = await Task.WhenAll(tasks);
                    foreach (var result in completedTasks)
                    {
                        if (result?.UserInfo != null && result.UserInfo.Count > 0)
                        {
                            userInfo.AddRange(result.UserInfo);
                        }
                    }

                    this.progress = (float)(((float)userInfo.Count * 100.0f) / (float)count);
                    this.ProcessDescription = $"Importing person info: {userInfo.Count}/{count} Person";
                }

                var people = await context.PersonRepository.QueryAsync(x => x.IsDeleted == false);
                List<Person> persons = new List<Person>();
                foreach (var user in userInfo)
                {
                    var facePictures = await _visionMachineService.SaveEmployeeFaceImage(Device, user.employeeNo, user.faceURL);
                    string fullName = user.name?.Trim() ?? "";
                    string firstName = "";
                    string lastName = "";

                    if (!string.IsNullOrEmpty(fullName))
                    {
                        string[] nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        firstName = nameParts[0];

                        if (nameParts.Length > 1)
                        {
                            lastName = string.Join(" ", nameParts.Skip(1));
                        }
                        else
                        {
                            lastName = ""; // Or set it to null / same as firstName / "N/A", depending on what you need
                        }
                    }
                    Person person = new Person()
                    {
                        FirstName = firstName,
                        Surname = lastName,
                        PersonNumber = user.employeeNo,
                        Gender = user.gender == "male" ? Enums.Gender.Male : (user.gender == "female" ? Enums.Gender.Female : Enums.Gender.Other),
                        IsDeleted = false,
                        DateCreated = DateTime.Now,

                    };

                    person.Oid = people.FirstOrDefault(x => x.PersonNumber == user.employeeNo)?.Oid ?? Guid.NewGuid();
                    persons.Add(person);
                    if (!people.Any(x => x.PersonNumber == user.employeeNo))
                    {
                        //Person person = user.ToPerson();

                        context.PersonRepository.Add(person);

                        await context.SaveChangesAsync();
                        if (!string.IsNullOrEmpty(facePictures.base64Image))
                        {
                            try
                            {


                                PersonImage face = new PersonImage()
                                {
                                    Oid = Guid.NewGuid(),
                                    ImageData = facePictures.binaryImage,
                                    ImageBase64 = facePictures.base64Image,
                                    DateCreated = DateTime.Now,
                                    PersonId = person.Oid,
                                    IsDeleted = false
                                };

                                context.PersonImageRepository.Add(face);
                                await context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {

                            }


                        }
                    }
                }
                List<Card> cards = new List<Card>();
                List<Task<VMCardInfoSearchResponse>> cardInfoTasks = new List<Task<VMCardInfoSearchResponse>>();
                List<VMSearchCardInfo> returnedCardsInfo = new List<VMSearchCardInfo>();
                List<List<VMEmployeeNoListItem>> employeeNoLists = new List<List<VMEmployeeNoListItem>>();
                int uu = 0;
                int jj = 0;
                employeeNoLists.Add(new List<VMEmployeeNoListItem>());
                foreach (var person in persons)
                {
                    if (uu == 20)
                    {
                        uu = 0;
                        jj++;
                        employeeNoLists.Add(new List<VMEmployeeNoListItem>());
                    }
                    employeeNoLists[jj].Add(new VMEmployeeNoListItem() { employeeNo = person.PersonNumber });
                    uu++;
                }
                int allCardsCount = 0;
                int returnedCardsCount = 0;
                int returnedCards = 0;
                foreach (var employeeNoList in employeeNoLists)
                {
                    int tempReturnedCardCount = 0;
                    int lastCardIndex = 0;
                    try
                    {
                        var returnedTemp = (await _visionMachineService
                                    .GetCardsByEmployees(this.Device, employeeNoList, lastCardIndex, 20));
                        lastCardIndex += 20;
                        returnedCardsInfo.AddRange(returnedTemp.CardInfoSearch.CardInfo);
                        returnedCardsCount += returnedTemp.CardInfoSearch.CardInfo.Count;
                        returnedCards += employeeNoList.Count;
                        tempReturnedCardCount = returnedTemp.CardInfoSearch.CardInfo.Count;
                        allCardsCount += returnedTemp.CardInfoSearch.totalMatches;
                        this.progress = (float)(((float)returnedCards * 100.0f) / (float)persons.Count / 3 + 33);
                        this.ProcessDescription = $"Importing card info: {returnedCards}/{persons.Count} card";
                    }
                    catch
                    {
                        // TODO: Add somthing to error list and log somthing
                    }

                    while (tempReturnedCardCount != 0)
                    {
                        try
                        {
                            tempReturnedCardCount = 0;
                            var returned = (await _visionMachineService
                                .GetCardsByEmployees(this.Device, employeeNoList, lastCardIndex, 20)).CardInfoSearch.CardInfo;
                            lastCardIndex += 20;
                            returnedCardsInfo.AddRange(returned);
                            returnedCardsCount += returned.Count;
                            tempReturnedCardCount = returned.Count;
                        }
                        catch
                        {
                            // TODO: Add somthing to error list and log somthing
                        }
                    }
                    //logger.LogError("#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3#3##3#3#################################################"); // TODO: remove this

                    foreach (var task in cardInfoTasks)
                    {
                        var returned = (await task).CardInfoSearch.CardInfo;
                        returnedCardsInfo.AddRange(returned);
                        returnedCardsCount += returned.Count;
                        this.progress = (float)(((float)returnedCardsCount * 100.0f) / (float)count / 3 + 66);
                        this.ProcessDescription = $"Importing card info: {returnedCardsCount}/{allCardsCount} card";
                    }
                }
                //start from here tomorrow commented from line 209 till 257
                var existingCards = await context.CardRepository.QueryAsync(x => x.IsDeleted == false);
                //logger.LogError("#4#4#4#4#4#4#4#4#4#4#4#4#44##4#4#4#4#4#4#4#4#4#4#4#4#4#########################################################"); // TODO: remove this
                foreach (var cardInfo in returnedCardsInfo)
                {
                    if (!existingCards.Any(x => x.CardNumber == cardInfo.cardNo))
                    {
                        Card card = new Card();
                        card.CardNumber = cardInfo.cardNo;
                        card.IsDeleted = false;
                        card.DateCreated = DateTime.Now;
                        card.Status = Enums.Status.Allocated;
                        card.DateCreated = DateTime.Now;
                        context.CardRepository.Add(card);
                        await context.SaveChangesAsync();

                        var tempPerson = await context.PersonRepository.FirstOrDefaultAsync(x => x.PersonNumber == cardInfo.employeeNo);
                        if (tempPerson != null)
                        {
                            var temp = await context.IdentifiedAssignCardRepository.FirstOrDefaultAsync(x => x.PersonId == tempPerson.Oid);
                            if (temp == null)
                            {
                                IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                                {
                                    IsDeleted = false,
                                    DateCreated = DateTime.Now,
                                    CardId = card.Oid,
                                    PersonId = tempPerson.Oid,
                                };
                                context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        var tempPerson = await context.PersonRepository.FirstOrDefaultAsync(x => x.PersonNumber == cardInfo.employeeNo);
                        if (tempPerson != null)
                        {
                            var temp = await context.IdentifiedAssignCardRepository.FirstOrDefaultAsync(x => x.PersonId == tempPerson.Oid);
                            if (temp == null)
                            {
                                IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                                {
                                    IsDeleted = false,
                                    DateCreated = DateTime.Now,
                                    CardId = existingCards.Where(x => x.CardNumber == cardInfo.cardNo).FirstOrDefault().Oid,
                                    PersonId = tempPerson.Oid,
                                };
                                context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                }
                try
                {
                    context.CardRepository.AddRange(cards);
                    //logger.LogError("#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5#5##5#5#5############################################################");// TODO: remove this
                    await context.SaveChangesAsync();
                }
                catch
                {

                }
                // Fingerprints
                List<FingerPrint> fingerPrints = new List<FingerPrint>();
                int returnedFingerprints = 0;
                foreach (var person in persons)
                {
                    try
                    {
                        //if (person.EmployeeNumber == "00000020")
                        //{ 
                        //    int a = 0; 
                        //}
                        var returned = (await _visionMachineService
                                .GetFingerprintsByEmployeeId(this.Device, person.PersonNumber)).FingerPrintList;


                        foreach (var fingerprint in returned)
                        {
                            FingerPrint fp = new FingerPrint();
                            fp.FingerNumber = (FingerNumber)fingerprint.fingerPrintID;
                            //fp.Type = fingerprint.fingerType.ToFingerprintType();
                            //fp.CardReaderNumber = fingerprint.cardReaderNo;
                            fp.Data = fingerprint.fingerData;
                            //       fp.Person = person;
                            fp.PersonId = person.Oid;
                            fp.IsDeleted = false;
                            fp.DateCreated = DateTime.Now;
                            //   fingerPrints.Add(fingerprint.ToFingerprint(person));
                            var checkFingerPrint = await context.FingerPrintRepository.FirstOrDefaultAsync(x => x.PersonId == person.Oid && x.FingerNumber == fp.FingerNumber && x.IsDeleted == false);
                            if (checkFingerPrint == null)
                                fingerPrints.Add(fp);

                        }

                        context.FingerPrintRepository.AddRange(fingerPrints);
                        await context.SaveChangesAsync();

                        returnedFingerprints++;
                        this.progress = (float)(((float)returnedFingerprints * 100.0f) / (float)persons.Count / 3 + 66);
                        this.ProcessDescription = $"Importing fingerprints: {returnedFingerprints}/{persons.Count} fingerprint";
                    }
                    catch
                    {
                        // TODO: Add somthing to error list and log somthing
                    }
                }
                context.FingerPrintRepository.AddRange(fingerPrints);
                await context.SaveChangesAsync();
                this.ProcessState = ProcessState.Finished;
                this.progress = 100;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                WriteLogToFile($"Exception in ImportPeople from Device Process:{ex.Message} ");
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
