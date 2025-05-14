using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IUserAccountRepository : IRepository<UserAccount>
    {
        /// <summary>
        /// The method is used to get a user account by username.
        /// </summary>
        /// <param name="username">Username of a user.</param>
        /// <returns>Returns a user account if the username is matched.
        public Task<UserAccount> GetUserAccountByUsername(string username);


        /// <summary>
        /// The method is used to get a user account by key.
        /// </summary>
        /// <param name="key">Primary key of the table UserAccounts.</param>
        /// <returns>Returns a user account if the key is matched.</returns>
        public Task<UserAccount> GetUserAccountByKey(Guid key);

        /// <summary>
        /// The method is used to get a user account by first name.
        /// </summary>
        /// <param name="firstName">First name of a user.</param>
        /// <returns>Returns a user account if the first name is matched.</returns>
        public Task<UserAccount> GetUserAccountByFirstName(string firstName);

        /// <summary>
        /// The method is used to get a user account by surname.
        /// </summary>
        /// <param name="surname">Surname of a user.</param>
        /// <returns>Returns a user account if the surname is matched.</returns>
        public Task<UserAccount> GetUserAccountBySurname(string surname);

        /// <summary>
        /// The method is used to get a user account by cellphone number.
        /// </summary>
        /// <param name="cellphone">Cellphone number of a user.</param>
        /// <returns>Returns a user account if the cellphone number is matched.</returns>
        public Task<UserAccount> GetUserAccountByCellphone(string cellphone);

        /// <summary>
        /// The method is used to get a user account by cellphone number.
        /// </summary>
        /// <param name="cellphone">Cellphone number of a user.</param>
        /// <param name="CountryCode">Cellphone number of a user.</param>
        /// <returns>Returns a user account if the cellphone number is matched.</returns>
        public Task<UserAccount> GetUserAccountByCellphoneNCountryCode(string cellphone, string CountryCode);
  

        /// <summary>
        /// The method is used to get the list of user accounts.
        /// </summary>
        /// <returns>Returns a list of all user accounts.</returns>
        public Task<IEnumerable<UserAccount>> GetUserAccounts();
        public Task<IEnumerable<UserAccount>> GetUserAccounts(UserAccountFilterDto userAccountFilterDto);
        public Task<int> GetUserAccountsCount(UserAccountFilterDto userAccountFilterDto);

    }
}
