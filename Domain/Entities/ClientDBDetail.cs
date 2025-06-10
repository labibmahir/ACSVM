using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Utilities.Constants.Enums;

namespace Domain.Entities
{
    public class ClientDBDetail : BaseModel
    {
        [Key]
        public int Oid { get; set; }

        [Required]
        public DatabaseType DatabaseType { get; set; }

        [Required]
        [StringLength(250)]
        public string ServerName { get; set; }

        [Required]
        [StringLength(250)]
        public string DatabaseName { get; set; }

        [StringLength(50)]
        public string? ServiceName { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(50)]
        public string? ConnectionMode { get; set; }

        public bool IsConnectionActive { get; set; }
        public bool UseClientDb { get; set; }

        [Required]
        [StringLength(250)]
        public string UserId { get; set; }

        [Required]
        [StringLength(250)]
        public string Password { get; set; }

        public int? Port { get; set; }

        [JsonIgnore]
        public IEnumerable<ClientFieldMapping> ClientFieldMappings { get; set; } = new List<ClientFieldMapping>();
    }
}
