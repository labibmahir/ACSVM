using Azure;
using Domain.Dto;
using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using Utilities.Constants;
using Utilities.Encryptions;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly IUnitOfWork context;
        private readonly ILogger<UserAccountController> logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context">Instance of the UnitOfWork.</param>
        public UserAccountController(IUnitOfWork context, ILogger<UserAccountController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// URL: api/user-account
        /// </summary>
        /// <param name="userAccount">UserAccountDto object.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpPost]
        [Route(RouteConstants.CreateUserAccount)]
        [AllowAnonymous]
        public async Task<ActionResult<UserAccount>> CreateUserAccount(UserAccount userAccount)
        {
            try
            {


                var userAccountWithSameUsername = await context.UserAccountRepository.GetUserAccountByUsername(userAccount.Username);

                if (userAccountWithSameUsername != null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UsernameTaken);

                if (userAccount.CountryCode == "+260" && userAccount.CellPhone[0] == '0')
                    userAccount.CellPhone = userAccount.CellPhone.Substring(1);

                var userAccountWithSameCellphone = await context.UserAccountRepository.GetUserAccountByCellphone(userAccount.CellPhone);

                if (userAccountWithSameCellphone != null)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.DuplicateCellphoneError);

                userAccount.Oid = Guid.NewGuid();
                userAccount.DateCreated = DateTime.Now;
                userAccount.IsAccountActive = true;
                userAccount.IsDeleted = false;

                EncryptionHelpers encryptionHelpers = new EncryptionHelpers();
                string encryptedPassword = encryptionHelpers.Encrypt(userAccount.Password);
                userAccount.Password = encryptedPassword;

                context.UserAccountRepository.Add(userAccount);
                await context.SaveChangesAsync();

                return Ok(userAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }
        /// <summary>
        /// URL: api/user-account/key/{key}
        /// </summary>
        /// <param name="key">Primary key of the table UserAccounts.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadUserAccountByKey)]
        [Authorize]
        public async Task<IActionResult> ReadUserAccountByKey(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var userAccount = await context.UserAccountRepository.GetUserAccountByKey(key);

                if (userAccount == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                return Ok(userAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL:api/user-accounts
        /// </summary>
        /// <returns>A list of user accounts.</returns>
        [HttpGet]
        [Route(RouteConstants.ReadUserAccounts)]
        [Authorize]
        public async Task<IActionResult> ReadUserAccount([FromQuery] UserAccountFilterDto userAccountFilterDto)
        {
            try
            {
                if (userAccountFilterDto.PageSize == 0)
                {
                    var users = await context.UserAccountRepository.GetUserAccounts();
                    return Ok(users);
                }
                else
                {
                    int currentPage = userAccountFilterDto.Page;
                    userAccountFilterDto.Page = ((userAccountFilterDto.Page - 1) * (userAccountFilterDto.PageSize));
                    var users = await context.UserAccountRepository.GetUserAccounts(userAccountFilterDto);
                    PagedResultDto<UserAccount> userAccountDto = new PagedResultDto<UserAccount>()
                    {
                        Data = users.ToList(),
                        PageNumber = currentPage,
                        PageSize = userAccountFilterDto.PageSize,
                        TotalItems = await context.UserAccountRepository.GetUserAccountsCount(userAccountFilterDto)
                    };

                    return Ok(userAccountDto);

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/user-account/{key}
        /// </summary>
        /// <param name="key">Primary key of the table UserAccounts.</param>
        /// <param name="userAccount">UserAccount to be updated.</param>
        /// <returns>Http status code: NoContent.</returns>
        [HttpPut]
        [Route(RouteConstants.UpdateUserAccount)]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUserAccount(Guid key, UserAccount userAccount)
        {
            try
            {
                if (key != userAccount.Oid)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.UnauthorizedAttemptOfRecordUpdateError);

                var userWithSameNRC = await context.UserAccountRepository.GetUserAccountByKey(key);


                var userInDb = await context.UserAccountRepository.GetUserAccountByKey(userAccount.Oid);

                userInDb.FirstName = userAccount.FirstName;
                userInDb.Surname = userAccount.Surname;
                userInDb.CountryCode = userAccount.CountryCode;
                userInDb.CellPhone = userAccount.CellPhone;
                userInDb.DateModified = userAccount.DateModified;
                userInDb.ModifiedBy = userAccount.ModifiedBy;
                userInDb.IsAccountActive = userAccount.IsAccountActive;
                userAccount.OrganizationId = userAccount.OrganizationId;

                context.UserAccountRepository.Update(userInDb);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }


        /// <summary>
        /// URL: api/user-account/{key}
        /// </summary>
        /// <param name="key">Primary key of the table UserAccounts.</param>
        /// <returns>Http status code: Ok.</returns>
        [HttpDelete]
        [Route(RouteConstants.DeleteUserAccount)]
        public async Task<IActionResult> DeleteUserAccount(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                    return StatusCode(StatusCodes.Status400BadRequest, MessageConstants.InvalidParameterError);

                var userAccountInDb = await context.UserAccountRepository.GetUserAccountByKey(key);

                if (userAccountInDb == null)
                    return StatusCode(StatusCodes.Status404NotFound, MessageConstants.NoMatchFoundError);

                userAccountInDb.DateModified = DateTime.Now;
                userAccountInDb.IsDeleted = true;

                context.UserAccountRepository.Update(userAccountInDb);
                await context.SaveChangesAsync();

                return Ok(userAccountInDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        /// <summary>
        /// URL: api/user-account/login
        /// Authenticates a user with the provided credentials and returns user details with a token.
        /// </summary>
        /// <param name="loginDto">The login credentials, including username and password.</param>
        /// <returns>A response containing user details and a token on successful login or an appropriate error response.</returns>
        [HttpPost]
        [Route(RouteConstants.UserLogin)]
        [AllowAnonymous]
        public async Task<IActionResult> UserLogin(LoginDto loginDto)
        {
            try
            {

                // Encrypt the provided password
                EncryptionHelpers encryptionHelpers = new EncryptionHelpers();
                string encryptedPassword = encryptionHelpers.Encrypt(loginDto.Password);

                // Retrieve the user account by username
                var user = await context.UserAccountRepository.GetUserAccountByUsername(loginDto.Username);

                // Validate username and password
                if (user == null)
                    return StatusCode(StatusCodes.Status401Unauthorized, MessageConstants.InvalidLogin);

                if (user.Password != encryptedPassword)
                    return StatusCode(StatusCodes.Status401Unauthorized, MessageConstants.InvalidLogin);


                string token = "";
                token = encryptionHelpers.Encrypt($"{user.Oid}||{user.Username}||{DateTime.UtcNow}")
                    ;
                // Prepare user details response with token
                UserLoginSuccessDto userDetailsDto = new UserLoginSuccessDto
                {
                    Oid = user.Oid,
                    Firstname = user.FirstName,
                    Surname = user.Surname,
                    Email = user.Email,
                    CountryCode = user.CountryCode,
                    CellPhone = user.CellPhone,
                    Username = user.Username,

                    //   Token = encryptionHelpers.Encrypt($"{user.Oid}||{user.Username}||{DateTime.UtcNow}||MobileApp")
                    Token = GenerateJwtToken(user)
                };

                return StatusCode(StatusCodes.Status202Accepted, userDetailsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, MessageConstants.GenericError);
            }
        }

        [HttpPut]
        [Route(RouteConstants.ChangedPassword)]
        public async Task<IActionResult> ChangedPassword(Guid id, UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                EncryptionHelpers encryptionHelpers = new EncryptionHelpers();
                string encryptedOldPassword = encryptionHelpers.Encrypt(updatePasswordDto.OldPassword);

                var userAccountInDb = await context.UserAccountRepository.FirstOrDefaultAsync(x =>
                    x.Oid == id &&
                    x.Password == encryptedOldPassword);

                if (userAccountInDb == null)
                    return BadRequest("UserAccount not found");

                string encryptedPassword = encryptionHelpers.Encrypt(updatePasswordDto.NewPassword);

                userAccountInDb.Password = encryptedPassword;
                userAccountInDb.IsDeleted = false;
                userAccountInDb.DateModified = DateTime.Now;

                context.UserAccountRepository.Update(userAccountInDb);
                await context.SaveChangesAsync();

                return Ok(userAccountInDb);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw ex;
            }
        }

        private string GenerateJwtToken(UserAccount user)
        {

            int tokenValidityInMinutes = int.Parse(_configuration["JwtSettings:TokenValidityInMinutes"]);
            string securityKey = _configuration["JwtSettings:SecurityKey"];

            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
            var tokenKey = Encoding.ASCII.GetBytes(securityKey);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.NameId,user.Oid.ToString()),
     new Claim(ClaimTypes.Role, user.RoleId.ToString())
         });

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityHandler.WriteToken(securityToken);

            return token;
        }

    }
}
