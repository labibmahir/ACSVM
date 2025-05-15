using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IAccessLevelRepository : IRepository<AccessLevel>
    {

        /// <summary>
        /// The method is used to get a AccessLevel by key.
        /// </summary>
        /// <param name="key">Primary key of the table AccessLevel.</param>
        /// <returns>Returns a AccessLevel if the key is matched.</returns>
        public Task<AccessLevel> GetAccessLevelByKey(int key);

        /// <summary>
        /// The method is used to get a AccessLevel by description.
        /// </summary>
        /// <param name="description">description of a AccessLevel.</param>
        /// <returns>Returns a AccessLevel if the description is matched.
        public Task<AccessLevel> GetAccessLevelByDescription(string description);



        /// <summary>
        /// The method is used to get the list of AccessLevel.
        /// </summary>
        /// <returns>Returns a list of all AccessLevel.</returns>
        public Task<IEnumerable<AccessLevel>> GetAccessLevels();

        public Task<IEnumerable<AccessLevel>> GetAccessLevels(PaginationDto paginationDto);
        public Task<int> GetAccessLevelsCount(PaginationDto paginationDto);
    }
}
