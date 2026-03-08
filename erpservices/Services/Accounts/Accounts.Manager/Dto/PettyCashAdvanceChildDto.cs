using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(PettyCashAdvanceChild)), Serializable]
    public class PettyCashAdvanceChildDto : Auditable
    {
        public long PCACID { get; set; }
        public long PCAMID { get; set; }
        public string Details { get; set; }
        public string ProjectName { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string Remarks { get; set; } 
        public decimal ResubmitAmount { get; set; }
        public string ResubmitRemarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
}
