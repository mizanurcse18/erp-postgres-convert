using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace External.API.Models
{
    public class SMSModel
    {
        public string OTPText { get; set; }
        public string MessageBody { get; set; }
        public string APINotFoundError { get; set; }
        public string APIUnreachableError { get; set; }
        public string OTPMismatchOrInvalidError { get; set; }
        public string OTPInvalidError { get; set; }
        public string WorkMobileEmptyError { get; set; }
        public string MessageBodyError { get; set; }
        public string CategoryType { get; set; }
    }
}
