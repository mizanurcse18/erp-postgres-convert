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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json;

namespace Approval.Manager.Implementations
{
    public class ApprovalRequestManager : ManagerBase, IApprovalRequestManager
    {

        private readonly IRepository<ApprovalPanelEmployee> ApprovalPanelEmployeeRepo;

        private readonly IRepository<ApprovalEmployeeFeedbackRemarks> ApprovalEmployeeFeedbackRemarksRepo;
        private readonly IRepository<ApprovalEmployeeFeedback> ApprovalEmployeeFeedbackRepo;
        private readonly IRepository<ApprovalProcess> ApprovalProcessRepo;
        IManager manager = new ManagerBase();
        public ApprovalRequestManager(IRepository<ApprovalPanelEmployee> appRepo, IRepository<ApprovalEmployeeFeedbackRemarks> approvalEmployeeFeedbackRemarksRepo, IRepository<ApprovalEmployeeFeedback> approvalEmployeeFeedbackRepo, IRepository<ApprovalProcess> approvalProcessRepo)
        {
            ApprovalPanelEmployeeRepo = appRepo;
            ApprovalEmployeeFeedbackRemarksRepo = approvalEmployeeFeedbackRemarksRepo;
            ApprovalEmployeeFeedbackRepo = approvalEmployeeFeedbackRepo;
            ApprovalProcessRepo = approvalProcessRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalRequestListDicOld()
        {
            var sql = $@"
                        SELECT  
								vA.* 
							FROM {AppContexts.GetDatabaseName(ConnectionName.Default)}..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT 
									ApprovalProcessID,
									APPanelID
								FROM 
									{AppContexts.GetDatabaseName(ConnectionName.Default)}..ApprovalProcessPanelMap 
							)Panel ON Panel.ApprovalProcessID = vA.ApprovalProcessID
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM {AppContexts.GetDatabaseName(ConnectionName.Default)}..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> {(int)Util.ApprovalFeedback.Forwarded}

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))
                        ORDER BY OrderFeedbackRequestDate DESC";
            return await Task.FromResult(ApprovalPanelEmployeeRepo.GetDataDictCollection(sql));
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalRequestListDic()
        {
            var sql = $@"
                        SELECT * FROM  
						(
                        SELECT  
						va.OrderFeedbackRequestDate, va.Description,
								va.FeedbackRequestDate,va.Title, va.ReferenceID, va.ApprovalProcessID,va.APEmployeeFeedbackID,va.APTypeID,convert(varchar(max),va.APTypeName) APTypeName, va.APForwardInfoID ,va.IsAPEditable, va.APFeedbackID, va.IsEditable, va.IsSCM, va.IsMultiProxy, va.DepartmentID, CASE WHEN vA.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy, va.Particulars
							FROM {AppContexts.GetDatabaseName(ConnectionName.Default)}..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT 
									ApprovalProcessID,
									APPanelID
								FROM 
									{AppContexts.GetDatabaseName(ConnectionName.Default)}..ApprovalProcessPanelMap 
							)Panel ON Panel.ApprovalProcessID = vA.ApprovalProcessID
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM {AppContexts.GetDatabaseName(ConnectionName.Default)}..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> {(int)Util.ApprovalFeedback.Forwarded}

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))
                            
                            UNION							                         

							SELECT 
							v.OrderFeedbackRequestDate,v.Description,
							v.FeedbackRequestDate,v.Title,v.ReferenceID, v.ApprovalProcessID,v.APEmployeeFeedbackID,v.APTypeID,convert(varchar(max),v.APTypeName) APTypeName, v.APForwardInfoID ,v.IsAPEditable
							, v.APFeedbackID, v.IsEditable, v.IsSCM, 0 IsMultiProxy, 0 DepartmentID, '' Proxy, '' Particulars
							FROM 
								HRMS..ViewALLUnSettledApprovedAdminSupportRequest v
							WHERE CreatedByEmployeeID =  {AppContexts.User.EmployeeID}

                            UNION 

							SELECT  
							v.OrderFeedbackRequestDate,v.Description,
							v.FeedbackRequestDate,v.Title,v.ReferenceID, v.ApprovalProcessID,v.APEmployeeFeedbackID,v.APTypeID,convert(varchar(max),v.APTypeName) APTypeName, v.APForwardInfoID ,v.IsAPEditable
							, v.APFeedbackID, v.IsEditable, v.IsSCM, 0 IsMultiProxy, 0 DepartmentID, '' Proxy, '' Particulars
							FROM 
								HRMS..ViewALLPendingRemoteAttendance v
							WHERE EmployeeID =  {AppContexts.User.EmployeeID}

                            
                            )A
                            ORDER BY OrderFeedbackRequestDate DESC";
            return await Task.FromResult(ApprovalPanelEmployeeRepo.GetDataDictCollection(sql));
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalRequestListDicForNFA()
        {
            var sql = $@"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.Default)}..viewAllPendingApproval 
                        WHERE (EmployeeID = {AppContexts.User.EmployeeID} OR ProxyEmployeeID = {AppContexts.User.EmployeeID}) AND APTypeID = {(int)Util.ApprovalType.NFA}";
            return await Task.FromResult(ApprovalPanelEmployeeRepo.GetDataDictCollection(sql));
        }


        public async Task<(bool, string)> BulkApproveOrRejectLeaveApplication(BulkSubmissionDto dto)
        {

            foreach(var model in dto.bulkList)
            {
                bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
                if (!isCurrentEmployee)
                {
                    return (false, "You are not current panel member.");
                }

                using (var unitOfWork = new UnitOfWork())
                {
                    if (model.APForwardInfoID > 0)
                    {
                        SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                    }
                    else
                    {
                        UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID
                        , model.ReferenceID, model.ToAPMemberFeedbackID, model.IsEditable, (int)Util.BudgetRemarksCategory.StrategicBudget == model.BudgetPlanCategoryID ? true : false);
                    }
                    //else
                    //{
                    //    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                    //}
                    //UpdateAttendanceSummaryTable(model.EmployeeID, model.RequestStartDate, model.RequestEndDate);


                    unitOfWork.CommitChangesWithAudit();
                    SendApprovalMail(model);
                }
            }

            return (true, "Approval Submitted Successfully");

        }

        public async Task<NotificationResponseDto> SubmitApproval(ApprovalSubmissionDto model)
        {
            NotificationResponseDto response = new NotificationResponseDto()
            {
                Success = false,
                CurrentNotificaitonEmplyeeID = string.Empty,
                NotificationMessage = string.Empty,
                EmployeeIds = string.Empty,
                Message = string.Empty
            };
            //string EmployeeIds = string.Empty;
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                response.Message = "You are not current panel member.";
                return response;
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                    var result = await GetApprovalEmployees(model.ReferenceID, model.APTypeID, AppContexts.GetDatabaseName(ConnectionName.Default));
                    response.EmployeeIds = result["EmployeeIDs"].ToString() + $",{AppContexts.User.EmployeeID}";
                }
                else
                {
                    response.EmployeeIds = UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID
                    , model.ReferenceID, model.ToAPMemberFeedbackID, model.IsEditable, (int)Util.BudgetRemarksCategory.StrategicBudget == model.BudgetPlanCategoryID ? true : false);
                }
                //else
                //{
                //    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                //}
                //UpdateAttendanceSummaryTable(model.EmployeeID, model.RequestStartDate, model.RequestEndDate);


                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            response.Message = "Approval Submitted Successfully";
            response.Success = true;

            return response;
        }

        public async Task<(bool, string)> SubmitNFAApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Security..NFAMaster WHERE NFAID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This NFA Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {

                    var isExistMD = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.EmployeeID == 1 && x.ApprovalProcessID == model.ApprovalProcessID);

                    bool AlreadyAddedMD = isExistMD.Count() > 0 ? true : false;


                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID
                    , model.ReferenceID, model.ToAPMemberFeedbackID, model.IsEditable, (int)Util.BudgetRemarksCategory.StrategicBudget == model.BudgetPlanCategoryID ? true : false, AlreadyAddedMD);
                    //UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (!string.IsNullOrEmpty(model.BudgetPlanRemarks) && model.BudgetPlanCategoryID > 0)
                {
                    SubmitBudgetPlanRemarks(model.ReferenceID, model.BudgetPlanRemarks, model.BudgetPlanCategoryID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }


        public async Task<(bool, string)> SubmitIOUClaimApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..IOUMaster WHERE IOUMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This IOU Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitExpenseClaimApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..ExpenseClaimMaster WHERE ECMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Expense Claim Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitIOUExpenseClaimSattlementApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..IOUOrExpensePaymentMaster  WHERE PaymentMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This IOU/Expense Sattlement Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }


        public async Task<(bool, string)> SubmitEPA(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Security..EmployeeProfileApproval  WHERE EPAID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Employee Profile Update Already Deleted/Cancelled By The Initiator");
            }

            string sqlEPA = $@"SELECT * FROM Security..EmployeeProfileApproval EPA WHERE EPA.EPAID={model.ReferenceID}";
            var data = ApprovalPanelEmployeeRepo.GetData(sqlEPA);
            EmployeeProfileApprovalDtoCore obj = data.MapTo<EmployeeProfileApprovalDtoCore>();

            string existingPersonSql = $@"SELECT * FROM Security..Person WHERE PersonID={obj.PersonID}";
            var personData = ApprovalPanelEmployeeRepo.GetData(existingPersonSql);

            ApprovalEmployeeFeedback aef = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.ApprovalProcessID == model.ApprovalProcessID).OrderByDescending(y => y.SequenceNo).FirstOrDefault();

            Dictionary<string, object> buildParam = new Dictionary<string, object>();
            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                bool isLastMemberFeedback = false;
                if (aef.APEmployeeFeedbackID == model.APEmployeeFeedbackID)
                {
                    isLastMemberFeedback = true;
                }

                if (model.APFeedbackID == (int)Util.ApprovalFeedback.Approved)

                {
                    if (obj.IsNotNull())
                    {
                        var newObj = JsonSerializer.Deserialize<PersonUpdateModel>(obj.NewValue);
                        //check the configuration master person table fields
                        if (newObj.NIDNumber.IsNotNull())
                            buildParam.Add("NIDNumber", newObj.NIDNumber);
                        else
                            buildParam.Add("NIDNumber", personData["NIDNumber"]);
                        if (newObj.PassportNumber.IsNotNull())
                            buildParam.Add("PassportNumber", newObj.PassportNumber);
                        else
                            buildParam.Add("PassportNumber", personData["PassportNumber"]);
                        if (newObj.FatherName.IsNotNull())
                            buildParam.Add("FatherName", newObj.FatherName);
                        else
                            buildParam.Add("FatherName", personData["FatherName"]);
                        if (newObj.MotherName.IsNotNull())
                            buildParam.Add("MotherName", newObj.MotherName);
                        else
                            buildParam.Add("MotherName", personData["MotherName"]);
                        if (newObj.SpouseName.IsNotNull())
                            buildParam.Add("SpouseName", newObj.SpouseName);
                        else
                            buildParam.Add("SpouseName", personData["SpouseName"]);
                        if (newObj.FatherDOB.IsNotNull())
                            buildParam.Add("FatherDOB", Convert.ToDateTime(newObj.FatherDOB).ToString("yyyy-MM-dd"));
                        else
                            buildParam.Add("FatherDOB", personData["FatherDOB"].ToString() == "" ? null : Convert.ToDateTime(personData["FatherDOB"]).ToString("yyyy-MM-dd"));
                        if (newObj.PassportIssueDate.IsNotNull())
                            buildParam.Add("PassportIssueDate", Convert.ToDateTime(newObj.PassportIssueDateFormatted).ToString("yyyy-MM-dd"));
                        else
                            buildParam.Add("PassportIssueDate", personData["PassportIssueDate"].ToString() == "" ? null : Convert.ToDateTime(personData["PassportIssueDate"]).ToString("yyyy-MM-dd"));
                        if (newObj.MotherDOB.IsNotNull())
                            buildParam.Add("MotherDOB", Convert.ToDateTime(newObj.MotherDOB).ToString("yyyy-MM-dd"));
                        else
                            buildParam.Add("MotherDOB", personData["MotherDOB"].ToString() == "" ? null : Convert.ToDateTime(personData["MotherDOB"]).ToString("yyyy-MM-dd"));
                        if (newObj.SpouseDOB.IsNotNull())
                            buildParam.Add("SpouseDOB", Convert.ToDateTime(newObj.SpouseDOB).ToString("yyyy-MM-dd"));
                        else
                            buildParam.Add("SpouseDOB", personData["SpouseDOB"].ToString() == "" ? null : Convert.ToDateTime(personData["SpouseDOB"]).ToString("yyyy-MM-dd"));
                        if (newObj.MarriageDate.IsNotNull())
                            buildParam.Add("MarriageDate", Convert.ToDateTime(newObj.MarriageDate).ToString("yyyy-MM-dd"));
                        else
                            buildParam.Add("MarriageDate", personData["MarriageDate"].ToString() == "" ? null : Convert.ToDateTime(personData["MarriageDate"]).ToString("yyyy-MM-dd"));
                        if (newObj.IsFatherAlive.IsNotNull())
                            buildParam.Add("IsFatherAlive", newObj.IsFatherAlive == "True" ? 1 : 0);
                        else
                            buildParam.Add("IsFatherAlive", Convert.ToBoolean(personData["IsFatherAlive"]) == true ? 1 : 0);
                        if (newObj.IsMotherAlive.IsNotNull())
                            buildParam.Add("IsMotherAlive", newObj.IsMotherAlive == "True" ? 1 : 0);
                        else
                            buildParam.Add("IsMotherAlive", Convert.ToBoolean(personData["IsMotherAlive"]) == true ? 1 : 0);
                        if (newObj.SpouseGenderID.IsNotNull())
                            buildParam.Add("SpouseGenderID", newObj.SpouseGenderID);
                        else
                            buildParam.Add("SpouseGenderID", personData["SpouseGenderID"].ToString() == "" ? 0 : Convert.ToInt32(personData["SpouseGenderID"]));

                        var FDOB = buildParam["FatherDOB"].IsNull() ? @$"ISNULL(null, null)" : @$"ISNULL('{buildParam["FatherDOB"]}', null)";
                        var motherDOB = buildParam["MotherDOB"].IsNull() ? @$"ISNULL(null, null)" : @$"ISNULL('{buildParam["MotherDOB"]}', null)";
                        var spouseDOB = buildParam["SpouseDOB"].IsNull() ? @$"ISNULL(null, null)" : @$"ISNULL('{buildParam["SpouseDOB"]}', null)";
                        var marriageDOB = buildParam["MarriageDate"].IsNull() ? @$"ISNULL(null, null)" : @$"ISNULL('{buildParam["MarriageDate"]}', null)";
                        var passportissueDate = buildParam["PassportIssueDate"].IsNull() ? @$"ISNULL(null, null)" : @$"ISNULL('{buildParam["PassportIssueDate"]}', null)";


                        if (isLastMemberFeedback)
                        {
                            string sqlPersonUpdate = $@"UPDATE Security..Person SET PassportNumber='{buildParam["PassportNumber"]}',NIDNumber='{buildParam["NIDNumber"]}',FatherName='{buildParam["FatherName"]}',MotherName='{buildParam["MotherName"]}',SpouseName='{buildParam["SpouseName"]}',PassportIssueDate={passportissueDate},FatherDOB={FDOB},MotherDOB={motherDOB},SpouseDOB={spouseDOB},MarriageDate={marriageDOB},IsFatherAlive={buildParam["IsFatherAlive"]},IsMotherAlive={buildParam["IsMotherAlive"]},SpouseGenderID={buildParam["SpouseGenderID"]} WHERE PersonID={obj.PersonID}";
                            ApprovalPanelEmployeeRepo.ExecuteSqlCommand(sqlPersonUpdate);

                            if (newObj.ChildrenInfo.IsNotNull())
                            {
                                foreach (var item in newObj.ChildrenInfo)
                                {
                                    var isDelete = item.isDelete == true ? 1 : 0;
                                    string sqlc = $@"EXEC Security..SP_InsertOrUpdateFamilyInformation '{item.Name}',{item.GenderID},'{Convert.ToDateTime(item.DateOfBirth).ToString("yyyy-MM-dd")}',{item.PersonID},{item.PFIID}, '{AppContexts.User.CompanyID}', {AppContexts.User.UserID}, '{AppContexts.User.IPAddress}', '{DateTime.Now.ToString("yyyy-MM-dd")}',{isDelete}";
                                    ApprovalPanelEmployeeRepo.ExecuteSqlCommand(sqlc);
                                }
                            }
                            if (newObj.NomineeInfo.IsNotNull())
                            {
                                foreach (var item in newObj.NomineeInfo)
                                {
                                    var isDelete = item.isDelete == true ? 1 : 0;
                                    string sqlc = $@"EXEC Security..SP_InsertOrUpdateNomineeInformation '{item.NomineeName}','{item.NomineeAddress}','{item.RelationShip}','{Convert.ToDateTime(item.DateOfBirth).ToString("yyyy-MM-dd")}',{item.Percentage},'{item.NomineeBehalf}',{item.PersonID},{item.NIID}, '{AppContexts.User.CompanyID}', {AppContexts.User.UserID}, '{AppContexts.User.IPAddress}', '{DateTime.Now.ToString("yyyy-MM-dd")}',{isDelete}";
                                    ApprovalPanelEmployeeRepo.ExecuteSqlCommand(sqlc);
                                }
                            }
                        }
                    }

                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitMicroSiteApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM MicroSite..MicroSiteMaster  WHERE MicroSiteMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This MicroSite Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitPurchaseOrderApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM SCM..PurchaseOrderMaster  WHERE POMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Purchase Order Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitGRNApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM SCM..MaterialReceive  WHERE MRID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This GRN Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitQCApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM SCM..QCMaster  WHERE QCMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This QC Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitDocumentApprovalApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Approval..DocumentApprovalMaster  WHERE DAMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Document Approval Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> ApprovalSubmissionForExitInterview(ExitInterviewApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..EmployeeExitInterview  WHERE EEIID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Exit Interview Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.TemplateBody.IsNotNullOrEmpty())
                {
                    SubmitTemplateBodyForExitInterview(model.APTypeID, model.ReferenceID, model.TemplateBody);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMailExitInterview(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitTaxationVettingApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..TaxationVettingMaster  WHERE TVMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Taxation Vetting Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                if (model.BudgetPlanRemarks.IsNotNullOrEmpty())
                {
                    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, "", false);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }


        public async Task<(bool, string)> SubmitPurchaseRequisitionApproval(SCMApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM SCM..PurchaseRequisitionMaster WHERE PRMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Purchase Requisition Already Deleted/Cancelled By The Initiator");
            }
            var removeAttachmentList = RemoveAttachments(model);
            var removeBudgetList = RemoveCostCenterBudget(model);
            //var removeQuotationList = RemoveQuotations(model);
            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    var isExistMD = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.EmployeeID == 1 && x.ApprovalProcessID == model.ApprovalProcessID);

                    bool AlreadyAddedMD = isExistMD.Count() > 0 ? true : false;

                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID
                    , model.IsEditable, (int)Util.BudgetRemarksCategory.StrategicBudget == model.BudgetPlanCategoryID ? true : false, AlreadyAddedMD);
                }
                //if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                //{
                //    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);

                //}
                else if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }

                if (model.IsEditable || model.IsSCM)
                {
                    SubmitSCMBugetPlanRemarksPR(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, model.BudgetPlanCategoryID, model.SCMRemarks);
                }

                // Update PRNFAMap
                if (model.IsSCM)
                {
                    UpdatePRNFAMap((int)model.PRNFAMap.PRNFAMapID, (int)model.PRNFAMap.PRMID, (int)model.PRNFAMap.NFAID, model.PRNFAMap.NFAReferenceNo, model.PRNFAMap.NFAAmount, model.PRNFAMap.IsFromSystem, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress);
                }
                // End of Update PRNFAMap

                //update purchase requisition child
                if (model.Quotations.IsNotNull() && model.Quotations.Count > 0 && model.IsSCM)//model.IsSCM && 
                {
                    UpdatePurchaseRequisitionMaster(model.ReferenceID, (decimal)model.Quotations.Where(x => x.Amount > 0).Select(x => x.Amount).DefaultIfEmpty(0).Sum());

                    var list = model.Quotations.Where(x => x.Amount > 0).ToList();
                    //For Update Child                    
                    if (list.IsNotNull() && list.Count > 0)
                    {
                        foreach (var child in list)
                        {
                            int childId = model.ItemDetails.Where(x => x.ItemID == child.ItemID && x.PRMasterID == child.PRMasterID && x.Description == child.Description).Select(y => y.PRCID).FirstOrDefault();
                            UpdatePurchaseRequisitionChild(childId, (int)child.PRMasterID, child.QuotedUnitPrice.Value, child.Amount.Value, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, (int)child.ItemID, child.Description);
                        }
                    }
                }
                //end of update purchase requisition child

                if (model.IsSCM && model.Quotations.IsNotNull() && model.Quotations.Count > 0)
                {



                    var updateList = model.Quotations.Where(x => x.PRQID > 0).ToList();
                    //For Update Quotation                    
                    if (updateList.IsNotNull() && updateList.Count > 0)
                    {
                        foreach (var quotation in updateList)
                        {
                            //var list = quotation.Items.Where(x => x.ItemID > 0).Select(y => y.ItemID).ToList();
                            //string quotedItemsStr = string.Join<int>(",", list);

                            SaveSCMRequisitionQuotation(quotation.PRQID, model.ReferenceID, quotation.SupplierID, quotation.Amount ?? 0, 1, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, AppContexts.User.UserID, DateTime.Now, AppContexts.User.IPAddress, 2, string.IsNullOrEmpty(quotation.Description) ? "" : quotation.Description, quotation.QuotedQty ?? 0, quotation.QuotedUnitPrice ?? 0, quotation.ItemID ?? 0, quotation.PRCID);
                        }
                    }

                    var quotationList = model.Quotations.Where(x => x.PRQID == 0).ToList();

                    //For Add New Quotation
                    if (quotationList.IsNotNull() && quotationList.Count > 0)
                    {
                        foreach (var quotation in quotationList)
                        {
                            //var list = quotation.Items.Where(x => x.ItemID > 0).Select(y => y.ItemID).ToList();
                            //string quotedItemsStr = string.Join<int>(",", list);

                            SetQuotationNewId(quotation);
                            SaveSCMRequisitionQuotation(quotation.PRQID, model.ReferenceID, quotation.SupplierID, quotation.Amount ?? 0, 1, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, 0, DateTime.Now, "", 1, string.IsNullOrEmpty(quotation.Description) ? "" : quotation.Description, quotation.QuotedQty ?? 0, quotation.QuotedUnitPrice ?? 0, quotation.ItemID ?? 0, quotation.PRCID);
                        }
                    }
                    //For Remove Quotation                    
                    //if (removeQuotationList.IsNotNull() && removeQuotationList.Count > 0)
                    //{
                    //    foreach (var quotation in removeQuotationList)
                    //    {
                    //        SaveSCMRequisitionQuotation(quotation.PRQID, model.ReferenceID, quotation.SupplierID, quotation.Amount ?? 0, 1, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, 0, DateTime.Now, "", 3, string.IsNullOrEmpty(quotation.Description) ? "" : quotation.Description, quotation.QuotedQty ?? 0, quotation.QuotedUnitPrice ?? 0, quotation.ItemID ?? 0, quotation.PRCID);
                    //    }
                    //}


                }
                if (model.IsSCM && model.Attachments.IsNotNull() && model.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(model.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.ReferenceID, "PurchaseRequisitionQuotation", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeAttachmentList.IsNotNull() && removeAttachmentList.Count > 0)
                    {
                        foreach (var attachemnt in removeAttachmentList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.ReferenceID, "PurchaseRequisitionQuotation", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }
                if (model.IsEditable)
                {
                    foreach (var item in model.ItemDetails)
                    {
                        UpdateCostCenter(item.PRCID, item.ForID);
                    }

                }
                unitOfWork.CommitChangesWithAudit();
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    SendApprovalMail(model);
                }
            }
            return (true, "Approval Submitted Successfully");
        }

        private bool ValidateUserInformationForSCC(SCCApprovalSubmissionDto model)
        {
            bool isValidUserFields = true;
            bool ServicePeriodFromTo = true;
            if (model.APFeedbackID != 6 && model.APFeedbackID != 11)
            {
                for (int i = 0; i < model.SCCItemDetails.Count; i++)
                {
                    var item = model.SCCItemDetails[i];
                    if (item.DeliveryOrJobCompletionDate.IsNull())
                    {
                        isValidUserFields = isValidUserFields && false;
                    }
                    if (item.ReceivedQty <= 0)
                    {
                        isValidUserFields = isValidUserFields && false;
                    }
                    if (item.Rate <= 0)
                    {
                        isValidUserFields = isValidUserFields && false;
                    }
                    if (item.TotalAmountIncludingVat <= 0)
                    {
                        isValidUserFields = isValidUserFields && false;
                    }
                }

                if (model.ServicePeriodFrom.IsNull() || model.ServicePeriodTo.IsNull())
                {
                    ServicePeriodFromTo = false;
                }

            }

            bool paymentValidation = true;
            bool lifecycleValidation = true;
            if (model.Lifecycle == 2)
            {
                if (model.LifecycleComment.IsNullOrEmpty())
                {
                    lifecycleValidation = lifecycleValidation && false;
                }
            }
            if (model.PaymentType == 173 || model.PaymentType == 174)
            {
                if (model.PaymentFixedOrPercentAmount <= 0)
                {
                    paymentValidation = paymentValidation && false;
                }
            }
            return (
                //model.ServicePeriodFrom.IsNotNull() &&
                //model.ServicePeriodTo.IsNotNull() &&
                ServicePeriodFromTo &&
                lifecycleValidation &&
                paymentValidation &&
                isValidUserFields
            );

        }

        private void AddAttachments(SCCApprovalSubmissionDto SCC)
        {
            if (SCC.ProposedAttachments.IsNotNull() && SCC.ProposedAttachments.Count > 0)
            {
                var attachemntsList = SCC.ProposedAttachments.Where(x => x.ID == 0).ToList();
                int sl = 0;
                foreach (var attachment in attachemntsList)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        // To Add Physical Files

                        string filename = $"SCCMasterProposed-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "SCCMasterProposed\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        // To Add Into DB
                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)SCC.ReferenceID, "SCCMasterProposed", false, attachment.Size, 0, false, attachment.Description ?? "");

                        sl++;
                    }
                }
            }
        }

        private void RemoveAttachments(SCCApprovalSubmissionDto SCC)
        {
            if (SCC.ReferenceID > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMasterProposed' AND ReferenceID={SCC.ReferenceID}";
                var prevAttachment = GetListOfDictionaryWithSql(attachmentSql).Result;

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachments
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeFiles = attachemntList.Where(x => !SCC.ProposedAttachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeFiles.Count > 0)
                {
                    foreach (var data in removeFiles)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "SCCMasterProposed";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        //File.Delete(str + "\\" + data.FileName);
                        System.IO.File.Delete(str + "\\" + data.FileName);
                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)SCC.ReferenceID, "SCCMasterProposed", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
            }
        }
        private List<Attachment> RemoveProposedAttachments(SCCApprovalSubmissionDto SCC)
        {
            if (SCC.ProposedAttachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMasterProposed' AND ReferenceID={SCC.ReferenceID}";
                var prevAttachment = GetListOfDictionaryWithSql(attachmentSql).Result;

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachment
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !SCC.ProposedAttachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "ProposedSCC";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);
                    }

                }
                return removeList;
            }

            return null;
        }

        public async Task<(bool, string)> SubmitSCCApproval(SCCApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            bool isEditable = CheckIsEditableEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);


            if (isEditable && !ValidateUserInformationForSCC(model) && (model.APFeedbackID != 6 && model.APFeedbackID != 11))
            {
                return (false, "Invalid Data to submit this SCC.");
            }

            string sql = @$"SELECT * FROM SCM..SCCMaster WHERE SCCMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This SCC Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);

                }
                else if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }

                if (model.IsEditable && (model.APFeedbackID != 6 && model.APFeedbackID != 11))
                {
                    SubmitSCCUserData(model.APTypeID, model.ReferenceID, model.ServicePeriodFrom, model.ServicePeriodTo, model.PaymentType, Convert.ToInt32(model.PaymentFixedOrPercent), model.PaymentFixedOrPercentAmount, model.PaymentFixedOrPercentTotalAmount, model.SCCAmount, model.Lifecycle, model.LifecycleComment, model.PerformanceAssessment1, model.PerformanceAssessment2, model.PerformanceAssessment3, model.PerformanceAssessment4, model.PerformanceAssessment5, model.PerformanceAssessment6, model.PerformanceAssessmentComment, model.TotalReceivedQty);
                }
                if (model.IsEditable && (model.APFeedbackID != 6 && model.APFeedbackID != 11))
                {
                    foreach (var item in model.SCCItemDetails)
                    {
                        if (model.ReferenceID == item.SCCMID && model.InvoiceAmountFromVendor < item.InvoiceAmount)
                        {
                            return (false, "Invoice Amount can not greater User Invoice Amount.");
                        }
                        UpdateSCCChild((int)item.SCCCID, item.DeliveryOrJobCompletionDate, item.ReceivedQty, item.InvoiceAmount, item.TotalAmount, item.TotalAmountIncludingVat, item.VatAmount, item.Remarks);
                    }
                }
                RemoveAttachments(model);
                AddAttachments(model);

                unitOfWork.CommitChangesWithAudit();
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    SendApprovalMail(model);
                }
            }

            return (true, "Approval Submitted Successfully");
        }
        //mmm
        public async Task<(bool, string)> SubmitLeaveEncashApproval(LEApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..AnnualLeaveEncashmentMaster WHERE ALEMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Data has Already been Deleted/Cancelled By The Initiator!");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                unitOfWork.CommitChangesWithAudit();
                //SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitMaterialRequisitionApproval(SCMApprovalSubmissionForMRDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM SCM..MaterialRequisitionMaster WHERE MRMasterID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Material Requisition Already Deleted/Cancelled By The Initiator");
            }
            var removeAttachmentList = RemoveAttachmentsMR(model);
            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);

                }
                else if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }

                //if (model.IsEditable || model.IsSCM)
                //{
                //    SubmitSCMBugetPlanRemarks(model.APTypeID, model.ReferenceID, model.BudgetPlanRemarks, model.SCMRemarks);
                //}
                if (model.ItemDetails.IsNotNull() && model.ItemDetails.Count > 0)//model.IsSCM && 
                {
                    UpdateMaterialRequisitionMasterSCMRemarks(model.ReferenceID, model.SCMRemarks, (decimal)model.ItemDetails.Where(x => x.MRCID > 0).Select(x => x.Amount).DefaultIfEmpty(0).Sum());

                    var updateList = model.ItemDetails.Where(x => x.MRCID > 0).ToList();
                    //For Update Child                    
                    if (updateList.IsNotNull() && updateList.Count > 0)
                    {
                        foreach (var child in updateList)
                        {
                            UpdateMaterialRequisitionChild(child.MRCID, child.MRMasterID, child.Price.Value, child.Amount, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, child.ItemID, child.Description);
                        }
                    }
                }
                if (model.IsSCM && model.Attachments.IsNotNull() && model.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(model.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.ReferenceID, "MaterialRequisitionMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeAttachmentList.IsNotNull() && removeAttachmentList.Count > 0)
                    {
                        foreach (var attachemnt in removeAttachmentList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.ReferenceID, "MaterialRequisitionMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                unitOfWork.CommitChangesWithAudit();
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    SendApprovalMail(model);
                }
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitApprovalCommon(ApprovalSubmissionDto model)
        {

            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        private void SendApprovalMailEAD(EADApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.BudgetPlanRemarks;
            SendApprovalMail(dto);
        }
        private void SendApprovalMail(SCMApprovalSubmissionForMRDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.BudgetPlanRemarks;
            SendApprovalMail(dto);
        }

        private void SendApprovalMail(SCMApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.BudgetPlanRemarks;
            SendApprovalMail(dto);
        }

        private void SendApprovalMail(SCCApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            SendApprovalMail(dto);
        }
        private void SendApprovalMailExitInterview(ExitInterviewApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.TemplateBody;
            SendApprovalMail(dto);
        }
        private void SendApprovalMailDocumentUpload(ApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.BudgetPlanRemarks;
            SendApprovalMail(dto);
        }
        private void SendApprovalMailSupportRequest(SRApprovalSubmissionDto model)
        {
            ApprovalSubmissionDto dto = new ApprovalSubmissionDto();
            dto.ApprovalProcessID = model.ApprovalProcessID;
            dto.APEmployeeFeedbackID = model.APEmployeeFeedbackID;
            dto.APFeedbackID = model.APFeedbackID;
            dto.Remarks = model.Remarks;
            dto.APTypeID = model.APTypeID;
            dto.ReferenceID = model.ReferenceID;
            dto.ToAPMemberFeedbackID = model.ToAPMemberFeedbackID;
            dto.APForwardInfoID = model.APForwardInfoID;
            dto.BudgetPlanRemarks = model.BudgetPlanRemarks;
            dto.SupportTypeID = model.SupportTypeID;
            dto.EmployeeID = model.EmployeeID;
            SendApprovalMail(dto);
        }
        private async Task SendApprovalMail(ApprovalSubmissionDto model)
        {
            var recieverMail = GetApprovalActionEmployeesForMultiProxyEmail(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.ToAPMemberFeedbackID).Result;

            switch (model.APTypeID)
            {

                case (int)Util.ApprovalType.LeaveApplication:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveApplication, (int)Util.MailGroupSetup.LeaveApprovalMail);
                            break;
                        default:
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.NFA:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.NFAInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.FinalNFAAPprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.FinalNFAAPprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.NFAForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.NFAForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.NFA, (int)Util.MailGroupSetup.NFAForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.MicroSite:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.MicroSiteInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.FinalMicroSiteApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.FinalMicroSiteApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.MicroSiteForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.MicroSiteForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MicroSite, (int)Util.MailGroupSetup.MicroSiteForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.EmployeeProfileApproval:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.EmployeeProfileInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.FinalEmployeeProfileApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.FinalEmployeeProfileApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.EmployeeProfileForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.EmployeeProfileForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.MailGroupSetup.EmployeeProfileForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.ExpenseClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.ExpenseClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.FinalExpenseCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.FinalExpenseCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.ExpenseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.ExpenseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpenseClaim, (int)Util.MailGroupSetup.ExpenseClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.IOUClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.IOUClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.FinalIOUCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.FinalIOUCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.IOUClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.IOUClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUClaim, (int)Util.MailGroupSetup.IOUClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.ExpensePayment:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.ExpensePaymentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.FinalExpensePaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.FinalExpensePaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.ExpensePaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.ExpensePaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExpensePayment, (int)Util.MailGroupSetup.ExpensePaymentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.IOUPayment:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.IOUPaymentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.FinalIOUPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.FinalIOUPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.IOUPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.IOUPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.IOUPayment, (int)Util.MailGroupSetup.IOUPaymentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PR:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, (int)Util.MailGroupSetup.PRInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            List<int> mailGroups = new List<int>()
                            {
                                (int)Util.MailGroupSetup.FinalPRApprovalStatusToInitiator,
                                (int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved
                            };
                            await SendMailBase(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, mailGroups, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, (int)Util.MailGroupSetup.FinalPRApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, (int)Util.MailGroupSetup.PRForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, (int)Util.MailGroupSetup.PRForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PR, (int)Util.MailGroupSetup.PRForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PO:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.POInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.FinalPOApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.FinalPOApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.POForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.POForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PO, (int)Util.MailGroupSetup.POForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.GRN:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.GRNInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.FinalGRNApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.FinalGRNApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.GRNForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.GRNForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.GRN, (int)Util.MailGroupSetup.GRNForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.QC:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.QCInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.FinalQCApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.FinalQCApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.QCForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.QCForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.QC, (int)Util.MailGroupSetup.QCForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.DocumentApproval:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.FinalDocumentApprovalApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.FinalDocumentApprovalApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.DocumentApprovalForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.DocumentApprovalForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.DocumentApproval, (int)Util.MailGroupSetup.DocumentApprovalForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.Invoice:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.InvoiceInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.FinalInvoiceApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.FinalInvoiceApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.InvoiceForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.InvoiceForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.Invoice, (int)Util.MailGroupSetup.GRNForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.InvoicePayment:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.InvoicePaymentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.FinalInvoicePaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.FinalInvoicePaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.InvoicePaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.InvoicePaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.InvoicePayment, (int)Util.MailGroupSetup.InvoicePaymentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.MR:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, (int)Util.MailGroupSetup.MRInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            List<int> mailGroups = new List<int>()
                            {
                                (int)Util.MailGroupSetup.FinalMRApprovalStatusToInitiator
                                //(int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved
                            };
                            await SendMailBase(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, mailGroups, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, (int)Util.MailGroupSetup.FinalMRApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, (int)Util.MailGroupSetup.MRForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, (int)Util.MailGroupSetup.MRForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.MR, (int)Util.MailGroupSetup.MRForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.TaxationVetting:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.TaxationVettingInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.FinalTaxationVettingApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.FinalTaxationVettingApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.TaxationVettingForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.TaxationVettingForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVetting, (int)Util.MailGroupSetup.TaxationVettingForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.TaxationVettingPayment:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.TaxationPaymentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.FinalTaxationPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.FinalTaxationPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.TaxationPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.TaxationPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.MailGroupSetup.TaxationPaymentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;

                case (int)Util.ApprovalType.ExitInterview:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.ExitInterviewInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.FinalExitInterviewApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.FinalExitInterviewApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExitInterview, (int)Util.MailGroupSetup.ExitInterviewForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;

                case (int)Util.ApprovalType.AccessDeactivation:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.ExitInterviewInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.FinalEmployeeAccessDeactivationApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.FinalExitInterviewApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.MailGroupSetup.ExitInterviewForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.SCC:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, (int)Util.MailGroupSetup.SCCInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            List<int> mailGroups = new List<int>()
                            {
                                (int)Util.MailGroupSetup.FinalSCCApprovalStatusToInitiator,
                                //(int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved
                            };
                            await SendMailBase(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, mailGroups, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, (int)Util.MailGroupSetup.FinalSCCApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, (int)Util.MailGroupSetup.SCCForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, (int)Util.MailGroupSetup.SCCForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.SCC, (int)Util.MailGroupSetup.SCCForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.LeaveEncashmentApplication:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveEncashmentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            List<int> mailGroups = new List<int>()
                            {
                                (int)Util.MailGroupSetup.FinalLeaveEncashmentApprovalStatusToInitiator,
                            };
                            await SendMailBase(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, mailGroups, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.FinalLeaveEncashmentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveEncashmentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveEncashmentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveEncashmentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.EmployeeeDocumentUpload:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.AdminSupportRequest:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeInitiatedMail : (int)Util.MailGroupSetup.AdminRequestSupportInitiatedMail);
                            break;
                        //Final Approved
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            //await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, (int)Util.MailGroupSetup.FinalAdminRequestSupportApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            if (model.SupportTypeID == (int)Util.AdminSupportCategory.Vehicle)
                            {
                                await SendMailToSettledReceiver(model.ApprovalProcessID.ToString(), model.ReferenceID, model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportSettlementEmployeeFeedbackReceieve : (int)Util.MailGroupSetup.AdminRequestSupportSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                            }
                            else if (model.SupportTypeID == (int)Util.AdminSupportCategory.ConsumbleGoods)
                            {
                                await SendMailToSettledReceiver(model.ApprovalProcessID.ToString(), model.ReferenceID, model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeConsumbleGoodsFeedbackReceieve : (int)Util.MailGroupSetup.AdminRequestSupportConsumbleGoodsFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                            }
                            else if (model.SupportTypeID == (int)Util.AdminSupportCategory.RenovationOrMaintenance)
                            {
                                await SendMailToSettledReceiver(model.ApprovalProcessID.ToString(), model.ReferenceID, model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeRenovationOrMaintenanceFeedbackReceieve : (int)Util.MailGroupSetup.AdminRequestSupportRenovationOrMaintenanceFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);
                            }
                            else
                            {
                                await SendMailToSettledReceiver(model.ApprovalProcessID.ToString(), model.ReferenceID, model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeWithoutVehicleSettlementFeedbackReceieve : (int)Util.MailGroupSetup.AdminRequestSupportWithoutVehicleSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);
                            }

                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:

                            //int MailCategory = model.SupportTypeID == (int)Util.AdminSupportCategory.Vehicle ? (int)Util.MailGroupSetup.AdminRequestSupportVehicleSupportRejectMail : (int)Util.MailGroupSetup.FinalAdminRequestSupportApprovalStatusToInitiator;
                            int MailCategory = model.SupportTypeID == (int)Util.AdminSupportCategory.Vehicle ?
                                              (model.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeVehicleSupportRejectMail :
                                                                     (int)Util.MailGroupSetup.AdminRequestSupportVehicleSupportRejectMail) :
                                              (model.EmployeeID > 0 ? (int)Util.MailGroupSetup.FinalAdminRequestSupportEmployeeApprovalStatusToInitiator :
                                                                     (int)Util.MailGroupSetup.FinalAdminRequestSupportApprovalStatusToInitiator);
                            recieverMail = GetAPEmployeeEmailsForRejectPanelMembers(Convert.ToInt32(model.ApprovalProcessID)).Result;


                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, MailCategory, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, (int)Util.MailGroupSetup.AdminRequestSupportForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, (int)Util.MailGroupSetup.AdminRequestSupportForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.AdminSupportRequest, (int)Util.MailGroupSetup.AdminRequestSupportForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PettyCashExpenseClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.PettyCashExpenseClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.FinalPettyCashExpenseCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.FinalPettyCashExpenseCliamApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashExpenseClaim, (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PettyCashAdvanceClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.PettyCashAdvanceClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.FinalPettyCashAdvanceClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.FinalPettyCashAdvanceClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.FinalPettyCashAdvanceResubmitClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.FinalPettyCashAdvanceResubmitClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PettyCashReimburseClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.PettyCashReimburseClaimInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.FinalPettyCashReimburseClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.FinalPettyCashReimburseClaimApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.PettyCashPaymentClaim:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.PettyCashPaymentInitiatedMail);
                            break;
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.FinalPettyCashPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Rejected when model.ToAPMemberFeedbackID == 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.FinalPettyCashPaymentApprovalStatusToInitiator, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.PettyCashPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        case (int)Util.ApprovalFeedback.Forwarded when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.PettyCashPaymentForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                        default:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.PettyCashPaymentForwardFeedbackReceieve, recieverMail.Item1, recieverMail.Item2, (int)Util.ApprovalFeedback.ForwardResponseReceived);
                            break;
                    }
                    break;
                case (int)Util.ApprovalType.ExternalAudit:
                    switch (model.APFeedbackID)
                    {
                        case (int)Util.ApprovalFeedback.Approved when recieverMail.Item1.Count.IsNotZero():
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExternalAudit, (int)Util.MailGroupSetup.FinalExternalAuditApprovalStatusToInitiator);
                            break;
                        case (int)Util.ApprovalFeedback.Returned when model.ToAPMemberFeedbackID > 0:
                            await SendMail(model.ApprovalProcessID.ToString(), model.ReferenceID, (int)Util.ApprovalType.ExternalAudit, (int)Util.MailGroupSetup.ExternalAuditForwardedOrRejectionMail, recieverMail.Item1, recieverMail.Item2, model.APFeedbackID);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task SendMailToSettledReceiver(string ApprovalProcessID, int ReferenceId, int MailGroupID, int APTypeID)
        {
            var recieverMail = GetAPEmployeeEmailsForAllPanelMembers(ApprovalProcessID.ToInt()).Result;
            await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, MailGroupID, false, recieverMail.Item1, recieverMail.Item2, null, ReferenceId);
        }
        private async Task SendMail(string ApprovalProcessID, int ReferenceId, int APTypeId, int GroupId, List<string> ToEmail = null, List<string> CCEmail = null, int APFeedbackID = 0)
        {
            List<string> ToEmailAddress = new List<string>();
            List<string> CCEmailAddress = new List<string>();
            if (ToEmail.IsNullOrEmpty() || ToEmail.Count.IsZero())
            {
                var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
                ToEmailAddress = new List<string>() { mail.Item1 };
                CCEmailAddress = mail.Item2;
            }
            else
            {
                ToEmailAddress = ToEmail;//new List<string>() { ToEmail.Trim() };
                CCEmailAddress = CCEmail; //new List<string>() { CCEmail.Trim() };
            }

            await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeId, GroupId, false, ToEmailAddress, CCEmailAddress, null, ReferenceId, APFeedbackID);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }


        private void SetQuotationNewId(Quotations quotation)
        {
            var code = GenerateSystemCode("PurchaseRequisitionQuotation", AppContexts.User.CompanyID);
            quotation.PRQID = code.MaxNumber;
        }

        private List<PurchaseRequisitionChildCostCenterBudgetDto> RemoveCostCenterBudget(SCMApprovalSubmissionDto scmModel)
        {
            if (scmModel.CostCenterBudget.Count > 0)
            {
                var budgetList = new List<PurchaseRequisitionChildCostCenterBudgetDto>();
                string budgetListSql = $@"SELECT * FROM  SCM..PurchaseRequisitionChildCostCenterBudget WHERE PRMasterID={scmModel.ReferenceID}";
                var prevBudgets = ApprovalPanelEmployeeRepo.GetDataDictCollection(budgetListSql);

                foreach (var data in prevBudgets)
                {
                    budgetList.Add(new PurchaseRequisitionChildCostCenterBudgetDto
                    {
                        PRCCCBID = Convert.ToInt32(data["PRCCCBID"]),
                        PRMasterID = Convert.ToInt32(data["PRMasterID"]),
                        ForID = Convert.ToInt32(data["ForID"]),
                        FromDate = Convert.ToDateTime(data["FromDate"]),
                        ToDate = Convert.ToDateTime(data["ToDate"]),
                        AllocatedBudgetAmount = Convert.ToDecimal(data["AllocatedBudgetAmount"]),
                        RemainingBudgetAmount = Convert.ToDecimal(data["RemainingBudgetAmount"]),
                        Note = data["Note"].ToString()
                    });
                }
                var removeList = budgetList.Where(x => !scmModel.CostCenterBudget.Where(z => z.PRCCCBID != 0).Select(y => y.PRCCCBID).Contains(x.PRCCCBID)).ToList();

                return removeList;
            }
            return null;
        }

        private void SetPurchaseRequisitionChildCostCenterBudgetNewId(PurchaseRequisitionChildCostCenterBudgetDto budget)
        {
            var code = GenerateSystemCode("PurchaseRequisitionChildCostCenterBudget", AppContexts.User.CompanyID);
            budget.PRCCCBID = code.MaxNumber;
        }

        private List<Attachment> RemoveAttachmentsMR(SCMApprovalSubmissionForMRDto quotation)
        {
            if (quotation.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialRequisitionMaster' AND ReferenceID={quotation.ReferenceID}";
                var prevAttachment = ApprovalPanelEmployeeRepo.GetDataDictCollection(attachmentSql);

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachment
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !quotation.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "MaterialRequisition";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }

        private List<Attachment> RemoveAttachments(SCMApprovalSubmissionDto quotation)
        {
            if (quotation.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseRequisitionQuotation' AND ReferenceID={quotation.ReferenceID}";
                var prevAttachment = ApprovalPanelEmployeeRepo.GetDataDictCollection(attachmentSql);

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachment
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !quotation.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PurchaseRequisitionQuotation";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private List<Attachment> AddAttachments(List<Attachment> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"PurchaseRequisitionQuotation-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PurchaseRequisitionQuotation\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        attachemntList.Add(new Attachment
                        {
                            FilePath = filePath,
                            OriginalName = Path.GetFileNameWithoutExtension(attachment.OriginalName),
                            FileName = filename,
                            Type = Path.GetExtension(attachment.OriginalName),
                            Size = attachment.Size,
                            Description = attachment.Description
                        });

                        sl++;
                    }

                }
                return attachemntList;
            }
            return null;
        }
        private List<Quotations> RemoveQuotations(SCMApprovalSubmissionDto scmModel)
        {
            if (scmModel.Quotations.Count > 0)
            {
                var quotationList = new List<Quotations>();
                string quotationListSql = $@"SELECT * FROM  SCM..PurchaseRequisitionQuotation WHERE PRMasterID={scmModel.ReferenceID}";
                var prevQuotations = ApprovalPanelEmployeeRepo.GetDataDictCollection(quotationListSql);

                foreach (var data in prevQuotations)
                {
                    quotationList.Add(new Quotations
                    {
                        PRQID = Convert.ToInt32(data["PRQID"]),
                        PRMasterID = Convert.ToInt32(data["PRMasterID"]),
                        SupplierID = Convert.ToInt32(data["SupplierID"]),
                        Description = data["Description"].ToString(),
                        Amount = Convert.ToDecimal(data["Amount"]),
                        QuotedQty = Convert.ToDecimal(data["QuotedQty"]),
                        QuotedUnitPrice = Convert.ToDecimal(data["QuotedUnitPrice"]),
                        TaxTypeID = Convert.ToInt32(data["TaxTypeID"]),
                        ItemID = Convert.ToInt32(data["ItemID"]),
                        PRCID = Convert.ToInt32(data["PRCID"])

                    });
                }
                var removeList = quotationList.Where(x => !scmModel.Quotations.Where(z => z.PRQID != 0).Select(y => y.PRQID).Contains(x.PRQID)).ToList();

                return removeList;
            }
            return null;
        }

        public async Task<(bool, string)> ResetPurchaseOrderApproval(ApprovalSubmissionDto mdl)
        {
            //bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, mdl.ApprovalProcessID);
            //if (!isCurrentEmployee)
            //{
            //    return (false, "You are not current panel member.");
            //}
            using (var unitOfWork = new UnitOfWork())
            {
                var ApprovalProcess = ApprovalProcessRepo.Entities.Single(x => x.ApprovalProcessID == mdl.ApprovalProcessID && x.APTypeID == (int)Util.ApprovalType.PO);

                if (ApprovalProcess.IsNotNull())
                {
                    ApprovalProcess.APStatusID = 2;
                    ApprovalProcess.SetModified();
                }

                var ApprovalEmployeeFeedback = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.ApprovalProcessID == mdl.ApprovalProcessID).ToList();

                ApprovalEmployeeFeedback.ForEach(x =>
                {
                    var existingModelData = ApprovalEmployeeFeedbackRepo.FirstOrDefault(y => y.APEmployeeFeedbackID == x.APEmployeeFeedbackID);

                    if (x.SequenceNo == 1)
                    {
                        x.APFeedbackID = 2;
                        x.FeedbackRequestDate = DateTime.Now;
                        x.FeedbackLastResponseDate = null;
                        x.FeedbackSubmitDate = null;
                    }

                    if (x.SequenceNo > 1)
                    {
                        x.APFeedbackID = 1;
                        x.FeedbackRequestDate = null;
                        x.FeedbackLastResponseDate = null;
                        x.FeedbackSubmitDate = null;
                    }
                    x.CreatedBy = existingModelData.CreatedBy;
                    x.CreatedDate = existingModelData.CreatedDate;
                    x.CreatedIP = existingModelData.CreatedIP;
                    x.RowVersion = existingModelData.RowVersion;
                    x.SetModified();

                });

                int[] apPanelIds = { 13, 14 };
                var proxyEmps = ApprovalPanelEmployeeRepo.Entities.Where(ap => apPanelIds.Contains(ap.APPanelID) && ap.IsProxyEmployeeEnabled).Select(s => new
                {
                    s.ProxyEmployeeID
                }).Distinct().ToList();

                //select* from Approval..ApprovalPanel where Name = 'PO Below The Limit';

                //select* from Approval..ApprovalPanelEmployee where APPanelID in (13, 14)
                //int proxyEmpID = 0;
                //foreach(var pe in proxyEmps)
                //{
                //    if((int)pe.ProxyEmployeeID == (int)AppContexts.User.EmployeeID)
                //    {
                //        proxyEmpID = (int)AppContexts.User.EmployeeID;
                //        break;
                //    }
                //}

                var ApprovalEmployeeFeedbackRemarks = new ApprovalEmployeeFeedbackRemarks
                {
                    ApprovalProcessID = mdl.ApprovalProcessID,
                    Remarks = mdl.Remarks,
                    RemarksDateTime = DateTime.Now,
                    EmployeeID = (int)AppContexts.User.EmployeeID,
                    APFeedbackID = 10,
                    //ProxyEmployeeID = 0,
                    IsProxyEmployeeRemarks = false,
                    //ProxyEmployeeRemarks = ""
                };
                ApprovalEmployeeFeedbackRemarks.SetAdded();
                SetApprovalEmployeeFeedbackRemarksNewId(ApprovalEmployeeFeedbackRemarks);

                SetAuditFields(ApprovalEmployeeFeedbackRemarks);
                SetAuditFields(ApprovalProcess);
                SetAuditFields(ApprovalEmployeeFeedback);

                ApprovalEmployeeFeedbackRepo.AddRange(ApprovalEmployeeFeedback);
                ApprovalProcessRepo.Add(ApprovalProcess);
                ApprovalEmployeeFeedbackRemarksRepo.Add(ApprovalEmployeeFeedbackRemarks);

                // UpdateApprovalProcessFeedback(mdl.ApprovalProcessID, mdl.APEmployeeFeedbackID, 10, mdl.Remarks, (int)Util.ApprovalType.PO, ApprovalProcess.ReferenceID, mdl.ToAPMemberFeedbackID);
                unitOfWork.CommitChangesWithAudit();

            }

            return (true, "Reset Successfully");

        }

        public async Task<(bool, string)> ResetApproval(ApprovalSubmissionDto mdl)
        {

            string sql = $@" SELECT IPM . * from Approval..ApprovalProcess AP
                        LEFT JOIN Accounts..ExpenseClaimMaster ECM ON ECM.ECMasterID = AP.ReferenceID AND ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}
                        LEFT JOIN Accounts..IOUOrExpensePaymentChild IPC ON IPC.IOUOrExpenseClaimID = AP.ReferenceID
						JOIN Accounts..IOUOrExpensePaymentMaster IPM ON IPM.PaymentMasterID = IPC.PaymentMasterID AND IPM.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                        WHERE ApprovalProcessID = " + mdl.ApprovalProcessID +" And APTypeID = "+ (int)mdl.APTypeID +" ";
            var res = ApprovalProcessRepo.GetDataDictCollection(sql);

            if (res.Count() > 0)
            {
                return (false, "Reset fail!");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                var ApprovalProcess = ApprovalProcessRepo.Entities.Single(x => x.ApprovalProcessID == mdl.ApprovalProcessID && x.APTypeID == (int)mdl.APTypeID);

                if (ApprovalProcess.IsNotNull())
                {
                    ApprovalProcess.APStatusID = 2;
                    ApprovalProcess.SetModified();
                }

                var ApprovalEmployeeFeedback = ApprovalEmployeeFeedbackRepo.Entities.Where(x => x.ApprovalProcessID == mdl.ApprovalProcessID).ToList();

                ApprovalEmployeeFeedback.ForEach(x =>
                {
                    var existingModelData = ApprovalEmployeeFeedbackRepo.FirstOrDefault(y => y.APEmployeeFeedbackID == x.APEmployeeFeedbackID);

                    if (x.SequenceNo == 1)
                    {
                        x.APFeedbackID = 2;
                        x.FeedbackRequestDate = DateTime.Now;
                        x.FeedbackLastResponseDate = null;
                        x.FeedbackSubmitDate = null;
                    }

                    if (x.SequenceNo > 1)
                    {
                        x.APFeedbackID = 1;
                        x.FeedbackRequestDate = null;
                        x.FeedbackLastResponseDate = null;
                        x.FeedbackSubmitDate = null;
                    }
                    x.CreatedBy = existingModelData.CreatedBy;
                    x.CreatedDate = existingModelData.CreatedDate;
                    x.CreatedIP = existingModelData.CreatedIP;
                    x.RowVersion = existingModelData.RowVersion;
                    x.SetModified();

                });

                var ApprovalEmployeeFeedbackRemarks = new ApprovalEmployeeFeedbackRemarks
                {
                    ApprovalProcessID = mdl.ApprovalProcessID,
                    Remarks = mdl.Remarks,
                    RemarksDateTime = DateTime.Now,
                    EmployeeID = (int)AppContexts.User.EmployeeID,
                    APFeedbackID = 10,
                    //ProxyEmployeeID = 0,
                    IsProxyEmployeeRemarks = false,
                    //ProxyEmployeeRemarks = ""
                };
                ApprovalEmployeeFeedbackRemarks.SetAdded();
                SetApprovalEmployeeFeedbackRemarksNewId(ApprovalEmployeeFeedbackRemarks);

                SetAuditFields(ApprovalEmployeeFeedbackRemarks);
                SetAuditFields(ApprovalProcess);
                SetAuditFields(ApprovalEmployeeFeedback);

                ApprovalEmployeeFeedbackRepo.AddRange(ApprovalEmployeeFeedback);
                ApprovalProcessRepo.Add(ApprovalProcess);
                ApprovalEmployeeFeedbackRemarksRepo.Add(ApprovalEmployeeFeedbackRemarks);

                unitOfWork.CommitChangesWithAudit();

            }

            return (true, "Reset Successfully");

        }

        private void SetApprovalEmployeeFeedbackRemarksNewId(ApprovalEmployeeFeedbackRemarks master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("ApprovalEmployeeFeedbackRemarks", AppContexts.User.CompanyID);
            master.APEmployeeFeedbackRemarksID = code.MaxNumber;
        }

        private bool CheckCurrentAPEmployee(int EmployeeID, int ApprovalProcessID)
        {
            var sql = $@"SELECT DBO.fnValidateCurrentAPEmployee({EmployeeID},{ApprovalProcessID}) IsCurrent";
            var result = ApprovalProcessRepo.GetData(sql);
            if (result.Count() > 0)
            {
                return (bool)result["IsCurrent"];
            }
            return false;
        }

        private bool CheckCurrentAPEmployeeParallel(int EmployeeID, int ApprovalProcessID, int APEmployeeFeedbackID)
        {
            var sql = $@"SELECT DBO.fnValidateCurrentAPEmployeeParallel({EmployeeID},{ApprovalProcessID},{APEmployeeFeedbackID}) IsCurrent";
            var result = ApprovalProcessRepo.GetData(sql);
            if (result.Count() > 0)
            {
                return (bool)result["IsCurrent"];
            }
            return false;
        }
        private bool CheckIsEditableEmployee(int EmployeeID, int ApprovalProcessID)
        {
            var sql = $@"SELECT DBO.fnValidateIsEditableEmployee({EmployeeID},{ApprovalProcessID}) IsEditable";
            var result = ApprovalProcessRepo.GetData(sql);
            if (result.Count() > 0)
            {
                return (bool)result["IsEditable"];
            }
            return false;
        }
        public async Task<(bool, string)> ApprovalSubmissionForAccessDeactivation(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployeeParallel(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID, model.APEmployeeFeedbackID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..EmployeeAccessDeactivation WHERE EADID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Data has Already been Deleted/Cancelled By The Initiator!");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedbackForParallel(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                unitOfWork.CommitChangesWithAudit();
                //SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> ApprovalSubmissionForDivisionClearence(EADApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..EmployeeAccessDeactivation WHERE EADID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Data has Already been Deleted/Cancelled By The Initiator!");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                if (model.IsEditable.IsTrue())
                {
                    SubmitFunctionalForAccessDeactivation(model.ReferenceID, model.IsCoreFunctional);
                }

                unitOfWork.CommitChangesWithAudit();
                SendApprovalMailEAD(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        //ApprovalSubmissionForDocumentUpload
        public async Task<(bool, string, bool)> ApprovalSubmissionForDocumentUpload(ApprovalSubmissionDto model)
        {
            bool IsLastApproval = false;
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.", IsLastApproval);
            }
            string sql = @$"SELECT * FROM HRMS..DocumentUpload WHERE DUID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Data has Already been Deleted/Cancelled By The Initiator!", IsLastApproval);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                unitOfWork.CommitChangesWithAudit();
                SendApprovalMailDocumentUpload(model);
                var recieverMail = GetApprovalActionEmployeesForMultiProxyEmail(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.ToAPMemberFeedbackID).Result;
                if (model.APFeedbackID == (int)Util.ApprovalFeedback.Approved && recieverMail.Item1.Count.IsNotZero())
                {
                    IsLastApproval = true;
                }
            }
            return (true, "Approval Submitted Successfully", IsLastApproval);
        }

        public async Task<(bool, string, bool)> ApprovalSubmissionForSupportRequest(SRApprovalSubmissionDto model)
        {
            bool IsLastApproval = false;
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.", IsLastApproval);
            }
            string sql = @$"SELECT * FROM HRMS..RequestSupportMaster WHERE RSMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Data has Already been Deleted/Cancelled By The Initiator!", IsLastApproval);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {

                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID
                    , model.IsEditable, false, false);
                }

                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                //else
                //{
                //    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                //}

                if (model.IsEditable == true)
                {
                    if (!string.IsNullOrEmpty(model.AdminRemarks))
                    {
                        SubmitAdminRemarks(model.ReferenceID, model.AdminRemarks);
                    }
                    //st
                    if (model.SupportTypeID == (int)Util.SystemVariables.VehicleOrTransport)
                    {
                        UpdateVehicleInfo(model.ReferenceID, model.FromDateChange, model.ToDateChange, model.InTime.ToString(), model.OutTime.ToString(), model.Duration, model.TransportTypeID, model.TransportQuantity, model.IsOthers, model.Vehicle, model.Driver, model.ContactNumber);
                    }
                    if (model.SupportTypeID == (int)Util.SystemVariables.FacilitiesOrSupport)
                    {
                        UpdateFacilitiesDetailsInfo(model.ReferenceID, model.NeededByDateChng);
                    }

                    if (model.SupportTypeID == (int)Util.SystemVariables.ConsumableGoods)
                    {
                        foreach (var item in model.ItemDetails)
                        {
                            UpdateItemQuantity(item.RSIDID, (decimal)item.Quantity, item.ItemID);
                        }

                    }

                }

                //end
                unitOfWork.CommitChangesWithAudit();
                if (model.APFeedbackID != (int)Util.ApprovalFeedback.Draft)
                {
                    SendApprovalMailSupportRequest(model);
                }

            }
            return (true, "Approval Submitted Successfully", false);
        }


        public async Task<(bool, string)> SubmitPettyCashExpenseApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..PettyCashExpenseMaster WHERE PCEMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Petty Cash Expense Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitPettyCashAdvanceApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..PettyCashAdvanceMaster WHERE PCAMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Petty Cash Advance Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitPettyCashAdvanceApprovalResubmit(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..PettyCashAdvanceMaster WHERE PCAMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Petty Cash Advance Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        //SubmitPettyCashReimburseApproval
        public async Task<(bool, string)> SubmitPettyCashReimburseApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..PettyCashReimburseMaster WHERE PCRMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Petty Cash Reimburse Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }
        public async Task<(bool, string)> SubmitPettyCashPaymentApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM Accounts..PettyCashPaymentMaster WHERE PCPMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Petty Cash Reimburse Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        //SubmitSupportRequisitionApproval
        public async Task<(bool, string)> SubmitSupportRequisitionApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..SupportRequisitionMaster WHERE SRMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This Support Requisition Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (model.APForwardInfoID > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedback(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }

                if (model.IsEditable)
                {
                    SubmitITRecommendation(model.ReferenceID, model.ITRecommendation);
                }
                unitOfWork.CommitChangesWithAudit();
                SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }

        public async Task<(bool, string)> SubmitExternalAuditApproval(ApprovalSubmissionDto model)
        {
            bool isCurrentEmployee = CheckCurrentAPEmployee(AppContexts.User.EmployeeID.Value, model.ApprovalProcessID);
            if (!isCurrentEmployee)
            {
                return (false, "You are not current panel member.");
            }
            string sql = @$"SELECT * FROM HRMS..ExternalAuditMaster where EAMID={model.ReferenceID}";
            var isExist = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
            if (isExist.Count() <= 0)
            {
                return (false, "Sorry, This External Audit Already Deleted/Cancelled By The Initiator");
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (Convert.ToInt32(model.APForwardInfoID) > 0)
                {
                    SubmitComment(model.APForwardInfoID, model.APEmployeeFeedbackID, model.Remarks);
                }
                else
                {
                    UpdateApprovalProcessFeedbackForParallel(model.ApprovalProcessID, model.APEmployeeFeedbackID, model.APFeedbackID, model.Remarks, model.APTypeID, model.ReferenceID, model.ToAPMemberFeedbackID);
                }
                unitOfWork.CommitChangesWithAudit();
                await SendApprovalMail(model);
            }
            return (true, "Approval Submitted Successfully");
        }


    }
}
