using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class SCMApprovalSubmissionDto
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
        public int BudgetPlanCategoryID { get; set; }
        public string SCMRemarks { get; set; }
        public bool IsEditable { get; set; }
        //public bool IsModified { get; set; }
        public bool IsSCM { get; set; }
        public List<Quotations> Quotations { get; set; } = new List<Quotations>();
        public List<ItemDetails> ItemDetails { get; set; } = new List<ItemDetails>();
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public PRNFAMapDto PRNFAMap { get; set; }
        public List<PurchaseRequisitionChildCostCenterBudgetDto> CostCenterBudget { get; set; } = new List<PurchaseRequisitionChildCostCenterBudgetDto>();

    }
    public class PRNFAMapDto
    {

        public long PRNFAMapID { get; set; }
        public long PRMID { get; set; }
        public int? NFAID { get; set; }
        public string NFAReferenceNo { get; set; }
        public decimal NFAAmount { get; set; }
        public bool IsFromSystem { get; set; }

    }

    public class Quotations
    {
        public int SupplierID { get; set; } = 0;
        public string Description { get; set; }
        public decimal? Amount { get; set; } = 0;
        public decimal? QuotedQty { get; set; } = 0;
        public decimal? QuotedUnitPrice { get; set; } = 0;
        public int TaxTypeID { get; set; }
        public int PRQID { get; set; }
        public long PRMasterID { get; set; }
        public long? ItemID { get; set; }
        //public List<Items> Items { get; set; } = new List<Items>();
        public int PRCID { get; set; }
    }
    public class Attachment
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
    }
}
