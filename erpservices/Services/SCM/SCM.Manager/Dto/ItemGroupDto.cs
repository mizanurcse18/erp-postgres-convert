using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(ItemGroup)), Serializable]
    public class ItemGroupDto:Auditable
    {

        public long ItemGroupID { get; set; }
        
        public string ItemGroupName { get; set; }
        
        public string ItemGroupDescription { get; set; }
        
        public string ItemGroupCode { get; set; }
        public bool IsRemovable { get; set; }

    }
}
