using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMUserInfo
    {
        /// <summary>
        /// Primary key of the table Users.
        /// </summary>
        public string employeeNo { get; set; }

        /// <summary>
        /// optional, boolean, whether to delete the person: "true"-yes. This node is required only when the person needs to
        /// be deleted; for adding or editing person information, this node can be set to NULL
        /// </summary>
        public bool? deleteUser { get; set; } = null;

        /// <summary>
        /// Name of the User.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// required, string, person type: "normal"-normal person (household), "visitor", "blackList"-person in blacklist
        /// </summary>
        public string userType { get; set; }

        /// <summary>
        /// optional, boolean, whether to enable door close delay: "true"-yes, "false"-no
        /// </summary>
        [JsonIgnore]
        public bool? closeDelayEnabled { get; set; } = false;

        /// <summary>
        /// required, parameters of the effective period, the effective period can be a period of time between 1970-01-01
        /// 00:00:00 and 2037-12-31 23:59:59
        /// </summary>
        public VMEffectivePeriod Valid { get; set; }

        /// <summary>
        /// optional, string, group
        /// </summary>
        [JsonIgnore]
        public string? belongGroup { get; set; } = string.Empty;

        /// <summary>
        /// optional, string, password
        /// </summary>
        [JsonIgnore]
        public string? password { get; set; } = string.Empty;

        /// <summary>
        /// optional, string, No. of the door or lock that has access permission, e.g., "1,3" indicates having permission to access
        /// door(lock) No. 1 and No. 3
        /// </summary>
        public string? doorRight { get; set; } = string.Empty;

        /// <summary>
        /// optional, door permission schedule (lock permission schedule)
        /// </summary>
        public List<VMDoorPermissionSchedule?> RightPlan { get; set; } = new List<VMDoorPermissionSchedule?>();

        /// <summary>
        /// optional, integer, maximum authentication attempts, 0-unlimited
        /// </summary>
        [JsonIgnore]
        public int? maxOpenDoorTime { get; set; } = 0;

        /// <summary>
        /// optional, integer, read-only, authenticated attempts
        /// </summary>
        [JsonIgnore]
        public int? openDoorTime { get; set; } = 0;

        /// <summary>
        /// optional, integer, room No.
        /// </summary>
        [JsonIgnore]
        public int? roomNumber { get; set; } = 0;

        /// <summary>
        /// optional, integer, floor No.
        /// </summary>
        [JsonIgnore]
        public int? floorNumber { get; set; } = 0;

        /// <summary>
        /// optional, boolean, whether to have the permission to open the double-locked door: "true"-yes, "false"-no
        /// </summary>
        [JsonIgnore]
        public bool? doubleLockRight { get; set; } = false;

        /// <summary>
        /// optional, boolean, whether to have the permission to access the device local UI: "true"-yes, "false"-no
        /// </summary>
        public bool? localUIRight { get; set; } = false;

        /// <summary>
        /// optional, string, person authentication mode: "cardAndPw"-card+password, "card"-card, "cardOrPw"-card or
        /// password, "fp"-fingerprint, "fpAndPw"-fingerprint+password, "fpOrCard"-fingerprint or card, "fpAndCard"-fingerprint
        /// +card, "fpAndCardAndPw"-fingerprint+card+password, "faceOrFpOrCardOrPw"-face or fingerprint or card or
        /// password, "faceAndFp"-face+fingerprint, "faceAndPw"-face+password, "faceAndCard"-face+card, "face"-face,
        /// "employeeNoAndPw"-employee No.+password, "fpOrPw"-fingerprint or password, "employeeNoAndFp"-employee
        /// No.+fingerprint, "employeeNoAndFpAndPw"-employee No.+fingerprint+password, "faceAndFpAndCard"-face
        /// +fingerprint+card, "faceAndPwAndFp"-face+password+fingerprint, "employeeNoAndFace"-employee No.+face,
        /// "faceOrfaceAndCard"-face or face+card, "fpOrface"-fingerprint or face, "cardOrfaceOrPw"-card or face or password,
        /// "cardOrFace"-card or face, "cardOrFaceOrFp"-card or face or fingerprint, "cardOrFpOrPw"-card or fingerprint or
        /// password.The priority of the person authentication mode is higher than that of the card reader authentication
        /// mode
        /// </summary>
        public string? userVerifyMode { get; set; }

        /// <summary>
        /// optional, boolean, whether to verify the duplicated person information: "false"-no, "true"-yes. If checkUser is not
        /// configured, the device will verify the duplicated person information by default. When there is no person information,
        /// you can set checkUser to "false" to speed up data applying; otherwise, it is not recommended to configure this node
        /// </summary>
        [JsonIgnore]
        public bool? checkUser { get; set; } = true;

        /// <summary>
        /// optional, boolean type, whether to add the person if the person information being edited does not exist: "false"-no
        /// (if the device has checked that the person information being edited does not exist, the failure response message will
        /// be returned along with the error code), "true"-yes(if the device has checked that the person information being edited
        /// does not exist, the success response message will be returned, and the person will be added). If this node is not
        /// configured, the person will not be added by default
        /// </summary>
        [JsonIgnore]
        public bool? addUser { get; set; } = true; // Must be set in the API Call 

        /// <summary>
        /// optional, string type, room No. list to be called, by default, its format is X-X-X-X (e.g., 1-1-1-401), which is extended
        /// from roomNumber; for standard SIP, it can be the SIP number
        /// </summary>
        [JsonIgnore]
        public List<string>? callNumbers { get; set; } = null;

        /// <summary>
        /// optional, integer type, floor No. list, which is extended from floorNumber
        /// </summary>
        [JsonIgnore]
        public List<int>? floorNumbers { get; set; } = null;

        /// <summary>
        /// optional, read-only, number of linked face pictures. If this field is not returned, it indicates that this function is not
        /// supported
        /// </summary>
        public int? numOfFace { get; set; } = null;

        /// <summary>
        /// optional, read-only, number of linked fingerprints. If this field is not returned, it indicates that this function is not
        /// supported
        /// </summary>
        [JsonIgnore]
        public int? numOfFP { get; set; } = null;

        /// <summary>
        /// optional, read-only, number of linked cards. If this field is not returned, it indicates that this function is not
        // supported
        /// </summary>
        [JsonIgnore]
        public int? numOfCard { get; set; } = null;

        public int? numOfRemoteControl { get; set; } = null;

        /// <summary>
        /// optional, string, gender of the person in the face picture: "male", "female", "unknown"
        /// </summary>
        public string? gender { get; set; } = null;
        public string? faceURL { get; set; } = null;

        public int? sortByNamePosition { get; set; } = null;

        public string? sortByNameFlag { get; set; } = null;

        /// <summary>
        /// optional, person extension information
        /// </summary>
        [JsonIgnore]
        public List<VMPersonInfoExtends?> PersonInfoExtends { get; set; } = new List<VMPersonInfoExtends?>();
    }
}
