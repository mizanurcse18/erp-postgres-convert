using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class MailSetupDto
    {
        public int MailId { get; set; }       
        public int GroupId { get; set; }        
        public string To_CC_BCC { get; set; }      
        public string Email { get; set; }
    }
}
