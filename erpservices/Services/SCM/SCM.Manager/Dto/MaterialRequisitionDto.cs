using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class MaterialRequisitionDto
    {
        public long MRMasterID { get; set; }
        public DateTime? MRDate { get; set; }
        public string ReferenceNo { get; set; }
        public string Subject { get; set; }
        public string Preamble { get; set; }
        public List<MaterialRequisitionItemDetails> ItemDetails { get; set; }
        public string PriceAndCommercial { get; set; }
        public string Solicitation { get; set; }
        public string BudgetPlanRemarks { get; set; }
        public int ApprovalProcessID { get; set; }=0;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public string Description { get;set; }
        public string ReferenceKeyword { get;set; }
        public int DeliveryLocation { get;set; }
        public string SCMRemarks { get;set; }
        public DateTime? RequiredByDate { get; set; }
        public bool IsDraft { get; set; }
        
        public List<ManualApprovalPanelEmployeeDto> MRApprovalPanelList { get; set; }
    }
}
