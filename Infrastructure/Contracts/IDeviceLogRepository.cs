using Domain.Dto.PaginationFiltersDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    public interface IDeviceLogRepository : IRepository<DeviceLog>
    {
        public Task<IEnumerable<DeviceLog>> GetDeviceLogsWithOutPagintion(DeviceLogFilterDto deviceLogFilterDto);
        public Task<IEnumerable<DeviceLog>> GetDeviceLogsWithPagintion(DeviceLogFilterDto deviceLogFilterDto);
        public Task<int> GetDeviceLogCount(DeviceLogFilterDto deviceLogFilterDto);

        public Task<IEnumerable<DeviceLog>> GetDeviceLogsByDeviceId(int deviceId, DeviceLogFilterByDeviceDto deviceLogFilterByDeviceDto);
        public Task<int> GetDeviceLogsByDeviceIdCount(int deviceId, DeviceLogFilterByDeviceDto deviceLogFilterByDeviceDto);
        public Task<IEnumerable<DeviceLog>> GetDeviceLogsBetweenDates(DateTime startDate, DateTime endDate, int? deviceId = null);
    }
}
