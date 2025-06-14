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
        public const string ReadUserAccountAdvanceFilteration = "user-accounts-advance-filteration";

        public const string ReadUserAccountByKey = "user-account/key/{key}";

        public const string UpdateUserAccount = "user-account/{key}";

        public const string DeleteUserAccount = "user-account/{key}";

        public const string UserLogin = "user-account/login";

        public const string RefreshToken = "user-account/refresh-token";


        public const string ChangedPassword = "user-account/change-password";

        #endregion

        #region Device
        public const string CreateDevice = "device";

        public const string ReadDevices = "devices";
        public const string ReadAssignedDevicesByPerson = "assigned-devices-by-person/{personId}";
        public const string ReadUnAssignedDevicesByPerson = "unassigned-devices-by-person/{personId}";

        public const string ReadDeviceByKey = "device/key/{key}";

        public const string UpdateDevice = "device/{key}";
        public const string ActivateDevice = "device/activate/{key}";
        public const string DeActivateDevice = "device/deactivate/{key}";

        public const string DeleteDevice = "device/{key}";
        #endregion

        #region AccessLevel
        public const string CreateAccessLevel = "accesslevel";

        public const string ReadAccessLevels = "accesslevels";

        public const string ReadAccessLevelByKey = "accesslevel/key/{key}";

        public const string UpdateAccessLevel = "accesslevel/{key}";

        public const string DeleteAccessLevel = "accesslevel/{key}";
        #endregion


        #region Person
        public const string CreatePerson = "person";
        public const string CreatePersonFromExcelOrCSV = "person-import-from-excel-or-csv";
        public const string AssignedDeviceToPerson = "assign-device-to-person";
        public const string UnAssignedDeviceToPerson = "unassign-device-to-person";

        public const string ReadPersons = "persons";
        public const string ReadPersonsMinifiedData = "persons-minified-data";

        public const string ReadPersonByKey = "person/key/{key}";

        public const string UpdatePerson = "person/{key}";

        public const string DeletePerson = "person/{key}";
        public const string ImportPersonFromDevice = "person/importpersonfromdevice";
        public const string ExportPeopleToDevice = "person/export-people-to-device";
        #endregion

        #region Visitor
        public const string CreateVisitor = "visitor";

        public const string ReadVisitors = "visitors";

        public const string ReadVisitorByKey = "visitor/key/{key}";

        public const string ReadVisitorByPhone = "visitor/phone-number/{phoneNo}";

        public const string UpdateVisitor = "visitor/{key}";
        public const string AssignedDeviceToVisitor = "assigned-device-to-visitor";
        public const string UnAssignedDeviceToVisitor = "unassigned-device-to-visitor";

        public const string DeleteVisitor = "visitor/{key}";
        #endregion

        #region FingerPrint
        public const string CaptureFingerPrints = "capturefingerprint";
        public const string CreatePersonFingerPrint = "fingerprint-person";

        public const string CreateVisitorFingerPrint = "fingerprint-visitor";

        public const string ReadFingerPrints = "fingerprints";

        public const string ReadFingerPrintByKey = "fingerprint/key/{key}";

        public const string ReadFingerPrintByPerson = "fingerPrint/person/{PersonId}";
        public const string ReadFingerPrintByVisitor = "fingerPrint/visitor/{VisitorId}";

        public const string UpdateFingerPrint = "fingerprint/{key}";

        public const string DeleteFingerPrint = "fingerprint/{key}";
        #endregion

        #region Card
        public const string CreateCard = "card";

        public const string AssignCardToPerson = "card-assign-person";
        public const string UnAssignCardToPerson = "card-un-assign-person";
        public const string AssignCardToVisitor = "card-assign-visitor";
        public const string UnAssignCardToVisitor = "card-un-assign-visitor";

        public const string ReactivatedCard = "card-reactivated/{key}";

        public const string ReadCards = "cards";

        public const string ReadAllInActiveCards = "inactive-cards";

        public const string ReadCardByKey = "card/key/{key}";

        public const string UpdateCard = "card/{key}";

        public const string DeleteCard = "card/{key}";
        #endregion


        #region FingerPrint
        public const string CreatePersonImage = "person-image";

        public const string CreateVisitorImage = "visitor-image";

        public const string ReadPersonImages = "person-images";

        public const string ReadPersonImageByKey = "person-image/key/{key}";
        public const string ReadPersonImageByPersonId = "person-image/person/{PersonId}";
        public const string ReadPersonImageByVisitorId = "person-image/visitor/{VisitorId}";

        public const string UpdatePersonImage = "person-image/{key}";

        public const string DeletePersonImage = "person-image/{key}";
        #endregion


        #region Appointment
        public const string CreateAppointment = "appointment";

        public const string ReadAppointment = "appointments";

        public const string ReadLastAppointmentByVisitorNo = "last-appointment-by-visitor-number/{visitorNumber}";
        public const string ReadLastAppointmentByPhoneNo = "last-appointment-by-phone-number/{phoneNumber}";

        public const string ReadAppointmentByKey = "appointment/key/{key}";

        public const string UpdateAppointment = "appointment/{key}";
        public const string CancelAppointment = "appointment-cancel/{key}";
        public const string CompleteAppointment = "appointment-complete/{key}";

        public const string DeleteAppointment = "appointment/{key}";
        #endregion

        #region ClientDBDetail
        public const string CreateClientDBDetail = "clientdbdetail";

        public const string ReadClientDBDetails = "clientdbdetails";

        public const string ReadClientDBDetailByKey = "clientdbdetail/key/{key}";

        public const string UpdateClientDBDetail = "clientdbdetail/{key}";

        public const string DeleteClientDBDetail = "clientdbdetail/{key}";
        #endregion

        #region Attendance

        public const string ReadPersonAttendances = "person-attendances";

        public const string ReadAttendanceByPersonId = "attendance/person/{personId}";

        public const string ReadVisitorAttendances = "visitor-attendances";

        public const string ReadAttendanceByVisitorId = "attendance/visitor/{visitorId}";
        public const string DeletePersonAttendance = "attendance/person/delete";
        public const string DeleteVisitorAttendance = "attendance/visitor/delete";
        #endregion

        #region DeviceLog
        public const string ReadDeviceLogs = "device-logs";
        public const string ReadDeviceLogsByDeviceId = "device-logs/{deviceId}";
        public const string DeleteDeviceLogs = "device-logs-delete";

        #endregion

        #region VisitorLog
        public const string ReadVisitHistoryByVisitorId = "visitor-history-by-visitor/{visitorId}";
        public const string DeleteVisitorLog = "visitor-log-delete";

        #endregion
        #region Dashboard
        public const string ReadDashboardData = "dashboard-data"; 

        #endregion
    }
}
