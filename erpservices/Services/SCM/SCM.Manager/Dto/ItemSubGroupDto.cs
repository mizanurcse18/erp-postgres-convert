
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(ItemSubGroup)), Serializable]
    public class ItemSubGroupDto : Auditable
    {
        public long ItemSubGroupID { get; set; }
       
        public string ItemSubGroupName { get; set; }
        public string ItemSubGroupNameError { get; set; }

        public string ItemSubGroupDescription { get; set; }
      
        public string ItemSubGroupCode { get; set; }

        public long ItemGroupID { get; set; }
        public string ItemGroupName{ get; set; }

        public long? GLID { get; set; }

    }
}
