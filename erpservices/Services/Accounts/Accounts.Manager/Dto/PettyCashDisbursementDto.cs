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
    public class PettyCashDisbursementDto
    {
        public PettyCashAdvanceMasterDto advanceMaster { get; set; }
        public List<DAdvChildItemDetails> advanceChilds { get; set; }
        public PettyCashExpenseMasterDto expenseMaster { get; set; }
        public List<DExpChildItemDetails> expenseChilds { get; set; }
        public int ApprovalProcessID { get; set; }
        public List<DAdvChildItemDetails> AdvItemDetails { get; set; }
        public List<DExpChildItemDetails> ExpItemDetails { get; set; }
    }

    [AutoMap(typeof(PettyCashAdvanceChild)), Serializable]
    public class DAdvChildItemDetails : Auditable
    {
        public long PCACID { get; set; }
        public long PCAMID { get; set; }
        public string Details { get; set; }
        public string ProjectName { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
    [AutoMap(typeof(PettyCashExpenseChild)), Serializable]
    public class DExpChildItemDetails : Auditable
    {
        public long PCACID { get; set; }
        public long PCAMID { get; set; }
        public string Details { get; set; }
        public string ProjectName { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
}
