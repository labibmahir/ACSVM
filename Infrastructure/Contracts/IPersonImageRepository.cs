using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Infrastructure.Contracts
{
    public interface IPersonImageRepository : IRepository<PersonImage>
    {

        /// <summary>
        /// The method is used to get a PersonImage by key.
        /// </summary>
        /// <param name="key">Primary key of the table PersonImage.</param>
        /// <returns>Returns a PersonImage if the key is matched.</returns>
        public Task<PersonImage> GetPersonImageByKey(Guid key);
        public Task<PersonImage> GetImageByPersonId(Guid personId);
        public Task<PersonImage> GetImageByVisitorId(Guid visitorId);

    }
}
