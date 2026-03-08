using Accounts.DAL.Entities;
using AutoMapper;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(PettyCashExpenseChild)), Serializable]
    public class PettyCashExpenseChildDto : Auditable
    {
        public long PCECID { get; set; }
        public long PCEMID { get; set; }

        public DateTime ExpenseDate { get; set; }

        public int PurposeID { get; set; }

        public string Details { get; set; }

        public decimal ExpenseAmount { get; set; }

        public string Remarks { get; set; }
        public string Purpose { get; set; }

        public List<Attachments> Attachments { get; set; }
    }
}
