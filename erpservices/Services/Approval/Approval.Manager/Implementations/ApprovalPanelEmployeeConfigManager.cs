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
    public class ApprovalPanelEmployeeConfigManager : ManagerBase, IApprovalPanelEmployeeConfigManager
    {

        private readonly IRepository<ApprovalPanelEmployeeConfig> ApprovalPanelEmployeeConfigRepo;
        private readonly IRepository<ApprovalPanelProxyEmployeeConfig> ApprovalPanelProxyEmployeeRepo;
        private readonly IRepository<ApprovalEmployeeFeedback> ApprovalEmployeeFeedbackRepo;
        private readonly IRepository<ApprovalEmployeeFeedbackRemarks> ApprovalEmployeeFeedbackRemarksRepo;
        private readonly IRepository<ApprovalMultiProxyEmployeeInfo> ApprovalMultiProxyEmployeeInfoRepo;
        public ApprovalPanelEmployeeConfigManager(IRepository<ApprovalPanelEmployeeConfig> appRepo, IRepository<ApprovalPanelProxyEmployeeConfig> proxyRepo, IRepository<ApprovalEmployeeFeedback> approvalEmployeeFeedbackRepo, IRepository<ApprovalEmployeeFeedbackRemarks> approvalEmployeeFeedbackRemarksRepo
            , IRepository<ApprovalMultiProxyEmployeeInfo> approvalMultiProxyEmployeeInfoRepo)
        {
            ApprovalPanelEmployeeConfigRepo = appRepo;
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
                        FROM ApprovalPanelEmployeeConfig APT
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID
                        --LEFT JOIN (
	                       -- SELECT DISTINCT ApprovalPanelEmployeeID
	                       -- FROM Region
	                       -- ) RG ON APT.ApprovalPanelEmployeeID = RG.ApprovalPanelEmployeeID
                        ORDER BY APT.APPEConfigID DESC";
            var listDict = ApprovalPanelEmployeeConfigRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployee(int APPanelID)
        {
            string condition = "";
            //if (APPanelID != (int)Util.ApprovalPanel.EmployeeProfileApproval && APPanelID != (int)Util.ApprovalPanel.AccessDeactivation)
            //{
            //    condition = $@" AND APE.DivisionID={DivisionID} AND APE.DepartmentID={DepartmentID}";
            //}

            //string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName, VAE.ImagePath
            //            FROM viewMainApprovalPanelEmployee APE
            //            LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
            //            INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
            //            WHERE APE.APPanelID={APPanelID} {condition} ORDER BY APE.SequenceNo";

            string sql = @$"select AP.Name PanelName, sv.SystemVariableCode ApprovalPanelCategoryName,  sv1.SystemVariableCode NFAApprovalSequenceTypeName,VAE.FullName EmployeeName, VAE.EmployeeCode, VAE.DepartmentName, VAE1.FullName ProxyEmployeeName, APC .* 
                            from Approval..ApprovalPanelEmployeeConfig APC
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APC.TemplateID
                            LEFT JOIN Security..SystemVariable SV1 ON SV1.SystemVariableID = APC.NFAApprovalSequenceType
                            LEFT JOIN Approval..ApprovalPanel AP ON AP.APPanelID = APC.APPanelID
                            LEFT JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APC.EmployeeID
                            LEFT JOIN HRMS..ViewALLEmployee VAE1 ON VAE1.EmployeeID = APC.ProxyEmployeeID
                            WHERE APC.APPanelID = {APPanelID} ORDER BY APC.SequenceNo";


            var list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);
            list.ForEach(x =>
            {
                if (x.IsProxyEmployeeEnabled ?? false && x.IsMultiProxy)
                {
                    string proxySql = @$"SELECT	AP.*,
                        		Emp.FullName AS ProxyEmployeeName,
                                Emp.ImagePath AS ProxyEmployeeImagePath,
                        		D.DivisionName,Dept.DepartmentName 
                        		FROM ApprovalPanelProxyEmployeeConfig AP
                        		LEFT JOIN HRMS..Division D ON AP.DivisionID=D.DivisionID
                        		LEFT JOIN HRMS..Department Dept ON AP.DepartmentID=Dept.DepartmentID
                        		LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
                        WHERE APPEConfigID={x.APPEConfigID}";
                    x.MultipleProxyDetails = ApprovalPanelProxyEmployeeRepo.GetDataModelCollection<MultipleProxyDetailsConfigDto>(proxySql);
                }
            });

            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeList(int EmployeeID, int APPanelID)
        {
            string filter =  APPanelID != (int)Util.ApprovalPanel.HRSupportDocApproval ? " AND APE.DivisionID=Emp.DivisionID AND APE.DepartmentID=Emp.DepartmentID " : "";
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
						INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
						INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
                        WHERE APE.APPanelID={APPanelID} {filter}
                        ORDER BY APE.SequenceNo";

            var list = new List<ApprovalPanelEmployeeConfigDto>();
            list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListByPanelID(int EmployeeID, int APPanelID)
        {
            string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName,VAE.ImagePath ApproverPhotoUrl
                        FROM viewMainApprovalPanelEmployee APE
                        LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID 
						INNER JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APE.EmployeeID
						INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = {EmployeeID}
                        WHERE APE.APPanelID={APPanelID}
                        ORDER BY APE.SequenceNo";

            var list = new List<ApprovalPanelEmployeeConfigDto>();
            list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<ApprovalPanelEmployeeConfigDto> GetApprovalPanelEmployeeSingleInfoForEdit(ApprovalPanelEmployeeConfigDto ape)
        {
            //string sql = $@"SELECT APE.*, SV.SystemVariableCode AS NFAApprovalSequenceTypeName
            //            FROM viewMainApprovalPanelEmployee APE
            //            LEFT JOIN Security..SystemVariable SV ON APE.NFAApprovalSequenceType = SV.SystemVariableID WHERE APE.APPEConfigID={ape.APPEConfigID}";

            string sql = @$"select AP.Name PanelName, sv.SystemVariableCode ApprovalPanelCategoryName,  sv1.SystemVariableCode NFAApprovalSequenceTypeName,VAE.FullName EmployeeName, VAE.EmployeeCode, VAE.DepartmentName, VAE1.FullName ProxyEmployeeName, APC .* 
                            from Approval..ApprovalPanelEmployeeConfig APC
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APC.TemplateID
                            LEFT JOIN Security..SystemVariable SV1 ON SV1.SystemVariableID = APC.NFAApprovalSequenceType
                            LEFT JOIN Approval..ApprovalPanel AP ON AP.APPanelID = APC.APPanelID
                            LEFT JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APC.EmployeeID
                            LEFT JOIN HRMS..ViewALLEmployee VAE1 ON VAE1.EmployeeID = APC.ProxyEmployeeID
                            WHERE APC.APPEConfigID = {ape.APPEConfigID}";

            var model = ApprovalPanelEmployeeConfigRepo.GetModelData<ApprovalPanelEmployeeConfigDto>(sql);
            if (model.IsProxyEmployeeEnabled ?? false && model.IsMultiProxy)
            {
                string proxySql = @$"SELECT	AP.*,
                        		Emp.EmployeeCode+'-'+Emp.FullName AS ProxyEmployeeName,
                                Emp.ImagePath AS ProxyEmployeeImagePath,
                        		D.DivisionName,
								D.DivisionID,
								Dept.DepartmentName,
								Dept.DepartmentID
                        		FROM ApprovalPanelProxyEmployeeConfig AP
                        		LEFT JOIN HRMS..Division D ON AP.DivisionID=D.DivisionID
                        		LEFT JOIN HRMS..Department Dept ON AP.DepartmentID=Dept.DepartmentID
                        		LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
                        WHERE APPEConfigID={ape.APPEConfigID}";
                model.MultipleProxyDetails = ApprovalPanelProxyEmployeeRepo.GetDataModelCollection<MultipleProxyDetailsConfigDto>(proxySql);
            }
            return await Task.FromResult(model);

        }


        public async Task Delete(int APPEConfigID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var ApprovalPanelEmployeeEnt = ApprovalPanelEmployeeConfigRepo.Entities.Where(x => x.APPEConfigID == APPEConfigID).FirstOrDefault();

                ApprovalPanelEmployeeEnt.SetDeleted();
                ApprovalPanelEmployeeConfigRepo.Add(ApprovalPanelEmployeeEnt);

                var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPEConfigID == APPEConfigID).ToList();
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

                //var approvalPanelEmployeeEnts = ApprovalPanelEmployeeConfigRepo.GetAllList(x => x.APPanelID == APPanelID && x.DivisionID == DivisionID
                //&& x.DepartmentID == DepartmentID).ToList();

                var approvalPanelEmployeeEnts = ApprovalPanelEmployeeConfigRepo.GetAllList(x => x.APPanelID == APPanelID).ToList();

                approvalPanelEmployeeEnts.ForEach(x => x.SetDeleted());
                ApprovalPanelEmployeeConfigRepo.AddRange(approvalPanelEmployeeEnts);

                var removeMultipleProxy = new List<ApprovalPanelProxyEmployeeConfig>();
                foreach (var data in approvalPanelEmployeeEnts)
                {
                    var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPEConfigID == data.APPEConfigID).ToList();
                    removeProxyEmployeeList.ForEach(x => x.SetDeleted());
                    removeMultipleProxy.AddRange(removeProxyEmployeeList);
                }
                ApprovalPanelProxyEmployeeRepo.AddRange(removeMultipleProxy);
                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task<List<ApprovalPanelEmployeeConfigDto>> SaveReorderedList(List<ApprovalPanelEmployeeConfigDto> childs)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<ApprovalPanelEmployeeConfigDto>();

                foreach (var child in childs)
                {
                    //    var existsPolicy = ApprovalPanelEmployeeConfigRepo.Entities.SingleOrDefault(x => x.APPanelID == child.APPanelID && x.DivisionID == child.DivisionID
                    //&& x.DepartmentID == child.DepartmentID && x.EmployeeID == child.EmployeeID).MapTo<ApprovalPanelEmployeeConfigDto>();

                    var existsPolicy = ApprovalPanelEmployeeConfigRepo.Entities.SingleOrDefault(x => x.APPanelID == child.APPanelID && x.EmployeeID == child.EmployeeID).MapTo<ApprovalPanelEmployeeConfigDto>();

                    child.SetModified();

                    child.RowVersion = existsPolicy.RowVersion;
                    child.CreatedBy = existsPolicy.CreatedBy;
                    child.CreatedDate = existsPolicy.CreatedDate;
                    child.CreatedIP = existsPolicy.CreatedIP;

                }
                var childsEnt = childs.MapTo<List<ApprovalPanelEmployeeConfig>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                ApprovalPanelEmployeeConfigRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

            }
            await Task.CompletedTask;

            return childs;
        }
        public async Task<ApprovalPanelEmployeeConfigDto> SaveChanges(ApprovalPanelEmployeeConfigDto ApprovalPanelEmployeeConfigDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = new ApprovalPanelEmployeeConfigDto();
                var existPanel = new ApprovalPanelEmployeeConfigDto();
                
                existUser = ApprovalPanelEmployeeConfigRepo.Entities.SingleOrDefault(x => x.APPEConfigID == ApprovalPanelEmployeeConfigDto.APPEConfigID).MapTo<ApprovalPanelEmployeeConfigDto>();
                existPanel = ApprovalPanelEmployeeConfigRepo.Entities.SingleOrDefault(x => x.APPanelID == ApprovalPanelEmployeeConfigDto.APPanelID && x.TemplateID == ApprovalPanelEmployeeConfigDto.TemplateID).MapTo<ApprovalPanelEmployeeConfigDto>();

                ApprovalPanelEmployeeConfigDto.ProxyEmployeeID = ApprovalPanelEmployeeConfigDto.IsProxyEmployeeEnabled ?? false == true ? ApprovalPanelEmployeeConfigDto.ProxyEmployeeID : null;

                if (existUser.IsNull())// || ApprovalPanelEmployeeConfigDto.APPEConfigID.IsZero()
                {
                    if (existPanel.IsNull())
                    {
                        ApprovalPanelEmployeeConfigDto.SetAdded();
                        SetNewUserID(ApprovalPanelEmployeeConfigDto);
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else
                {
                    if (existPanel.IsNull() || existPanel.APPEConfigID == existUser.APPEConfigID)
                    {
                      ApprovalPanelEmployeeConfigDto.SetModified();

                    ApprovalPanelEmployeeConfigDto.APPEConfigID = existUser.APPEConfigID;
                    ApprovalPanelEmployeeConfigDto.RowVersion = existUser.RowVersion;
                    ApprovalPanelEmployeeConfigDto.CreatedBy = existUser.CreatedBy;
                    ApprovalPanelEmployeeConfigDto.CreatedDate = existUser.CreatedDate;
                    ApprovalPanelEmployeeConfigDto.CreatedIP = existUser.CreatedIP;
                    }
                    else
                    {
                        return null;
                    }
                }

                var ApprovalPanelEmployeeEnt = ApprovalPanelEmployeeConfigDto.MapTo<ApprovalPanelEmployeeConfig>();
                SetAuditFields(ApprovalPanelEmployeeEnt);
                ApprovalPanelEmployeeConfigRepo.Add(ApprovalPanelEmployeeEnt);


                if (existUser.IsNotNull())
                {
                    var removeProxyEmployeeList = ApprovalPanelProxyEmployeeRepo.GetAllList(x => x.APPEConfigID == existUser.APPEConfigID).ToList();
                    removeProxyEmployeeList.ForEach(x => x.SetDeleted());
                    ApprovalPanelProxyEmployeeRepo.AddRange(removeProxyEmployeeList);
                }
                if (ApprovalPanelEmployeeConfigDto.IsMultiProxy)
                {
                    var proxyEmployeeList = ApprovalPanelEmployeeConfigDto.MultipleProxyDetails.Select(x => new ApprovalPanelProxyEmployeeConfig
                    {
                        APPEConfigID = ApprovalPanelEmployeeConfigDto.APPEConfigID,
                        DivisionID = x.DivisionID,
                        DepartmentID = x.DepartmentID,
                        EmployeeID = x.EmployeeID,
                        APPanelID = ApprovalPanelEmployeeConfigDto.APPanelID
                    }).ToList();
                    proxyEmployeeList.Where(x => x.EmployeeID != ApprovalPanelEmployeeConfigDto.EmployeeID && x.EmployeeID != 0).ToList().ForEach(x =>
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
            return ApprovalPanelEmployeeConfigDto;
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

        public async Task CopyPanelData(CopyApprovalPanelConfigDto copiedInfo)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var approvalPanelEmployeeList = new List<ApprovalPanelEmployeeConfigDto>();
                var approvalPanelProxyEmployeeList = new List<ApprovalPanelProxyEmployeeConfig>();
                foreach (var panel in copiedInfo.ApprovalPanels)
                {
                    foreach (var department in copiedInfo.Departments)
                    {
                        var division = GetListOfDictionaryWithSql($@"SELECT ISNULL(DivisionID,0) AS DivisionID FROM HRMS..Department WHERE DepartmentID={department}").Result;
                        //var existingPanel = ApprovalPanelEmployeeConfigRepo.GetAllList(x => x.APPanelID == panel && x.DivisionID == Convert.ToInt32(division[0]["DivisionID"]) && x.DepartmentID == department).ToList();
                        var existingPanel = ApprovalPanelEmployeeConfigRepo.GetAllList(x => x.APPanelID == panel).ToList();
                        if (existingPanel.Count <= 0)
                        {
                            var dtoModel = copiedInfo.PanleData.Select(x => new ApprovalPanelEmployeeConfigDto
                            {
                                //DepartmentID = department,
                                EmployeeID = x.EmployeeID,
                                SequenceNo = x.SequenceNo,
                                ProxyEmployeeID = x.ProxyEmployeeID,
                                IsProxyEmployeeEnabled = x.IsProxyEmployeeEnabled,
                                APPanelID = panel,
                                //DivisionID = Convert.ToInt32(division[0]["DivisionID"]),
                                IsEditable = x.IsEditable,
                                NFAApprovalSequenceType = x.NFAApprovalSequenceType,
                                //IsSCM = x.IsSCM,
                                IsMultiProxy = x.IsMultiProxy,
                                MultipleProxyDetails = x.MultipleProxyDetails
                            }).ToList();

                            dtoModel.ForEach(x =>
                            {
                                x.SetAdded();
                                SetNewUserID(x);
                                if (x.IsMultiProxy)
                                {
                                    var proxyEmployeeList = x.MultipleProxyDetails.Select(y => new ApprovalPanelProxyEmployeeConfig
                                    {
                                        APPEConfigID = x.APPEConfigID,
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
                var ApprovalPanelEmployeeList = approvalPanelEmployeeList.MapTo<List<ApprovalPanelEmployeeConfig>>();


                SetAuditFields(ApprovalPanelEmployeeList);
                SetAuditFields(approvalPanelProxyEmployeeList);
                ApprovalPanelEmployeeConfigRepo.AddRange(ApprovalPanelEmployeeList);
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
        private void SetNewUserID(ApprovalPanelEmployeeConfigDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanelEmployeeConfig", AppContexts.User.CompanyID);
            obj.APPEConfigID = code.MaxNumber;
        }
        private void SetApprovalPanelProxyEmployeeNewID(ApprovalPanelProxyEmployeeConfig obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanelProxyEmployeeConfig", AppContexts.User.CompanyID);
            obj.APPPECID = code.MaxNumber;
        }


        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeaveOld(int EmployeeID, int APPanelID)
        {
            string sql = $@"SELECT
	                    APPEConfigID,
	                    NFAApprovalSequenceTypeName,
	                    ApproverPhotoUrl,
	                    EmployeeName,
	                    ROW_NUMBER() OVER(ORDER BY SequenceNo) SequenceNo
                    FROM
                    (
                    SELECT 
		                    ROW_NUMBER() OVER(ORDER BY VESM.EmployeeID) APPEConfigID, 
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
	                    APE.APPEConfigID, 
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

            var list = new List<ApprovalPanelEmployeeConfigDto>();
            list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal Days)
        {
            string sql = $@"EXEC HRMS..spGetLeaveApprovalPanel {EmployeeID},{LeaveTypeID},{(IsLFA == true ? 1 : 0)},{(IsFestival == true ? 1 : 0)},{Days}";

            var list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);


            await Task.CompletedTask;
            return list;
        }
        

        public async Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeaveEncashment()
        {
            string sql = $@"EXEC HRMS..spGetLeaveEncashmentApprovalPanel {AppContexts.User.EmployeeID}";

            var list = ApprovalPanelEmployeeConfigRepo.GetDataModelCollection<ApprovalPanelEmployeeConfigDto>(sql);


            await Task.CompletedTask;
            return list;
        }








        public GridModel GetListForGrid(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND DU.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND DU.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            string sql = $@"select AP.Name PanelName, sv.SystemVariableCode ApprovalPanelCategoryName,  sv1.SystemVariableCode NFAApprovalSequenceTypeName,VAE.FullName EmployeeName, VAE.EmployeeCode, VAE.DepartmentName, VAE1.FullName ProxyEmployeeName, APC .* from Approval..ApprovalPanelEmployeeConfig APC
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APC.TemplateID
                            LEFT JOIN Security..SystemVariable SV1 ON SV1.SystemVariableID = APC.NFAApprovalSequenceType
                            LEFT JOIN Approval..ApprovalPanel AP ON AP.APPanelID = APC.APPanelID
                            LEFT JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = APC.EmployeeID
                            LEFT JOIN HRMS..ViewALLEmployee VAE1 ON VAE1.EmployeeID = APC.ProxyEmployeeID
";
            var result = ApprovalPanelEmployeeConfigRepo.LoadGridModelOptimized(parameters, sql);



            
            //string proxySql = @$"SELECT AP.*,
            //            		Emp.FullName AS ProxyEmployeeName, 
            //                    Emp.ImagePath AS ProxyEmployeeImagePath,
            //            		D.DivisionName,Dept.DepartmentName
            //                    FROM ApprovalPanelProxyEmployeeConfig AP

            //                    LEFT JOIN HRMS..Division D ON AP.DivisionID = D.DivisionID

            //                    LEFT JOIN HRMS..Department Dept ON AP.DepartmentID = Dept.DepartmentID

            //                    LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
            //            WHERE APPEConfigID = { result.Rows[0]. }";



            //IEnumerable<Dictionary<string, object>> proxyDetails = ApprovalPanelEmployeeConfigRepo.GetDataDictCollection(proxySql).ToList();


            //result.Rows[0](.ForEach.ForEach(x =>
            //{
            //    if (x.IsProxyEmployeeEnabled ?? false && x.IsMultiProxy)
            //    {
            //        string proxySql = @$"SELECT	AP.*,
            //            		Emp.FullName AS ProxyEmployeeName, 
            //                    Emp.ImagePath AS ProxyEmployeeImagePath,
            //            		D.DivisionName,Dept.DepartmentName 
            //            		FROM ApprovalPanelProxyEmployeeConfig AP
            //            		LEFT JOIN HRMS..Division D ON AP.DivisionID=D.DivisionID
            //            		LEFT JOIN HRMS..Department Dept ON AP.DepartmentID=Dept.DepartmentID
            //            		LEFT JOIN HRMS..ViewALLEmployee Emp ON AP.EmployeeID = Emp.EmployeeID
            //            WHERE APPEConfigID={x.APPEConfigID}";
            //        x.MultipleProxyDetails = ApprovalPanelProxyEmployeeRepo.GetDataModelCollection<MultipleProxyDetailsConfigDto>(proxySql);
            //    }
            //});











            //string leaveBalanceSql = $@"SELECT 
            //                            FinancialYearID,
            //                         EmployeeID,
            //                         LeaveCategoryID,
            //                         SystemVariableCode LeaveType,
            //                         LeaveDays,
            //                         ApprovedDays NoOfApprovedLeaveDays,
            //                         PendingDays NoOfPendingLeaveDays,
            //                         RemainingDays Balance,
            //                         PreviousLeaveDays
            //                        FROM 
            //                         {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..EmployeeLeaveAccount ELA
            //                         LEFT JOIN (SELECT SystemVariableID, SystemVariableCode FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable WHERE EntityTypeID = 10) SV ON ELA.LeaveCategoryID = SV.SystemVariableID
            //                         where EmployeeID={EmployeeID}";

            //IEnumerable<Dictionary<string, object>> leaveBalanceData = EmployeeLeaveApplicationRepo.GetDataDictCollection(leaveBalanceSql).ToList();




            return result;
        }






    }
}
