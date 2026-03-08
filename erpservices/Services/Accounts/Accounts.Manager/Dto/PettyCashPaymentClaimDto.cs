using Accounts.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class PettyCashPaymentClaimDto
    {
        public PettyCashPaymentMasterDto master { get; set; }
        public long PCPMID { get; set; }
        public long PCRMID { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsDraft { get; set; }
        public decimal GrandTotal { get; set; }
        public string Remarks { get; set; }
        public DateTime RequestDate { get; set; }
        public List<PRPChildItemDetails> Details { get; set; }
        public List<Attachments> Attachments { get; set; }


    }

    [AutoMap(typeof(PettyCashPaymentChild)), Serializable]
    public class PRPChildItemDetails : Auditable
    {
        public long PCPCID { get; set; }
        public long PCPMID { get; set; }
        public long PCRCID { get; set; }
        public long PCRMID { get; set; }
        public long PCCID { get; set; }
        public int ClaimID { get; set; }
        public long ClaimTypeID { get; set; }
        public string Details { get; set; }
        public string ProjectName { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ResubmitAmount { get; set; }
        public string ResubmitRemarks { get; set; }
        public string Remarks { get; set; }
        public string ClaimType { get; set; }
    }
}
