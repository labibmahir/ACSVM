using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class VMPersonInfoExtends
    {
        /// <summary>
        /// optional, string, name of the person extension information
        /// </summary>
        public string? name { get; set; } = null;

        /// <summary>
        /// optional, string, content of the person extension information
        /// </summary>
        public string? value { get; set; } = null;
    }
}
