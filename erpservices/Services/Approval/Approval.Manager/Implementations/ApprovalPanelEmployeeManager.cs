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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace Approval.Manager.Implementations
{
    public class ApprovalPanelEmployeeManager : ManagerBase, IApprovalPanelEmployeeManager
    {

        private readonly IRepository<ApprovalPanelEmployee> ApprovalPanelEmployeeRepo;
        private readonly IRepository<ApprovalPanelProxyEmployee> ApprovalPanelProxyEmployeeRepo;
        private readonly IRepository<ApprovalEmployeeFeedback> ApprovalEmployeeFeedbackRepo;
        private readonly IRepository<ApprovalEmployeeFeedbackRemarks> ApprovalEmployeeFeedbackRemarksRepo;
        private readonly IRepository<ApprovalMultiProxyEmployeeInfo> ApprovalMultiProxyEmployeeInfoRepo;
        public ApprovalPanelEmployeeManager(IRepository<ApprovalPanelEmployee> appRepo, IRepository<ApprovalPanelProxyEmployee> proxyRepo, IRepository<ApprovalEmployeeFeedback> approvalEmployeeFeedbackRepo, IRepository<ApprovalEmployeeFeedbackRemarks> approvalEmployeeFeedbackRemarksRepo
            , IRepository<ApprovalMultiProxyEmployeeInfo> approvalMultiProxyEmployeeInfoRepo)
        {
            ApprovalPanelEmployeeRepo = appRepo;
            ApprovalPanelProxyEmployeeRepo = proxyRepo;
            ApprovalEmployeeFeedbackRepo = approvalEmployeeFeedbackRepo;
            ApprovalEmployeeFeedbackRemarksRepo = approvalEmployeeFeedbackRemarksRepo;
            ApprovalMultiProxyEmployeeInfoRepo = approvalMultiProxyEmployeeInfoRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelEmployeeListDic()
        {
            string sql = $@"SELECT APT.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName
	                        --,CASE 
		                       -- WHEN RG.ApprovalPanelEmployeeID IS NULL
			                      --  THEN CAST(1 AS BIT)
		                       -- ELSE CAST(0 AS BIT)
		                       -- END IsRemovable
                        FROM ApprovalPanelEmployee APT
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID
                        --LEFT JOIN (
	                       -- SELECT DISTINCT ApprovalPanelEmployeeID
	                       -- FROM Region
	                       -- ) RG ON APT.ApprovalPanelEmployeeID = RG.ApprovalPanelEmployeeID
                        ORDER BY APT.APPanelEmployeeID DESC";
            var listDict = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployee(int APPanelID, int DivisionID, int DepartmentID)
        {
            string condition = "";
            if (APPanelID != (int)Util.ApprovalPanel.EmployeeProfileApproval && APPanelID != (int)Util.ApprovalPanel.AccessDeactivation)
            {
                condition = $@" AND APE.DivisionID={DivisionID} AND APE.DepartmentID={DepartmentID}";
            }
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName, VAE.ImagePath
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
                        INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
                        WHERE APE.APPanelID={APPanelID} {condition} ORDER BY APE.SequenceNo";

            var list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);
            list.ForEach(x =>
            {
                if (x.IsProxyEmployeeEnabled ?? false && x.IsMultiProxy)
                {
                    string proxySql = @$"SELECT	AP.*,
                        		Emp.FullName AS ProxyEmployeeName,
                                Emp.ImagePath AS ProxyEmployeeImagePath,
                        		D.DivisionName,Dept.DepartmentName 
                        		FROM ApprovalPanelProxyEmployee AP
                        		LEFT JOIN HRMS..Division D ON AP.DivisionID=D.DivisionID
                        		LEFT JOIN HRMS..Department Dept ON AP.DepartmentID=Dept.DepartmentID
                        		LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
                        WHERE APPanelEmployeeID={x.APPanelEmployeeID}";
                    x.MultipleProxyDetails = ApprovalPanelProxyEmployeeRepo.GetDataModelCollection<MultipleProxyDetailsDto>(proxySql);
                }
            });

            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeList(int EmployeeID, int APPanelID)
        {
            string filter = APPanelID != (int)Util.ApprovalPanel.HRSupportDocApproval ? " AND APE.DivisionID=Emp.DivisionID AND APE.DepartmentID=Emp.DepartmentID " : "";
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
						INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
						INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
                        WHERE APE.APPanelID={APPanelID} {filter}
                        ORDER BY APE.SequenceNo";

            var list = new List<ApprovalPanelEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListByPanelID(int EmployeeID, int APPanelID)
        {
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
						INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
						INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
                        WHERE APE.APPanelID={APPanelID}
                        ORDER BY APE.SequenceNo";

            var list = new List<ApprovalPanelEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<ApprovalPanelEmployeeDto> GetApprovalPanelEmployeeSingleInfoForEdit(ApprovalPanelEmployeeDto ape)
        {
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID WHERE APE.APPanelEmployeeID={ape.APPanelEmployeeID}";

            var model = ApprovalPanelEmployeeRepo.GetModelData<ApprovalPanelEmployeeDto>(sql);
            if (model.IsProxyEmployeeEnabled ?? false && model.IsMultiProxy)
            {
                string proxySql = @$"SELECT	AP.*,
                        		Emp.EmployeeCode+'-'+Emp.FullName AS ProxyEmployeeName,
                                Emp.ImagePath AS ProxyEmployeeImagePath,
                        		D.DivisionName,
								D.DivisionID,
								Dept.DepartmentName,
								Dept.DepartmentID
                        		FROM ApprovalPanelProxyEmployee AP
                        		LEFT JOIN HRMS..Division D ON AP.DivisionID=D.DivisionID
                        		LEFT JOIN HRMS..Department Dept ON AP.DepartmentID=Dept.DepartmentID
                        		LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
                        WHERE APPanelEmployeeID={ape.APPanelEmployeeID}";
                model.MultipleProxyDetails = ApprovalPanelProxyEmployeeRepo.GetDataModelCollection<MultipleProxyDetailsDto>(proxySql);
            }
            return await Task.FromResult(model);

        }


        public async Task Delete(int APPanelEmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var ApprovalPanelEmployeeEnt = ApprovalPanelEmployeeRepo.Entities.Where(x => x.APPanelEmployeeID == APPanelEmployeeID).FirstOrDefault();

                ApprovalPanelEmployeeEnt.SetDeleted();
                ApprovalPanelEmployeeRepo.Add(ApprovalPanelEmployeeEnt);

                var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPanelEmployeeID == APPanelEmployeeID).ToList();
                removeProxyEmployeeList.ForEach(x => x.SetDeleted());
                ApprovalPanelProxyEmployeeRepo.AddRange(removeProxyEmployeeList);

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

                var removeMultipleProxy = new List<ApprovalPanelProxyEmployee>();
                foreach (var data in approvalPanelEmployeeEnts)
                {
                    var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPanelEmployeeID == data.APPanelEmployeeID).ToList();
                    removeProxyEmployeeList.ForEach(x => x.SetDeleted());
                    removeMultipleProxy.AddRange(removeProxyEmployeeList);
                }
                ApprovalPanelProxyEmployeeRepo.AddRange(removeMultipleProxy);
                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task<List<ApprovalPanelEmployeeDto>> SaveReorderedList(List<ApprovalPanelEmployeeDto> childs)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<ApprovalPanelEmployeeDto>();

                foreach (var child in childs)
                {
                    var existsPolicy = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == child.APPanelID && x.DivisionID == child.DivisionID
                && x.DepartmentID == child.DepartmentID && x.EmployeeID == child.EmployeeID).MapTo<ApprovalPanelEmployeeDto>();

                    child.SetModified();

                    child.RowVersion = existsPolicy.RowVersion;
                    child.CreatedBy = existsPolicy.CreatedBy;
                    child.CreatedDate = existsPolicy.CreatedDate;
                    child.CreatedIP = existsPolicy.CreatedIP;

                }
                var childsEnt = childs.MapTo<List<ApprovalPanelEmployee>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                ApprovalPanelEmployeeRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

            }
            await Task.CompletedTask;

            return childs;
        }
        public async Task<ApprovalPanelEmployeeDto> SaveChanges(ApprovalPanelEmployeeDto approvalPanelEmployeeDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = new ApprovalPanelEmployeeDto();
                if (approvalPanelEmployeeDto.APPanelID == (int)Util.ApprovalPanel.DivisionClearance)
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == approvalPanelEmployeeDto.APPanelID && x.DivisionID == approvalPanelEmployeeDto.DivisionID
                    && x.EmployeeID == approvalPanelEmployeeDto.EmployeeID).MapTo<ApprovalPanelEmployeeDto>();

                }
                if (approvalPanelEmployeeDto.APPanelID == (int)Util.ApprovalPanel.ExitInterview)
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == approvalPanelEmployeeDto.APPanelID && x.EmployeeID == approvalPanelEmployeeDto.EmployeeID).MapTo<ApprovalPanelEmployeeDto>();

                }
                else
                {
                    existUser = ApprovalPanelEmployeeRepo.Entities.SingleOrDefault(x => x.APPanelID == approvalPanelEmployeeDto.APPanelID && x.DivisionID == approvalPanelEmployeeDto.DivisionID
                    && x.DepartmentID == approvalPanelEmployeeDto.DepartmentID && x.EmployeeID == approvalPanelEmployeeDto.EmployeeID).MapTo<ApprovalPanelEmployeeDto>();

                }
                approvalPanelEmployeeDto.ProxyEmployeeID = approvalPanelEmployeeDto.IsProxyEmployeeEnabled ?? false == true ? approvalPanelEmployeeDto.ProxyEmployeeID : null;

                if (existUser.IsNull())// || ApprovalPanelEmployeeDto.APPanelEmployeeID.IsZero()
                {
                    approvalPanelEmployeeDto.SetAdded();
                    SetNewUserID(approvalPanelEmployeeDto);
                }
                else
                {
                    approvalPanelEmployeeDto.SetModified();

                    approvalPanelEmployeeDto.APPanelEmployeeID = existUser.APPanelEmployeeID;
                    approvalPanelEmployeeDto.RowVersion = existUser.RowVersion;
                    approvalPanelEmployeeDto.CreatedBy = existUser.CreatedBy;
                    approvalPanelEmployeeDto.CreatedDate = existUser.CreatedDate;
                    approvalPanelEmployeeDto.CreatedIP = existUser.CreatedIP;
                }

                var ApprovalPanelEmployeeEnt = approvalPanelEmployeeDto.MapTo<ApprovalPanelEmployee>();
                SetAuditFields(ApprovalPanelEmployeeEnt);
                ApprovalPanelEmployeeRepo.Add(ApprovalPanelEmployeeEnt);


                if (existUser.IsNotNull())
                {
                    var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPanelEmployeeID == existUser.APPanelEmployeeID).ToList();
                    removeProxyEmployeeList.ForEach(x => x.SetDeleted());
                    ApprovalPanelProxyEmployeeRepo.AddRange(removeProxyEmployeeList);
                }
                if (approvalPanelEmployeeDto.IsMultiProxy)
                {
                    var proxyEmployeeList = approvalPanelEmployeeDto.MultipleProxyDetails.Select(x => new ApprovalPanelProxyEmployee
                    {
                        APPanelEmployeeID = approvalPanelEmployeeDto.APPanelEmployeeID,
                        DivisionID = x.DivisionID,
                        DepartmentID = x.DepartmentID,
                        EmployeeID = x.EmployeeID,
                        APPanelID = approvalPanelEmployeeDto.APPanelID
                    }).ToList();
                    proxyEmployeeList.Where(x => x.EmployeeID != approvalPanelEmployeeDto.EmployeeID && x.EmployeeID != 0).ToList().ForEach(x =>
                    {
                        x.SetAdded();
                        SetApprovalPanelProxyEmployeeNewID(x);

                    });
                    SetAuditFields(proxyEmployeeList);
                    ApprovalPanelProxyEmployeeRepo.AddRange(proxyEmployeeList);
                }


                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
            return approvalPanelEmployeeDto;
        }

        public async Task SaveReplaceOrProxyForPendingList(ReplaceOrProxyForPendingListDto model)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                List<ApprovalEmployeeFeedback> existApprovalEmployeeFeedback = new List<ApprovalEmployeeFeedback>();
                List<ApprovalEmployeeFeedbackRemarks> ApprovalEmployeeFeedbackRemarksList = new List<ApprovalEmployeeFeedbackRemarks>();
                List<ApprovalMultiProxyEmployeeInfo> ApprovalMultiProxyEmployeeInfoList = new List<ApprovalMultiProxyEmployeeInfo>();
                List<ApprovalMultiProxyEmployeeInfo> existingMultiProxy = new List<ApprovalMultiProxyEmployeeInfo>();


                existApprovalEmployeeFeedback = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.APFeedbackID == 2 && x.EmployeeID == model.ReplacingEmployeeID).ToList();

                existApprovalEmployeeFeedback.ForEach(x =>
                {
                    if (model.IsReplaced)
                    {
                        x.EmployeeID = model.ReplacedEmployeeID;
                        x.SetModified();

                        ApprovalEmployeeFeedbackRemarks newRemarks = new ApprovalEmployeeFeedbackRemarks();
                        newRemarks.Remarks = "Approval Panel Updated. " + model.ReplacingEmployeeName + " is replaced by " + model.ReplacedEmployeeName + ". Remarks:  " + model.ReplaceProxyRemarks;
                        newRemarks.ApprovalProcessID = x.ApprovalProcessID;
                        newRemarks.APFeedbackID = (int)Util.ApprovalFeedback.MemberReplaced;
                        newRemarks.EmployeeID = (int)AppContexts.User.EmployeeID;
                        newRemarks.RemarksDateTime = DateTime.Now;
                        newRemarks.SetAdded();

                        SetNewApprovalEmployeeFeedbackRemarksID(newRemarks);
                        newRemarks.APEmployeeFeedbackRemarksID = (int)newRemarks.APEmployeeFeedbackRemarksID;
                        ApprovalEmployeeFeedbackRemarksList.Add(newRemarks);
                    }
                    else
                    {
                        x.IsMultiProxy = true;
                        x.IsProxyEmployeeEnabled = true;
                        x.SetModified();

                        existingMultiProxy = ApprovalMultiProxyEmployeeInfoRepo.Entities.Where(y => y.APEmployeeFeedbackID == x.APEmployeeFeedbackID && y.ApprovalProcessID == x.ApprovalProcessID).ToList();



                        ApprovalEmployeeFeedbackRemarks newRemarks = new ApprovalEmployeeFeedbackRemarks();
                        newRemarks.Remarks = "Approval Panel Updated. Proxy Member Added. Remarks: " + model.ReplaceProxyRemarks;
                        newRemarks.ApprovalProcessID = x.ApprovalProcessID;
                        newRemarks.APFeedbackID = (int)Util.ApprovalFeedback.ProxyAdded;
                        newRemarks.EmployeeID = (int)AppContexts.User.EmployeeID;
                        newRemarks.RemarksDateTime = DateTime.Now;
                        newRemarks.SetAdded();

                        SetNewApprovalEmployeeFeedbackRemarksID(newRemarks);
                        newRemarks.APEmployeeFeedbackRemarksID = (int)newRemarks.APEmployeeFeedbackRemarksID;
                        ApprovalEmployeeFeedbackRemarksList.Add(newRemarks);


                        foreach (var item in model.ProxyEmployeeForPendingList)
                        {
                            ApprovalMultiProxyEmployeeInfoList.Add(new ApprovalMultiProxyEmployeeInfo
                            {

                                APEmployeeFeedbackID = x.APEmployeeFeedbackID,
                                ApprovalProcessID = x.ApprovalProcessID,
                                EmployeeID = item.value,
                                DivisionID = Convert.ToInt32(item.extraJsonProps.Split("-")[0]),
                                DepartmentID = Convert.ToInt32(item.extraJsonProps.Split("-")[1])

                            });
                        }
                        ApprovalMultiProxyEmployeeInfoList.ForEach(y =>
                        {
                            {
                                y.SetAdded();
                                SetNewApprovalMultiProxyEmployeeInfoID(y);
                            }
                        });
                    }
                });
                existingMultiProxy.ForEach(x => x.SetDeleted());

                SetAuditFields(existApprovalEmployeeFeedback);
                SetAuditFields(ApprovalEmployeeFeedbackRemarksList);
                SetAuditFields(existingMultiProxy);
                SetAuditFields(ApprovalMultiProxyEmployeeInfoList);

                ApprovalEmployeeFeedbackRepo.AddRange(existApprovalEmployeeFeedback);
                ApprovalEmployeeFeedbackRemarksRepo.AddRange(ApprovalEmployeeFeedbackRemarksList);
                ApprovalMultiProxyEmployeeInfoRepo.AddRange(existingMultiProxy);
                ApprovalMultiProxyEmployeeInfoRepo.AddRange(ApprovalMultiProxyEmployeeInfoList);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public async Task CopyPanelData(CopyApprovalPanelDto copiedInfo)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var approvalPanelEmployeeList = new List<ApprovalPanelEmployeeDto>();
                var approvalPanelProxyEmployeeList = new List<ApprovalPanelProxyEmployee>();
                foreach (var panel in copiedInfo.ApprovalPanels)
                {
                    foreach (var department in copiedInfo.Departments)
                    {
                        var division = GetListOfDictionaryWithSql($@"SELECT ISNULL(DivisionID,0) AS DivisionID FROM HRMS..Department WHERE DepartmentID={department}").Result;
                        var existingPanel = ApprovalPanelEmployeeRepo.GetAllList(x => x.APPanelID == panel && x.DivisionID == Convert.ToInt32(division[0]["DivisionID"]) && x.DepartmentID == department).ToList();
                        if (existingPanel.Count <= 0)
                        {
                            var dtoModel = copiedInfo.PanleData.Select(x => new ApprovalPanelEmployeeDto
                            {
                                DepartmentID = department,
                                EmployeeID = x.EmployeeID,
                                SequenceNo = x.SequenceNo,
                                ProxyEmployeeID = x.ProxyEmployeeID,
                                IsProxyEmployeeEnabled = x.IsProxyEmployeeEnabled,
                                APPanelID = panel,
                                DivisionID = Convert.ToInt32(division[0]["DivisionID"]),
                                IsEditable = x.IsEditable,
                                NFAApprovalSequenceType = x.NFAApprovalSequenceType,
                                IsSCM = x.IsSCM,
                                IsMultiProxy = x.IsMultiProxy,
                                MultipleProxyDetails = x.MultipleProxyDetails
                            }).ToList();

                            dtoModel.ForEach(x =>
                            {
                                x.SetAdded();
                                SetNewUserID(x);
                                if (x.IsMultiProxy)
                                {
                                    var proxyEmployeeList = x.MultipleProxyDetails.Select(y => new ApprovalPanelProxyEmployee
                                    {
                                        APPanelEmployeeID = x.APPanelEmployeeID,
                                        DivisionID = y.DivisionID,
                                        DepartmentID = y.DepartmentID,
                                        EmployeeID = y.EmployeeID,
                                        APPanelID = x.APPanelID
                                    }).ToList();
                                    proxyEmployeeList.Where(y => y.EmployeeID != x.EmployeeID && y.EmployeeID != 0).ToList().ForEach(x =>
                                    {
                                        x.SetAdded();
                                        SetApprovalPanelProxyEmployeeNewID(x);

                                    });
                                    approvalPanelProxyEmployeeList.AddRange(proxyEmployeeList);
                                }
                            }
                    );
                            approvalPanelEmployeeList.AddRange(dtoModel);

                        }
                    }

                }
                var ApprovalPanelEmployeeList = approvalPanelEmployeeList.MapTo<List<ApprovalPanelEmployee>>();


                SetAuditFields(ApprovalPanelEmployeeList);
                SetAuditFields(approvalPanelProxyEmployeeList);
                ApprovalPanelEmployeeRepo.AddRange(ApprovalPanelEmployeeList);
                ApprovalPanelProxyEmployeeRepo.AddRange(approvalPanelProxyEmployeeList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetNewApprovalMultiProxyEmployeeInfoID(ApprovalMultiProxyEmployeeInfo obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalMultiProxyEmployeeInfo", AppContexts.User.CompanyID);
            obj.AMPEIID = code.MaxNumber;
        }
        private void SetNewApprovalEmployeeFeedbackRemarksID(ApprovalEmployeeFeedbackRemarks obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalEmployeeFeedbackRemarks", AppContexts.User.CompanyID);
            obj.APEmployeeFeedbackRemarksID = code.MaxNumber;
        }
        private void SetNewUserID(ApprovalPanelEmployeeDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanelEmployee", AppContexts.User.CompanyID);
            obj.APPanelEmployeeID = code.MaxNumber;
        }
        private void SetApprovalPanelProxyEmployeeNewID(ApprovalPanelProxyEmployee obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanelProxyEmployee", AppContexts.User.CompanyID);
            obj.APPanelProxyEmployeeID = code.MaxNumber;
        }


        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeaveOld(int EmployeeID, int APPanelID)
        {
            string sql = $@"SELECT
	                    APPanelEmployeeID,
	                    NFAApprovalSequenceTypeName,
	                    ApproverPhotoUrl,
	                    EmployeeName,
	                    ROW_NUMBER() OVER(ORDER BY SequenceNo) SequenceNo
                    FROM
                    (
                    SELECT 
		                    ROW_NUMBER() OVER(ORDER BY VESM.EmployeeID) APPanelEmployeeID, 
		                    SV.SystemVariableCode NFAApprovalSequenceTypeName,
		                    Emp.ImagePath ApproverPhotoUrl,
		                    VESM.SupervisorFullName EmployeeName,
		                    0 SequenceNo
	                    FROM 
		                    HRMS..ViewALLEmployee Emp 
		                    LEFT JOIN HRMS..ViewEmployeeSupervisorMap VESM ON Emp.EmployeeID = VESM.EmployeeSupervisorID
		                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = 62
	                    WHERE VESM.EmployeeID = {EmployeeID}

                    UNION ALL

                    SELECT 
	                    APE.APPanelEmployeeID, 
	                    SV.SystemVariableCode AS NFAApprovalSequenceTypeName,
	                    VAE.ImagePath ApproverPhotoUrl,
	                    EmployeeName,
	                    ROW_NUMBER() OVER(ORDER BY APE.SequenceNo) SequenceNo
                    FROM 
	                    viewMainApprovalPanelEmployee APE
	                    LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
	                    INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
	                    INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
	                    WHERE APE.APPanelID={APPanelID} AND APE.DivisionID=Emp.DivisionID AND APE.DepartmentID=Emp.DepartmentID 
	                    AND APE.EmployeeID NOT IN (SELECT EmployeeSupervisorID FROM HRMS..ViewEmployeeSupervisorMap WHERE EmployeeID = {EmployeeID})
                    )A";

            var list = new List<ApprovalPanelEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal Days)
        {
            string sql = $@"EXEC HRMS..spGetLeaveApprovalPanel {EmployeeID},{LeaveTypeID},{(IsLFA == true ? 1 : 0)},{(IsFestival == true ? 1 : 0)}, {Days}";

            var list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        //public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal Days)
        //{

        //    {
        //        int maxRetries = 3;
        //        int retryCount = 0;
        //        int baseDelay = 1000; // Base delay of 1 second

        //        while (true)
        //        {
        //            try
        //            {
        //                // Use a transaction with ReadUncommitted isolation level
        //                using (var scope = new TransactionScope(
        //                    TransactionScopeOption.Required,
        //                    new TransactionOptions
        //                    {
        //                        IsolationLevel = IsolationLevel.ReadUncommitted
        //                    },
        //                    TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    string sql = $@"EXEC HRMS..spGetLeaveApprovalPanel {EmployeeID},{LeaveTypeID},{(IsLFA == true ? 1 : 0)},{(IsFestival == true ? 1 : 0)}, {Days}";

        //                    var list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);

        //                    scope.Complete();
        //                    return list;
        //                }
        //            }
        //            catch (SqlException ex) when (ex.Number == 1205) // SQL Server deadlock victim error
        //            {
        //                retryCount++;

        //                if (retryCount >= maxRetries)
        //                {
        //                    throw new DeadlockException(
        //                        $"Maximum retry attempts ({maxRetries}) reached while attempting to resolve deadlock",
        //                        ex);
        //                }

        //                // Exponential backoff with jitter
        //                int delay = baseDelay * (int)Math.Pow(2, retryCount - 1);
        //                Random jitter = new Random();
        //                delay += jitter.Next(0, 100); // Add random jitter between 0-100ms

        //                await Task.Delay(delay);
        //                continue;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception($"Error getting approval panel employees: {ex.Message}", ex);
        //            }
        //        }
        //    }
        //}


        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeaveEncashment()
        {
            string sql = $@"EXEC HRMS..spGetLeaveEncashmentApprovalPanel {AppContexts.User.EmployeeID}";

            var list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }

        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForStNfa(int EmployeeID, int APPanelID, int TemplateID)
        {
            string filter = APPanelID != (int)Util.ApprovalPanel.HRSupportDocApproval ? " AND APEE.DivisionID=Emp.DivisionID AND APEE.DepartmentID=Emp.DepartmentID " : "";
            //      string sql = $@"select t.* from 
            //                  (SELECT APEE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
            //                  FROM viewMainApprovalPanelEmployee APEE
            //                  LEFT JOIN Security..SystemVariable SV ON APEE.NFAApprovalSequenceType = SV.SystemVariableID 
            //INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APEE.EmployeeID
            //INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
            //                  WHERE APEE.APPanelID={APPanelID} {filter} 

            //UNION ALL
            //                  SELECT APE.APPEConfigID APPanelEmployeeID,Emp.DepartmentID,APE.EmployeeID,APE.SequenceNo,APE.ProxyEmployeeID,APE.IsProxyEmployeeEnabled,APE.CompanyID
            //	,APE.CreatedBy,APE.CreatedDate,APE.CreatedIP,APE.UpdatedBy,APE.UpdatedDate,APE.UpdatedIP,APE.RowVersion ,APE.APPanelID,Emp.DivisionID,
            //	APE.IsEditable,54 NFAApprovalSequenceType,0 IsSCM,APE.IsMultiProxy,''Particulars,DivisionName, DepartmentName, Emp.FullName AS EmployeeName,
            //	EmpPr.FullName AS ProxyEmployeeName,'', AP.Name AS PanelName,SV.SystemVariableCode AS NFAApprovalSequenceTypeName,Emp.ImagePath ApproverPhotoUrl
            //	FROM ApprovalPanelEmployeeConfig APE
            //	LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID
            //                      LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
            //                      LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID						
            //                      LEFT JOIN ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
            //	WHERE APE.APPanelID = {APPanelID}
            //	)AS t

            //	ORDER BY t.SequenceNo";

            string sql = $@"select 
                        ROW_NUMBER() OVER (ORDER BY t.SequenceNo) AS SequenceNo,  t.* from 
                        (					
						SELECT 
						APEE.APPanelEmployeeID,APEE.DepartmentID,APEE.EmployeeID,APEE.SequenceNo,APEE.ProxyEmployeeID,APEE.IsProxyEmployeeEnabled,APEE.CompanyID
						,APEE.CreatedBy,APEE.CreatedDate,APEE.CreatedIP,APEE.UpdatedBy,APEE.UpdatedDate,APEE.UpdatedIP,APEE.RowVersion ,APEE.APPanelID,APEE.DivisionID,
						APEE.IsEditable,APEE.NFAApprovalSequenceType,APEE.IsSCM,APEE.IsMultiProxy,APEE.Particulars, APEE.DivisionName, APEE.DepartmentName, VAE.FullName AS EmployeeName,
						VAE.FullName AS ProxyEmployeeName, APEE.PanelName AS PanelName,				
						SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
                        FROM viewMainApprovalPanelEmployee APEE
                        LEFT JOIN Security..SystemVariable SV ON APEE.NFAApprovalSequenceType = SV.SystemVariableID 
						INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APEE.EmployeeID
						INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
                        WHERE APEE.APPanelID={APPanelID} {filter} 
						
						UNION ALL
                        SELECT APE.APPEConfigID APPanelEmployeeID,Emp.DepartmentID,APE.EmployeeID,APE.SequenceNo,APE.ProxyEmployeeID,APE.IsProxyEmployeeEnabled,APE.CompanyID
							,APE.CreatedBy,APE.CreatedDate,APE.CreatedIP,APE.UpdatedBy,APE.UpdatedDate,APE.UpdatedIP,APE.RowVersion ,APE.APPanelID,Emp.DivisionID,
							APE.IsEditable,54 NFAApprovalSequenceType,0 IsSCM,APE.IsMultiProxy,''Particulars,DivisionName, DepartmentName, Emp.FullName AS EmployeeName,
							EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName,SV.SystemVariableCode AS NFAApprovalSequenceTypeName,Emp.ImagePath ApproverPhotoUrl
							FROM ApprovalPanelEmployeeConfig APE
							LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID						
                            LEFT JOIN ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							WHERE APE.APPanelID = {APPanelID}
							)AS t
                    
							ORDER BY t.SequenceNo";

            var list = new List<ApprovalPanelEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }


        public async Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListByMinMaxLimit(int EmployeeID, int APTypeID, double total)
        {
            string sql = $@"SELECT  DISTINCT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl,BM.AttachmentRequiredAmount
                        FROM 
                        Accounts..BudgetChildWithApprovalPanelMap BCAP
                        INNER JOIN Accounts..BudgetChild BC ON BC.BudgetChildID = BCAP.BudgetChildID
                        INNER JOIN Accounts..BudgetMaster BM ON BC.BudgetMasterID = BM.BudgetMasterID
                        INNER JOIN Approval..ApprovalPanel AP ON AP.APPanelID = BCAP.APPanelID

						INNER JOIN viewMainApprovalPanelEmployee APE ON APE.APPanelID = BCAP.APPanelID AND APE.DepartmentID = BM.DepartmentID
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
                        INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
                        WHERE APE.DivisionID={AppContexts.User.DivisionID} AND APE.DepartmentID={AppContexts.User.DepartmentID}
                        AND {total}  BETWEEN BC.MinAmount AND BC.MaxAmount AND AP.APTypeID = {APTypeID}
                            
                        ORDER BY APE.SequenceNo";

            var list = new List<ApprovalPanelEmployeeDto>();
            list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }

        public async Task<List<ApprovalPanelEmployeeDto>> GetDynamicApprovalPanelEmployeeList(int EmployeeID, int ApTypeID, decimal Amount)
        {
            string sql = $@"EXEC ACCOUNTS..spGetDynamicApprovalPanel {EmployeeID},{AppContexts.User.DivisionID},{AppContexts.User.DepartmentID}, {Amount}, {ApTypeID}";

            var list = ApprovalPanelEmployeeRepo.GetDataModelCollection<ApprovalPanelEmployeeDto>(sql);


            await Task.CompletedTask;
            return list;
        }


    }
}
