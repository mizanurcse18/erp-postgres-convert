using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class EmailDtoCore
    {
        public string FromEmailAddress { get; set; }
        public string FromEmailAddressDisplayName { get; set; }
        public DateTime EmailDate { get; set; }
        public List<string> ToEmailAddress { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public List<string> CCEmailAddress { get; set; }
        public List<string> BCCEmailAddress { get; set; }
        public string UserName { get; set; }
        public string GeneratedPassword { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
