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
    public class PersonController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<PersonController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public PersonController(IUnitOfWork context, ILogger<PersonController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }
        /// <summary>
        /// URL: api/device
        /// </summary>
        /// <param name="person">person object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreatePerson)]
        public async Task<ActionResult<Person>> CreateDevice(Person person)
        {
            try
            {
                var personWithSamePersonNumer = await context.PersonRepository.GetPersonByPersonNumber(person.PersonNumber);

                if (personWithSamePersonNumer != null && personWithSamePersonNumer.OrganizationId == person.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DeviceNameTaken);

                var deviceWithSameIP = await context.PersonRepository.GetPersonByphoneNumber(person.PhoneNumber);

                if (deviceWithSameIP != null && deviceWithSameIP.OrganizationId == person.OrganizationId)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateIPError);

                person.DateCreated = DateTime.Now;
                person.IsDeleted = false;
                person.CreatedBy = GetLoggedInUserId();

                context.PersonRepository.Add(person);
                await context.SaveChangesAsync();

                return Ok(person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
    }
}
