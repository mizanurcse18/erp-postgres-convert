using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(PurchaseRequisitionChild)), Serializable]
    public class PurchaseRequisitionChildDto:Auditable
    {
        public int PRCID { get; set; }
        public int PRQID { get; set; }
        
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int? ForID { get; set; }
        
        public string Description { get; set; }
        
        public decimal Qty { get; set; }
        
        public string UOM { get; set; }
        
        public Decimal Price { get; set; }
        
        public string VatTaxStatus { get; set; }
        
        public string Vendor { get; set; }
        
        public Decimal Amount { get; set; }
        
        public int PRMasterID { get; set; }

        public string Remarks { get; set; }
        public string UnitCode { get; set; }
        public string CostCenterName { get; set; }

        public decimal PurchasedAmount { get; set; }
        public decimal PurchasedQty { get; set; }
        public decimal PRAmount { get; set; }
        public int InventoryTypeID { get; set; }
        public int SupplierID { get; set; }
        public int POMasterID { get; set; }

    }
}
