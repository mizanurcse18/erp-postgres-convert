using Accounts.DAL.Entities;
using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(BudgetChild)), Serializable]
    public class BudgetChildDto : Auditable
    {
        public long BudgetChildID { get; set; }
        public long BudgetMasterID { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool isDeletedFromUI { get; set; }
        public List<BudgetChildWithApprovalPanelMap> BudgetChildPanelMap { get; set; }
        public List<ComboModel> BudgetChildPanelCombo { get; set; }
    }
}
