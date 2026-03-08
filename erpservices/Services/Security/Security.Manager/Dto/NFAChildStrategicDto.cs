using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Security.Manager
{
    [AutoMap(typeof(NFAChildStrategic)), Serializable]
    public class NFAChildStrategicDto : Auditable
    {
        public int NFACSID { get; set; }
        
        public long ItemID { get; set; }
        
        public string Description { get; set; }
        
        public int UOM { get; set; }
        
        public Decimal Qty { get; set; }
        
        public Decimal UnitPrice { get; set; }
        
        public Decimal TotalAmount { get; set; }
        
        public int NFAMasterID { get; set; }
        public string ItemName { get; set; }
        public string UnitCode { get; set; }
    }
}
