using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMUserInfoSearchResponse
    {
        /// <summary>
        /// required, string type, search ID, which is used to confirm the upper-level platform or system. If the platform or the
        /// system is the same one during two searching, the search history will be saved in the memory to speed up next
        /// searching
        /// </summary>
        public string searchID { get; set; }

        /// <summary>
        /// required, string, search status: "OK"-searching completed, "NO MATCH"-no matched results, "MORE"-searching for
        /// more results
        /// </summary>
        public string responseStatusStrg { get; set; }

        /// <summary>
        /// required, integer32, number of returned results this time
        /// </summary>
        public int numOfMatches { get; set; }

        /// <summary>
        /// required, integer32, total number of matched results
        /// </summary>
        public int totalMatches { get; set; }

        /// <summary>
        /// optional, person information
        /// </summary>
        public List<VMUserInfo> UserInfo { get; set; } = new List<VMUserInfo>();
    }
}
