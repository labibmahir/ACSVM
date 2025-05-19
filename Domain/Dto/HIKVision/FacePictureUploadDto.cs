using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public record FacePictureUploadDto
    {
        /// <summary>
        /// optional, face picture library type: "blackFD"-list library, "staticFD"-static library, string type, the maximum size is 32
        /// bytes
        /// </summary>
        public string? faceLibType { get; set; }

        /// <summary>
        /// optional, face picture library ID, string type, the maximum size is 63 bytes
        /// </summary>
        public string? FDID { get; set; }

        public string? FPID { get; set; }
    }
}
