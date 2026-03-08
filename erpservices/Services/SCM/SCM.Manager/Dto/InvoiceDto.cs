using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class InvoiceDto
    {
        public long InvoiceMasterID { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime InvoiceReceiveDate { get; set; }
        public DateTime? AccountingDate { get; set; }
        public int? InvoiceTypeID { get; set; }
        public long? POMasterID { get; set; }
        public int CurrencyID { get; set; }
        public bool IsAdvanceInvoice { get; set; }
        public string InvoiceDescription { get; set; }
        public long SupplierID { get; set; }
        public string ProjectNumber { get; set; }
        public decimal? CurrencyRate { get; set; }
        public decimal TotalPayableAmount { set; get; }
        public decimal AdvanceDeductionAmount { set; get; }
        public decimal BaseAmount { set; get; }
        public decimal? BasePercent { set; get; }
        public decimal TaxAmount { set; get; }
        public decimal? TaxPercent { set; get; }
        public string ExternalID { set; get; }
        public int ApprovalStatusID { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public int ApprovalProcessID { get; set; } = 0;
        public bool IsDraft { get; set; }
        public List<int> MaterialReceiveIDs { get; set; }
        public List<int>SCCIDs { get; set; }
        public int? GracePeriod { get; set; }

        public string MushakChalanNo { get; set; }

        public DateTime? MushakChalanDate { get; set; }
    }

    public class ChildList
    {
        public long InvoiceChildID { get; set; }
        public long InvoiceMasterID { get; set; }
        public long ItemID { get; set; }
        public long MRCIDOrPOCID { get; set; }
        public Decimal ItemQty { get; set; }
        public Decimal ItemRate { get; set; }
        public Decimal ItemAmount { get; set; }
        public Decimal ItemVatAmount { get; set; }
        public Decimal ItemVatPercent { get; set; }
        public Decimal TotalAmountIncludingVat { get; set; }

    }
}
