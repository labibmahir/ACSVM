using Domain.Dto;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Utilities.Constants;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientDBDetailController : ApiBaseController
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<ClientDBDetailController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>

        public ClientDBDetailController(IUnitOfWork context, ILogger<ClientDBDetailController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }



        /// <summary>
        /// URL: api/clientdbdetail
        /// </summary>
        /// <param name="ClientDBDetail">ClientDBDetail object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateClientDBDetail)]
        public async Task<ActionResult<ClientDBDetail>> CreateAccessLevel(ClientDBDto dto)
        {
            try
            {


                try
                {
                    var test = context.ClientDBDetailRepository.GetAll().Where(t => t.IsConnectionActive == true);
                    if (test == null) return NotFound("The specified client db details does not exist");

                    foreach (var item in test)
                    {
                        item.IsConnectionActive = false;

                        context.ClientDBDetailRepository.Update(item);
                    }

                    await context.SaveChangesAsync();

                    var clientDBDetail = new ClientDBDetail
                    {
                        DatabaseType = dto.DatabaseType,
                        ServerName = dto.ServerName,
                        DatabaseName = dto.DatabaseName,
                        ServiceName = dto.ServiceName,
                        Role = dto.Role,
                        ConnectionMode = dto.ConnectionMode,
                        IsConnectionActive = true,
                        UserId = dto.UserId,
                        Password = dto.Password,
                        Port = dto.Port
                    };

                    var addClientDB = context.ClientDBDetailRepository.Add(clientDBDetail);

                    await context.SaveChangesAsync();

                    var clientMappedFields = context.ClientFieldMappingRepository.GetAll();

                    if (dto.ClientMappings != null && dto.ClientMappings.Any())
                    {
                        foreach (var item in dto.ClientMappings)
                        {
                            var clientMapping = new ClientFieldMapping
                            {
                                Oid = item.Oid,
                                TableName = item.TableName,
                                StandardField = item.StandardField,
                                ClientField = item.ClientField,
                                FormatType = item.FormatType,
                                ClientDBDetailId = addClientDB.Oid
                            };

                            item.ClientDBDetailId = addClientDB.Oid;
                            context.ClientFieldMappingRepository.Update(clientMapping);
                        }

                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        foreach (var item in clientMappedFields)
                        {
                            var clientMapping = new ClientFieldMapping
                            {
                                Oid = item.Oid,
                                TableName = item.TableName,
                                StandardField = item.StandardField,
                                ClientField = item.ClientField,
                                FormatType = item.FormatType,
                                ClientDBDetailId = addClientDB.Oid
                            };

                            item.ClientDBDetailId = addClientDB.Oid;
                            context.ClientFieldMappingRepository.Update(clientMapping);
                        }

                        await context.SaveChangesAsync();
                    }

                    var clientInDB = await context.ClientDBDetailRepository.FirstOrDefaultAsync(x => x.Oid == addClientDB.Oid);
                    var clientDbFieldMapping = await context.ClientFieldMappingRepository.QueryAsync(x => x.ClientDBDetailId == clientInDB.Oid);
                    clientInDB.ClientFieldMappings = clientDbFieldMapping;

                    var clientInDBDetail = clientInDB.ClientFieldMappings.Select(x => new ClientDBDto
                    {
                        Id = x.Oid,
                        DatabaseType = clientInDB.DatabaseType,
                        ServerName = clientInDB.ServerName,
                        DatabaseName = clientInDB.DatabaseName,
                        ServiceName = clientInDB.ServiceName,
                        Role = clientInDB.Role,
                        ConnectionMode = clientInDB.ConnectionMode,
                        IsConnectionActive = clientInDB.IsConnectionActive,
                        UserId = clientInDB.UserId,
                        Password = clientInDB.Password,
                        Port = clientInDB.Port,
                        ClientMappings = clientInDB.ClientFieldMappings.Any() ? clientInDB.ClientFieldMappings
                                .Select(cm => new ClientMappingDto
                                {
                                    Oid = cm.Oid,
                                    TableName = cm.TableName,
                                    StandardField = cm.StandardField,
                                    ClientField = cm.ClientField,
                                    FormatType = cm.FormatType,
                                    ClientDBDetailId = cm.ClientDBDetailId
                                }).ToList()
                                : clientMappedFields.Select(cmf => new ClientMappingDto
                                {
                                    Oid = cmf.Oid,
                                    TableName = cmf.TableName,
                                    StandardField = cmf.StandardField,
                                    ClientField = cmf.ClientField,
                                    FormatType = cmf.FormatType,
                                    ClientDBDetailId = cmf.ClientDBDetailId
                                }).ToList()
                    }).ToList();

                    return Ok(clientInDBDetail);
                }
                catch (Exception ex)
                {
                    //logger.LogError(ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Could not add the client db details");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        [HttpGet]
        [Route(RouteConstants.ReadClientDBDetailByKey)]

        public async Task<IActionResult> ReadClientDBDetailByKey(int key)
        {
            try
            {
                var clientInDB = await context.ClientDBDetailRepository.FirstOrDefaultAsync(x => x.OrganizationId == key && x.IsConnectionActive == true && x.IsDeleted == false);
                return Ok(clientInDB);
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not get the client info");
            }
        }

        [HttpGet]
        [Route(RouteConstants.ReadClientDBDetails)]

        public async Task<IActionResult> ReadClientDBDetails(int key)
        {
            try
            {
                var clientInDB = await context.ClientDBDetailRepository.QueryAsync(x => x.IsDeleted == false);
                return Ok(clientInDB);
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not get the client info");
            }
        }

    }
}
