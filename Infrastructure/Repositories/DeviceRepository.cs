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
    public class DeviceRepository : Repository<Device>, IDeviceRepository
    {
        public DeviceRepository(DataContext context) : base(context)
        {
        }
        /// <summary>
        /// The method is used to get a Device by key.
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Returns a Deviceif the key is matched.</returns>
        public async Task<Device> GetDeviceByKey(int key)
        {
            try
            {
                return await FirstOrDefaultAsync(d => d.Oid == key && d.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// The method is used to get a device by deviceName.
        /// </summary>
        /// <param name="deviceName">deviceName of a user.</param>
        /// <returns>Returns a device if the deviceName is matched.
        public async Task<Device> GetDeviceByDeviceName(string deviceName)
        {
            try
            {
                return await FirstOrDefaultAsync(u => u.DeviceName.ToLower().Trim() == deviceName.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// The method is used to get a Device by IP.
        /// </summary>
        /// <param name="DeviceIP">IP of a Device.</param>
        /// <returns>Returns a Device if the IP is matched.</returns>
        public async Task<Device> GetDeviceByIP(string DeviceIP)
        {
            try
            {
                return await FirstOrDefaultAsync(u => u.DeviceIP.ToLower().Trim() == DeviceIP.ToLower().Trim() && u.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<IEnumerable<Device>> GetDevices()
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Device>> GetDevicesByAccessLevel(int AccessLevelId)
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false && x.AccessLevelId == AccessLevelId);
            }
            catch
            {
                throw;
            }

        }
        public async Task<IEnumerable<Device>> GetDevicesByAccessLevels(int[] AccessLevelIds)
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false && x.AccessLevelId != null && AccessLevelIds.Contains(x.AccessLevelId.Value));
            }
            catch
            {
                throw;
            }

        }
        public async Task<IEnumerable<Device>> GetDevicesByDeviceIds(int[] DeviceIds)
        {
            try
            {
                return await QueryAsync(x => x.IsDeleted == false && DeviceIds.Contains(x.Oid));
            }
            catch
            {
                throw;
            }

        }
        public async Task<IEnumerable<DeviceReadDto>> GetDevices(DeviceFilterDto deviceFilterDto)
        {
            try
            {
                var query = context.Devices.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(deviceFilterDto.search))
                    query = query.Where(x => x.DeviceName.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()) || x.DeviceLicence.ToLower().Contains(deviceFilterDto.search.ToLower().Trim())
                    || x.DeviceIP.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()) || (x.ModelName != null && x.ModelName.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()))
                    || (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(deviceFilterDto.search.ToLower().Trim())) || (x.MacAddress != null && x.MacAddress.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()))
                       );


                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceName))
                    query = query.Where(x => x.DeviceName.ToLower().Contains(deviceFilterDto.DeviceName.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceLicence))
                    query = query.Where(x => x.DeviceLicence.ToLower().Contains(deviceFilterDto.DeviceLicence.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceIP))
                    query = query.Where(x => x.DeviceIP.ToLower().Contains(deviceFilterDto.DeviceIP.ToLower().Trim()));

                if (deviceFilterDto.IsActive.HasValue)
                    query = query.Where(x => x.IsActive == deviceFilterDto.IsActive);

                if (!string.IsNullOrEmpty(deviceFilterDto.FirmwareVersion))
                    query = query.Where(x => x.FirmwareVersion.ToLower() == deviceFilterDto.FirmwareVersion.ToLower().Trim());


                if (deviceFilterDto.FirmwareReleasedDate.HasValue && deviceFilterDto.FirmwareReleasedDate != DateTime.MinValue)
                    query = query.Where(x => x.FirmwareReleasedDate != null && x.FirmwareReleasedDate == deviceFilterDto.FirmwareReleasedDate);

                if (!string.IsNullOrEmpty(deviceFilterDto.ModelName))
                    query = query.Where(x => x.ModelName.ToLower() == deviceFilterDto.ModelName.ToLower().Trim());

                if (!string.IsNullOrEmpty(deviceFilterDto.SerialNumber))
                    query = query.Where(x => x.SerialNumber.ToLower().Contains(deviceFilterDto.SerialNumber.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.MacAddress))
                    query = query.Where(x => x.SerialNumber.ToLower().Contains(deviceFilterDto.MacAddress.ToLower().Trim()));


                if (deviceFilterDto.orderBy.ToLower().Trim() == "desc")
                    query = query.OrderByDescending(d => d.DateCreated);
                else
                    query = query.OrderBy(d => d.DateCreated);

                var result = await query
                    .Select(d => new DeviceReadDto()
                    {
                        AccessLevelId = d.AccessLevelId,
                        AccessLevelName = d.AccessLevel.Description,
                        DeviceIP = d.DeviceIP,
                        DeviceName = d.DeviceName,
                        DeviceLicence = d.DeviceLicence,
                        FirmwareReleasedDate = d.FirmwareReleasedDate,
                        FirmwareVersion = d.FirmwareVersion,
                        IsActive = d.IsActive,
                        MacAddress = d.MacAddress,
                        ModelName = d.ModelName,
                        Oid = d.Oid,
                        Password = d.Password,
                        Port = d.Port,
                        SerialNumber = d.SerialNumber,
                        Username = d.Username,

                    })
                    .Skip(deviceFilterDto.Page).Take(deviceFilterDto.PageSize)
                  .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> GetDeviceCount(DeviceFilterDto deviceFilterDto)
        {
            try
            {
                var query = context.Devices.Where(i => i.IsDeleted == false).AsQueryable();

                if (!string.IsNullOrEmpty(deviceFilterDto.search))
                    query = query.Where(x => x.DeviceName.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()) || x.DeviceLicence.ToLower().Contains(deviceFilterDto.search.ToLower().Trim())
                    || x.DeviceIP.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()) || (x.ModelName != null && x.ModelName.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()))
                    || (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(deviceFilterDto.search.ToLower().Trim())) || (x.MacAddress != null && x.MacAddress.ToLower().Contains(deviceFilterDto.search.ToLower().Trim()))
                       );


                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceName))
                    query = query.Where(x => x.DeviceName.ToLower().Contains(deviceFilterDto.DeviceName.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceLicence))
                    query = query.Where(x => x.DeviceLicence.ToLower().Contains(deviceFilterDto.DeviceLicence.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.DeviceIP))
                    query = query.Where(x => x.DeviceIP.ToLower().Contains(deviceFilterDto.DeviceIP.ToLower().Trim()));

                if (deviceFilterDto.IsActive.HasValue)
                    query = query.Where(x => x.IsActive == deviceFilterDto.IsActive);

                if (!string.IsNullOrEmpty(deviceFilterDto.FirmwareVersion))
                    query = query.Where(x => x.FirmwareVersion.ToLower() == deviceFilterDto.FirmwareVersion.ToLower().Trim());


                if (deviceFilterDto.FirmwareReleasedDate.HasValue && deviceFilterDto.FirmwareReleasedDate != DateTime.MinValue)
                    query = query.Where(x => x.FirmwareReleasedDate != null && x.FirmwareReleasedDate == deviceFilterDto.FirmwareReleasedDate);

                if (!string.IsNullOrEmpty(deviceFilterDto.ModelName))
                    query = query.Where(x => x.ModelName.ToLower() == deviceFilterDto.ModelName.ToLower().Trim());

                if (!string.IsNullOrEmpty(deviceFilterDto.SerialNumber))
                    query = query.Where(x => x.SerialNumber.ToLower().Contains(deviceFilterDto.SerialNumber.ToLower().Trim()));

                if (!string.IsNullOrEmpty(deviceFilterDto.MacAddress))
                    query = query.Where(x => x.SerialNumber.ToLower().Contains(deviceFilterDto.MacAddress.ToLower().Trim()));


                var result = await query
                  .CountAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
