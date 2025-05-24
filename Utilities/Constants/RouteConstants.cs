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

        #region AccessLevel
        public const string CreateAccessLevel = "accesslevel";

        public const string ReadAccessLevels = "accesslevels";

        public const string ReadAccessLevelByKey = "accesslevel/key/{key}";

        public const string UpdateAccessLevel = "accesslevel/{key}";

        public const string DeleteAccessLevel = "accesslevel/{key}";
        #endregion


        #region Person
        public const string CreatePerson = "person";

        public const string ReadPersons = "persons";

        public const string ReadPersonByKey = "person/key/{key}";

        public const string UpdatePerson = "person/{key}";

        public const string DeletePerson = "person/{key}";
        public const string ImportPersonFromDevice = "person/importpersonfromdevice";
        #endregion

        #region Visitor
        public const string CreateVisitor = "visitor";

        public const string ReadVisitors = "visitors";

        public const string ReadVisitorByKey = "visitor/key/{key}";

        public const string UpdateVisitor = "visitor/{key}";

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
        public const string AssignCardToVisitor = "card-assign-visitor";

        public const string ReadCards = "cards";

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

        public const string ReadAppointmentByKey = "appointment/key/{key}";

        public const string UpdateAppointment = "appointment/{key}";

        public const string DeleteAppointment = "appointment/{key}";
        #endregion

        #region ClientDBDetail
        public const string CreateClientDBDetail = "clientdbdetail";

        public const string ReadClientDBDetails = "clientdbdetails";

        public const string ReadClientDBDetailByKey = "clientdbdetail/key/{key}";

        public const string UpdateClientDBDetail = "clientdbdetail/{key}";

        public const string DeleteClientDBDetail = "clientdbdetail/{key}";
        #endregion
    }
}
