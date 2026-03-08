using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IHolidayManager
    {
        void SaveChanges(List<HolidayDto> holiday);
        Task<List<HolidayListDto>> GetHolidayList();
        Task<List<HolidayListDto>> GetHoliday(int financialYearID);
        void RemoveHoliday(int financialYearID);
    }
}
