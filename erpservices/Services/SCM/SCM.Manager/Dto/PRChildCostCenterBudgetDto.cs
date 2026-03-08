using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(PurchaseRequisitionChildCostCenterBudget)), Serializable]
    public class PRChildCostCenterBudgetDto : Auditable
    {
        public int PRCCCBID { get; set; }
        public long PRMasterID { get; set; }
        public int ForID { get; set; }
        public string CostCenterName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Decimal? AllocatedBudgetAmount { get; set; }
        public Decimal? RemainingBudgetAmount { get; set; }
        public string Note { get; set; }
    }
}
