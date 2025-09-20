using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CareerConnect.Models
{
    public class UserMV
    {
        public UserMV()
        {
            Company = new CompanyMV();
        }

        public int UserID { get; set; }
        public int UserTypeID { get; set; }        // ID from UserTable
        public string UserTypeName { get; set; }   // Name from UserTypeTable.UserType
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNo { get; set; }
        public int AccountStatusID { get; set; }
        public CompanyMV Company { get; set; }
    }
}
