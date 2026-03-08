using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
   public interface IShiftManager
    {
        Task<ShiftingMaster> SaveChanges(ShiftDto shiftDto);
        Task<ShiftDto> GetShfitByShiftingMasterId(int shiftMasterId);
        Task<List<ShiftListDto>> GetShfitList();
        Task RemoveShfitByShiftingMasterId(int shiftMasterId);
    }
}
