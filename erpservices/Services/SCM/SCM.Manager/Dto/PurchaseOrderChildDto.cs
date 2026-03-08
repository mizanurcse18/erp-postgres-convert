using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(PurchaseOrderChild)), Serializable]
    public class PurchaseOrderChildDto:Auditable
    {
        public int POCID { get; set; }
        public int PRCID { get; set; }
        
        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string PaymentTerm { get; set; }
        

        public DateTime DeliveryWithinDate { get; set; }
        public string Description { get; set; }
        
        public int UOM { get; set; }

        public Decimal POQty { get; set; }
        public Decimal PRQty { get; set; }
        
        public Decimal Qty { get; set; }

        public Decimal Rate { get; set; }
        public Decimal Price { get; set; }

        public Decimal Amount { get; set; }
        public Decimal RebatePercentage { get; set; }
        public string Rebateable { get; set; }

        public long VatInfoID { get; set; }
        public long InventoryTypeID { get; set; }
        
        public string VatInfo { get; set; }
        public string UnitCode { get; set; }

        public decimal VatPercent { get; set; }
        
        public bool IsRebateable { get; set; }
        
        
        public Decimal TotalAmountIncludingVat { get; set; }
        
        public long POMasterID { get; set; }
        public decimal PurchasedAmount { get; set; }
        public decimal PurchasedQty { get; set; }
        public decimal PRAmount { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal PRPrice { get; set; }
        public string InventoryTypeName { get; set; }
        public string ItemDescription { get; set; }
        public int SupplierID { get; set; }
        public int PRQID { get; set; }

        public decimal QCSuppliedQty { get; set; }
        public decimal QCAcceptedQty { get; set; }
        public decimal VATAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        
    }
}
