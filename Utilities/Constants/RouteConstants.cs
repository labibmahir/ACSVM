using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Constants
{
    /// <summary>
    /// RouteConstants.
    /// </summary>
    public static class RouteConstants
    {        // MODULE
        #region UserAccount
        public const string CreateUserAccount = "user-account";

        public const string ReadUserAccounts = "user-accounts";

        public const string ReadUserAccountByKey = "user-account/key/{key}";

        public const string UpdateUserAccount = "user-account/{key}";

        public const string DeleteUserAccount = "user-account/{key}";

        public const string UserLogin = "user-account/login";


        public const string ChangedPassword = "user-account/change-password";

        #endregion

        #region Device
        public const string CreateDevice = "device";

        public const string ReadDevices = "devices";

        public const string ReadDeviceByKey = "device/key/{key}";

        public const string UpdateDevice = "device/{key}";

        public const string DeleteDevice = "device/{key}";
        #endregion
    }
}
