using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IComboManager
    {

        Task<List<ComboModel>> GetApprovalPanelCombo();
        Task<IEnumerable<Dictionary<string, object>>> GetDynamicApprovalPanelCombo();
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelByEmployeeCombo(int EmployeeID);
        Task<List<ComboModel>> GetApprovalTypesList();
        Task<List<ComboModel>> GetsTemplateCombo(int isHR);
        
    }
}
