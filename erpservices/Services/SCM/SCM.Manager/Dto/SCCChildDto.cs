using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(SCCChild)), Serializable]
    public class SCCChildDto : Auditable
    {
        public long SCCCID { get; set; }
        public long SCCMID { get; set; }
        public long ItemID { get; set; }
        public int POCID { get; set; }
        public Decimal? ReceivedQty { get; set; }
        public Decimal? SccReceivedQty { get; set; }
        public DateTime? DeliveryOrJobCompletionDate { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public string SCCCNote { get; set; }
        public decimal AlreadyReceivedQty { get; set; }
        public Decimal Rate { get; set; }
        public Decimal TotalAmount { get; set; }
        public decimal TotalAmountIncludingVat { get; set; }
        public decimal VatAmount { get; set; }
        public string Remarks { get; set; }

        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string Description { get; set; }
        public string UnitCode { get; set; }
        public Decimal POQty { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal POTotalAmountIncludingVat { get; set; }
        public decimal POVATAmount { get; set; }
        public decimal POVatPercent { get; set; }
        public decimal GRNReceivedQty { get; set; }

        public string PODescription { get; set; }
        public decimal QCSuppliedQty { get; set; }
        public decimal QCAcceptedQty { get; set; }


    }
}
