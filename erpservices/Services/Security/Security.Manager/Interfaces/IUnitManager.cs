using Core;
using Manager.Core.CommonDto;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IUnitManager
    {
        Task<List<UnitDto>> GetUnitList();
        void SaveChanges(UnitDto unitDto);
        void DeleteUnit(int unitId);
        Task<UnitDto> GetUnit(int unitId);


    }
}
