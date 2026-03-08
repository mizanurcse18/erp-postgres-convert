using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(QCChild)), Serializable]
    public class QCChildDto : Auditable
    {
        public long QCCID { get; set; }
        
        
        public long QCMID { get; set; }
        
        
        public long ItemID { get; set; }
        
        
        public int POCID { get; set; }
        
        
        public Decimal SuppliedQty { get; set; }
        
        
        public Decimal AcceptedQty { get; set; }
        
        
        public Decimal RejectedQty { get; set; }
        
        public string QCCNote { get; set; }


        public Decimal TotalAmount { get; set; }
        public decimal ReceivedQty { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string UnitCode { get; set; }
        public Decimal POQty { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal POTotalAmountIncludingVat { get; set; }
        public decimal POVATAmount { get; set; }
        public decimal POVatPercent { get; set; }
        public decimal Rate { get; set; }
        public decimal GRNReceivedQty { get; set; }

        public string PODescription { get; set; }
        public decimal QCSuppliedQty { get; set; }
        public decimal QCAcceptedQty { get; set; }


    }
}
