using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class ApprovalSubmissionDto
    {
        public int ApprovalProcessID { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APFeedbackID { get; set; }
        public string Remarks { get; set; }
        public int APTypeID { get; set; }
        public int ReferenceID { get; set; }
        public int ToAPMemberFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public string BudgetPlanRemarks { get; set; }
        public string AdminRemarks { get; set; }
        public int BudgetPlanCategoryID { get; set; }
        public bool IsEditable { get; set; }
        //public bool IsModified { get; set; }


        public int SupportTypeID { get; set; }
        public int TransportTypeID { get; set; }
        public decimal TransportQuantity { get; set; }
        public DateTime FromDateChange { get; set; }
        public DateTime ToDateChange { get; set; }
        public DateTime NeededByDateChng { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public int EmployeeID { get; set; }
        public string ITRecommendation { get; set; }



        public string ids { get; set; }
        public string approvalType { get; set; }

    }
    public class BulkSubmissionDto
    {
        public List<ApprovalSubmissionDto> bulkList { get; set; }

    }
}
