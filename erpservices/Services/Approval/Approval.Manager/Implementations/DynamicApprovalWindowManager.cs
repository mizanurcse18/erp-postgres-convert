using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;
using Newtonsoft.Json.Linq;

namespace Approval.Manager.Implementations
{
    public class DynamicApprovalWindowManager : ManagerBase, IDynamicApprovalWindowManager
    {
        private readonly IRepository<DynamicApprovalPanelEmployee> DynamicApprovalWindowMasterRepo;
        public DynamicApprovalWindowManager(IRepository<DynamicApprovalPanelEmployee> annualLeaveEncashmentWindowMasterRepo)
        {
            DynamicApprovalWindowMasterRepo = annualLeaveEncashmentWindowMasterRepo;

        }
        public async Task<DynamicApprovalPanelWindowDto> GetDynamicApprovalWindow(int id)
        {
            if (id == 0)
            {
                string sql = @$"SELECT DAPE.* FROM DynamicApprovalPanelEmployee DAPE WHERE DAPE.DAPEID={id}";
                var setting = Task.Run(() => DynamicApprovalWindowMasterRepo.GetModelData<DynamicApprovalPanelWindowDto>(sql));

                return await setting;
            }
            else
            {
                string sql = @$"SELECT DAPE.*
                             , (SELECT DivisionName label, DivisionID value from HRMS..Division
                              WHERE DivisionID IN (Select * from dbo.fnReturnStringArray(DAPE.DivisionIDs,','))
                             FOR JSON PATH) DivisionIDsStr
                             , (SELECT DepartmentName label, DepartmentID value,CONVERT(INT, DivisionID) AS DivisionID from HRMS..Department
                              WHERE DepartmentID IN (Select * from dbo.fnReturnStringArray(DAPE.DepartmentIDs,','))
                             FOR JSON PATH) DepartmentIDsStr
							 , (SELECT FullName label, EmployeeID value from HRMS..Employee
                              WHERE EmployeeID IN (Select * from dbo.fnReturnStringArray(DAPE.EmployeeIDs,','))
                             FOR JSON PATH) EmployeeIDsStr
							 , (SELECT FullName label, EmployeeID value from HRMS..Employee
                              WHERE EmployeeID IN (Select * from dbo.fnReturnStringArray(DAPE.HRProxyEmployeeIDs,','))
                             FOR JSON PATH) HRProxyEmployeeStr
							 , (SELECT Name label, APPanelID value from ApprovalPanel
                              WHERE APPanelID IN (Select * from dbo.fnReturnStringArray(DAPE.ApprovalPanels,','))
                             FOR JSON PATH) ApprovalPanelsIDsStr
                              ,J.JobGradeName
							 ,E.FullName EmployeeName
                             FROM DynamicApprovalPanelEmployee DAPE
                             LEFT JOIN HRMS..Employee E ON E.EmployeeID=DAPE.HREmployeeID
                             LEFT JOIN HRMS..JobGrade J ON J.JobGradeID=DAPE.MaximumJobGrade
                             WHERE DAPE.DAPEID={id}";
                var setting = Task.Run(() => DynamicApprovalWindowMasterRepo.GetModelData<DynamicApprovalPanelWindowDto>(sql));

                return await setting;

            }
        }


        public async Task<(bool, string)> Save(DynamicApprovalPanelWindowDto settings)
        {

            var existingWindowMaster = DynamicApprovalWindowMasterRepo.Entities.SingleOrDefault(x => x.DAPEID == settings.DAPEID);

            string sqlExtEmp = $@"SELECT *
                                    FROM DynamicApprovalPanelEmployee
                                    WHERE IsActive = 1 AND DAPEID <> {settings.DAPEID}
                                    AND (
                                        EXISTS (SELECT 1 FROM STRING_SPLIT('{settings.ApprovalPanels}', ',') WHERE CHARINDEX(value, ApprovalPanels) > 0)
                                        AND (EXISTS (SELECT 1 FROM STRING_SPLIT('{settings.DivisionIDs}', ',') WHERE CHARINDEX(value, DivisionIDs) > 0)
                                        AND EXISTS (SELECT 1 FROM STRING_SPLIT('{settings.DepartmentIDs}', ',') WHERE CHARINDEX(value, DepartmentIDs) > 0))
                                    )";


            var exitPanel = DynamicApprovalWindowMasterRepo.GetModelData<DynamicApprovalPanelWindowDto>(sqlExtEmp);

            if (exitPanel.DAPEID.IsNotZero())
            {
                return (false, $"Sorry, Already Exist!");
            }

            var masterModel = new DynamicApprovalPanelEmployee
            {
                Title = settings.Title,
                HierarchyLevel = settings.HierarchyLevel,
                MaximumJobGrade = settings.MaximumJobGrade,
                Remarks = settings.Remarks,
                DivisionIDs = settings.DivisionIDs,
                DepartmentIDs = settings.DepartmentIDs,
                EmployeeIDs = settings.EmployeeIDs,
                ApprovalPanels = settings.ApprovalPanels,
                IncludeDivisionHead = settings.IncludeDivisionHead,
                IncludeDepartmentHead = settings.IncludeDepartmentHead,
                IncludeHR = settings.IncludeHR,
                HREmployeeID = settings.HREmployeeID,
                HRProxyEmployeeIDs = settings.HRProxyEmployeeIDs,
                MinLimitAmount = settings.MinLimitAmount,
                MaxLimitAmount = settings.MaxLimitAmount,
                IsActive = settings.IsActive,
                ExternalID = settings.ExternalID
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (settings.DAPEID.IsZero() && existingWindowMaster.IsNull())
                {
                    masterModel.SetAdded();
                    SetMasterNewId(masterModel);
                    settings.DAPEID = (int)masterModel.DAPEID;
                }
                else
                {
                    masterModel.DAPEID = existingWindowMaster.DAPEID;
                    masterModel.CreatedBy = existingWindowMaster.CreatedBy;
                    masterModel.CreatedDate = existingWindowMaster.CreatedDate;
                    masterModel.CreatedIP = existingWindowMaster.CreatedIP;
                    masterModel.RowVersion = existingWindowMaster.RowVersion;
                    masterModel.SetModified();
                }


                SetAuditFields(masterModel);



                DynamicApprovalWindowMasterRepo.Add(masterModel);

                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;

            return (true, $"Window with Panel Saved Successfully");

        }



        public async Task<List<DynamicApprovalPanelWindowDto>> GetAll()
        {
            string sql = @$"SELECT *,CASE WHEN IsActive=1 THEN 'Yes' Else 'NO' END ActiveStatus FROM DynamicApprovalPanelEmployee";
            var data = DynamicApprovalWindowMasterRepo.GetDataModelCollection<DynamicApprovalPanelWindowDto>(sql);

            return data;
        }

        private void SetMasterNewId(DynamicApprovalPanelEmployee master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("DynamicApprovalPanelEmployee", AppContexts.User.CompanyID);
            master.DAPEID = code.MaxNumber;
        }
    }
}
