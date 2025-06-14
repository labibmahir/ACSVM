﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto
{
    public class PersonDto
    {
        public Guid Oid { get; set; }

        public int[] AccessLevelIds { get; set; }
        public int[] DeviceIdList { get; set; }
        public int? OrganizationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; }

        //[MaxLength(50)]
        //public string PersonNumber { get; set; }
        
        public string Department { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        public Enums.Gender Gender { get; set; }

        [Required]
        public bool IsDeviceAdministrator { get; set; }

        [Required]
        [Column(TypeName = "smalldatetime")]
        public DateTime ValidateStartPeriod { get; set; }

        [Required]
        [Column(TypeName = "smalldatetime")]
        public DateTime? ValidateEndPeriod { get; set; }

        public Enums.UserVerifyMode? UserVerifyMode { get; set; }
    }
}
