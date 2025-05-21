using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class AcsEventResponse
    {
        [JsonPropertyName("AcsEvent")]
        public AcsEvent AcsEvent { get; set; }
    }

    public class AcsEvent
    {
        [JsonPropertyName("searchID")]
        public string SearchID { get; set; }

        [JsonPropertyName("responseStatusStrg")]
        public string ResponseStatus { get; set; }

        [JsonPropertyName("numOfMatches")]
        public int NumOfMatches { get; set; }

        [JsonPropertyName("totalMatches")]
        public int TotalMatches { get; set; }

        [JsonPropertyName("InfoList")]
        public List<Info> InfoList { get; set; }
    }

    public class Info
    {
        [JsonPropertyName("major")]
        public int Major { get; set; }

        [JsonPropertyName("minor")]
        public int Minor { get; set; }
        [JsonPropertyName("mask")]
        public string Mask { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset EventTime { get; set; }

        [JsonPropertyName("netUser")]
        public string NetUser { get; set; }

        [JsonPropertyName("remoteHostAddr")]
        public string RemoteHostAddr { get; set; }

        [JsonPropertyName("cardNo")]
        public string CardNo { get; set; }

        [JsonPropertyName("cardType")]
        public int? CardType { get; set; }

        [JsonPropertyName("doorNo")]
        public int? DoorNo { get; set; }

        [JsonPropertyName("cardReaderNo")]
        public int? CardReaderNo { get; set; }

        [JsonPropertyName("employeeNoString")]
        public string EmployeeNo { get; set; }

        [JsonPropertyName("userType")]
        public string UserType { get; set; }

        [JsonPropertyName("currentVerifyMode")]
        public string CurrentVerifyMode { get; set; }

        [JsonPropertyName("serialNo")]
        public int SerialNo { get; set; }

        [JsonPropertyName("MACAddr")]
        public string MACAddr { get; set; }

        [JsonPropertyName("picEnable")]
        public bool? PicEnable { get; set; }

        [JsonPropertyName("attendanceStatus")]
        public string AttendanceStatus { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("FaceRect")]
        public FaceRect FaceRect { get; set; }
    }
    public class FaceRect
    {
        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }
}
