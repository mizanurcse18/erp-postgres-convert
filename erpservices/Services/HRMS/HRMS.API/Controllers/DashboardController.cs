using API.Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardManager Manager;

        public DashboardController(IDashboardManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAdminDashboardData/{PersonID:int}")]
        public async Task<ActionResult> GetAdminDashboardData(int PersonID)
        {
            var employeeList = Manager.GetAllEmployeeAttendanceForToday().Result;
            int totalEmployee = employeeList.Count();

            int totalPresent = employeeList.Where(x => x.IN_TIME != "").Count();
            int totalLate = employeeList.Where(x => x.LateOrOnTime.Equals("Late")).Count();
            double latePercent = totalLate > 0 ? ((double)totalLate / (double)totalEmployee) * 100 : 0;//await Manager.GetTotalLatePercent();

            double totalAbsent = employeeList.Where(x => x.IN_TIME == "" && x.Leave == "").Count();
            double totalleaveToday = await Manager.GetTotalLeaveToday();

            var pendingList = await Manager.GetTotalPendingApproval();

            return OkResult(new { TotalEmployee = totalEmployee, TotalPresent = totalPresent, TotalLatePercent = latePercent, TotalAbsent = totalAbsent, TotalLeaveToday = totalleaveToday, PendingObj = new { pendingNFA = pendingList.Item1, pendingLeave = pendingList.Item2, pendingRemoteAttendance = pendingList.Item3 } });
        }

        [HttpGet("GetOrganogram")]
        public async Task<ActionResult> GetOrganogram()
        {
            var list = Manager.GetOrganogram().Result;
            return OkResult(new { data = list });
        }
    }
}