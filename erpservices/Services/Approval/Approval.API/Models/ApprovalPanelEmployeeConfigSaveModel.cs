
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using System.Collections.Generic;

namespace Approval.API.Models
{
    public class ApprovalPanelEmployeeConfigSaveModel
    {
        public ApprovalPanelEmployeeConfigDto MasterModel { get; set; }
        public List<ApprovalPanelEmployeeConfigDto> ChildModels { get; set; }
    }
}
