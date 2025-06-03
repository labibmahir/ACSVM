using Domain.Dto.HIKVision;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using SurveillanceDevice.Integration.HIKVision;
using Utilities.Constants;
using System.Net.NetworkInformation;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonImageController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<PersonImageController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>
        public PersonImageController(IUnitOfWork context, ILogger<PersonImageController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
        }


        /// <summary>
        /// URL: api/person-image
        /// </summary>
        /// <param name="personImageDto">personImageDto object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreatePersonImage)]
        public async Task<ActionResult<PersonImage>> CreatePersonImage(PersonImageDto personImageDto)
        {
            try
            {
                if (personImageDto.Image == null || personImageDto.Image.Length == 0)
                {
                    return BadRequest(MessageConstants.NoImageFileProvided);
                }

                var allowedImageTypes = new[] { "image/jpeg", "image/png" };

                if (!allowedImageTypes.Contains(personImageDto.Image.ContentType.ToLower()))
                {
                    return BadRequest("Only image files (JPEG, PNG) are allowed.");
                }


                var person = await context.PersonRepository.GetPersonByKey(personImageDto.PersonId);
                if (person == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                var personImageInDb = await context.PersonImageRepository.GetImageByPersonId(personImageDto.PersonId);

                if (personImageInDb != null && personImageInDb.OrganizationId == personImageInDb.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateFingerError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(person.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }
                byte[] imageBytes;

                using (var memoryStream = new MemoryStream())
                {
                    await personImageDto.Image.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                string base64Image = Convert.ToBase64String(imageBytes);

                PersonImage personImage = new PersonImage()
                {
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    ImageBase64 = base64Image,
                    ImageData = imageBytes,
                    PersonId = personImageDto.PersonId,
                    Oid = Guid.NewGuid(),
                    IsDeleted = false,
                };

                FacePictureUploadDto vMPersonImageSetUpRequest = new FacePictureUploadDto()
                {
                    faceLibType = "blackFD",
                    FDID = "1",
                    FPID = person.PersonNumber
                };

                foreach (var device in assignedDevices)
                {


                    var (IsSuccess, Message) = await _visionMachineService.PostFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, vMPersonImageSetUpRequest, imageBytes);

                    if (IsSuccess)
                    {
                        var personImageInserted = await context.PersonImageRepository.GetImageByPersonId(person.Oid);
                        if (personImageInserted == null)
                        {
                            context.PersonImageRepository.Add(personImage);
                            await context.SaveChangesAsync();
                        }
                    }

                }

                return Ok(personImage);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/visitor-image
        /// </summary>
        /// <param name="VisitorImageDto">VisitorImageDto object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateVisitorImage)]
        public async Task<ActionResult<PersonImage>> CreateVisitorImage(VisitorImageDto visitorImageDto)
        {
            try
            {
                if (visitorImageDto.Image == null || visitorImageDto.Image.Length == 0)
                {
                    return BadRequest(MessageConstants.NoImageFileProvided);
                }

                var allowedImageTypes = new[] { "image/jpeg", "image/png" };

                if (!allowedImageTypes.Contains(visitorImageDto.Image.ContentType.ToLower()))
                {
                    return BadRequest("Only image files (JPEG, PNG) are allowed.");
                }


                var visitor = await context.VisitorRepository.GetVisitorByKey(visitorImageDto.VisitorId);
                if (visitor == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.NoMatchFoundError);

                var visitorImageInDb = await context.PersonImageRepository.GetImageByVisitorId(visitorImageDto.VisitorId);

                if (visitorImageInDb != null && visitorImageInDb.OrganizationId == visitorImageInDb.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateFingerError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(visitor.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }
                byte[] imageBytes;

                using (var memoryStream = new MemoryStream())
                {
                    await visitorImageDto.Image.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                string base64Image = Convert.ToBase64String(imageBytes);

                PersonImage personImage = new PersonImage()
                {
                    CreatedBy = GetLoggedInUserId(),
                    DateCreated = DateTime.Now,
                    ImageBase64 = base64Image,
                    ImageData = imageBytes,
                    VisitorId = visitorImageDto.VisitorId,
                    Oid = Guid.NewGuid(),
                    IsDeleted = false,
                };
                FacePictureUploadDto vMPersonImageSetUpRequest = new FacePictureUploadDto()
                {
                    faceLibType = "blackFD",
                    FDID = "1",
                    FPID = visitor.VisitorNumber
                };

                foreach (var device in assignedDevices)
                {


                    var (IsSuccess, Message) = await _visionMachineService.PostFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, vMPersonImageSetUpRequest, imageBytes);

                    if (IsSuccess)
                    {
                        var personImageInserted = await context.PersonImageRepository.GetImageByVisitorId(visitor.Oid);
                        if (personImageInserted == null)
                        {
                            context.PersonImageRepository.Add(personImage);
                            await context.SaveChangesAsync();
                        }
                    }

                }

                return Ok(personImage);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person-image/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonImageByKey)]
        public async Task<IActionResult> ReadPersonImageByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var personImage = await context.PersonImageRepository.GetPersonImageByKey(key);


                return Ok(personImage);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/person-image/person/{PersonId}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonImageByPersonId)]
        public async Task<IActionResult> ReadPersonImageByPersonId(Guid PersonId)
        {
            try
            {
                if (PersonId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var personImage = await context.PersonImageRepository.GetImageByPersonId(PersonId);

                return Ok(personImage);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person-image/visitor/{VisitorId}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadPersonImageByVisitorId)]
        public async Task<IActionResult> ReadPersonImageByVisitorId(Guid VisitorId)
        {
            try
            {
                if (VisitorId == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var visitorImage = await context.PersonImageRepository.GetImageByVisitorId(VisitorId);


                return Ok(visitorImage);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/person-image/{key}
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <param name="personImageUpdateDto">PersonImage to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdatePersonImage)]

        public async Task<IActionResult> UpdatePersonImage(Guid key, PersonImageUpdateDto personImageUpdateDto)
        {
            try
            {
                if (key != personImageUpdateDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);


                var personImageInDb = await context.PersonImageRepository.GetPersonImageByKey(personImageUpdateDto.Oid);

                if (personImageInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                if (personImageUpdateDto.Image == null || personImageUpdateDto.Image.Length == 0)
                {
                    return BadRequest(MessageConstants.NoImageFileProvided);
                }

                var allowedImageTypes = new[] { "image/jpeg", "image/png" };

                if (!allowedImageTypes.Contains(personImageUpdateDto.Image.ContentType.ToLower()))
                {
                    return BadRequest("Only image files (JPEG, PNG) are allowed.");
                }
                Person person = new Person();
                Visitor visitor = new Visitor();
                if (personImageInDb.PersonId.HasValue)
                {
                    person = await context.PersonRepository.GetPersonByKey(personImageInDb.PersonId.Value);
                }
                else if (personImageInDb.VisitorId.HasValue)
                {
                    visitor = await context.VisitorRepository.GetVisitorByKey(personImageInDb.VisitorId.Value);
                }

                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                if (personImageInDb.VisitorId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(personImageInDb.VisitorId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                }
                else if (personImageInDb.PersonId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(personImageInDb.PersonId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                }

                foreach (var item in identifiedAssignDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                byte[] imageBytes;

                using (var memoryStream = new MemoryStream())
                {
                    await personImageUpdateDto.Image.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
                foreach (var device in identifiedAssignDevices)
                {
                    if (personImageInDb.PersonId.HasValue)
                    {
                        var (IsSuccess, Message) = await _visionMachineService.DeleteFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, new FacePictureRemoveDto
                        {
                            faceLibType = "blackFD",
                            FDID = "1",
                            FPID = person.PersonNumber,
                            deleteFP = true
                        }, imageBytes);

                        if (IsSuccess)
                        {
                            FacePictureUploadDto vMPersonImageSetUpRequest = new FacePictureUploadDto()
                            {
                                faceLibType = "blackFD",
                                FDID = "1",
                                FPID = person.PersonNumber
                            };

                            var (IsSuccessUpload, MessageUpload) = await _visionMachineService.PostFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, vMPersonImageSetUpRequest, imageBytes);

                        }

                    }
                    else if (personImageInDb.VisitorId.HasValue)
                    {
                        var (IsSuccess, Message) = await _visionMachineService.DeleteFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, new FacePictureRemoveDto
                        {
                            faceLibType = "blackFD",
                            FDID = "1",
                            FPID = visitor.VisitorNumber,
                            deleteFP = true
                        }, imageBytes);

                        if (IsSuccess)
                        {
                            FacePictureUploadDto vMPersonImageSetUpRequest = new FacePictureUploadDto()
                            {
                                faceLibType = "blackFD",
                                FDID = "1",
                                FPID = visitor.VisitorNumber
                            };

                            var (IsSuccessUpload, MessageUpload) = await _visionMachineService.PostFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, vMPersonImageSetUpRequest, imageBytes);

                        }
                    }

                }


                string base64Image = Convert.ToBase64String(imageBytes);

                personImageInDb.ImageData = imageBytes;
                personImageInDb.ImageBase64 = base64Image;
                personImageInDb.IsDeleted = false;
                personImageInDb.ModifiedBy = GetLoggedInUserId();
                personImageInDb.DateModified = DateTime.Now;

                context.PersonImageRepository.Update(personImageInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/person-image/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeletePersonImage)]
        public async Task<IActionResult> DeletePersonImage(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var personImageInDb = await context.PersonImageRepository.GetPersonImageByKey(key);

                if (personImageInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                Person person = new Person();
                Visitor visitor = new Visitor();
                if (personImageInDb.PersonId.HasValue)
                {
                    person = await context.PersonRepository.GetPersonByKey(personImageInDb.PersonId.Value);
                }
                else if (personImageInDb.VisitorId.HasValue)
                {
                    visitor = await context.VisitorRepository.GetVisitorByKey(personImageInDb.VisitorId.Value);
                }

                List<IdentifiedAssignDevice> identifiedAssignDevices = new List<IdentifiedAssignDevice>();
                if (personImageInDb.VisitorId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(personImageInDb.VisitorId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                }
                else if (personImageInDb.PersonId.HasValue)
                {
                    var assignDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(personImageInDb.PersonId.Value);
                    identifiedAssignDevices = assignDevices.ToList();
                }

                foreach (var item in identifiedAssignDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                foreach (var device in identifiedAssignDevices)
                {
                    if (personImageInDb.PersonId.HasValue)
                    {
                        var (IsSuccess, Message) = await _visionMachineService.DeleteFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, new FacePictureRemoveDto
                        {
                            faceLibType = "blackFD",
                            FDID = "1",
                            FPID = person.PersonNumber,
                            deleteFP = true
                        }, personImageInDb.ImageData);



                    }
                    else if (personImageInDb.VisitorId.HasValue)
                    {
                        var (IsSuccess, Message) = await _visionMachineService.DeleteFaceRecordToLibrary(device.Device.DeviceIP, Convert.ToInt16(device.Device.Port), device.Device.Username, device.Device.Password, new FacePictureRemoveDto
                        {
                            faceLibType = "blackFD",
                            FDID = "1",
                            FPID = visitor.VisitorNumber,
                            deleteFP = true
                        }, personImageInDb.ImageData);


                    }

                }

                personImageInDb.IsDeleted = true;
                personImageInDb.ModifiedBy = GetLoggedInUserId();
                personImageInDb.DateModified = DateTime.Now;

                context.PersonImageRepository.Update(personImageInDb);
                await context.SaveChangesAsync();

                return Ok(personImageInDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
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
