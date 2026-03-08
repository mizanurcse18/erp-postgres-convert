using Accounts.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(BudgetMaster)), Serializable]
    public class BudgetMasterDto : Auditable
    {
        public long BudgetMasterID { get; set; }
        public long DepartmentID { get; set; }
        public decimal MinAmount { get; set; }
        public decimal TotalMaxAmount { get; set; }
        public decimal AttachmentRequiredAmount { get; set; }
        public string DepartmentName { get; set; }
        public List<BudgetChildDto> ChildList { get; set; }
        public string DivisionName { get; set; }
    }
}
