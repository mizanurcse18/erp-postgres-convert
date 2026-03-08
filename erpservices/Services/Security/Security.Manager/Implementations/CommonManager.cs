using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class CommonManager : ManagerBase, ICommonManager
    {
        private readonly IRepository<SystemVariable> SystemVariableRepo;

        public CommonManager(IRepository<SystemVariable> systemVariableRepo)
        {
            SystemVariableRepo = systemVariableRepo;
        }

        public void DeleteSystemVariable(int systemVariableID)
        {
            using var unitOfWork = new UnitOfWork();
            var model = SystemVariableRepo.Entities.SingleOrDefault(x => x.SystemVariableID == systemVariableID);
            model.SetDeleted();
            SystemVariableRepo.Add(model);
            unitOfWork.CommitChangesWithAudit();
        }

        public async Task<SystemVariableDto> GetSystemVariable(int systemVariableID)
        {
            var data = SystemVariableRepo.Entities.SingleOrDefault(x => x.SystemVariableID == systemVariableID).MapTo<SystemVariableDto>();
            return await Task.FromResult(data);
        }

        public async Task<List<SystemVariableDto>> GetSystemVariableByEntityTypeID(int entityTypeID)
        {
            if (entityTypeID == 10)
            {
                var sql = @$"SELECT SV.*
	                        ,CASE
                                WHEN ELA.LeaveCategoryID IS NULL
                                    THEN CAST(1 AS BIT)

                                ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM Security..SystemVariable SV
                        LEFT JOIN(
                            SELECT DISTINCT LeaveCategoryID

                            FROM HRMS..EmployeeLeaveApplication
                            ) ELA ON ELA.LeaveCategoryID = SV.SystemVariableID

                            WHERE SV.EntityTypeID = {entityTypeID}
                        ORDER BY SV.Sequence ASC";
                var data = Task.Run(() => SystemVariableRepo.GetDataModelCollection<SystemVariableDto>(sql));
                return await data;
            }
            else
            {
                var data = Task.Run(() => SystemVariableRepo.Entities.Where(x => x.EntityTypeID == entityTypeID).ToList().MapTo<List<SystemVariableDto>>());
                return await data;
            }
        }

        public void SaveChanges(SystemVariableDto model)
        {
            using var unitOfWork = new UnitOfWork();
            var existSystemVariable = SystemVariableRepo.Entities.SingleOrDefault(x => x.SystemVariableID == model.SystemVariableID).MapTo<SystemVariable>();

            if (existSystemVariable.IsNull() || existSystemVariable.SystemVariableID.IsZero() || existSystemVariable.IsAdded)
            {
                model.Sequence = SystemVariableRepo.Entities.Where(x => x.EntityTypeID == model.EntityTypeID).Max(y => y.Sequence) + 1;
                model.SetAdded();
                SetNewSystemVariableID(model);
            }
            else
            {
                model.SetModified();
            }
            var systemVariableEnt = model.MapTo<SystemVariable>();
            systemVariableEnt.CompanyID = model.CompanyID ?? AppContexts.User.CompanyID;


            SystemVariableRepo.Add(systemVariableEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewSystemVariableID(SystemVariableDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("SystemVariable", AppContexts.User.CompanyID);
            obj.SystemVariableID = code.MaxNumber;
        }

        public bool DeleteFileFromPath(string folderName, string fileName)
        {
            try
            {
                bool canDelete = false;
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                switch (folderName)
                {
                    case var name when name.Contains("MC"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("BP"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("UDDOKTAS"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("TMRS"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("DSS"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("DM"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("BDO"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("distribution_houses"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("TM_TO_PRO"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("AM"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("RSM"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("CH"):
                        canDelete = true;
                        break;
                    case var name when name.Contains("HQ"):
                        canDelete = true;
                        break;
                }

                return canDelete ? UploadUtil.DeleteFileFromDisk(fileName, folderName) : false;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }


    }
}
