using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;
using Domain.Entities;

namespace Domain.Dto
{
    public class PersonReadDto
    {
        public Guid Oid { get; set; }
        public int? OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string PersonNumber { get; set; }
        public string Email { get; set; }

        public string Department { get; set; }
        public string PhoneNumber { get; set; }
        public Enums.Gender Gender { get; set; }

        public bool IsDeviceAdministrator { get; set; }

        public DateTime ValidateStartPeriod { get; set; }

        public DateTime? ValidateEndPeriod { get; set; }

        public Enums.UserVerifyMode? UserVerifyMode { get; set; }
        public string? imageBase64 { get; set; }
        
        public List<IdentifiedDevicesDto>? IdentifiedDevices { get; set; }
        
        public List<FingerPrint>? FingerPrints { get; set; }
        public int AssignedDevicesCount { get; set; }
        public int FingerPrintCount { get; set; }
        public int AssignedCardCount { get; set; }
        public int ImagesCount { get; set; }
    }

    public class IdentifiedDevicesDto
    {
        public int? AccessControlId { get; set; }
        
        public int? DeviceId { get; set; }
        
        public string DeviceName { get; set; }
        
        public string AccessControlName { get; set; }
        
    }
}
