using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Constants
{
    /// <summary>
    /// Error message constants.
    /// </summary>
    public static class MessageConstants
    {
        //COMMON:
        public const string GenericError = "Something went wrong! Please try after sometime. If you are experiencing similar problem frequently, please report it to helpdesk.";

        public const string DuplicateCellphoneError = "The cellphone number is associated with another user account!";
        //USER PROFILE REGISTRATION:
        public const string NoMatchFoundError = "No match found!";
        public const string UsernameTaken = "Username already exists!";
        public const string InvalidParameterError = "Invalid parameter(s)!";
        public const string UnauthorizedAttemptOfRecordUpdateError = "Unauthorized attempt of updating record!";
        public const string InvalidLogin = "Invalid username or password!";
    }
}
