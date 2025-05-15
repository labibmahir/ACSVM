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
        /// The method is used to get a person by personNumber.
        /// </summary>
        /// <param name="personNumber">personNumber of a person.</param>
        /// <returns>Returns a person if the personNumber is matched.
        public Task<Person> GetPersonByPersonNumber(string personNumber);

        /// <summary>
        /// The method is used to get a person by phoneNumber.
        /// </summary>
        /// <param name="phoneNumber">phoneNumber of a person.</param>
        /// <returns>Returns a person if the phoneNumber is matched.
        public Task<Person> GetPersonByphoneNumber(string phoneNumber);
    }
}
