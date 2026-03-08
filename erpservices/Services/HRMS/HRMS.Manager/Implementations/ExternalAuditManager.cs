using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class ExternalAuditManager : ManagerBase, IExternalAuditManager
    {
        private readonly IRepository<ExternalAuditMaster> MasterRepo;
        private readonly IRepository<ExternalAuditChild> ChildRepo;
        private readonly IRepository<AuditApprovalConfig> AuditApprovalConfigRepo;
        private readonly IRepository<UserWiseUddoktaOrMerchantMapping> WalletMappingRepo;
        private readonly IRepository<Department> DepartmentRepo;
        private readonly IRepository<AuditQuestion> AuditQuestionRepo;
        private readonly IRepository<DAL.Entities.ExternalAuditConfig> AuditConfig;

        public ExternalAuditManager(IRepository<ExternalAuditMaster> masterRepo,
                                    IRepository<ExternalAuditChild> childRepo,
                                    IRepository<AuditApprovalConfig> auditApprovalConfigRepo,
                                    IRepository<UserWiseUddoktaOrMerchantMapping> walletMappingRepo,
                                    IRepository<Department> departmentRepo,
                                    IRepository<AuditQuestion> auditQuestionRepo,
                                    IRepository<DAL.Entities.ExternalAuditConfig> auditConfig)
        {
            MasterRepo = masterRepo;
            ChildRepo = childRepo;
            AuditApprovalConfigRepo = auditApprovalConfigRepo;
            WalletMappingRepo = walletMappingRepo;
            DepartmentRepo = departmentRepo;
            AuditQuestionRepo = auditQuestionRepo;
            AuditConfig = auditConfig;
        }
        public async Task<(bool, string)> SaveChanges(AuditMasterDto dto)
        {
            try
            {
                string ApprovalProcessID = "0";
                var approvalProcessFeedBack = new Dictionary<string, object>();
                /// Check Merchant or Uddokta Wallet with the User
                //string[] walletNameNumber = dto.MercentOrUddokta.label.Split('#');
                var mappedWallet = WalletMappingRepo.FirstOrDefault(u => u.MAPID == dto.MercentOrUddokta.value && u.CreatedBy == AppContexts.User.UserID);
                if (mappedWallet == null)
                {
                    return (false, "You don't have permission to save this Audit.");
                }
                if (dto.EAMID > 0 && dto.ApprovalProcessID > 0)
                {
                    string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..ExternalAuditMaster EAD
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = EAD.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.Default)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = EAD.EAMID AND AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExternalAudit}
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EAMID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {dto.ApprovalProcessID}";
                    var canReassesstment = MasterRepo.GetData(sql);
                    if (canReassesstment.Count > 0)
                    {
                        var canSubmit = (bool)canReassesstment["CanReassestment"];
                        if (!canSubmit)
                        {
                            return (false, $"Sorry, You can not edit External Audit once it processed from approval panel");
                        }
                    }
                    else
                    {
                        return (false, $"Sorry, You can not edit External Audit once it processed from approval panel");
                    }
                    approvalProcessFeedBack = GetApprovalProcessFeedback(dto.EAMID, dto.ApprovalProcessID, (int)Util.ApprovalType.ExternalAudit);
                }

                var isValidWalletForUser = WalletMappingRepo.Entities.Where(w => w.CreatedBy == AppContexts.User.UserID && w.MAPID == dto.MercentOrUddokta.value && w.IsActive == true && w.IsTagged == true).ToList();

                if (isValidWalletForUser == null || isValidWalletForUser.Count < 1)
                {
                    return (false, $@"Wallet is not tagged with your profile");
                }

                var lastAuditDateList = MasterRepo.Entities.Where(a => a.MercentOrUdoktaID.Equals(dto.MercentOrUddokta.value))
                                                        .OrderByDescending(a => a.AuditDate).ToList();
                //var lastAuditDateList = MasterRepo.Entities.Where(a => a.MercentOrUdoktaNumber.Equals(mappedWallet.WalletNumber))
                //                                       .OrderByDescending(a => a.AuditDate).ToList();

                if (lastAuditDateList.Count > 0 && dto.EAMID == 0)
                {
                    var lastAuditDays = Convert.ToInt32((DateTime.Now - lastAuditDateList[0].AuditDate).TotalDays);
                    int auditThresoldDays = AuditConfig.Entities.FirstOrDefault(c => c.IsActive == true).NumberOfDays;
                   // if (lastAuditDays <= (int)Util.ExternalAuditConfig.AuditThresold)
                    if (lastAuditDays < auditThresoldDays)
                    {
                        return (false, $@"This wallet has already audited within {auditThresoldDays} days.");
                    }
                }

                var masterModel = new ExternalAuditMaster
                {
                    MercentOrUdoktaID = dto.MercentOrUddokta.value,
                    MercentOrUdoktaNumber = mappedWallet.WalletNumber,
                    Remarks = dto.Remarks ?? string.Empty,
                    IsDraft = dto.IsDraft,
                    ApprovalStatusID = dto.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                    Requirements = dto.Requirements,

                };
                var existingAudit = MasterRepo.FirstOrDefault(m => m.EAMID == dto.EAMID);
                var auditChildList = new List<ExternalAuditChild>();

                using (var unitOfWork = new UnitOfWork())
                {
                    if (existingAudit != null && existingAudit.EAMID > 0)
                    {
                        masterModel.AuditDate = existingAudit.AuditDate;
                        masterModel.CreatedBy = existingAudit.CreatedBy;
                        masterModel.CreatedDate = existingAudit.CreatedDate;
                        masterModel.CreatedIP = existingAudit.CreatedIP;
                        masterModel.RowVersion = existingAudit.RowVersion;
                        masterModel.EAMID = existingAudit.EAMID;
                        masterModel.Longtitude = existingAudit.Longtitude;
                        masterModel.Latitude = existingAudit.Latitude;
                        masterModel.Requirements = dto.Requirements;
                        masterModel.Remarks = dto.Remarks;
                        masterModel.AuditableEmployeeID = existingAudit.AuditableEmployeeID;
                        masterModel.ReferenceKeyword = existingAudit.ReferenceKeyword;
                        masterModel.ReferenceNo = existingAudit.ReferenceNo;
                        masterModel.CapturedImage = existingAudit.CapturedImage;
                        masterModel.SetModified();
                    }
                    else
                    {
                        masterModel.ReferenceNo = GenerateExternalAuditReference() + (string.IsNullOrWhiteSpace(dto.ReferenceKeyword) ? "" : $"/{dto.ReferenceKeyword.ToUpper()}");
                        masterModel.ReferenceKeyword = dto.ReferenceKeyword;
                        masterModel.AuditableEmployeeID = Convert.ToInt32(AppContexts.User.EmployeeID);
                        masterModel.Longtitude = dto.Longtitude;
                        masterModel.Latitude = dto.Latitude;
                        masterModel.AuditDate = DateTime.Now;
                        masterModel.CapturedImage = dto.CapturedImage;
                        masterModel.SetAdded();
                    }
                    SetAuditFields(masterModel);

                    MasterRepo.Add(masterModel);
                    MasterRepo.SaveChanges();

                    RemoveMasterAttachments(dto.Attachments, masterModel.EAMID);
                    AddMasterAttachments(dto.Attachments, masterModel.EAMID);
                    //using (var unitOfWork = new UnitOfWork())
                    //{

                    //    //unitOfWork.CommitChangesWithAudit();
                    //}

                    var existingChildItem = ChildRepo.Entities.Where(m => m.EAMID == masterModel.EAMID).ToList();
                    if (existingChildItem != null && existingChildItem.Count > 0)
                    {

                        // var deletedChildItem = existingChildItem.Where(i => !dto.AuditDetails.Any(n => n.EACID == i.EACID)).ToList();
                        // var modifiedChildItem = dto.AuditDetails.Where(i => existingChildItem.Any(n => n.EACID == i.EACID)).ToList();
                        //var newChildItem = dto.AuditDetails.Where(c => c.EACID == 0).ToList();

                        //Delete item
                        //foreach (var child in deletedChildItem)
                        //{
                        //    var childModel = new ExternalAuditChild
                        //    {
                        //        EAMID = child.EAMID,
                        //        EACID = child.EACID,
                        //        AuditQuestionID = child.AuditQuestionID,
                        //        DepartmentID = child.DepartmentID,
                        //        CompanyID = child.CompanyID,
                        //        CreatedBy = child.CreatedBy,
                        //        CreatedDate = child.CreatedDate,
                        //        CreatedIP = child.CreatedIP,
                        //        RowVersion  = child.RowVersion
                        //    };
                        //    childModel.SetDeleted();
                        //    auditChildList.Add(childModel);
                        //}

                        //Modify item
                        foreach (var child in dto.AuditDetails)
                        {
                            string posmIDS = string.Empty;
                            foreach (var posm in child.MultiPOSMList)
                            {
                                posmIDS = (string.IsNullOrEmpty(posmIDS) ? "" : (posmIDS + ",")) + posm.value.ToString();
                            }
                            var modifyChildItem = existingChildItem.FirstOrDefault(m => m.EACID == child.EACID);

                            var childModel = new ExternalAuditChild
                            {
                                EAMID = modifyChildItem.EAMID,
                                EACID = modifyChildItem.EACID,
                                DepartmentID = modifyChildItem.DepartmentID,
                                AuditQuestionID = child.Question[0].value,
                                QuestionFeedback = child.Feedback,
                                RowVersion = modifyChildItem.RowVersion,
                                CreatedBy = modifyChildItem.CreatedBy,
                                CreatedDate = modifyChildItem.CreatedDate,
                                CompanyID = modifyChildItem.CompanyID,
                                POSMIDs = posmIDS,
                                Requirements = child.Requirements
                            };
                            childModel.SetModified();
                            SetAuditFields(childModel);
                            auditChildList.Add(childModel);
                        }

                        //New item
                        //foreach (var child in newChildItem)
                        //{
                        //    var childModel = new ExternalAuditChild
                        //    {
                        //        EAMID = dto.EAMID,
                        //        AuditQuestionID = child.Question[0].value,
                        //        DepartmentID = child.DepartmentID,
                        //        QuestionFeedback = child.Feedback,
                        //    };
                        //    childModel.SetAdded();
                        //    SetAuditFields(childModel);
                        //    auditChildList.Add(childModel);
                        //}
                    }
                    else
                    {
                        foreach (var item in dto.AuditDetails)
                        {
                            string posmIDS = string.Empty;
                            foreach (var posm in item.MultiPOSMList)
                            {
                                posmIDS = (string.IsNullOrEmpty(posmIDS) ? "" : (posmIDS + ",")) + posm.value.ToString();
                            }
                            var auditApvConfigs = AuditApprovalConfigRepo.Entities.Where(apv => apv.QuestionIDs.Contains(item.Question[0].value.ToString())).ToList();
                            if (auditApvConfigs.Any())
                            {
                                string[] deptIDs = auditApvConfigs[0].DepartmentIDs.Split(',');
                                foreach (var deptID in deptIDs)
                                {
                                    var childModel = new ExternalAuditChild
                                    {
                                        EAMID = masterModel.EAMID,
                                        AuditQuestionID = item.Question[0].value,
                                        QuestionFeedback = item.Feedback,
                                        DepartmentID = Convert.ToInt32(deptID),
                                        Requirements = item.Requirements,
                                        POSMIDs = posmIDS
                                    };
                                    childModel.SetAdded();
                                    SetAuditFields(childModel);
                                    auditChildList.Add(childModel);
                                }
                            }
                        }
                    }
                    ChildRepo.AddRange(auditChildList);
                    ChildRepo.SaveChanges();
                    //using (var unitOfWork = new UnitOfWork())
                    //{
                    //    unitOfWork.CommitChangesWithAudit();
                    //}

                    //using (var unitOfWork = new UnitOfWork())
                    //{
                    List<Attachments> attachments = new List<Attachments>();
                    var newAttachments = auditChildList.Where(att => att.IsAdded).ToList();

                    if (dto.EAMID > 0)
                    {
                        foreach (var details in dto.AuditDetails)
                        {
                            foreach (var attchment in details.Attachments)
                            {
                                var att = new Attachments
                                {
                                    AID = attchment.AID,
                                    AttachedFile = attchment.AttachedFile,
                                    Description = attchment.Description,
                                    FileName = attchment.FileName,
                                    FilePath = attchment.FilePath,
                                    FUID = attchment.FUID,
                                    OriginalName = attchment.OriginalName,
                                    ReferenceId = details.EACID == 0 ? newAttachments.FirstOrDefault(n => n.DepartmentID == details.DepartmentID && n.AuditQuestionID == details.Question[0].value).EACID : details.EACID,
                                    Size = attchment.Size,
                                    TableName = "ExternalAuditChild",
                                    Type = attchment.Type
                                };
                                attachments.Add(att);
                            }
                        }
                    }
                    else
                    {
                        foreach (var details in dto.AuditDetails)
                        {
                            foreach (var item in auditChildList)
                            {
                                if (details.Question[0].value == item.AuditQuestionID)
                                {
                                    foreach (var attchment in details.Attachments)
                                    {
                                        var att = new Attachments
                                        {
                                            AID = attchment.AID,
                                            AttachedFile = attchment.AttachedFile,
                                            Description = attchment.Description,
                                            FileName = attchment.FileName,
                                            FilePath = attchment.FilePath,
                                            FUID = attchment.FUID,
                                            OriginalName = attchment.OriginalName,
                                            ReferenceId = item.EACID,
                                            Size = attchment.Size,
                                            TableName = "ExternalAuditChild",
                                            Type = attchment.Type
                                        };
                                        attachments.Add(att);
                                    }
                                }
                            }
                        }
                    }

                    RemoveAttachments(attachments, auditChildList);
                    AddAttachments(attachments);

                    bool IsResubmitted = false;

                    //approvalProcessFeedBack = GetApprovalProcessFeedback(masterModel.EAMID, dto.ApprovalProcessID, (int)Util.ApprovalType.ExternalAudit);

                    if (!dto.IsDraft)
                    {
                        if (masterModel.IsAdded || (existingAudit.IsDraft && masterModel.IsModified) && string.IsNullOrEmpty(existingAudit.ReturnDepartmentIDs))
                        {
                            string approvalTitle = $"{Util.EXAuditApprovalTitle} {masterModel.MercentOrUdoktaNumber}";
                            ApprovalProcessID = CreateApprovalProcessParallel((int)masterModel.EAMID, Util.AutoExAuditAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.ExternalAudit, (int)Util.ApprovalPanel.ExternalAudit);
                        }
                        else
                        {
                            if (approvalProcessFeedBack.Count > 0)
                            {
                                UpdateApprovalProcessFeedbackForParallel((int)approvalProcessFeedBack["ApprovalProcessID"],
                                    (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                    $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                    (int)approvalProcessFeedBack["APTypeID"],
                                    (int)approvalProcessFeedBack["ReferenceID"], 0, true);
                                IsResubmitted = true;
                                ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                            }
                        }
                    }

                    unitOfWork.CommitChangesWithAudit();
                }
                #region Email Send
                if (!dto.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0 && masterModel.EAMID > 0 && (existingAudit == null || string.IsNullOrEmpty(existingAudit.ReturnDepartmentIDs)))
                        await SendMail(ApprovalProcessID, false, masterModel.EAMID, (int)Util.MailGroupSetup.ExternalAuditInitiatedMail);

                    var auditConfigDepartments = AuditApprovalConfigRepo.Entities.Select(d => d.DepartmentIDs).ToList();
                    List<string> depetIDS = new List<string>();
                    var questions = AuditQuestionRepo.GetAllList();
                    foreach (var audit in auditConfigDepartments)
                    {
                        var deptArr = audit.Split(',');
                        foreach (var dept in deptArr)
                        {
                            depetIDS.Add(dept.Trim());
                        }
                    }
                    var deparrments = DepartmentRepo.GetAllList();
                    depetIDS = depetIDS.Select(d => d).Distinct().ToList();
                    StringBuilder stringChildDataBuilder = new StringBuilder();
                  
                    if (auditChildList.Count() > 0)
                    {
                        if (existingAudit != null && !string.IsNullOrEmpty(existingAudit.ReturnDepartmentIDs))
                        {
                            var returnDeptIds = existingAudit.ReturnDepartmentIDs.Split(",") ?? new string[] { };
                            foreach (var rtnDept in returnDeptIds)
                            {
                                foreach (var auditChild in auditChildList)
                                {
                                    if (Convert.ToInt32(rtnDept) == auditChild.DepartmentID)
                                    {
                                        var ques = questions.FirstOrDefault(q => q.QuestionID == auditChild.AuditQuestionID);
                                        string data = $@"<tr>
					                                    <td style=""text-align: left;border:1px solid"">{ques.Title}</th>
					                                    <td style=""text-align: left;border:1px solid"">{auditChild.QuestionFeedback}</td>
				                                       </tr>
                                                     ";
                                        stringChildDataBuilder.Append(data);
                                    }
                                }
                                var deptName = deparrments.FirstOrDefault(d => d.DepartmentID == Convert.ToInt32(rtnDept)).DepartmentName;
                                string modelTbl = $@" <h3>Audit Details For :  {deptName}</h3>
                                                    <table style=""border:1px solid"">
				                                    <tr>
					                                    <th style=""text-align: left;border:1px solid"">Question</th>
					                                    <th style=""text-align: left;border:1px solid"">Feedback</th>

				                                    </tr>
				                                    {stringChildDataBuilder}
			                                    </table>";
                                stringChildDataBuilder.Clear();

                                await SendMailFromRequestPanelWithDepartment(ApprovalProcessID, false, masterModel.EAMID,
                                            (int)Util.MailGroupSetup.ExternalAuditInitiatedMailDepartmentWise, (int)Util.ApprovalType.ExternalAudit,Convert.ToInt32(rtnDept), modelTbl);
                            }
                        }
                        else
                        {
                            foreach (var dep in depetIDS)
                            {
                                foreach (var auditChild in auditChildList)
                                {
                                    if (Convert.ToInt32(dep) == auditChild.DepartmentID)
                                    {
                                        var ques = questions.FirstOrDefault(q => q.QuestionID == auditChild.AuditQuestionID);
                                        string data = $@"<tr>
					                                    <td style=""text-align: left;border:1px solid"">{ques.Title}</th>
					                                    <td style=""text-align: left;border:1px solid"">{auditChild.QuestionFeedback}</td>
				                                       </tr>
                                                     ";
                                        stringChildDataBuilder.Append(data);
                                    }
                                }
                                var deptName = deparrments.FirstOrDefault(d => d.DepartmentID == Convert.ToInt32(dep)).DepartmentName;
                                string modelTbl = $@" <h3>Audit Details For :  {deptName}</h3>
                                                    <table style=""border:1px solid"">
				                                    <tr>
					                                    <th style=""text-align: left;border:1px solid"">Question</th>
					                                    <th style=""text-align: left;border:1px solid"">Feedback</th>

				                                    </tr>
				                                    {stringChildDataBuilder}
			                                    </table>";
                                stringChildDataBuilder.Clear();

                                await SendMailFromRequestPanelWithDepartment(ApprovalProcessID, false, masterModel.EAMID,
                                            (int)Util.MailGroupSetup.ExternalAuditInitiatedMailDepartmentWise, (int)Util.ApprovalType.ExternalAudit, Convert.ToInt32(dep), modelTbl);
                            }
                        }
                    }
                }
                #endregion
                await Task.CompletedTask;

                return (true, "Audit Saved Successfully.");
            }
            catch (Exception ex)
            {

                return (false, "Something went wrong. Please try later.");
            }
        }
        private string GenerateExternalAuditReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{DateTime.Now.Ticks}";
            return format;
        }
        public async Task SendMailFromRequestPanelWithDepartment(string ApprovalProcessID, bool IsResubmitted, long MasterID, int mailGroup, int APTypeID, int deptID, string modelData)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxyAndSupervisorWithDepartment(ApprovalProcessID.ToInt(), deptID).Result;
            //List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMailWithModel(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)MasterID, 0, modelData);

            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int EAMID, int mailGroup)
        {
            var mail = GetInitiatorEmployeeEmail(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail;
            //List<string> CCEmailAddress = mail.Item2.Where(x => x.IsNotNullOrEmpty()).ToList();
            var count = mail.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.ExternalAudit, mailGroup, IsResubmitted, ToEmailAddress, null, null, EAMID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private void AddAttachments(List<Attachments> auditAttachemnts)
        {
            var attachemntList = auditAttachemnts.Where(x => x.ID == 0).ToList();
            var attachemntUpdateList = auditAttachemnts.Where(x => x.ID != 0).ToList();

            if (attachemntList.IsNotNull() && attachemntList.Count > 0)
            {
                int sl = 0;
                foreach (var attachment in attachemntList)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        // To Add Physical Files

                        string filename = $"ExternalAudit-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "ExternalAuditDetails\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        // To Add Into DB

                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)attachment.ReferenceId, "ExternalAuditChild", false, attachment.Size, 0, false, attachment.Description ?? "");

                        sl++;
                    }
                }
            }
            // Update file
            else
            {
                foreach (var attachment in attachemntUpdateList)
                {
                    SaveSingleAttachment(attachment.FUID, attachment.FilePath, attachment.FileName, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)attachment.ReferenceId, "ExternalAuditChild", false, attachment.Size, 0, false, attachment.Description ?? "", true);
                }
            }
        }
        private void AddMasterAttachments(List<Attachments> auditAttachemnts, int EAMID)
        {
            var attachemntList = auditAttachemnts.Where(x => x.ID == 0).ToList();
            var attachemntUpdateList = auditAttachemnts.Where(x => x.ID != 0).ToList();

            if (attachemntList.IsNotNull() && attachemntList.Count > 0)
            {
                int sl = 0;
                foreach (var attachment in attachemntList)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        // To Add Physical Files
                        string filename = $"ExternalAudit-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "ExternalAuditMaster\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        // To Add Into DB
                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), EAMID, "ExternalAuditMaster", false, attachment.Size, 0, false, attachment.Description ?? "");

                        sl++;
                    }
                }
            }
            // Update file
            else
            {
                foreach (var attachment in attachemntUpdateList)
                {
                    SaveSingleAttachment(attachment.FUID, attachment.FilePath, attachment.FileName, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), EAMID, "ExternalAuditMaster", false, attachment.Size, 0, false, attachment.Description ?? "", true);
                }
            }
        }

        private void RemoveMasterAttachments(List<Attachments> auditAttachemnts, int EAMID)
        {
            var attachemntList = new List<Attachments>();
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='ExternalAuditMaster' AND ReferenceID={EAMID}";
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
                    ReferenceId = Convert.ToInt32(data["ReferenceID"])
                });
            }
            var removeFiles = attachemntList.Where(x => !auditAttachemnts.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

            if (removeFiles.Count > 0)
            {
                foreach (var data in removeFiles)
                {
                    // To Remove Physical Files

                    string attachmentFolder = "upload\\attachments";
                    string folderName = "ExternalAuditMaster";
                    IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                    string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                    //File.Delete(str + "\\" + data.FileName);
                    System.IO.File.Delete(str + "\\" + data.FileName);
                    // To Remove From DB

                    SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)data.ReferenceId, "ExternalAuditMaster", true, data.Size, 0, false, data.Description ?? "");

                }

            }
        }
        private void RemoveAttachments(List<Attachments> auditAttachemnts, List<ExternalAuditChild> childList)
        {
            foreach (var attachment in childList)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='ExternalAuditChild' AND ReferenceID={attachment.EACID}";
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
                        ReferenceId = Convert.ToInt32(data["ReferenceID"])
                    });
                }
                var removeFiles = attachemntList.Where(x => !auditAttachemnts.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeFiles.Count > 0)
                {
                    foreach (var data in removeFiles)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "ExternalAuditDetails";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        //File.Delete(str + "\\" + data.FileName);
                        System.IO.File.Delete(str + "\\" + data.FileName);
                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)data.ReferenceId, "ExternalAuditChild", true, data.Size, 0, false, data.Description ?? "");

                    }

                }
            }
        }
        public GridModel GetListForGrid(GridParameter parameters)
        {
            string filter = "";
            string filter2 = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND exaudit.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND exaudit.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})";
                    break;
                default:
                    break;
            }

            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"select distinct exaudit.EAMID, 
	                           exaudit.ReferenceNo,
	                           exaudit.ReferenceKeyword,
	                           exaudit.MercentOrUdoktaNumber AuditedWalletNo,
	                           ISNULL(map.WalletName,'') AuditedWalletName,
	                           ISNULL(wallettype.SystemVariableCode,'') WalletType,
	                           exaudit.AuditDate,
                               exaudit.CreatedDate,
							   exaudit.CreatedBy,
	                           exaudit.ApprovalStatusID,
	                           sysvar.SystemVariableCode ApprovalStatus,
	                           VA.DepartmentID
	                           ,VA.DepartmentName
	                           ,VA.FullName AS EmployeeName, 
	                           VA.DivisionID, 
	                           VA.DivisionName,
	                           VA.ImagePath
	                           ,VA.EmployeeCode,
	                           VA.FullName+' '+VA.EmployeeCode+' ' +VA.DepartmentName EmployeeWithDepartment,
	                           ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                                CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployeeParallel]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0),AEF.APEmployeeFeedbackID)) AS Bit) IsCurrentAPEmployee
                                ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                                ,ISNULL(APForwardInfoID,0) APForwardInfoID
            	                ,ISNULL(AEF.IsEditable,0) IsEditable
								,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
								,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
								,CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy
								,exaudit.ReturnDepartmentIDs
                                 ,ISNULL(AEF.IsSCM,0) IsSCM
								,Particulars
								,Particulars+' '+(CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END) ParticularsWtihProxy
                        from HRMS..ExternalAuditMaster exaudit
                        LEFT JOIN Security..SystemVariable sysvar ON sysvar.SystemVariableID = exaudit.ApprovalStatusID
                        LEFT JOIN Security..Users U ON U.UserID = exaudit.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..UserWiseUddoktaOrMerchantMapping map on map.MAPID = exaudit.MercentOrUdoktaID
                        LEFT JOIN Security..SystemVariable wallettype on wallettype.SystemVariableID = map.TypeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = exaudit.EAMID AND AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit}
                        LEFT JOIN (
                                 SELECT 
                                       APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy, Particulars
                                 FROM 
                                     Approval.dbo.functionJoinListAEFParallel({AppContexts.User.EmployeeID})
                                             )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                                             LEFT JOIN 
                                               (SELECT 
                                                 APForwardInfoID,ApprovalProcessID 
                                                FROM 
                                                 Approval..ApprovalForwardInfo  
                                                WHERE 
                                                 EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                                             APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                                             LEFT JOIN 
      		                        (
      		                        SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
      		                        )							
                        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                                             LEFT JOIN (
         			                        SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
         			                        (
         			                        --SELECT 
         			                        --	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
         			                        --FROM 
         			                        --	Approval..ApprovalEmployeeFeedback  AEF
         			                        --	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
         			                        --where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExternalAudit} 
         			                        --GROUP BY ReferenceID

         			                        --UNION ALL

         			                        SELECT 
         				                        COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
         			                        FROM 
         				                        Approval..ApprovalEmployeeFeedback AEF
         				                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
         			                        where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND EmployeeID = {AppContexts.User.EmployeeID}
         			                        GROUP BY ReferenceID

         			                        )V
         			                        GROUP BY ReferenceID
         			                        ) EA ON EA.ReferenceID = exaudit.EAMID

                                                         LEFT JOIN(
         			                        SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
         			                        FROM 
         				                        Approval..ApprovalEmployeeFeedbackRemarks AEFR 
         				                        INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
         			                        WHERE APFeedbackID = 11 --Returned
         			                        GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
      		                        ) Rej ON Rej.ReferenceID = exaudit.EAMID
                                                     LEFT JOIN ( 
                                                             SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDateParallel({AppContexts.User.EmployeeID})                                
      		                        )APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID AND APSubmitDate.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
      		                        LEFT JOIN (
         						                        SELECT 
         						                           MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
         						                        FROM 
         							                        Approval..ApprovalForwardInfo  
         						                        WHERE 
         							                        EmployeeID = {AppContexts.User.EmployeeID} 
         						                        GROUP BY ApprovalProcessID

         							                        ) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                                     --LEFT JOIN (
   	                          --     SELECT 
         			                        --    AEF.EmployeeCode PendingEmployeeCode,
         			                        --    EmployeeName PendingEmployeeName,
         			                        --    DepartmentName PendingDepartmentName,
         			                        --    AEF.ReferenceID PendingReferenceID
      		                         --   FROM 
         			                        --    Approval..viewApprovalEmployeeFeedback AEF 
      		                         --   WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
   	                          -- )PendingAt ON  PendingAt.PendingReferenceID = exaudit.EAMID
                        WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                     OR exaudit.AuditableEmployeeID={AppContexts.User.EmployeeID}) {filter} {filter2}";

            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetAllListForGrid(GridParameter parameters)
        {

            string filter = "";
            string filter2 = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" WHERE CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" WHERE exaudit.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" WHERE exaudit.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" WHERE AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    //filter = $@" WHERE AP.ApprovalProcessID IN 
                    //            (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                    //            UNION  
                    //            SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                    //            UNION 
                    //            SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }

            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"select exaudit.EAMID, 
	                           exaudit.ReferenceNo,
	                           exaudit.ReferenceKeyword,
	                           exaudit.MercentOrUdoktaNumber AuditedWalletNo,
	                           map.WalletName AuditedWalletName,
	                           wallettype.SystemVariableCode WalletType,
	                           exaudit.AuditDate,
	                           exaudit.ApprovalStatusID,
	                           sysvar.SystemVariableCode ApprovalStatus,
	                           VA.DepartmentID
	                           ,VA.DepartmentName
	                           ,VA.FullName AS EmployeeName, 
	                           VA.DivisionID, 
	                           VA.DivisionName,
	                           VA.ImagePath
	                           ,VA.EmployeeCode,
	                           VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment,
	                           ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from HRMS..ExternalAuditMaster exaudit
                        LEFT JOIN Security..SystemVariable sysvar ON sysvar.SystemVariableID = exaudit.ApprovalStatusID
                        LEFT JOIN Security..Users U ON U.UserID = exaudit.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..UserWiseUddoktaOrMerchantMapping map on map.MAPID = exaudit.MercentOrUdoktaID
                        LEFT JOIN Security..SystemVariable wallettype on wallettype.SystemVariableID = map.TypeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = exaudit.EAMID AND AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit}
                        {filter} {filter2}";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public async Task<List<Attachments>> GetMasterAttachments(int EAMID)
        {
            string attchmentListSql = $@"select FUID,
                                                FUID AID,
		                                        '' AttachedFile,
		                                        FileType Type,
		                                        OriginalName,
		                                        FilePath,
		                                        FileName,
		                                        ReferenceID,
		                                        SizeInKB Size,
                                                Description
                                            from Security..FileUpload
                                            where TableName = 'ExternalAuditMaster' AND ReferenceID = {EAMID}";

            var attchments = MasterRepo.GetDataModelCollection<Attachments>(attchmentListSql);
            return attchments;
        }
        public async Task<dynamic> GetExternalAuditMaster(int EAMID)
        {
            //  string sql = $@"select exaudit.EAMID, 
            //                  exaudit.ReferenceNo,
            //                  exaudit.ReferenceKeyword,
            //                     exaudit.Requirements,
            //                     exaudit.Remarks,

            //                     map.MAPID AuditedWalletID,
            //                  exaudit.MercentOrUdoktaNumber AuditedWalletNo,
            //                  map.WalletName AuditedWalletName,
            //                  wallettype.SystemVariableCode WalletType,
            //                  exaudit.AuditDate,
            //                  exaudit.ApprovalStatusID,
            //                  sysvar.SystemVariableCode ApprovalStatus,
            //exaudit.IsDraft,
            //exaudit.CreatedBy,
            //exaudit.CreatedDate,
            //U.UserName,
            //                  VA.DepartmentID,
            //                  VA.DepartmentName,
            //                  VA.FullName AS EmployeeName, 
            //                  VA.DivisionID, 
            //                  VA.DivisionName,
            //VA.WorkMobile,
            //VA.WorkEmail,
            //                     VA.DesignationName,
            //                  VA.ImagePath,
            //                  VA.EmployeeCode,
            //                  VA.FullName+VA.EmployeeCode UserDetails,
            //                  ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
            //              from HRMS..ExternalAuditMaster exaudit
            //              LEFT JOIN Security..SystemVariable sysvar ON sysvar.SystemVariableID = exaudit.ApprovalStatusID
            //              LEFT JOIN Security..Users U ON U.UserID = exaudit.CreatedBy
            //              LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
            //              LEFT JOIN HRMS..UserWiseUddoktaOrMerchantMapping map on map.MAPID = exaudit.MercentOrUdoktaID
            //              LEFT JOIN Security..SystemVariable wallettype on wallettype.SystemVariableID = map.TypeID
            //              LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = exaudit.EAMID AND AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit}
            //              where exaudit.EAMID = {EAMID}";

            string sql = $@"select exaudit.EAMID, 
	                           exaudit.ReferenceNo,
	                           exaudit.ReferenceKeyword,
                               exaudit.Requirements,
							   exaudit.Remarks,
							   exaudit.IsDraft,
                               exaudit.CapturedImage,
                               map.MAPID AuditedWalletID,
	                           exaudit.MercentOrUdoktaNumber AuditedWalletNo,
	                           map.WalletName AuditedWalletName,
	                           wallettype.SystemVariableCode WalletType,
	                           exaudit.AuditDate,
                               exaudit.CreatedDate,
							   exaudit.CreatedBy,
	                           exaudit.ApprovalStatusID,
	                           sysvar.SystemVariableCode ApprovalStatus,
                               U.UserName,
                               VA.WorkMobile,
                               VA.WorkEmail,
	                           VA.DepartmentID
	                           ,VA.DepartmentName
	                           ,VA.FullName AS EmployeeName, 
	                           VA.DivisionID, 
	                           VA.DivisionName,
	                           VA.ImagePath
	                           ,VA.EmployeeCode,
	                           VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment,
	                           ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                                CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployeeParallel]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0),AEF.APEmployeeFeedbackID)) AS Bit) IsCurrentAPEmployee
                                ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                                ,ISNULL(APForwardInfoID,0) APForwardInfoID
            	                ,ISNULL(AEF.IsEditable,0) IsEditable
								,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
								,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
								,CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy
								,exaudit.ReturnDepartmentIDs
                                 ,ISNULL(AEF.IsSCM,0) IsSCM
								,Particulars
								,Particulars+' '+(CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END) ParticularsWtihProxy
                        from HRMS..ExternalAuditMaster exaudit
                        LEFT JOIN Security..SystemVariable sysvar ON sysvar.SystemVariableID = exaudit.ApprovalStatusID
                        LEFT JOIN Security..Users U ON U.UserID = exaudit.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..UserWiseUddoktaOrMerchantMapping map on map.MAPID = exaudit.MercentOrUdoktaID
                        LEFT JOIN Security..SystemVariable wallettype on wallettype.SystemVariableID = map.TypeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = exaudit.EAMID AND AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit}
                        LEFT JOIN (
                                 SELECT 
                                       APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy, Particulars
                                 FROM 
                                     Approval.dbo.functionJoinListAEFParallel({AppContexts.User.EmployeeID})
                                             )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                                             LEFT JOIN 
                                               (SELECT 
                                                 APForwardInfoID,ApprovalProcessID 
                                                FROM 
                                                 Approval..ApprovalForwardInfo  
                                                WHERE 
                                                 EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                                             APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                                             LEFT JOIN 
      		                        (
      		                        SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
      		                        )							
                        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                                             LEFT JOIN (
         			                        SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
         			                        (
         			                        --SELECT 
         			                        --	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
         			                        --FROM 
         			                        --	Approval..ApprovalEmployeeFeedback  AEF
         			                        --	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
         			                        --where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExternalAudit} 
         			                        --GROUP BY ReferenceID

         			                        --UNION ALL

         			                        SELECT 
         				                        COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
         			                        FROM 
         				                        Approval..ApprovalEmployeeFeedback AEF
         				                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
         			                        where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND EmployeeID = {AppContexts.User.EmployeeID}
         			                        GROUP BY ReferenceID

         			                        )V
         			                        GROUP BY ReferenceID
         			                        ) EA ON EA.ReferenceID = exaudit.EAMID

                                                         LEFT JOIN(
         			                        SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
         			                        FROM 
         				                        Approval..ApprovalEmployeeFeedbackRemarks AEFR 
         				                        INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
         			                        WHERE APFeedbackID = 11 --Returned
         			                        GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
      		                        ) Rej ON Rej.ReferenceID = exaudit.EAMID
                                                     LEFT JOIN ( 
                                                             SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDateParallel({AppContexts.User.EmployeeID})                                
      		                        )APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID AND APSubmitDate.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
      		                        LEFT JOIN (
         						                        SELECT 
         						                           MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
         						                        FROM 
         							                        Approval..ApprovalForwardInfo  
         						                        WHERE 
         							                        EmployeeID = {AppContexts.User.EmployeeID} 
         						                        GROUP BY ApprovalProcessID

         							                        ) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                                     --LEFT JOIN (
   	                          --     SELECT 
         			                        --    AEF.EmployeeCode PendingEmployeeCode,
         			                        --    EmployeeName PendingEmployeeName,
         			                        --    DepartmentName PendingDepartmentName,
         			                        --    AEF.ReferenceID PendingReferenceID
      		                         --   FROM 
         			                        --    Approval..viewApprovalEmployeeFeedback AEF 
      		                         --   WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
   	                          -- )PendingAt ON  PendingAt.PendingReferenceID = exaudit.EAMID
                        WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                         OR exaudit.AuditableEmployeeID={AppContexts.User.EmployeeID})
                        AND exaudit.EAMID = {EAMID}";
            var masterData = MasterRepo.GetDataDictCollection(sql);
            return masterData;
        }

        public async Task<List<AuditChildDto>> GetExternalAuditChild(int EAMID)
        {
            List<AuditChildDto> auditChildList = new List<AuditChildDto>();

            var childDataList = ChildRepo.Entities.Where(c => c.EAMID == EAMID).ToList();
            var masterData = MasterRepo.Entities.Where(m => m.EAMID == EAMID).ToList().FirstOrDefault();
            var userData = WalletMappingRepo.Entities.Where(u => u.WalletNumber.Equals(masterData.MercentOrUdoktaNumber)).FirstOrDefault();

            var deptList = DepartmentRepo.GetAllList();
            var questions = AuditQuestionRepo.GetAllList();
            var config = AuditApprovalConfigRepo.GetAllList();

            if (childDataList.Any())
            {
                foreach (var child in childDataList)
                {
                    AuditChildDto auditChildDto = new AuditChildDto();
                    List<Question> questionList = new List<Question>();

                    var dept = deptList.FirstOrDefault(d => d.DepartmentID == child.DepartmentID);
                    var ques = questions.FirstOrDefault(d => d.QuestionID == child.AuditQuestionID);

                    string attchmentListSql = $@"select FUID,
                                                FUID AID,
		                                        '' AttachedFile,
		                                        FileType Type,
		                                        OriginalName,
		                                        FilePath,
		                                        FileName,
		                                        ReferenceID,
		                                        SizeInKB Size,
                                                Description
                                            from Security..FileUpload
                                            where TableName = 'ExternalAuditChild' AND ReferenceID = {child.EACID}";

                    var attchments = ChildRepo.GetDataModelCollection<Attachments>(attchmentListSql);
                    questionList.Add(new Question() { label = ques.Title, value = ques.QuestionID });

                    if (!string.IsNullOrEmpty(child.POSMIDs))
                    {
                        string selectedPOSMListSql = $@"select posm.SystemVariableID value, posm.SystemVariableCode label
                                                from Security..SystemVariable posm
                                                where posm.SystemVariableID in ({child.POSMIDs})";

                        var selectedPOSMList = ChildRepo.GetDataModelCollection<ComboModel>(selectedPOSMListSql);

                        int posmEntity = userData.TypeID == (int)Util.ExternalAuditWalletType.UDDOKTA ? 56 : 57;
                        string posmListSql = $@"select posm.SystemVariableID value, posm.SystemVariableCode label
                                                from Security..SystemVariable posm
                                                where posm.EntityTypeID in ({posmEntity})";

                        var posmList = ChildRepo.GetDataModelCollection<ComboModel>(posmListSql);

                        auditChildDto.MultiPOSMList = selectedPOSMList;
                        auditChildDto.POSM = posmList;
                        auditChildDto.POSMString = (string.Join(",", selectedPOSMList.Select(x => x.label.ToString()).ToArray()));
                    }

                    else
                    {
                        int posmEntity = userData.TypeID == (int)Util.ExternalAuditWalletType.UDDOKTA ? 56 : 57;
                        string posmListSql = $@"select posm.SystemVariableID value, posm.SystemVariableCode label
                                                from Security..SystemVariable posm
                                                where posm.EntityTypeID in ({posmEntity})";

                        var posmList = ChildRepo.GetDataModelCollection<ComboModel>(posmListSql);
                        auditChildDto.POSM = posmList;
                    }
                    auditChildDto.EACID = child.EACID;
                    auditChildDto.EAMID = child.EAMID;
                    auditChildDto.DepartmentID = dept.DepartmentID;
                    auditChildDto.DepartmentName = dept.DepartmentName;
                    auditChildDto.Feedback = child.QuestionFeedback;
                    auditChildDto.Question = questionList;
                    auditChildDto.Attachments = attchments;
                    auditChildDto.IsRequried = config.Where(c => Convert.ToInt32(c.QuestionIDs) == child.AuditQuestionID).FirstOrDefault().IsRequired;
                    auditChildDto.IsPOSMRequired = config.Where(c => Convert.ToInt32(c.QuestionIDs) == child.AuditQuestionID).FirstOrDefault().IsPOSMRequired;
                    auditChildDto.Requirements = child.Requirements;

                    auditChildList.Add(auditChildDto);
                }
            }
            return auditChildList;
        }

        public async Task<dynamic> GetDepartmentDetails(int EAMID)
        {
            string sql = $@"select distinct child.DepartmentID, dept.DepartmentName 
                            from HRMS..ExternalAuditChild child
                            LEFT JOIN HRMS..Department dept on dept.DepartmentID = child.DepartmentID
                            where child.EAMID = {EAMID}";
            var deptData = MasterRepo.GetDataDictCollection(sql);
            return deptData;
        }

        public async Task<(bool, string)> AddNewWallet(string walletNo, string walletName, int walletTypeID)
        {
            var auditConfig = AuditConfig.Entities.FirstOrDefault(c => c.IsActive == true);

            var mappedUddoktaWallet = WalletMappingRepo.Entities.Where(u => u.CreatedBy == AppContexts.User.UserID && u.TypeID == 235 && u.IsActive == true && u.IsTagged == true);
            var mappedMerchantWallet = WalletMappingRepo.Entities.Where(u => u.CreatedBy == AppContexts.User.UserID && u.TypeID == 236 && u.IsActive == true && u.IsTagged == true);


            if ((mappedUddoktaWallet.Count() + mappedMerchantWallet.Count()) == (auditConfig.NumberOfUddokta + auditConfig.NumberOfMerchant))
            {
                return (false, "Wallet add limit exceed");
            }

            if (mappedMerchantWallet.Count() == auditConfig.NumberOfMerchant && walletTypeID == (int)Util.ExternalAuditWalletType.MERCHANT)
            {
                return (false, "Merchant tagging maximum limit exceed");
            }

            if (mappedUddoktaWallet.Count() == auditConfig.NumberOfUddokta && walletTypeID == (int)Util.ExternalAuditWalletType.UDDOKTA)
            {
                return (false, "Uddokta tagging maximum limit exceed");
            }

            //var existsWalletNo = WalletMappingRepo.Entities.Where(w=> w.WalletNumber.Equals(walletNo) && w.IsActive == true && w.IsTagged == false).ToList();

            //if (existsWalletNo.Count > 0)
            //{
            //    return (false, "Wallet is already exists. Use Tag/Untag option to tag this wallet.");
            //}

            var taggedWallet = WalletMappingRepo.FirstOrDefault(u => u.WalletNumber == walletNo && u.IsActive == true && u.IsTagged == true);

            if (taggedWallet != null)
            {
                return (false, "Wallet is already tagged. Unable to add.");
            }

            var perviousTaggedWallet = WalletMappingRepo.FirstOrDefault(u => u.WalletNumber == walletNo && u.CreatedBy == AppContexts.User.UserID && u.IsTagged == false);

            var walletModel = new UserWiseUddoktaOrMerchantMapping();

            if (perviousTaggedWallet != null)
            {
                walletModel.MAPID = perviousTaggedWallet.MAPID;
                walletModel.WalletName = walletName;
                walletModel.WalletNumber = walletNo;
                walletModel.EmployeeID = perviousTaggedWallet.EmployeeID;
                walletModel.TypeID = walletTypeID;
                walletModel.IsActive = true;
                walletModel.IsTagged = true;
                walletModel.CreatedBy = perviousTaggedWallet.CreatedBy;
                walletModel.CreatedIP = perviousTaggedWallet.CreatedIP;
                walletModel.RowVersion = perviousTaggedWallet.RowVersion;
                walletModel.SetModified();
            }

            else
            {
                walletModel.WalletName = walletName;
                walletModel.WalletNumber = walletNo;
                walletModel.EmployeeID = Convert.ToInt32(AppContexts.User.EmployeeID);
                walletModel.TypeID = walletTypeID;
                walletModel.IsActive = true;
                walletModel.IsTagged = true;

                walletModel.SetAdded();
            }
            SetAuditFields(walletModel);

            using (var unitOfWork = new UnitOfWork())
            {
                WalletMappingRepo.Add(walletModel);
                WalletMappingRepo.SaveChanges();
                unitOfWork.CommitChangesWithAudit();
            }

            return (true, "Wallet successfully added");
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.ExternalAudit);
            return comments.Result;
        }

        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedFirstMember(aprovalProcessId).Result;
        }

        public IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForExternalAudit(int EAMID, int ApprovalProcessID)
        {
            string sql = $@"Declare
                            @NFAApprovalSequenceType int=55,
                            @EmployeeID int={AppContexts.User.EmployeeID}

                            SELECT 
		                    NFAApprovalSequenceType,
		                    AEF.EmployeeID,
		                    VE.FullName EmployeeName,
		                    ProxyEmployeeID,
		                    SequenceNo,
		                    ReferenceID,
		                    AP.APTypeID,
		                    AT.Name ApprovalTypeName,
		                    AEF.APFeedbackID,
		                    AEF.Particulars,
		                    AF.Name FeedbackName,
		                    FeedbackSubmitDate,				
		                    CASE 
		                    	WHEN AEF.APFeedbackID = 6 
		                    	THEN 'https://nagaderp.mynagad.com:7070/security/'+'/upload\images/approval\rejected.jpeg' 
		                    	WHEN AEF.APFeedbackID = 5  THEN 'https://nagaderp.mynagad.com:7070/security/'+Signtr.ImagePath 
		                    	ELSE NULL END SignatureImagePath,
		                    DivisionName,
		                    DepartmentName,
		                    DesignationName,
		                    SV.SystemVariableCode ExternalAuditApprovalSequenceTypeName,
		                    NFAApprovalSequenceType
	                        FROM 
	                        Approval..ApprovalEmployeeFeedback AEF 
	                        INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                        INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID
	                        LEFT JOIN Security..PersonImage Signtr ON Signtr.PersonID = VE.PersonID AND IsSignature = 1
	                        INNER JOIN Approval..ApprovalFeedback AF ON AF.APFeedbackID = AEF.APFeedbackID
	                        INNER JOIN Approval..ApprovalType AT ON AT.APTypeID = AP.APTypeID
	                        LEFT JOIN Security..SystemVariable SV ON AEF.NFAApprovalSequenceType = SV.SystemVariableID
	                        WHERE AP.APTypeID = {(int)Util.ApprovalType.ExternalAudit} AND ReferenceID = {EAMID} 
	                        AND NFAApprovalSequenceType <> 64 
                            AND ((@NFAApprovalSequenceType=55 AND @EmployeeID=(select EmployeeID from Approval..ApprovalEmployeeFeedback where ApprovalProcessID = {ApprovalProcessID} AND NFAApprovalSequenceType = @NFAApprovalSequenceType) and 1=1) 
                                    OR (AEF.EmployeeID={AppContexts.User.EmployeeID} OR ProxyEmployeeID={AppContexts.User.EmployeeID}))
	                        
	                        ORDER BY SequenceNo";
            var feedback = MasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task<List<ExternalAuditQuestionDeptPOSMDto>> GetExternalAuditQuestionDeptPOSM()
        {
            var auditQuesList = AuditQuestionRepo.GetAllListAsync().Result;
            var deptList = DepartmentRepo.GetAllListAsync().Result;

            string sql = @$"select sysvar.SystemVariableID value, sysvar.SystemVariableCode label 
                                from Security..SystemVariable sysvar
                                where EntityTypeID = {(int)Util.ExternalAuditPOSM.UDDOKTA_POSM}";
            var UddoktaPOSM = MasterRepo.GetDataModelCollection<ComboModel>(sql);

            sql = @$"select sysvar.SystemVariableID value, sysvar.SystemVariableCode label 
                                from Security..SystemVariable sysvar
                                where EntityTypeID = {(int)Util.ExternalAuditPOSM.MERCHANT_POSM}";
            var MerchantPOSM = MasterRepo.GetDataModelCollection<ComboModel>(sql);

            List<ExternalAuditQuestionDeptPOSMDto> reqestionDeptList = new List<ExternalAuditQuestionDeptPOSMDto>();
            var auditConfig = AuditApprovalConfigRepo.GetAllListAsync(x => x.IsActive == true).Result;
            foreach (var config in auditConfig)
            {
                ExternalAuditQuestionDeptPOSMDto quesDept = new ExternalAuditQuestionDeptPOSMDto();
                quesDept.QuestionID = Convert.ToInt32(config.QuestionIDs);
                quesDept.Question = auditQuesList.Where(q => q.QuestionID == Convert.ToInt32(config.QuestionIDs)).FirstOrDefault().Title;
                quesDept.IsRequried = config.IsRequired;
                quesDept.IsPOSMRequired = config.IsPOSMRequired;
                quesDept.Uddokta_POSM = config.IsPOSMRequired ? UddoktaPOSM : new List<ComboModel>();
                quesDept.Merchant_POSM = config.IsPOSMRequired ? MerchantPOSM : new List<ComboModel>();

                var deptArr = config.DepartmentIDs.Split(',');
                List<ComboModel> deptComboList = new List<ComboModel>();
                foreach (var dept in deptArr)
                {
                    ComboModel deptCombo = new ComboModel();
                    var dep = deptList.Where(d => d.DepartmentID == Convert.ToInt32(dept)).FirstOrDefault();
                    deptCombo.label = dep.DepartmentName;
                    deptCombo.value = dep.DepartmentID;
                    deptComboList.Add(deptCombo);
                }

                quesDept.Departments = deptComboList;

                reqestionDeptList.Add(quesDept);
            }

            return reqestionDeptList;
        }

    }
}

