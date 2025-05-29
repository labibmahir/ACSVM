using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class RefreshTokenDto
    {
        public string UserName { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
