using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class LeaveBalanceAndDetailsResponse
    {
        public List<LeaveBalance> LeaveBalances { get; set; }
        public List<LeaveDetails> LeaveDetails { get; set; }
        public int TotalEmployementDays { get; set; }
        public DateTime JoiningDate { get; set; }
    }

    public class LeaveDetails
    {
        public int ELADBDID { get; set; }
        public string Day { get; set; }
        public string DayStatus { get; set; }
        public DateTime DayDateTime { get { return DateTime.ParseExact(Day, "dd MMM yyyy", null); } }
        public bool IsCancel { get; set; }
    }

    public class LeaveBalance
    {
        public int EmployeeID { get; set; }
        public int LeaveCategoryID { get; set; }
        public string SystemVariableCode { get; set; }
        public decimal LeaveDays { get; set; }
        public decimal NoOfApprovedLeaveDays { get; set; }
        public decimal NoOfPendingLeaveDays { get; set; }
        public decimal Applying { get; set; }
        public decimal Balance { get; set; }
        public decimal PreviousLeaveDays { get; set; }
        public decimal EncashBalance { get; set; }
    }

    public class LeaveEncashmentApplication
    {
        public int ALEMasterID { get; set; }
        public long ALEWMasterID { get; set; }
        public int EmployeeID { get; set; }
        public int APTypeID { get; set; }
        public decimal TotalLeaveBalanceDays { get; set; }
        public decimal EncashedLeaveDays { get; set; }
        public int ApprovalStatusID { get; set; }

        public int ApprovalProcessID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ApprovalStatus { get; set; }
        public int SessionStatus { get; set; }
        public string EmployeeWithDepartment { get; set; }
        public string FinancialYear { get; set; }
        public DateTime? DateOfJoiningWork { get; set; }
        public List<Dictionary<string, object>> Comments { get; internal set; }
    }
    public class LeaveApplication
    {
        public int EmployeeLeaveAID { get; set; }
        public string RequestStartDate { get; set; }
        public DateTime RequestStartDateTime { get { return DateTime.ParseExact(RequestStartDate, "dd/MM/yyyy", null); } }
        public string RequestEndDate { get; set; }
        public DateTime RequestEndDateDateTime { get { return DateTime.ParseExact(RequestEndDate, "dd/MM/yyyy", null); } }
        public string LeaveDates { get; set; }
        public decimal NumberOfLeave { get; set; }
        public string BackupEmployeeName { get; set; }
        public int? BackupEmployeeID { get; set; }
        public int LeaveCategoryID { get; set; }
        public int CancellationStatus { get; set; }
        public string LeaveCategory { get; set; }
        public string Purpose { get; set; }
        public string LeaveLocation { get; set; }
        public string Remarks { get; set; }
        public string CancelledBy { get; set; }
        public DateTime? DateOfJoiningWork { get; set; }
        public List<LeaveDetails> LeaveDetails { get; set; } = new List<LeaveDetails>();
        public int ApprovalProcessID { get; set; }
        public IEnumerable<Dictionary<string, object>> Comments { get; set; }
        public List<Attachments> Attachments { get; set; }
        public bool IsLFA { get; set; }
        public bool IsFestival { get; set; }
        public LFADeclarationDto LFADeclaration { get; set; } = new LFADeclarationDto();
        public string FormType { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeWithCode { get; set; }
        public bool IsHrApplied { get; set; } = false;
    }

    public class LeaveApplicationWithComments : LeaveApplication
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string DivisionName { get; set; }
        public string ImagePath { get; set; }
        public string CancelledBy { get; set; }
        public string Remarks { get; set; }
        public List<LeaveBalance> LeaveBalances { get; set; }
        public IEnumerable<Dictionary<string, object>> Comments { get; set; }
        public List<ComboModel> RejectedMembers { get; set; }
        public List<ComboModel> ForwardingMembers { get; set; }
        public List<Attachments> Attachments { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public bool IsReassessment { get; set; }
        public int APForwardInfoID { get; set; }
        public int CancellationStatus { get; set; }
        public LFADeclarationDto LFADeclaration { get; set; } = new LFADeclarationDto();
    }


    public class LeaveEncashmentApplicationWithComments : LeaveEncashmentApplication
    {

        public int EmployeeLeaveAID { get; set; }
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string DivisionName { get; set; }
        public string ImagePath { get; set; }
        public string CancelledBy { get; set; }
        public string Remarks { get; set; }
        public List<LeaveBalance> LeaveBalances { get; set; }
        public IEnumerable<Dictionary<string, object>> Comments { get; set; }
        public List<ComboModel> RejectedMembers { get; set; }
        public List<ComboModel> ForwardingMembers { get; set; }
        public List<Attachments> Attachments { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public bool IsReassessment { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public int APTypeID { get; set; }
        public int CancellationStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class Attachments
    {
        public string AID { get; set; }
        public int FUID { get; set; }
        public int ID
        {
            get
            {
                int fuid;
                if (int.TryParse(AID, out fuid))
                {
                    return fuid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string AttachedFile { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ReferenceId { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
    }
}
