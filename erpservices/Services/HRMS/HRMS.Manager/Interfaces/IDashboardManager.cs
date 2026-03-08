using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IDashboardManager
    {
        Task<int> GetTotalEmployee();
        Task<int> GetTotalPresentToday();
        Task<int> GetTotalAbsentToday();
        Task<int> GetTotalLeaveToday(); 
         Task<double> GetTotalLatePercent();
        Task<(int, int, int)> GetTotalPendingApproval();
        Task<List<EmployeeDto>> GetAllEmployeeAttendanceForToday();
        Task<List<Dictionary<string,object>>> GetOrganogram();
    }
}
