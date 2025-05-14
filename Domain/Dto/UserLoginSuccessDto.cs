using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
  public  class UserLoginSuccessDto
    {  
        /// <summary>
       /// Gets or sets the unique identifier for the user.
       /// </summary>
        public Guid? Oid { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Gets or sets the last name (surname) of the user.
        /// </summary>
        public string Surname { get; set; }


        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the country code of the user's phone number.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the cellphone number of the user.
        /// </summary>
        public string CellPhone { get; set; }

 
        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the authentication token for the user.
        /// This is typically used for session management.
        /// </summary>
        public string Token { get; set; }
    }
}
