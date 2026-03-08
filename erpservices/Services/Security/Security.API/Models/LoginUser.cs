using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class LoginUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int CompanyID { get; set; }
        //public string APP_VERSION { get; set; }
        public string userName { get; set;}
        public string userPassword { get; set;}
        public string TokenHash { get; set; }
    }
}
