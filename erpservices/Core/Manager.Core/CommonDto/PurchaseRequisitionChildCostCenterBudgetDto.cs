using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class PurchaseRequisitionChildCostCenterBudgetDto
    {
      
        public int PRCCCBID { get; set; }
    
        public long PRMasterID { get; set; }
       
        public int ForID { get; set; }
      
        public DateTime? FromDate { get; set; }
       
        public DateTime? ToDate { get; set; }
      
        public Decimal? AllocatedBudgetAmount { get; set; }
       
        public Decimal? RemainingBudgetAmount { get; set; }
      
        public string Note { get; set; }
    }
}
