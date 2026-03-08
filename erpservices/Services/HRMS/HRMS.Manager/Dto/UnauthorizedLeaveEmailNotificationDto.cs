using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class UnauthorizedLeaveEmailNotificationDto
    {
        public int EmployeeID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string AttendanceDates { get; set; }
        public int GroupCount { get; set; }
        public string DateOfMonth { get; set; }
    }
}
