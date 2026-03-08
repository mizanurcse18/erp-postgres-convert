using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Approval.Manager.Implementations
{
    public class ApprovalPanelForwardEmployeeManager : ManagerBase, IApprovalPanelForwardEmployeeManager
    {

        private readonly IRepository<ApprovalPanelForwardEmployee> ApprovalPanelEmployeeRepo;
        public ApprovalPanelForwardEmployeeManager(IRepository<ApprovalPanelForwardEmployee> appRepo)
        {
            ApprovalPanelEmployeeRepo = appRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelForwardEmployeeListDic()
        {
            string sql = $@"SELECT APT.*
	                        
                        FROM ApprovalPanelForwardEmployee APT
                        
                        ORDER BY APT.APPanelForwardEmployeeID DESC";
            var listDict = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<List<ApprovalPanelForwardEmployeeDto>> GetApprovalPanelForwardEmployee(int APPanelID, int DivisionID, int DepartmentID)
        {
            string sql = $@"SELECT APE.*, Emp.FullName AS EmployeeName, Emp.EmployeeCode, AP.Name AS PanelName,
                        		D.DivisionName,Dept.DepartmentName
                        FROM ApprovalPanelForwardEmployee APE 
                        LEFT JOIN HRMS..Division D ON APE.DivisionID=D.DivisionID
                        LEFT JOIN HRMS..Department Dept ON APE.DepartmentID=Dept.DepartmentID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID				
                        LEFT JOIN ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
                        WHERE APE.APPanelID={APPanelID} AND APE.DivisionID={DivisionID} AND APE.DepartmentID={DepartmentID} ORDER BY APE.APPanelForwardEmployeeID";

            var list = new List<ApprovalPanelForwardEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelForwardEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<Dictionary<string, object>> GetApprovalPanelForwardEmployeeSingleInfoForEdit(ApprovalPanelForwardEmployeeDto ape)
        {
            string sql = $@"SELECT APE.* , DV.DivisionName, DP.DepartmentName, Emp.FullName AS EmployeeName, AP.Name AS PanelName                       
                            FROM ApprovalPanelForwardEmployee APE
							LEFT JOIN HRMS..Division DV ON DV.DivisionID = APE.DivisionID
							LEFT JOIN HRMS..Department DP ON DP.DepartmentID = APE.DepartmentID
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID						
                            LEFT JOIN ApprovalPanel AP ON APE.APPanelID = AP.APPanelID 
                            WHERE APE.APPanelForwardEmployeeID={ape.APPanelForwardEmployeeID}";


            return await Task.FromResult(ApprovalPanelEmployeeRepo.GetData(sql));

        }


        public async Task Delete(int APPanelForwardEmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var ApprovalPanelEmployeeEnt = ApprovalPanelEmployeeRepo.Entities.Where(x => x.APPanelForwardEmployeeID == APPanelForwardEmployeeID).FirstOrDefault();

                ApprovalPanelEmployeeEnt.SetDeleted();
                ApprovalPanelEmployeeRepo.Add(ApprovalPanelEmployeeEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        public async Task<List<ApprovalPanelForwardEmployeeDto>> SaveReorderedList(List<ApprovalPanelForwardEmployeeDto> childs)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<ApprovalPanelForwardEmployeeDto>();

                foreach (var child in childs)
                {
                    var existsPolicy = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == child.APPanelID && x.DivisionID == child.DivisionID
                && x.DepartmentID == child.DepartmentID && x.EmployeeID == child.EmployeeID).MapTo<ApprovalPanelForwardEmployeeDto>();

                    child.SetModified();

                    child.RowVersion = existsPolicy.RowVersion;
                    child.CreatedBy = existsPolicy.CreatedBy;
                    child.CreatedDate = existsPolicy.CreatedDate;
                    child.CreatedIP = existsPolicy.CreatedIP;

                }
                var childsEnt = childs.MapTo<List<ApprovalPanelForwardEmployee>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                ApprovalPanelEmployeeRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

            }
            await Task.CompletedTask;

            return childs;
        }
        public async Task<ApprovalPanelForwardEmployeeDto> SaveChanges(ApprovalPanelForwardEmployeeDto ApprovalPanelForwardEmployeeDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = new ApprovalPanelForwardEmployeeDto();
                if (ApprovalPanelForwardEmployeeDto.APPanelID == (int)Util.ApprovalPanel.DivisionClearance)
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == ApprovalPanelForwardEmployeeDto.APPanelID && x.DivisionID == ApprovalPanelForwardEmployeeDto.DivisionID
                    && x.EmployeeID == ApprovalPanelForwardEmployeeDto.EmployeeID).MapTo<ApprovalPanelForwardEmployeeDto>();

                }
                if (ApprovalPanelForwardEmployeeDto.APPanelID == (int)Util.ApprovalPanel.ExitInterview)
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == ApprovalPanelForwardEmployeeDto.APPanelID && x.EmployeeID == ApprovalPanelForwardEmployeeDto.EmployeeID).MapTo<ApprovalPanelForwardEmployeeDto>();

                }
                else
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == ApprovalPanelForwardEmployeeDto.APPanelID && x.DivisionID == ApprovalPanelForwardEmployeeDto.DivisionID
                    && x.DepartmentID == ApprovalPanelForwardEmployeeDto.DepartmentID && x.EmployeeID == ApprovalPanelForwardEmployeeDto.EmployeeID).MapTo<ApprovalPanelForwardEmployeeDto>();

                }
                if (existUser.IsNull())// || ApprovalPanelForwardEmployeeDto.APPanelEmployeeID.IsZero()
                {
                    ApprovalPanelForwardEmployeeDto.SetAdded();
                    SetNewUserID(ApprovalPanelForwardEmployeeDto);
                }
                else
                {
                    ApprovalPanelForwardEmployeeDto.SetModified();

                    ApprovalPanelForwardEmployeeDto.APPanelForwardEmployeeID = existUser.APPanelForwardEmployeeID;
                    ApprovalPanelForwardEmployeeDto.RowVersion = existUser.RowVersion;
                    ApprovalPanelForwardEmployeeDto.CreatedBy = existUser.CreatedBy;
                    ApprovalPanelForwardEmployeeDto.CreatedDate = existUser.CreatedDate;
                    ApprovalPanelForwardEmployeeDto.CreatedIP = existUser.CreatedIP;
                }

                var ApprovalPanelEmployeeEnt = ApprovalPanelForwardEmployeeDto.MapTo<ApprovalPanelForwardEmployee>();
                SetAuditFields(ApprovalPanelEmployeeEnt);
                ApprovalPanelEmployeeRepo.Add(ApprovalPanelEmployeeEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
            return ApprovalPanelForwardEmployeeDto;
        }

        private void SetNewUserID(ApprovalPanelForwardEmployeeDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanelForwardEmployee", AppContexts.User.CompanyID);
            obj.APPanelForwardEmployeeID = code.MaxNumber;
        }


        public async Task CopyPanelData(CopyApprovalPanelDto copiedInfo)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var approvalPanelForwardEmployeeList = new List<ApprovalPanelForwardEmployeeDto>();
                var approvalPanelProxyEmployeeList = new List<ApprovalPanelProxyEmployee>();
                foreach (var panel in copiedInfo.ApprovalPanels)
                {
                    foreach (var department in copiedInfo.Departments)
                    {
                        var division = GetListOfDictionaryWithSql($@"SELECT ISNULL(DivisionID,0) AS DivisionID FROM HRMS..Department WHERE DepartmentID={department}").Result;
                        var existingPanel = ApprovalPanelEmployeeRepo.GetAllList(x => x.APPanelID == panel && x.DivisionID == Convert.ToInt32(division[0]["DivisionID"]) && x.DepartmentID == department).ToList();
                        if (existingPanel.Count <= 0)
                        {
                            var dtoModel = copiedInfo.PanleData.Select(x => new ApprovalPanelForwardEmployeeDto
                            {
                                DepartmentID = department,
                                EmployeeID = x.EmployeeID,
                                APPanelID = panel,
                                DivisionID = Convert.ToInt32(division[0]["DivisionID"]),
                            }).ToList();

                            dtoModel.ForEach(x =>
                            {
                                x.SetAdded();
                                SetNewUserID(x);
                               
                            }
                    );
                            approvalPanelForwardEmployeeList.AddRange(dtoModel);

                        }
                    }

                }
                var ApprovalPanelEmployeeList = approvalPanelForwardEmployeeList.MapTo<List<ApprovalPanelForwardEmployee>>();


                SetAuditFields(ApprovalPanelEmployeeList);
                SetAuditFields(approvalPanelProxyEmployeeList);
                ApprovalPanelEmployeeRepo.AddRange(ApprovalPanelEmployeeList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task DeleteCompleteApprovalPanel(int APPanelID, int DivisionID, int DepartmentID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var approvalPanelEmployeeEnts = ApprovalPanelEmployeeRepo.GetAllList(x => x.APPanelID == APPanelID && x.DivisionID == DivisionID
              && x.DepartmentID == DepartmentID).ToList();

                approvalPanelEmployeeEnts.ForEach(x => x.SetDeleted());
                ApprovalPanelEmployeeRepo.AddRange(approvalPanelEmployeeEnts);

                
                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }


    }
}
