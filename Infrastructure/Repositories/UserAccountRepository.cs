using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IUserAccountRepository interface.
    /// </summary>
    public class UserAccountRepository : Repository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(DataContext context) : base(context)
        {

        }

        /// <summary>
        /// The method is used to get a user account by username.
        /// </summary>
        /// <param name="UserName">Username of a user.</param>
        /// <returns>Returns a user account if the username is matched.
        public async Task<UserAccount> GetUserAccountByUsername(string UserName)
        {
            try
            {
                return await LoadWithChildAsync<UserAccount>(u => u.Username.ToLower().Trim() == UserName.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a user account by key.
        /// </summary>
        /// <param name="key">Primary key of the table UserAccounts.</param>
        /// <returns>Returns a user account if the key is matched.</returns>
        public async Task<UserAccount> GetUserAccountByKey(Guid key)
        {
            try
            {
                return await FirstOrDefaultAsync(u => u.Oid == key && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a user account by first name.
        /// </summary>
        /// <param name="firstName">First name of a user.</param>
        /// <returns>Returns a user account if the first name is matched.</returns>
        public async Task<UserAccount> GetUserAccountByFirstName(string firstName)
        {
            try
            {
                return await FirstOrDefaultAsync(u => u.FirstName.ToLower().Trim() == firstName.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a user account by surname.
        /// </summary>
        /// <param name="surname">Surname of a user.</param>
        /// <returns>Returns a user account if the surname is matched.</returns>
        public async Task<UserAccount> GetUserAccountBySurname(string surname)
        {
            try
            {
                return await FirstOrDefaultAsync(u => u.Surname.ToLower().Trim() == surname.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a user account by cellphone number.
        /// </summary>
        /// <param name="cellphone">Cellphone number of a user.</param>
        /// <returns>Returns a user account if the cellphone number is matched.</returns>
        public async Task<UserAccount> GetUserAccountByCellphone(string cellphone)
        {
            try
            {
                return await LoadWithChildAsync<UserAccount>(u => u.CellPhone.ToLower().Trim() == cellphone.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a user account by cellphone number and Country Code.
        /// </summary>
        /// <param name="cellphone">Cellphone number of a user.</param>
        /// <param name="CountryCode">Country Code of a user.</param>
        /// <returns>Returns a user account if the cellphone number and Country Code is matched.</returns>
        public async Task<UserAccount> GetUserAccountByCellphoneNCountryCode(string cellphone, string CountryCode)
        {
            try
            {
                return await LoadWithChildAsync<UserAccount>(u => u.CellPhone.ToLower().Trim() == cellphone.ToLower().Trim() && u.CountryCode.ToLower().Trim() == CountryCode.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get the list of user accounts.
        /// </summary>
        /// <returns>Returns a list of all user accounts.</returns>
        public async Task<IEnumerable<UserAccount>> GetUserAccounts()
        {
            try
            {
                return await QueryAsync(u => u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a paginated list of user accounts.
        /// </summary>
        /// <param name="page">The current page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>Returns a paginated list of user accounts.</returns>
        public async Task<IEnumerable<UserAccount>> GetUserAccounts(UserAccountFilterDto userAccountFilterDto)
        {
            try
            {
                var query = context.UserAccounts.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(userAccountFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                    || x.Email.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                    || x.CellPhone.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                       );


                if (!string.IsNullOrEmpty(userAccountFilterDto.Firstname))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountFilterDto.Firstname.ToLower().Trim()));

                if (!string.IsNullOrEmpty(userAccountFilterDto.Surname))
                    query = query.Where(x => x.Surname.ToLower().Contains(userAccountFilterDto.Surname.ToLower().Trim()));

                if (!string.IsNullOrEmpty(userAccountFilterDto.Email))
                    query = query.Where(x => x.Email.ToLower() == userAccountFilterDto.Email.ToLower().Trim());


                if (!string.IsNullOrEmpty(userAccountFilterDto.CellPhone))
                    query = query.Where(x => x.CellPhone.ToLower() == userAccountFilterDto.CellPhone.ToLower().Trim());

                if (!string.IsNullOrEmpty(userAccountFilterDto.CountryCode))
                    query = query.Where(x => x.CountryCode.ToLower() == userAccountFilterDto.CountryCode.ToLower().Trim());


                if (userAccountFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(userAccountFilterDto.Page).Take(userAccountFilterDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> GetUserAccountsCount(UserAccountFilterDto userAccountFilterDto)
        {
            try
            {
                var query = context.UserAccounts.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(userAccountFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                    || x.Email.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                    || x.CellPhone.ToLower().Contains(userAccountFilterDto.search.ToLower().Trim())
                       );


                if (!string.IsNullOrEmpty(userAccountFilterDto.Firstname))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountFilterDto.Firstname.ToLower().Trim()));

                if (!string.IsNullOrEmpty(userAccountFilterDto.Surname))
                    query = query.Where(x => x.Surname.ToLower().Contains(userAccountFilterDto.Surname.ToLower().Trim()));

                if (!string.IsNullOrEmpty(userAccountFilterDto.Email))
                    query = query.Where(x => x.Email.ToLower() == userAccountFilterDto.Email.ToLower().Trim());


                if (!string.IsNullOrEmpty(userAccountFilterDto.CellPhone))
                    query = query.Where(x => x.CellPhone.ToLower() == userAccountFilterDto.CellPhone.ToLower().Trim());

                if (!string.IsNullOrEmpty(userAccountFilterDto.CountryCode))
                    query = query.Where(x => x.CountryCode.ToLower() == userAccountFilterDto.CountryCode.ToLower().Trim());

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<UserAccount>> GetUserAccounts(UserAccountAdvanceFilterDto userAccountAdvanceFilter)
        {
            try
            {
                var query = context.UserAccounts.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.Email.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.CellPhone.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                       );


                //if (!string.IsNullOrEmpty(userAccountAdvanceFilter.FullName))
                //    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountAdvanceFilter.FullName.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountAdvanceFilter.FullName.ToLower().Trim()));
                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.FullName))
                {
                    var fullNameSearch = userAccountAdvanceFilter.FullName.ToLower().Trim();
                    query = query.Where(x =>
                        (x.FirstName + " " + x.Surname).ToLower().Contains(fullNameSearch) ||
                        x.FirstName.ToLower().Contains(fullNameSearch) ||
                        x.Surname.ToLower().Contains(fullNameSearch));
                }


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.Email))
                    query = query.Where(x => x.Email.ToLower() == userAccountAdvanceFilter.Email.ToLower().Trim());


                //if (!string.IsNullOrEmpty(userAccountAdvanceFilter.CellPhoneAndCountryCode))
                //    query = query.Where(x => userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CountryCode + x.CellPhone.ToLower())
                //    || userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CellPhone)
                //    ); 
                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.CellPhoneAndCountryCode))
                {
                    var input = userAccountAdvanceFilter.CellPhoneAndCountryCode.ToLower().Trim();
                    query = query.Where(x =>
                        (x.CountryCode + x.CellPhone).ToLower().Contains(input) ||
                        x.CellPhone.ToLower().Contains(input)
                    );
                }



                if (userAccountAdvanceFilter.RoleId.HasValue && userAccountAdvanceFilter.RoleId != 0)
                    query = query.Where(x => x.RoleId == userAccountAdvanceFilter.RoleId.Value);


                if (userAccountAdvanceFilter.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(userAccountAdvanceFilter.Page).Take(userAccountAdvanceFilter.PageSize)
                  .ToListAsync();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> GetUserAccountsCount(UserAccountAdvanceFilterDto userAccountAdvanceFilter)
        {
            try
            {
                var query = context.UserAccounts.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.Email.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.CellPhone.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                       );


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.FullName))
                {
                    var fullName = userAccountAdvanceFilter.FullName.ToLower().Trim();
                    
                    query = query.Where(x => (x.FirstName + " " + x.Surname).ToLower().Trim() == fullName || 
                                             x.FirstName.ToLower().Trim() == fullName ||
                                             x.Surname.ToLower().Trim() == fullName);
                }


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.Email))
                    query = query.Where(x => x.Email.ToLower() == userAccountAdvanceFilter.Email.ToLower().Trim());


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.CellPhoneAndCountryCode))
                    query = query.Where(x => userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CountryCode + x.CellPhone.ToLower())
                    || userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CellPhone)
                    );

                if (userAccountAdvanceFilter.RoleId.HasValue && userAccountAdvanceFilter.RoleId != 0)
                    query = query.Where(x => x.RoleId == userAccountAdvanceFilter.RoleId.Value);


                if (userAccountAdvanceFilter.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query
                  .CountAsync();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<IEnumerable<UserAccount>> GetUserAccountsWithOutPagging(UserAccountAdvanceFilterDto userAccountAdvanceFilter)
        {
            try
            {
                var query = context.UserAccounts.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.Email.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                    || x.CellPhone.ToLower().Contains(userAccountAdvanceFilter.search.ToLower().Trim())
                       );


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.FullName))
                    query = query.Where(x => x.FirstName.ToLower().Contains(userAccountAdvanceFilter.FullName.ToLower().Trim()) || x.Surname.ToLower().Contains(userAccountAdvanceFilter.FullName.ToLower().Trim()));


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.Email))
                    query = query.Where(x => x.Email.ToLower() == userAccountAdvanceFilter.Email.ToLower().Trim());


                if (!string.IsNullOrEmpty(userAccountAdvanceFilter.CellPhoneAndCountryCode))
                    query = query.Where(x => userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CountryCode + x.CellPhone.ToLower())
                    || userAccountAdvanceFilter.CellPhoneAndCountryCode.Contains(x.CellPhone)
                    );

                if (userAccountAdvanceFilter.RoleId.HasValue && userAccountAdvanceFilter.RoleId != 0)
                    query = query.Where(x => x.RoleId == userAccountAdvanceFilter.RoleId.Value);


                if (userAccountAdvanceFilter.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query
                  .ToListAsync();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
