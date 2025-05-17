using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMFingerPrintSetUpStatusListItem
    {
        /// <summary>
        /// optional, integer, fingerprint module No.
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// optional, integer, fingerprint module status: 0-connecting failed, 1-connected, 2-the fingerprint module is offline, 3-
        /// the fingerprint quality is poor, try again, 4-the memory is full, 5-the fingerprint already exists, 6-the fingerprint ID
        /// already exists, 7-invalid fingerprint ID, 8-this fingerprint module is already configured, 10-the fingerprint module
        /// version is too old to support the employee No.
        /// </summary>
        public int? cardReaderRecvStatus { get; set; }

        /// <summary>
        /// optional, string, error information
        /// </summary>
        public string? errorMsg { get; set; }

    }
}
