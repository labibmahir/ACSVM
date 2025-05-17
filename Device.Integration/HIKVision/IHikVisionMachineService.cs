using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Dto;

namespace SurveillanceDevice.Integration.HIKVision
{
    public interface IHikVisionMachineService
    {
        Task<int> GetUserCount(Device device);
        Task<string> GetUserById(Device device, string Id);
        Task<string> AddBulkUser(Device device, VMUserInfoBulk user);
        Task<string> AddUser(Device device, VMUserInfo user);

    }
}
