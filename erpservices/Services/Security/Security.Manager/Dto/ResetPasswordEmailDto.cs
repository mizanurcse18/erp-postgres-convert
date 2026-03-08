using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class ResetPasswordEmailDto : EmailDto
    {
        public string UserName { get; set; }
        public string GeneratedPassword { get; set; }
    }
}
