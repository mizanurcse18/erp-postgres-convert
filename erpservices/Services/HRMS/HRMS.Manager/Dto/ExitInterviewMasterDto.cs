using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using HRMS.DAL.Entities;
using System;

namespace HRMS.Manager
{
    [AutoMap(typeof(EmployeeExitInterview)), Serializable]
    public class ExitInterviewMasterDto:Auditable
    {
        public long EEIID { get; set; }
        public DateTime? RequestDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public int TemplateID { get; set; }
        public string TemplateBody { get; set; }
        public string ExternalID { get; set; }
        public bool IsDraft { get; set; }

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
        public string ImagePath { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string POEmployeeCode { get; set; }
        public string POEmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool canModify { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public string WorkMobile { set; get; }
        public string WorkEmail { set; get; }
        public string ExitEmpName { set; get; }
        public string ExitEmpCode { set; get; }
        public string ExitEmpDivisionName { set; get; }
        public string ExitEmpDepartmentName { set; get; }
        public string ExitEmpWorkEmail { set; get; }
        public string ExitEmpWorkMobile { set; get; }
        public string CreatedDateStr { get { return CreatedDate.ToString("dd MMM yyyy hh:mm tt"); } }
        public string TemplateName { get; set; }
    }
}
