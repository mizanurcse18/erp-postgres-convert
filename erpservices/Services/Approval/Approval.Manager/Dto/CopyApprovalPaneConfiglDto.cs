using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class CopyApprovalPanelConfigDto
    {
        public List<ApprovalPanelEmployeeConfigDto> PanleData { get; set; }
        public List<int> ApprovalPanels { get; set; }
        public List<int> Departments { get; set; }
    }
}
