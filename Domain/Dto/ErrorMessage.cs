using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ErrorMessage
    {
        public int StatusCode { get; set; }
        public string StatusString { get; set; }
        public string SubStatusCode { get; set; }
        public long ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
    }
}
