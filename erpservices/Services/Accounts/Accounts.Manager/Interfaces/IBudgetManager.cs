using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IBudgetManager
    {

        //Task<IEnumerable<Dictionary<string, object>>> GetAllDeptBudgetList();
        Task<List<BudgetMasterDto>> GetAllDeptBudgetList();
        void SaveBudget(List<BudgetMasterDto> list); 
        Task<IEnumerable<Dictionary<string, object>>> GetIOUExpenseBudget(int DepartmentID);
    }
}
