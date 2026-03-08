using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class NotificationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string EmployeeIds { get; set; }
        public string NotificationMessage { get; set; }
        public string CurrentNotificaitonEmplyeeID { get; set; }
    }
}
