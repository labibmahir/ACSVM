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
        public const string DuplicateIPError = "The IP Address is already in used!";
        public const string InvalidDeviceId = "Please provide valid device Id;s";
        public const string DuplicateFingerError = "The FingerPrint is already added for this finger!";
        //USER PROFILE REGISTRATION:
        public const string NoMatchFoundError = "No match found!";
        public const string CardNotInService = "Card not in Service!";
        public const string NoImageFileProvided = "No match found!";
        public const string UsernameTaken = "Username already exists!";
        public const string DeviceNameTaken = "Device Name already exists!";
        public const string PersonNumberTaken = "Person Number already exists!";
        public const string PersonNotFound = "Person Not does not exists!";
        public const string VisitorNumberTaken = "Visitor Number already exists!";
        public const string CardNumberTake = "Card Number already exists!";
        public const string CardAlreadyAllocated = "Card is already allocated!";
        public const string CardCurrenlyActive = "Card is currently in use of visitor!";
        public const string DeviceCannotBeDeleted = "Device Cannot be deleted because its been assigned to some users!";
        public const string CardCannotBeDeleted = "Card Cannot be deleted because its been assigned to some users!";
        public const string InvalidParameterError = "Invalid parameter(s)!";
        public const string UnauthorizedAttemptOfRecordUpdateError = "Unauthorized attempt of updating record!";
        public const string DeviceNotFoundAccessLevelError = "No device is assigned or deleted to your selected AccessLevel";
        public const string DeviceNotActive = "Devices are currently not active";
        public const string SelectedDeviceNotActive = "Select Device is currently offline.";
        public const string InvalidLogin = "Invalid username or password!";
        public const string AlreadyHaveAppointment = "Already have appointment. Please cancel previous appointment to proceed";
        public const string UserNameNotExist = "UserName not exixts.";
        public const string InvalidRefreshToken = "Invalid Refresh Token.";
    }
}
