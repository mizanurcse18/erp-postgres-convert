
using Approval.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(ApprovalType)), Serializable]
    public class ApprovalTypeDto : Auditable
    {
        public int APTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
