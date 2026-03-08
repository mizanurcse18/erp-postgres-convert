using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class AttendanceDto
    {
        public int PrimaryID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceDateString { get { return AttendanceDate.ToString("yyyy-MM-dd"); } }
        public string EmployeeCode { get; set; }
        public string IN_TIME { get; set; }
        public string OUT_TIME { get; set; }
        public string ATTENDANCE_STATUS { get; set; }
        public string AttendanceDateNew { get; set; }
        public int AttendanceStatus { get; set; }
        public string ATTENDANCE_STATUS_STRING
        {
            get
            {

                string status = "";

                switch (AttendanceStatus)
                {
                    case (int)Util.AttendanceStatus.Absent:
                        status = "bg-red-900 text-white";
                        break;
                    case (int)Util.AttendanceStatus.Late:
                        status = "bg-red-400 text-white";
                        break;
                    case (int)Util.AttendanceStatus.Holiday:
                    case (int)Util.AttendanceStatus.OffDay:
                    case (int)Util.AttendanceStatus.Weekend:
                        status = "bg-blue text-white";
                        break;
                    case (int)Util.AttendanceStatus.Invalid:
                        status = "bg-orange text-white";
                        break;
                    case (int)Util.AttendanceStatus.Leave:
                    case (int)Util.AttendanceStatus.LeavePending:
                        status = "bg-purple text-white";
                        break;
                    case (int)Util.AttendanceStatus.RemotePending:
                        status = "bg-red-400 text-white";
                        break;
                    default:
                        status = "bg-green text-white";
                        break;
                }
                return status;
            }
        }
        public string WORK_HOUR { get; set; }
        public string ActualWorkingHour { get; set; }
        public decimal WORK_HOURDecimal { get; set; }
        public string DAY_STATUS { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public string EmployeeID { get; set; }
        public string Details { get; set; } = "";
        public string Action { get; set; }
        public string AttendanceDay { get; set; }
    }
}
