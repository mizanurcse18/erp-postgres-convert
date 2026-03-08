using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class EADApprovalSubmissionDto
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
        public bool IsEditable { get; set; }
        public bool IsCoreFunctional { get; set; }
    }
}
