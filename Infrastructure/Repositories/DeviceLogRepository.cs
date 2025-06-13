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
    public class DeviceLogRepository : Repository<DeviceLog>, IDeviceLogRepository
    {
        public DeviceLogRepository(DataContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DeviceLog>> GetDeviceLogsWithOutPagintion(DeviceLogFilterDto deviceLogFilterDto)
        {
            try
            {
                var query = context.DeviceLogs.Include(p => p.Person)
                    .Include(d => d.Device).Include(p => p.Visitor)
                    .Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(deviceLogFilterDto.search))
                    query = query.Where(x => x.Device.DeviceName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Device.DeviceLicence.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Device.DeviceIP.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || (x.Device.ModelName != null && x.Device.ModelName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                    || x.Person.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Person.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Visitor.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Visitor.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || (x.Device.SerialNumber != null && x.Device.SerialNumber.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())) || (x.Device.MacAddress != null && x.Device.MacAddress.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                       );

                if (deviceLogFilterDto.DeviceId.HasValue)
                    query = query.Where(x => x.DeviceId == deviceLogFilterDto.DeviceId.Value);

                if (deviceLogFilterDto.PersonId.HasValue && deviceLogFilterDto.PersonId != Guid.Empty)
                    query = query.Where(x => x.PersonId != null && x.PersonId == deviceLogFilterDto.PersonId.Value);

                if (deviceLogFilterDto.VisitorId.HasValue && deviceLogFilterDto.VisitorId != Guid.Empty)
                    query = query.Where(x => x.VisitorId != null && x.VisitorId == deviceLogFilterDto.VisitorId.Value);

                if (deviceLogFilterDto.IsSync.HasValue)
                    query = query.Where(x => x.IsSync != null && x.IsSync == deviceLogFilterDto.IsSync.Value);

                if (deviceLogFilterDto.StartDate.HasValue && deviceLogFilterDto.StartDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value >= deviceLogFilterDto.StartDate.Value);

                if (deviceLogFilterDto.EndDate.HasValue && deviceLogFilterDto.EndDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value <= deviceLogFilterDto.EndDate.Value);

                if (deviceLogFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                return await query.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceLog>> GetDeviceLogsWithPagintion(DeviceLogFilterDto deviceLogFilterDto)
        {
            try
            {
                var query = context.DeviceLogs.Include(p => p.Person)
                    .Include(d => d.Device).Include(p => p.Visitor)
                    .Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(deviceLogFilterDto.search))
                    query = query.Where(x => x.Device.DeviceName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Device.DeviceLicence.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Device.DeviceIP.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || (x.Device.ModelName != null && x.Device.ModelName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                    || x.Person.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Person.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Visitor.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Visitor.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || (x.Device.SerialNumber != null && x.Device.SerialNumber.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())) || (x.Device.MacAddress != null && x.Device.MacAddress.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                       );

                if (deviceLogFilterDto.DeviceId.HasValue)
                    query = query.Where(x => x.DeviceId == deviceLogFilterDto.DeviceId.Value);

                if (deviceLogFilterDto.PersonId.HasValue && deviceLogFilterDto.PersonId != Guid.Empty)
                    query = query.Where(x => x.PersonId != null && x.PersonId == deviceLogFilterDto.PersonId.Value);

                if (deviceLogFilterDto.VisitorId.HasValue && deviceLogFilterDto.VisitorId != Guid.Empty)
                    query = query.Where(x => x.VisitorId != null && x.VisitorId == deviceLogFilterDto.VisitorId.Value);

                if (deviceLogFilterDto.IsSync.HasValue)
                    query = query.Where(x => x.IsSync != null && x.IsSync == deviceLogFilterDto.IsSync.Value);

                if (deviceLogFilterDto.StartDate.HasValue && deviceLogFilterDto.StartDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value >= deviceLogFilterDto.StartDate.Value);

                if (deviceLogFilterDto.EndDate.HasValue && deviceLogFilterDto.EndDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value <= deviceLogFilterDto.EndDate.Value);

                if (deviceLogFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                return await query.Skip(deviceLogFilterDto.Page).Take(deviceLogFilterDto.PageSize).ToListAsync();
            }
            catch
            {
                throw;
            }
        }


        public async Task<int> GetDeviceLogCount(DeviceLogFilterDto deviceLogFilterDto)
        {
            try
            {
                var query = context.DeviceLogs.Include(p => p.Person)
                    .Include(d => d.Device).Include(p => p.Visitor)
                    .Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(deviceLogFilterDto.search))
                    query = query.Where(x => x.Device.DeviceName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Device.DeviceLicence.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Device.DeviceIP.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || (x.Device.ModelName != null && x.Device.ModelName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                    || x.Person.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Person.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || x.Visitor.FirstName.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()) || x.Visitor.Surname.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())
                    || (x.Device.SerialNumber != null && x.Device.SerialNumber.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim())) || (x.Device.MacAddress != null && x.Device.MacAddress.ToLower().Contains(deviceLogFilterDto.search.ToLower().Trim()))
                       );

                if (deviceLogFilterDto.DeviceId.HasValue)
                    query = query.Where(x => x.DeviceId == deviceLogFilterDto.DeviceId.Value);

                if (deviceLogFilterDto.PersonId.HasValue && deviceLogFilterDto.PersonId != Guid.Empty)
                    query = query.Where(x => x.PersonId != null && x.PersonId == deviceLogFilterDto.PersonId.Value);

                if (deviceLogFilterDto.VisitorId.HasValue && deviceLogFilterDto.VisitorId != Guid.Empty)
                    query = query.Where(x => x.VisitorId != null && x.VisitorId == deviceLogFilterDto.VisitorId.Value);

                if (deviceLogFilterDto.IsSync.HasValue)
                    query = query.Where(x => x.IsSync != null && x.IsSync == deviceLogFilterDto.IsSync.Value);

                if (deviceLogFilterDto.StartDate.HasValue && deviceLogFilterDto.StartDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value >= deviceLogFilterDto.StartDate.Value);

                if (deviceLogFilterDto.EndDate.HasValue && deviceLogFilterDto.EndDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value <= deviceLogFilterDto.EndDate.Value);


                return await query.CountAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceLog>> GetDeviceLogsByDeviceId(int deviceId, DeviceLogFilterByDeviceDto deviceLogFilterByDeviceDto)
        {
            try
            {
                var query = context.DeviceLogs.Include(p => p.Person)
                    .Include(d => d.Device).Include(p => p.Visitor)
                    .Where(i => i.IsDeleted == false && i.DeviceId == deviceId).AsQueryable();

                if (!string.IsNullOrEmpty(deviceLogFilterByDeviceDto.search))
                    query = query.Where(x => x.Device.DeviceName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Device.DeviceLicence.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || x.Device.DeviceIP.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || (x.Device.ModelName != null && x.Device.ModelName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()))
                    || x.Person.FirstName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Person.Surname.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || x.Visitor.FirstName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Visitor.Surname.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || (x.Device.SerialNumber != null && x.Device.SerialNumber.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())) || (x.Device.MacAddress != null && x.Device.MacAddress.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()))
                       );


                if (deviceLogFilterByDeviceDto.PersonId.HasValue && deviceLogFilterByDeviceDto.PersonId != Guid.Empty)
                    query = query.Where(x => x.PersonId != null && x.PersonId == deviceLogFilterByDeviceDto.PersonId.Value);

                if (deviceLogFilterByDeviceDto.VisitorId.HasValue && deviceLogFilterByDeviceDto.VisitorId != Guid.Empty)
                    query = query.Where(x => x.VisitorId != null && x.VisitorId == deviceLogFilterByDeviceDto.VisitorId.Value);

                if (deviceLogFilterByDeviceDto.IsSync.HasValue)
                    query = query.Where(x => x.IsSync != null && x.IsSync == deviceLogFilterByDeviceDto.IsSync.Value);

                if (deviceLogFilterByDeviceDto.StartDate.HasValue && deviceLogFilterByDeviceDto.StartDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value >= deviceLogFilterByDeviceDto.StartDate.Value);

                if (deviceLogFilterByDeviceDto.EndDate.HasValue && deviceLogFilterByDeviceDto.EndDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value <= deviceLogFilterByDeviceDto.EndDate.Value);

                if (deviceLogFilterByDeviceDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                if (deviceLogFilterByDeviceDto.PageSize > 0)
                    return await query.Skip(deviceLogFilterByDeviceDto.Page).Take(deviceLogFilterByDeviceDto.PageSize).ToListAsync();
                else
                    return await query.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetDeviceLogsByDeviceIdCount(int deviceId, DeviceLogFilterByDeviceDto deviceLogFilterByDeviceDto)
        {
            try
            {
                var query = context.DeviceLogs.Include(p => p.Person)
                    .Include(d => d.Device).Include(p => p.Visitor)
                    .Where(i => i.IsDeleted == false && i.DeviceId == deviceId).AsQueryable();

                if (!string.IsNullOrEmpty(deviceLogFilterByDeviceDto.search))
                    query = query.Where(x => x.Device.DeviceName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Device.DeviceLicence.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || x.Device.DeviceIP.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || (x.Device.ModelName != null && x.Device.ModelName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()))
                    || x.Person.FirstName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Person.Surname.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || x.Visitor.FirstName.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()) || x.Visitor.Surname.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())
                    || (x.Device.SerialNumber != null && x.Device.SerialNumber.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim())) || (x.Device.MacAddress != null && x.Device.MacAddress.ToLower().Contains(deviceLogFilterByDeviceDto.search.ToLower().Trim()))
                       );


                if (deviceLogFilterByDeviceDto.PersonId.HasValue && deviceLogFilterByDeviceDto.PersonId != Guid.Empty)
                    query = query.Where(x => x.PersonId != null && x.PersonId == deviceLogFilterByDeviceDto.PersonId.Value);

                if (deviceLogFilterByDeviceDto.VisitorId.HasValue && deviceLogFilterByDeviceDto.VisitorId != Guid.Empty)
                    query = query.Where(x => x.VisitorId != null && x.VisitorId == deviceLogFilterByDeviceDto.VisitorId.Value);

                if (deviceLogFilterByDeviceDto.IsSync.HasValue)
                    query = query.Where(x => x.IsSync != null && x.IsSync == deviceLogFilterByDeviceDto.IsSync.Value);

                if (deviceLogFilterByDeviceDto.StartDate.HasValue && deviceLogFilterByDeviceDto.StartDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value >= deviceLogFilterByDeviceDto.StartDate.Value);

                if (deviceLogFilterByDeviceDto.EndDate.HasValue && deviceLogFilterByDeviceDto.EndDate != DateTime.MinValue)
                    query = query.Where(x => x.DateCreated != null && x.DateCreated.Value <= deviceLogFilterByDeviceDto.EndDate.Value);


                return await query.CountAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceLog>> GetDeviceLogsBetweenDates(DateTime startDate, DateTime endDate, int? deviceId = null)
        {
            try
            {
                if (deviceId.HasValue && deviceId.Value > 0)
                    return await QueryAsync(x => x.DeviceId == deviceId.Value && x.DateCreated >= startDate && x.DateCreated <= endDate);
                else
                    return await QueryAsync(x =>x.DateCreated >= startDate && x.DateCreated <= endDate);
            }
            catch
            {
                throw;
            }
        }
    }
}
