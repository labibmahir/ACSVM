using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;
using Microsoft.AspNetCore.Http;

namespace Domain.Dto
{
    public class AppointmentDto
    {
        [Key]
        public Guid Oid { get; set; }

        public int[] AccessLevelList { get; set; }
        public int[] DeviceIdList { get; set; }

        public Guid[] PersonIds { get; set; }

        public int? OrganizationId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }
        //[MaxLength(50)]
        //public string VisitorNumber { get; set; }
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public Enums.Gender Gender { get; set; }
        public string Address { get; set; }

        public string CompanyName { get; set; }

        [Required]
        [Column(TypeName = "smalldatetime")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Column(TypeName = "time(7)")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column(TypeName = "time(7)")]
        public TimeSpan EndTime { get; set; }

        public Enums.UserVerifyMode? VisitorVerifyMode { get; set; }
        
        public IFormFile Image { get; set; }
        //[Required]
        //public bool IsCompleted { get; set; }

        //[Required]
        //public bool IsCancelled { get; set; }


    }
}
