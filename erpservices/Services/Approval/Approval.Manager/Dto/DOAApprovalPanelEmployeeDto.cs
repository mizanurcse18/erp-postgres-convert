using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Approval.DAL.Entities;
using System;
using Core;
using System.Collections.Generic;

namespace Approval.Manager
{
    [AutoMap(typeof(DOAApprovalPanelEmployee)), Serializable]
    public class DOAApprovalPanelEmployeeDto : Auditable
    {
        public long DOAApprovalPanelEmployeeID { get; set; }
        public long DOAMasterID { get; set; }
        public long AssigneeEmployeeID { get; set; }
        public int TypeID { get; set; }
        public int APPanelID { get; set; }
        public long GroupID { get; set; }
        public string APPanelName { get; set; }
        public string EmployeeName { get; set; }
        public string DOATypeName { get; set; }
        public ComboModel DOAType { get; set; }
        public List<ComboModel> MultipleAPPanelDetails { get; set; }
        public List<ComboModel> MultipleEmployeeDetails { get; set; }
    }
}
