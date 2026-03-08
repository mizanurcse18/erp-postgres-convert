
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using System.Collections.Generic;

namespace Approval.API.Models
{
    public class ApprovalPanelForwardEmployeeSaveModel
    {
        public ApprovalPanelForwardEmployeeDto MasterModel { get; set; }
        public List<ApprovalPanelForwardEmployeeDto> ChildModels { get; set; }
    }
}
