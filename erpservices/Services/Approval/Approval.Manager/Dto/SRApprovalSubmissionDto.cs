using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class SRApprovalSubmissionDto
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
        public string Duration { get; set; }
        public bool IsOthers { get; set; }
        public string Vehicle { get; set; }
        public string Driver { get; set; }
        public string ContactNumber { get; set; }
        public List<ItemDetailsSR> ItemDetails { get; set; } = new List<ItemDetailsSR>();
        public int EmployeeID { get; set; }


    }
}
