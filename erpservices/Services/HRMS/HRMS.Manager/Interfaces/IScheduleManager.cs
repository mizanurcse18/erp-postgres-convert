using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IScheduleManager
    {
        Task ExecuteSchedule(ScheduleUtilityDto obj);
    }
}
