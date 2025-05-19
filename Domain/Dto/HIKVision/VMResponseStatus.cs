using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMResponseStatus
    {
        /// <summary>
        /// optional, string type, request URL
        /// </summary>
        public string? requestURL { get; set; }

        /// <summary>
        /// required, integer type, status code
        /// </summary>
        public int? statusCode { get; set; }

        /// <summary>
        /// required, string type, status description
        /// </summary>
        public string? statusString { get; set; }

        /// <summary>
        /// required, string type, sub status code
        /// </summary>
        public string? subStatusCode { get; set; }

        /// <summary>
        /// optional, integer type, error code, which corresponds to subStatusCode, this field is required when statusCode is not 1
        /// </summary>
        public int? errorCode { get; set; }

        /// <summary>
        /// optional, string type, error details, this field is required when statusCode is not 1
        /// </summary>
        public string? errorMsg { get; set; }

        /// <summary>
        /// optional, integer, number of retry attempts, it is returned when configuring card encryption
        /// </summary>
        public int? tryTimes { get; set; }
    }
}
