
using Approval.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(APStatus)), Serializable]
    public class APStatusDto : Auditable
    {
        public int APStatusID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
