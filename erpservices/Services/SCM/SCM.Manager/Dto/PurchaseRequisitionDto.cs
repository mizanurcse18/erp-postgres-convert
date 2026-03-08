using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class PurchaseRequisitionDto
    {
        public long PRMasterID { get; set; }
        public DateTime? PRDate { get; set; }
        public string ReferenceNo { get; set; }
        public string Subject { get; set; }
        public string Preamble { get; set; }
        public List<ItemDetails> ItemDetails { get; set; }
        public string PriceAndCommercial { get; set; }
        public string Solicitation { get; set; }
        public string BudgetPlanRemarks { get; set; }
        public int BudgetPlanCategoryID { get; set; }
        public int ApprovalProcessID { get; set; }=0;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public string Description { get;set; }
        public string ReferenceKeyword { get;set; }
        public int DeliveryLocation { get;set; }
        public string SCMRemarks { get;set; }
        public DateTime? RequiredByDate { get; set; }
        public bool IsDraft { get; set; }
        public long MRMasterID { get; set; }
        public bool IsArchive { get; set; }
        public int? NFAID { get; set; }
        public decimal NFAAmount { get; set; }
        public PRNFAMapDto PRNFAMap { get; set; }
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
        public int ParentFUID { get; set; }
       
    }
}
