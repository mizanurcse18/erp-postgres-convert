using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;

namespace SCM.Manager
{
    [AutoMap(typeof(InvoiceChild)), Serializable]
    public class InvoiceChildDto : Auditable
    {
        public long InvoiceChildID { get; set; }
        public long InvoiceMasterID { get; set; }
        public long ItemID { get; set; }
        public long MRCIDOrPOCID { get; set; }
        public Decimal ItemQty { get; set; }
        public Decimal ItemRate { get; set; }
        public Decimal ItemAmount { get; set; }
    }
}
