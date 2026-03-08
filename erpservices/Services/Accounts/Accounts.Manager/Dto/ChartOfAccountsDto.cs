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
    [AutoMap(typeof(ChartOfAccounts)), Serializable]
    public class ChartOfAccountsDto : Auditable
    {
        public long COAID { get; set; }     
        public String AccountName { get; set; }       
        public String AccountCode { get; set; }
        public long? ParentID { get; set; }
        public int ACClassID { get; set; }
        public int CategoryID { get; set; }
        public String HierarchyLevel { get; set; }
        public int Level { get; set; }
        public int SequenceNo { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? OpeningBalance { get; set; }
        public DateTime? OpeningBalanceOn { get; set; }
        public string ExternalID { get; set; }
        public string BalanceType { get; set; }
        public bool IsBudgetEnable { get; set; }
        public bool IsActive { get; set; }
        public bool IsAllowManualPosting { get; set; }
        public DateTime? StartDate { get; set; }
        public string CategoryShortCode { get; set; }
        public int FinalChildCount { get; set; }
        public string ItemAccCode { get; set; }
    }
}
