using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class UserLoginPolicyDto
    {
        public UserDto User { get; set; }
        public int ReasonID { get; set; }
        public string Message { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public int UserAccountLockedDurationInMin { get; set; }
        public int FailedCount { get; set; }
    }
}
