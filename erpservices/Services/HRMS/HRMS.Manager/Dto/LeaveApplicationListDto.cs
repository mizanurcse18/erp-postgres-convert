using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class LeaveApplicationListDto
    {
        public int EmployeeLeaveAID { get; set; }
        public string LeaveCategory { get; set; }
        public int LeaveCategoryID { get; set; }
        public string LeaveDates { get; set; }
        public decimal NumberOfLeave { get; set; }
        public string ApprovalStatus { get; set; }
        public int ApprovalStatusID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsRejected { get; set; }
        public bool IsCurrentAPMember { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsLFAApplied { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public int TotalEmployementDays { get; set; }
    }
}
