using Domain.Dto.PaginationFiltersDto;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilities.Constants;
using Domain.Dto.HIKVision;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Authorization;
using SurveillanceDevice.Integration.HIKVision;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<CardController> logger;
        private readonly IConfiguration _configuration;
        private readonly IHikVisionMachineService _visionMachineService;
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public CardController(IUnitOfWork context, ILogger<CardController> logger, IConfiguration configuration, IHikVisionMachineService visionMachineService)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
            _visionMachineService = visionMachineService;
        }

        /// <summary>
        /// URL: api/card
        /// </summary>
        /// <param name="card">Card object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateCard)]
        public async Task<ActionResult<Card>> CreateCard(CardDto cardDto)
        {
            try
            {
                var cardWithSameCardNumber = await context.CardRepository.GetCardByCardNumber(cardDto.CardNumber);

                if (cardWithSameCardNumber != null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.CardNumberTake);

                Card card = new Card();

                card.DateCreated = DateTime.Now;
                card.IsDeleted = false;
                card.CreatedBy = GetLoggedInUserId();
                card.CardNumber = cardDto.CardNumber;
                card.Status = Enums.Status.Inactive;

                context.CardRepository.Add(card);
                await context.SaveChangesAsync();

                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/card-assign-person
        /// </summary>
        /// <param name="card">Card object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.AssignCardToPerson)]
        public async Task<ActionResult<Card>> AssignCardToPerson(CardAssignmentDto cardAssignmentDto)
        {
            try
            {
                var card = await context.CardRepository.GetCardByKey(cardAssignmentDto.CardId);

                if (card == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var person = await context.PersonRepository.GetPersonByKey(cardAssignmentDto.PersonId);

                if (person == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByPerson(person.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                {
                    IsPermanent = cardAssignmentDto.IsPermanent,
                    PersonId = cardAssignmentDto.PersonId,
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    CreatedBy = GetLoggedInUserId(),
                    CardId = cardAssignmentDto.CardId
                };

                VMCardInfo vMCardInfo = new VMCardInfo()
                {
                    addCard = true,
                    cardNo = card.CardNumber,
                    cardType = "normalCard",
                    employeeNo = person.PersonNumber
                };
                foreach (var item in assignedDevices)
                {
                    var result = await _visionMachineService.AddCard(item.Device, vMCardInfo);

                    if (!result.Success)
                    {

                    }
                }
                context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);
                card.Status = Enums.Status.Allocated;
                context.CardRepository.Update(card);
                await context.SaveChangesAsync();

                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
        /// <summary>
        /// URL: api/card-assign-visitor
        /// </summary>
        /// <param name="card">Card object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.AssignCardToVisitor)]
        public async Task<ActionResult<Card>> AssignCardToVisitor(CardAssignmentToVisitorDto cardAssignmentToVisitorDto)
        {
            try
            {
                var card = await context.CardRepository.GetCardByKey(cardAssignmentToVisitorDto.CardId);

                if (card == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                if (card.Status == Enums.Status.Allocated)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardAlreadyAllocated);

                if (card.Status == Enums.Status.Active)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.CardCurrenlyActive);

                var visitor = await context.VisitorRepository.GetVisitorByKey(cardAssignmentToVisitorDto.VisitorId);

                if (visitor == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var assignedDevices = await context.IdentifiedAssignDeviceRepository.GetIdentifiedAssignDeviceByVisitor(visitor.Oid);

                foreach (var item in assignedDevices)
                {
                    if (!await IsDeviceActive(item.Device.DeviceIP))
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Person assigned with Device IP {item.Device.DeviceIP} is not active at the moment.");
                    }
                }

                IdentifiedAssignCard identifiedAssignCard = new IdentifiedAssignCard()
                {
                    IsPermanent = cardAssignmentToVisitorDto.IsPermanent,
                    VisitorId = cardAssignmentToVisitorDto.VisitorId,
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    CreatedBy = GetLoggedInUserId(),
                    CardId = cardAssignmentToVisitorDto.CardId
                };

                //VMCardInfo vMCardInfo = new VMCardInfo()
                //{
                //    addCard = true,
                //    cardNo = card.CardNumber,
                //    cardType = "normalCard",
                //    employeeNo = visitor.VisitorNumber
                //};
                //foreach (var item in assignedDevices)
                //{
                //    var result = await _visionMachineService.AddCard(item.Device, vMCardInfo);

                //    if (!result.Success)
                //    {

                //    }
                //}
                context.IdentifiedAssignCardRepository.Add(identifiedAssignCard);

                await context.SaveChangesAsync();

                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/card/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Card.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadCardByKey)]
        public async Task<IActionResult> ReadCardByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var card = await context.CardRepository.GetCardByKey(key);

                if (card == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/card/inactive-cards
        /// </summary>
        /// <param name="key">Primary key of the table Card.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadAllInActiveCards)]
        public async Task<IActionResult> ReadAllInActiveCards()
        {
            try
            {

                var card = await context.CardRepository.GetAvailableCards();

                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL:api/cards
        /// </summary>
        /// <returns>A list of Cards.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadCards)]
        public async Task<IActionResult> ReadCards([FromQuery] PaginationDto paginationDto)
        {
            try
            {
                if (paginationDto.PageSize == 0)
                {
                    var cards = await context.CardRepository.GetCards();

                    return Ok(cards);
                }
                else
                {
                    int currentPage = paginationDto.Page;
                    paginationDto.Page = ((paginationDto.Page - 1) * (paginationDto.PageSize));
                    var cards = await context.CardRepository.GetCards(paginationDto);

                    PagedResultDto<Card> cardDto = new PagedResultDto<Card>()
                    {
                        Data = cards.ToList(),
                        PageNumber = currentPage,
                        PageSize = paginationDto.PageSize,
                        TotalItems = await context.CardRepository.GetCardCount(paginationDto)
                    };

                    return Ok(cardDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/card/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Card.</param>
        /// <param name="card">Card to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateCard)]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateCard(Guid key, CardDto cardDto)
        {
            try
            {
                if (key != cardDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var cardWithSameCardNumber = await context.CardRepository.GetCardByCardNumber(cardDto.CardNumber);

                if (cardWithSameCardNumber != null && cardWithSameCardNumber.OrganizationId == cardWithSameCardNumber.OrganizationId && cardWithSameCardNumber.Oid != cardDto.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.CardNumberTake);


                var cardInDb = await context.CardRepository.GetCardByKey(cardDto.Oid);

                if (cardInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                cardInDb.CardNumber = cardDto.CardNumber;
                cardInDb.DateModified = DateTime.Now;
                cardInDb.ModifiedBy = GetLoggedInUserId();

                context.CardRepository.Update(cardInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/Card/{key}
        /// </summary>
        /// <param name="key">Primary key of the table Card.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteCard)]
        public async Task<IActionResult> DeleteCard(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var cardInDb = await context.CardRepository.GetCardByKey(key);

                if (cardInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                var checkIfCardIsAssigned = await context.IdentifiedAssignCardRepository.FirstOrDefaultAsync(x => x.IsDeleted == false && x.CardId == key);

                if (checkIfCardIsAssigned == null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.CardCannotBeDeleted);

                cardInDb.DateModified = DateTime.Now;
                cardInDb.ModifiedBy = GetLoggedInUserId();
                cardInDb.IsDeleted = true;

                context.CardRepository.Update(cardInDb);
                await context.SaveChangesAsync();

                return Ok(cardInDb);
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
