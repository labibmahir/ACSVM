using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Domain.Dto.HIKVision
{
    [XmlRoot(ElementName = "CaptureFingerPrint", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class VMCaptureFingerPrintResponse
    {
        /// <summary>
        /// dep, xs:string, fingerprint data, which is between 1 and 768, and it should be encoded by Base64
        /// </summary>
        [XmlElement("fingerData")]
        public string? fingerData { get; set; }

        /// <summary>
        /// req, xs:integer, finger No., which is between 1 and 10
        /// </summary>
        [XmlElement("fingerNo")]
        public int? fingerNo { get; set; }

        /// <summary>
        /// req, xs:integer, fingerprint quality, which is between 1 and 100
        /// </summary>
        [XmlElement("fingerPrintQuality")]
        public int? fingerPrintQuality { get; set; }
    }
}
