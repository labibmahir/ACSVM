using Domain.Dto;
using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    class PersonRepository : Repository<Person>, IPersonRepository
    {
        public PersonRepository(DataContext context) : base(context)
        {
        }
        public async Task<Person> GetPersonByKey(Guid key)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.Oid == key);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Person>> GetPersons()
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<PersonReadDto>> GetPersons(PersonFilterDto personFilterDto)
        {
            try
            {
                var query = context.Persons.AsNoTracking().Include(i => i.PeopleImages).Include(f => f.FingerPrints)
                    .Include(x => x.IdentifiedAssignCards).Include(x => x.IdentifiedAssignDevices)
                    .Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(personFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(personFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(personFilterDto.search.ToLower().Trim())
                    || x.PersonNumber.ToLower().Contains(personFilterDto.search.ToLower().Trim()) ||
                    (x.PhoneNumber.ToLower().Contains(personFilterDto.search.ToLower().Trim())) || (x.Email.ToLower().Contains(personFilterDto.search))
                       );

                if (!string.IsNullOrEmpty(personFilterDto.FirstName))
                {
                    query = query.Where(x => x.FirstName.ToLower().Contains(personFilterDto.FirstName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.Surname))
                {
                    query = query.Where(x => x.Surname.ToLower().Contains(personFilterDto.Surname.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.PhoneNumber))
                {
                    query = query.Where(x => x.PhoneNumber.ToLower().Contains(personFilterDto.PhoneNumber.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.Email))
                {
                    query = query.Where(x => x.Email.ToLower().Contains(personFilterDto.Email.ToLower().Trim()));
                }

                if (personFilterDto.Gender.HasValue)
                {
                    query = query.Where(x => x.Gender == personFilterDto.Gender);
                }

                if (personFilterDto.IsDeviceAdministrator.HasValue)
                {
                    query = query.Where(x => x.IsDeviceAdministrator == personFilterDto.IsDeviceAdministrator);
                }

                if (personFilterDto.UserVerifyMode.HasValue)
                {
                    query = query.Where(x => x.UserVerifyMode == personFilterDto.UserVerifyMode);
                }


                if (personFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Select(x => new PersonReadDto()
                {
                    Email = x.Email,
                    FirstName = x.FirstName,
                    Surname = x.Surname,
                    Gender = x.Gender,
                    imageBase64 = x.PeopleImages.Any() ? x.PeopleImages.FirstOrDefault().ImageBase64 : null,
                    FingerPrints = x.FingerPrints.ToList(),
                    IsDeviceAdministrator = x.IsDeviceAdministrator,
                    Oid = x.Oid,
                    OrganizationId = x.OrganizationId,
                    PersonNumber = x.PersonNumber,
                    PhoneNumber = x.PhoneNumber,
                    UserVerifyMode = x.UserVerifyMode,
                    ValidateEndPeriod = x.ValidateEndPeriod,
                    ValidateStartPeriod = x.ValidateStartPeriod,
                    AssignedDevicesCount = x.IdentifiedAssignDevices.Count(),
                    AssignedCardCount = x.IdentifiedAssignCards.Count(),
                    FingerPrintCount = x.FingerPrints.Count(),
                    ImagesCount = x.PeopleImages.Count()

                }).Skip(personFilterDto.Page).Take(personFilterDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> GetPersonsCount(PersonFilterDto personFilterDto)
        {
            try
            {
                var query = context.Persons.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(personFilterDto.search))
                    query = query.Where(x => x.FirstName.ToLower().Contains(personFilterDto.search.ToLower().Trim()) || x.Surname.ToLower().Contains(personFilterDto.search.ToLower().Trim())
                    || x.PersonNumber.ToLower().Contains(personFilterDto.search.ToLower().Trim()) ||
                    (x.PhoneNumber.ToLower().Contains(personFilterDto.search.ToLower().Trim())) || (x.Email.ToLower().Contains(personFilterDto.search))
                       );

                if (!string.IsNullOrEmpty(personFilterDto.FirstName))
                {
                    query = query.Where(x => x.FirstName.ToLower().Contains(personFilterDto.FirstName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.Surname))
                {
                    query = query.Where(x => x.Surname.ToLower().Contains(personFilterDto.Surname.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.PhoneNumber))
                {
                    query = query.Where(x => x.PhoneNumber.ToLower().Contains(personFilterDto.PhoneNumber.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(personFilterDto.Email))
                {
                    query = query.Where(x => x.Email.ToLower().Contains(personFilterDto.Email.ToLower().Trim()));
                }

                if (personFilterDto.Gender.HasValue)
                {
                    query = query.Where(x => x.Gender == personFilterDto.Gender);
                }

                if (personFilterDto.IsDeviceAdministrator.HasValue)
                {
                    query = query.Where(x => x.IsDeviceAdministrator == personFilterDto.IsDeviceAdministrator);
                }

                if (personFilterDto.UserVerifyMode.HasValue)
                {
                    query = query.Where(x => x.UserVerifyMode == personFilterDto.UserVerifyMode);
                }


                if (personFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query.Skip(personFilterDto.Page).Take(personFilterDto.PageSize)
                  .CountAsync();

                return result;

            }
            catch
            {
                throw;
            }

        }
        public async Task<Person> GetPersonByPersonNumber(string personNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PersonNumber.ToLower().Trim() == personNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }
        }

        public async Task<Person> GetPersonByphoneNumber(string phoneNumber)
        {
            try
            {
                return await FirstOrDefaultAsync(x => x.IsDeleted == false && x.PhoneNumber.ToLower().Trim() == phoneNumber.ToLower().Trim());
            }
            catch
            {
                throw;
            }

        }
    }
}
