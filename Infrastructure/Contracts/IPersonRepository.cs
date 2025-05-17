using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IPersonRepository : IRepository<Person>
    {
        /// <summary>
        /// The method is used to get a Person by key.
        /// </summary>
        /// <param name="key">Primary key of the table Person.</param>
        /// <returns>Returns a Person if the key is matched.</returns>
        public Task<Person> GetPersonByKey(Guid key);
        /// <summary>
        /// The method is used to get a person by personNumber.
        /// </summary>
        /// <param name="personNumber">personNumber of a person.</param>
        /// <returns>Returns a person if the personNumber is matched.
        /// 
        /// <summary>
        /// The method is used to get the list of Persons.
        /// </summary>
        /// <returns>Returns a list of all Persons.</returns>
        public Task<IEnumerable<Person>> GetPersons();
        public Task<IEnumerable<Person>> GetPersons(PersonFilterDto personFilterDto);
        public Task<int> GetPersonsCount(PersonFilterDto personFilterDto);
        public Task<Person> GetPersonByPersonNumber(string personNumber);

        /// <summary>
        /// The method is used to get a person by phoneNumber.
        /// </summary>
        /// <param name="phoneNumber">phoneNumber of a person.</param>
        /// <returns>Returns a person if the phoneNumber is matched.
        public Task<Person> GetPersonByphoneNumber(string phoneNumber);
    }
}
