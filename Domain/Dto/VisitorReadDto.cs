using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Dto
{
    public class VisitorReadDto
    {
        public Guid Oid { get; set; }
        public int? OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string VisitorNumber { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public Enums.Gender Gender { get; set; }
        public DateTime ValidateStartPeriod { get; set; }

        public DateTime? ValidateEndPeriod { get; set; }

        public Enums.UserVerifyMode? UserVerifyMode { get; set; }
        public string? imageBase64 { get; set; }
        public List<FingerPrint>? FingerPrints { get; set; }
    }
}
