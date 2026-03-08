using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class ResetPassword
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
