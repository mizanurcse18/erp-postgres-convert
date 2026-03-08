using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(RTVChild)), Serializable]
    public class RTVChildDto : Auditable
    {
        public long RTVCID { get; set; }
        
        
        public long RTVMID { get; set; }
        
        
        public long ItemID { get; set; }
        
        
        public int POCID { get; set; }
        
        
        public Decimal SuppliedQty { get; set; }
        
        
        public Decimal ReturnQty { get; set; }
        
        public string RTVCNote { get; set; }


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
