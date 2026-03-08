using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeDto
    {

        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public int PersonID { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string WorkEmail { get; set; }
        public string WorkMobile { get; set; }
        public int? EmployeeStatusID { get; set; }
        public DateTime? DiscontinueDate { get; set; }
        public int? EmploymentCategoryID { get; set; }
        public string DesignationName { get; set; }
        public string ImagePath { get; set; }
        public int EmployeeSupervisorID { get; set; }
        public string IN_TIME { get; set; }
        public string OUT_TIME { set; get; }
        public string ATTENDANCE_STATUS { get; set; }
        public string LateOrOnTime { set; get; }
        public string EntryType { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceDateString { get { return AttendanceDate.ToString("dd-MM-yyyy"); } }
        public int AttendanceStatus { get; set; }
        public string DepartmentName {get;set;}
        public string DivisionName   {get;set;}
        public string Leave { get;set;}
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
                    default:
                        status = "bg-green text-white";
                        break;
                }
                return status;
            }
        }
    }
}
