using System;
using System.Collections.Generic;
using System.Text;
using Manager.Core.Mapper;
using SCM.DAL.Entities;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(PurchaseRequisitionQuotation)), Serializable]
    public class QuotationDto
    {
        public long PRQID { get; set; }
        public long PRMasterID { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int TaxTypeID { get; set; }
        public string TaxTypeString { get; set; }
        public int ReferenceId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string UnitCode { get; set; }
        public long? ItemID { get; set; }
        public decimal QuotedQty { get; set; }
        public decimal QuotedUnitPrice { get; set; }
        public decimal PurchasedAmount { get; set; }
        public decimal PurchasedQty { get; set; }
        public int PRCID { get; set; }
    }
}
