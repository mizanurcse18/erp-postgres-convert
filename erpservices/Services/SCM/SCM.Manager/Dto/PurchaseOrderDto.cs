using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class PurchaseOrderDto
    {
        public long POMasterID { get; set; }


        public string ReferenceNo { get; set; }

        public string ReferenceKeyword { get; set; }
        public DateTime PODate { get; set; }

        public long? DeliveryLocation { set; get; }
        public long SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierContact { get; set; }


        public string PRReferenceNo { get; set; }
        public DateTime PRDate { get; set; }
        public string PREmployeeName { get; set; }
        public long PRMasterID { get; set; }

        public decimal GrandTotal { get; set; }

        public int ApprovalStatusID { get; set; }
        public DateTime DeliveryWithinDate { get; set; }
        public string AmountInWords { get; set; }

        public string ContactPerson { get; set; }

        public string ContactNumber { get; set; }

        public string Remarks { get; set; }
        public string PORemarks { get; set; }

        public string QuotationNo { get; set; }

        public DateTime? QuotationDate { get; set; }

        public int? PaymentTermsID { get; set; }

        public int? InventoryTypeID { get; set; }

        public bool IsDraft { get; set; }

        public string BudgetPlanRemarks { get; set; }

        public string SCMRemarks { get; set; }
        public string CloseRemarks { get; set; }
        public bool IsClosed { get; set; }

        public List<ItemDetailsPO> ItemDetails { get; set; }
        public int ApprovalProcessID { get; set; }=0;
        public int PRApprovalProcessID { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public string Comment { get; set; }
        public int CreditDay { get; set; }

    }

    public class ItemDetailsPO
    {
        public int POCID { get; set; }
        public int PRCID { get; set; }

        public long ItemID { get; set; }
        public string ItemName { get; set; }

        public string Description { get; set; }

        public int UOM { get; set; }

        public Decimal POQty { get; set; }
        public Decimal PRQty { get; set; }
        public Decimal Qty { get; set; }

        public Decimal Rate { get; set; }
        public Decimal Price { get; set; }

        public Decimal Amount { get; set; }

        public long VatInfoID { get; set; }
        public string VatInfo { get; set; }
        public Decimal RebatePercentage { get; set; }
        public string Rebateable { get; set; }

        public decimal VatPercent { get; set; }
        public string UnitCode { get; set; }

        public bool IsRebateable { get; set; }


        public decimal TotalAmountIncludingVat { get; set; }

        public long POMasterID { get; set; }
        public long InventoryTypeID { get; set; }
        public decimal PurchasedAmount { get; set; }
        public decimal PurchasedQty { get; set; }
        public decimal PRAmount { get; set; }

    }
    
}
