
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using System.Collections.Generic;

namespace Approval.API.Models
{
    public class ApprovalPanelEmployeeSaveModel
    {
        public ApprovalPanelEmployeeDto MasterModel { get; set; }
        public List<ApprovalPanelEmployeeDto> ChildModels { get; set; }
    }
}
