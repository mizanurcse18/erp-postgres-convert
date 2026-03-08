
using Approval.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(ApprovalPanel)), Serializable]
    public class ApprovalPanelDto : Auditable
    {
        public int APPanelID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int APTypeID { get; set; }
        public string APTypeName { get; set; }
        public bool IsParallelApproval { get; set; }
        public bool IsDynamicApproval { get; set; }


    }
}
