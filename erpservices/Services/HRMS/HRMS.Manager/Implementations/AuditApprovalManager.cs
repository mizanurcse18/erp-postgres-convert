using HRMS.Manager.Interfaces;
using Manager.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Core.Util;
using Newtonsoft.Json.Linq;

namespace HRMS.Manager.Implementations
{
    public class AuditApprovalManager : ManagerBase, IAuditApprovalManager
    {
        private readonly IRepository<AuditApprovalConfig> AuditApprovalRepo;
        public AuditApprovalManager(IRepository<AuditApprovalConfig> auditApprovalRepo)
        {
            this.AuditApprovalRepo = auditApprovalRepo;
        }

        public async Task<List<AuditApprovalDto>> GetAll()
        {
            string sql = @$"SELECT AAC.*, AAC.DepartmentEmails Email
                             ,  (SELECT DepartmentName label, DepartmentID value,CONVERT(INT, DivisionID) AS DivisionID from HRMS..Department
                              WHERE DepartmentID IN (Select * from hrms..fnReturnStringArray(AAC.DepartmentIDs,','))
                             FOR JSON PATH) DepartmentIDsStr
							 ,  (SELECT Title label, QuestionID value,CONVERT(INT, QuestionID) AS QuestionID from HRMS..AuditQuestion
                              WHERE QuestionID IN (Select * from hrms..fnReturnStringArray(AAC.QuestionIDs,','))
                             FOR JSON PATH) QuestionIDsStr
                             from HRMS..AuditApprovalConfig AAC";
           var data = AuditApprovalRepo.GetDataModelCollection<AuditApprovalDto>(sql);

            return data;

        }

        public async Task<(bool, string)> Save(List<AuditApprovalDto> dtos)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                var existingConfigs = AuditApprovalRepo.GetAllList();
                var auditApprovalConfigs = new List<AuditApprovalConfig>();

                foreach (var dto in dtos)
                {
                    var config = new AuditApprovalConfig
                    {
                        MapID = dto.MapID,
                        DepartmentEmails = dto.Email,
                        QuestionIDs = string.Join(",", dto.MultiQuestionList.Select(q => q.Value.ToString())),
                        DepartmentIDs = string.Join(",", dto.MultiDepartmentList.Select(d => d.Value.ToString())),
                        Remarks = dto.Remarks,
                        IsActive = dto.IsActive,
                        IsRequired = dto.IsRequired,
                        IsPOSMRequired = dto.IsPOSMRequired
                    };

                    var existingConfig = existingConfigs.FirstOrDefault(c => c.MapID == config.MapID);
                    if (existingConfig != null)
                    {
                        config.CreatedBy = existingConfig.CreatedBy;
                        config.CreatedDate = existingConfig.CreatedDate;
                        config.CreatedIP = existingConfig.CreatedIP;
                        config.RowVersion = existingConfig.RowVersion;
                        config.SetModified();
                    }
                    else
                    {
                        config.SetAdded();
                    }

                    auditApprovalConfigs.Add(config);
                }


                //var removeFiles = existingConfigs.Where(x => !dtos.M.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                // Remove the configurations not present in the dtos
                var dtoMapIds = dtos.Select(dto => dto.MapID).ToList();
                var configsToRemove = existingConfigs
                    .Where(config => !dtoMapIds.Contains(config.MapID))
                    .ToList();

                configsToRemove.ForEach(config =>
                {
                    config.SetDeleted();
                    AuditApprovalRepo.Add(config);
                });


                AuditApprovalRepo.AddRange(auditApprovalConfigs);
                unitOfWork.CommitChangesWithAudit();
             }
                await Task.CompletedTask;
                return (true, $"Window with Panel Saved Successfully");

        }

        
    }

}