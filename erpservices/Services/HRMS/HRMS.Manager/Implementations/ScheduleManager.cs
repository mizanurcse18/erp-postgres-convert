using Core.Extensions;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class ScheduleManager : ManagerBase, IScheduleManager
    {
        private readonly IRepository<EmployeeLeaveAccount> Repo;
        public ScheduleManager(IRepository<EmployeeLeaveAccount> repo)
        {
            Repo = repo;
        }

        public async Task ExecuteSchedule(ScheduleUtilityDto obj)
        {
            if ((obj.FromDate.IsNull() || obj.FromDate > DateTime.Now || obj.ToDate.IsNull() || obj.ToDate > DateTime.Now ) && obj.ScheduleNo == 1)
                return;

                string sql = "";
            if (obj.ScheduleNo == 1)
                sql = $@"EXEC CursorForAttendanceSchedule '{Convert.ToDateTime(obj.FromDate).ToString("yyyy-MM-dd")}','{Convert.ToDateTime(obj.ToDate).ToString("yyyy-MM-dd")}'";
            else if (obj.ScheduleNo == 2)
                sql = $@"EXEC AssignLeaveToAllEmployee";

            if (sql.IsNotNullOrEmpty())
            {
                Repo.ExecuteSqlCommand(sql);
            }
            
            await Task.CompletedTask;
        }
    }
}
