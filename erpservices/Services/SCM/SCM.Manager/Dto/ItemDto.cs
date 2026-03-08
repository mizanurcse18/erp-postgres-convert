
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(Item)), Serializable]
    public class ItemDto : Auditable
    {
        public long ItemID { get; set; }
        
        public string ItemName { get; set; }
        public string ItemNameError { get; set; }

        public string ItemDescription { get; set; }
        
        public string ItemCodePrefix { get; set; }
        
        public string ItemCodeSuffix { get; set; }
        
        public string ItemCode { get; set; }
        
        public long? ItemSubGroupID { get; set; }
        public string ItemSubGroupName { get; set; }

        public int? AssetTypeID { get; set; }
        public string AssetTypeName { get; set; }

        public int? InventoryTypeID { get; set; }
        public string InventoryTypeName { get; set; }

        public long? GLID { get; set; }
        public string GLName { get; set; }

        public int? UnitID { set; get; }
        public string UnitName { get; set; }

        public string ItemNature { get; set; }

        public decimal? Price { get; set; }
        
        public string Remarks { get; set; }
        
        public string ExternalID { set; get; }
        public List<Attachments> Attachments { get; set; }

    }
}
