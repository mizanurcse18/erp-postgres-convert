using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{

    public class UnitManager : ManagerBase, IUnitManager
    {
        private readonly IRepository<Unit> UnitRepo;
        public UnitManager(IRepository<Unit> unitRepo)
        {
            UnitRepo = unitRepo;
        }

        public async Task<List<UnitDto>> GetUnitList()
        {
            string sql = $@"SELECT unit.*, 1 IsRemovable
                        FROM Unit unit ORDER BY unit.UnitID DESC";

            return await Task.FromResult(UnitRepo.GetDataModelCollection<UnitDto>(sql));
        }






        public void SaveChanges(UnitDto unitDto)
        {
            using var unitOfWork = new UnitOfWork();
            var existUnit = UnitRepo.Entities.SingleOrDefault(x => x.UnitID == unitDto.UnitID).MapTo<Unit>();

            if (existUnit.IsNull() || existUnit.UnitID.IsZero() || existUnit.IsAdded)
            {
                unitDto.LelativeFactor = 0;
                unitDto.SetAdded();
                SetNewUnitID(unitDto);
            }
            else
            {
                unitDto.SetModified();
            }
            var userEnt = unitDto.MapTo<Unit>();
            userEnt.CompanyID = unitDto.CompanyID ?? AppContexts.User.CompanyID;


            UnitRepo.Add(userEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewUnitID(UnitDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Unit", AppContexts.User.CompanyID);
            obj.UnitID = code.MaxNumber;
        }









        public async Task<UnitDto> GetUnit(int unitId)
        {
            var unit = UnitRepo.Entities.SingleOrDefault(x => x.UnitID == unitId).MapTo<UnitDto>();
            return await Task.FromResult(unit);
        }








        public void DeleteUnit(int unitId)
        {
            using var unitOfWork = new UnitOfWork();
            var unit = UnitRepo.Entities.SingleOrDefault(x => x.UnitID == unitId);
            unit.SetDeleted();
            UnitRepo.Add(unit);

            unitOfWork.CommitChangesWithAudit();
        }

    }
}
