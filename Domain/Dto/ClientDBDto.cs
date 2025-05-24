using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.Constants.Enums;

namespace Domain.Dto
{
    public record ClientDBDto
    {
        public int Id { get; set; }

        public DatabaseType DatabaseType { get; set; }

        [StringLength(250)]
        public string ServerName { get; set; }

        [StringLength(250)]
        public string DatabaseName { get; set; }

        [StringLength(50)]
        public string? ServiceName { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(50)]
        public string? ConnectionMode { get; set; }

        public bool IsConnectionActive { get; set; }

        [StringLength(250)]
        public string UserId { get; set; }

        [StringLength(250)]
        public string Password { get; set; }

        public int? Port { get; set; }

        public List<ClientMappingDto>? ClientMappings { get; set; }
    }
}
