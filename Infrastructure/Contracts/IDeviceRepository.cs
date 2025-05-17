using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IDeviceRepository : IRepository<Device>
    {

        /// <summary>
        /// The method is used to get a Device by key.
        /// </summary>
        /// <param name="key">Primary key of the table Device.</param>
        /// <returns>Returns a Deviceif the key is matched.</returns>
        public Task<Device> GetDeviceByKey(int key);

        /// <summary>
        /// The method is used to get a device by deviceName.
        /// </summary>
        /// <param name="deviceName">deviceName of a user.</param>
        /// <returns>Returns a device if the deviceName is matched.
        public Task<Device> GetDeviceByDeviceName(string deviceName);
        /// <summary>
        /// The method is used to get a Device by IP.
        /// </summary>
        /// <param name="DeviceIP">IP of a Device.</param>
        /// <returns>Returns a Device if the IP is matched.</returns>
        public Task<Device> GetDeviceByIP(string DeviceIP);


        /// <summary>
        /// The method is used to get the list of Devices.
        /// </summary>
        /// <returns>Returns a list of all Devoces.</returns>
        public Task<IEnumerable<Device>> GetDevices();
        /// <summary>
        /// The method is used to get the list of Devices by AccessLevel.
        /// </summary>
        /// <returns>Returns a Devices of AccessLevel.</returns>
        /// /// <param name="DeviceIP">IP of a Device.</param>
        /// <returns>Returns a Device if the AccessLevelId is matched.</returns>
        public Task<IEnumerable<Device>> GetDevicesByAccessLevel(int AccessLevelId);
        public Task<IEnumerable<Device>> GetDevices(DeviceFilterDto deviceFilterDto);
        public Task<int> GetDeviceCount(DeviceFilterDto deviceFilterDto);
    }
}
