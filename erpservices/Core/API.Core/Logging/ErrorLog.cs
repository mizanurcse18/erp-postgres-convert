using System;
using System.Collections.Generic;
using System.Text;

namespace API.Core.Logging
{
    public class ErrorLog
    {
        public int ErrorId { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string Url { get; set; }
        public string ErrorBy { get; set; }
        public DateTime ErrorDate { get; set; }
        public string ErrorIP { get; set; }
    }
}
