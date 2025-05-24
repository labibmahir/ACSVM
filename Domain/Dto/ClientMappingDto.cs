using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ClientMappingDto
    {
        public int Oid { get; set; }
        public string? TableName { get; set; }
        public string? StandardField { get; set; }
        public string? ClientField { get; set; }
        public int? FormatType { get; set; }
        public int? ClientDBDetailId { get; set; }
    }
}
