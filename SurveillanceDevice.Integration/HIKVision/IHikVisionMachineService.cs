using Domain.Dto.HIKVision;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveillanceDevice.Integration.HIKVision
{
    public interface IHikVisionMachineService
    {
        #region Person
        Task<int> GetUserCount(Device device);
        //Task<string> GetUserById(Device device, string Id);
        //Task<string> AddBulkUser(Device device, VMUserInfoBulk user);
        Task<string> AddUser(Device device, VMUserInfo user);

        Task<string> UpdateUser(Device device, VMUserInfo user);
        Task<string> DeleteUserWithDetails(Device device, VMUserInfoDetailsDeleteRequest req);
        #endregion Person

        #region Fingerprint
        Task<(bool IsSuccess, string Message)> SetFingerprint(Device device, VMFingerPrintSetUpRequest fingerInfo);

        #endregion FingerPrint

        #region Cards
        Task<(bool Success, string Message)> AddCard(Device device, VMCardInfo card);
        #endregion Cards
    }
}
