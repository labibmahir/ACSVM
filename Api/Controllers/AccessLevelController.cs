using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccessLevelController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<AccessLevelController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public AccessLevelController(IUnitOfWork context, ILogger<AccessLevelController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }


        /// <summary>
        /// URL: api/accessLevel
        /// </summary>
        /// <param name="accessLevel">accessLevel object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateAccessLevel)]
        public async Task<ActionResult<AccessLevel>> CreateAccessLevel(AccessLevel accessLevel)
        {
            try
            {
                var acessLevelWithSameDeviceName = await context.AccessLevelRepository.GetAccessLevelByDescription(accessLevel.Description);

                if (acessLevelWithSameDeviceName != null && acessLevelWithSameDeviceName.OrganizationId == accessLevel.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNameTaken);


                accessLevel.DateCreated = DateTime.Now;
                accessLevel.IsDeleted = false;
                accessLevel.CreatedBy = GetLoggedInUserId();

                context.AccessLevelRepository.Add(accessLevel);
                await context.SaveChangesAsync();

                return Ok(accessLevel);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/accesslevel/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table AccessLevel.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAccessLevelByKey)]
        [Authorize]
        public async Task<IActionResult> ReadAccessLevelByKey(int key)
        {
            try
            {
                if (key <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var accessLevel = await context.AccessLevelRepository.GetAccessLevelByKey(key);

                if (accessLevel == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                return Ok(accessLevel);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/accessLevels
        /// </summary>
        /// <returns>A list of accessLevel.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAccessLevels)]
        public async Task<IActionResult> ReadAccessLevels([FromQuery] PaginationDto paginationDto)
        {
            try
            {
                if (paginationDto.PageSize == 0)
                {
                    var accessLevels = await context.AccessLevelRepository.GetAccessLevels();

                    return Ok(accessLevels);
                }
                else
                {
                    int currentPage = paginationDto.Page;
                    paginationDto.Page = ((paginationDto.Page - 1) * (paginationDto.PageSize));
                    var accessLevels = await context.AccessLevelRepository.GetAccessLevels(paginationDto);

                    PagedResultDto<AccessLevel> accessLevelsDto = new PagedResultDto<AccessLevel>()
                    {
                        Data = accessLevels.ToList(),
                        PageNumber = currentPage,
                        PageSize = paginationDto.PageSize,
                        TotalItems = await context.AccessLevelRepository.GetAccessLevelsCount(paginationDto)
                    };

                    return Ok(accessLevelsDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }



        /// <summary>
        /// URL: api/accessLevel/{key}
        /// </summary>
        /// <param name="key">Primary key of the table AccessLevel.</param>
        /// <param name="accessLevel">Device to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateAccessLevel)]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAccessLevel(int key, AccessLevel accessLevel)
        {
            try
            {
                if (key != accessLevel.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var accessLevelWithSameDescription = await context.AccessLevelRepository.GetAccessLevelByDescription(accessLevel.Description);

                if (accessLevelWithSameDescription != null && accessLevelWithSameDescription.OrganizationId == accessLevel.OrganizationId && accessLevelWithSameDescription.Oid != accessLevel.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNameTaken);


                var accessLevelInDb = await context.AccessLevelRepository.GetAccessLevelByKey(accessLevel.Oid);

                accessLevelInDb.Description = accessLevel.Description;
                accessLevelInDb.IsDeleted = false;
                accessLevelInDb.OrganizationId = accessLevel.OrganizationId;
                accessLevelInDb.DateModified = DateTime.Now;
                accessLevelInDb.ModifiedBy = GetLoggedInUserId();

                context.AccessLevelRepository.Update(accessLevel);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/accessLevel/{key}
        /// </summary>
        /// <param name="key">Primary key of the table accessLevel.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteAccessLevel)]
        public async Task<IActionResult> DeleteAccessLevel(int key)
        {
            try
            {
                if (key <= 0)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var accessLeveltInDb = await context.AccessLevelRepository.GetAccessLevelByKey(key);

                if (accessLeveltInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                accessLeveltInDb.DateModified = DateTime.Now;
                accessLeveltInDb.IsDeleted = true;

                context.AccessLevelRepository.Update(accessLeveltInDb);
                await context.SaveChangesAsync();

                return Ok(accessLeveltInDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

    }
}
