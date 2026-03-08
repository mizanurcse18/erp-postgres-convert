using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class CopyApprovalPanelDto
    {
        public List<ApprovalPanelEmployeeDto> PanleData { get; set; }
        public List<int> ApprovalPanels { get; set; }
        public List<int> Departments { get; set; }
    }
}
