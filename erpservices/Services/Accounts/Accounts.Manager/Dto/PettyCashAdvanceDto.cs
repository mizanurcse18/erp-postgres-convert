using Accounts.DAL.Entities;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class PettyCashAdvanceDto
    {
        public PettyCashAdvanceMasterDto master { get; set; }
        public List<PChildItemDetails> childs { get; set; }
        public int ApprovalProcessID { get; set; }
        public List<PChildItemDetails> ItemDetails { get; set; }
    }

    [AutoMap(typeof(PettyCashAdvanceChild)), Serializable]
    public class PChildItemDetails : Auditable
    {
        public long PCACID { get; set; }
        public long PCAMID { get; set; }
        public string Details { get; set; }
        public string ProjectName { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ResubmitAmount { get; set; }
        public string ResubmitRemarks { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
}
