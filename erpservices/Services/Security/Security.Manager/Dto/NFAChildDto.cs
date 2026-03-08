using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Security.Manager
{
    [AutoMap(typeof(NFAChild)), Serializable]
    public class NFAChildDto:Auditable
    {
        public int NFACID { get; set; }
        
        public string ItemName { get; set; }
        
        public string Description { get; set; }
        
        public decimal Unit { get; set; }
        
        public string UnitType { get; set; }
        
        public Decimal UnitPrice { get; set; }
        
        public string VatTaxStatus { get; set; }
        
        public string Vendor { get; set; }
        
        public Decimal TotalAmount { get; set; }
        
        public int NFAMasterID { get; set; }

        public string Type { get; set; }
       
        public string Duration { get; set; }
       
        public string CostType { get; set; }
       
        public decimal? EstimatedBudgetAmount { get; set; }
       
        public decimal? AITPercent { get; set; }
    }
}
