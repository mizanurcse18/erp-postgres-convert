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
using System.Threading.Tasks;
namespace Security.Manager.Implementations
{
    public class SystemVariableManager : ManagerBase, ISystemVariableManager
    {
        private readonly IRepository<SystemVariable> SystemVariableRepo;

        public SystemVariableManager(IRepository<SystemVariable> systemVariableRepo)
        {
            SystemVariableRepo = systemVariableRepo;
        }

        public async Task<List<SystemVariableDto>> GetSystemVariableList()
        {
            string sql = @"SELECT *, case when IsInactive=0 then 'Active' else 'InActive' end Status FROM SystemVariable ORDER BY SystemVariableID DESC";
            return await Task.FromResult(SystemVariableRepo.GetDataModelCollection<SystemVariableDto>(sql));
        }

        public void SaveChanges(SystemVariableDto systemVariableDto)
        {
            using var unitOfWork = new UnitOfWork();

            var existSystemVariable = SystemVariableRepo.Entities
                .SingleOrDefault(x => x.SystemVariableID == systemVariableDto.SystemVariableID)
                ?.MapTo<SystemVariable>();

            if (existSystemVariable == null || existSystemVariable.SystemVariableID == 0)
            {
                if (systemVariableDto.EntityTypeID == 0)
                {
                    var maxEntityTypeId = SystemVariableRepo.Entities.Any()
        ? SystemVariableRepo.Entities.Max(x => x.EntityTypeID)
        : 0;
                    systemVariableDto.EntityTypeID = maxEntityTypeId + 1;
                }

                systemVariableDto.SetAdded();
                SetNewSystemVariableID(systemVariableDto);
            }
            else
            {
                systemVariableDto.SetModified();
                systemVariableDto.RowVersion = existSystemVariable.RowVersion;
            }

            var systemVariableEnt = systemVariableDto.MapTo<SystemVariable>();
            systemVariableEnt.CompanyID = systemVariableDto.CompanyID ?? AppContexts.User.CompanyID;

            SystemVariableRepo.Add(systemVariableEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewSystemVariableID(SystemVariableDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("SystemVariable", AppContexts.User.CompanyID);
            obj.SystemVariableID = code.MaxNumber;
        }

        public async Task<SystemVariableDto> GetSystemVariable(int systemVariableId)
        {
            var systemVariable = SystemVariableRepo.Entities
                .SingleOrDefault(x => x.SystemVariableID == systemVariableId)
                .MapTo<SystemVariableDto>();
            return await Task.FromResult(systemVariable);
        }

        public void DeleteSystemVariable(int systemVariableId)
        {
            using var unitOfWork = new UnitOfWork();
            var systemVariable = SystemVariableRepo.Entities
                .SingleOrDefault(x => x.SystemVariableID == systemVariableId);
            systemVariable.SetDeleted();
            SystemVariableRepo.Add(systemVariable);

            unitOfWork.CommitChangesWithAudit();
        }
    }
} 