using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ClientFieldMapping : BaseModel
    {
        [Key]
        public int Oid { get; set; }
        public string? TableName { get; set; }
        public string? StandardField { get; set; }
        public string? ClientField { get; set; }
        public int? FormatType { get; set; }
        public int? ClientDBDetailId { get; set; }

        [ForeignKey("ClientDBDetailId")]
        public ClientDBDetail ClientDBDetail { get; set; }
    }
}
