using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ErrorMessage
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("statusString")]
        public string StatusString { get; set; }

        [JsonPropertyName("subStatusCode")]
        public string SubStatusCode { get; set; }

        [JsonPropertyName("errorCode")]
        public long ErrorCode { get; set; }

        [JsonPropertyName("errorMsg")]
        public string ErrorMsg { get; set; }
    }
}
