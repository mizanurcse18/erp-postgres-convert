using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class PettyCashPaymentManager : ManagerBase, IPettyCashPaymentManager
    {
        private readonly IRepository<PettyCashPaymentMaster> MasterRepo;
        private readonly IRepository<PettyCashPaymentChild> ChildRepo;
        //readonly IModelAdapter Adapter;
        public PettyCashPaymentManager(IRepository<PettyCashPaymentMaster> pettyCashPaymentMasterRepo, IRepository<PettyCashPaymentChild> pettyCashPaymentChildRepo)
        {
            MasterRepo = pettyCashPaymentMasterRepo;
            ChildRepo = pettyCashPaymentChildRepo;
            //PettyCashDisbursementChildRepo = pettyCashAdvanceChildRepo;
        }
        public GridModel GetAllApprovedReimburseClaimList(GridParameter parameters)
        {
            string sql = $@"SELECT 
                                DISTINCT	                            
                                PCM.CreatedDate,
                                PCM.PCRMID,
								PCM.ReferenceNo,
								PCM.ReimburseDate,
								PCM.ApprovalStatusID,
								PCM.GrandTotal,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,ISNULL(PCPM.PCPMID,0) PCPMID
                            FROM PettyCashReimburseMaster PCM
                            LEFT JOIN PettyCashPaymentChild PCPC ON PCPC.PCRMID=PCM.PCRMID
							LEFT JOIN PettyCashPaymentMaster PCPM ON PCPM.PCPMID=PCPC.PCPMID AND PCPM.ApprovalStatusID<>24
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PCM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCM.PCRMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim}
                            LEFT JOIN (
                                        SELECT 
                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                        FROM 
                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
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
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PCM.PCRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PCM.PCRMID
                                    LEFT JOIN (
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID  AND APSubmitDate.EmployeeID = {AppContexts.User.EmployeeID}
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
											   ) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PCM.PCRMID 
							WHERE PCM.ApprovalStatusID=23";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }


        public async Task<(bool, string)> SaveChanges(PettyCashPaymentClaimDto payment)
        {
            //var existingreimburse = PettyCashReimburseMasterRepo.Entities.Where(x => x.PCRMID == payment.PCRMID).SingleOrDefault();
            var existingreimburse = MasterRepo.Get(payment.PCPMID);
            var validate = CheckApprovalValidation(payment);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit payment claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsAutoApproved = false;
            bool IsResubmitted = false;


            var removeList = RemoveAttachments(payment);
            using (var unitOfWork = new UnitOfWork())
            {
                var masterModel = new PettyCashPaymentMaster
                {
                    ReferenceKeyword = payment.ReferenceKeyword,
                    GrandTotal = (decimal)payment.GrandTotal,
                    IsDraft = payment.IsDraft,
                    RequestDate = payment.RequestDate,
                    Remarks = payment.Remarks,
                    ReferenceNo = GeneratePettyCashPaymentClaimReference() + (string.IsNullOrWhiteSpace(payment.ReferenceKeyword) ? "" : $"/{payment.ReferenceKeyword.ToUpper()}"),
                    ApprovalStatusID = payment.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                };
                if (payment.PCRMID.IsZero() && existingreimburse.IsNull())
                {
                    masterModel.SetAdded();
                    SetPettyCashPaymentClaimMasterNewId(masterModel);
                    payment.PCPMID = masterModel.PCPMID;
                }
                else
                {
                    masterModel.CreatedBy = existingreimburse.CreatedBy;
                    masterModel.CreatedDate = existingreimburse.CreatedDate;
                    masterModel.CreatedIP = existingreimburse.CreatedIP;
                    masterModel.RowVersion = existingreimburse.RowVersion;
                    masterModel.PCPMID = payment.PCPMID;
                    masterModel.ReferenceNo = existingreimburse.ReferenceNo;
                    masterModel.RequestDate = existingreimburse.RequestDate;
                    masterModel.SetModified();
                }
                var childModel = GeneratePettyCashPaymentChild(payment);
                var months = $@"{string.Join(",", childModel.Select(x => x.CreatedDate.Month).Distinct())}";
                SetAuditFields(masterModel);
                if (payment.Attachments.IsNotNull() && payment.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(payment.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)payment.PCPMID, "PettyCashPaymentMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)payment.PCPMID, "PettyCashPaymentMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                SetAuditFields(childModel);

                MasterRepo.Add(masterModel);
                ChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingreimburse.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.PettyCashPaymentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Payment Reference No:{masterModel.ReferenceNo}";
                        //ApprovalProcessID = CreateApprovalProcessByAPTypeIDAndAPPanelID((int)masterModel.PCRMID, Util.AutoPettyCashReimburseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.ApprovalPanel.PettyCashReimburse);
                        var obj = CreateApprovalProcessForLimit((int)masterModel.PCPMID, Util.AutoPettyCashPaymentAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashPaymentClaim, masterModel.GrandTotal, months);
                        ApprovalProcessID = obj.ApprovalProcessID;
                        IsAutoApproved = obj.IsAutoApproved;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);
                            IsResubmitted = true;
                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (IsAutoApproved && !masterModel.IsDraft)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PCPMID, (int)Util.ApprovalType.PettyCashPaymentClaim);
                }

                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                        await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PCPMID, (int)Util.ApprovalType.PettyCashPaymentClaim, (int)Util.MailGroupSetup.PettyCashPaymentInitiatedMail);

                }

            }

            await Task.CompletedTask;

            return (true, $"Petty Cash Payment Submitted Successfully");
        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long IOUTMasterID, int APTypeID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)IOUTMasterID, 0);
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(PettyCashPaymentClaimDto payment)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (payment.PCPMID > 0 && payment.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM PettyCashPaymentMaster ECM
                                 LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCPMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim}
                                 LEFT JOIN (SELECT 
                                     APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
                                    FROM 
                                     Approval..ApprovalEmployeeFeedback AEF
                                    WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                 )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID

                                 LEFT JOIN 
            		(
            		SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
            		UNION ALL 
            		SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
            		)							
            F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                                 LEFT JOIN (
            			SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
            			(
            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback  AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = ECM.PCPMID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {payment.ApprovalProcessID}";
                var canReassesstment = MasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, null);
                    }
                }
                else
                {
                    return (false, null);
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)payment.PCRMID, payment.ApprovalProcessID, (int)Util.ApprovalType.PettyCashPaymentClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        private List<Attachments> RemoveAttachments(PettyCashPaymentClaimDto ead)
        {
            if (ead.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashPaymentMaster' AND ReferenceID={ead.PCPMID}";
                var prevAttachment = MasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !ead.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PCR";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }

        private string GeneratePettyCashPaymentClaimReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/PAY/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("PettyCashPaymentClaimReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        private void SetPettyCashPaymentClaimMasterNewId(PettyCashPaymentMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PettyCashPaymentMaster", AppContexts.User.CompanyID);
            master.PCPMID = code.MaxNumber;
        }
        private void SetPettyCashPaymentClaimChildNewId(PettyCashPaymentChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PettyCashPaymentChild", AppContexts.User.CompanyID);
            child.PCPCID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private List<Attachments> AddAttachments(List<Attachments> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"PCP-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PCP\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        attachemntList.Add(new Attachments
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

        private List<PettyCashPaymentChild> GeneratePettyCashPaymentChild(PettyCashPaymentClaimDto reimburse)
        {
            var existingPurchaseRequisitionChild = ChildRepo.GetAllList(x => x.PCPMID == reimburse.PCPMID).ToList();
            var childModel = new List<PettyCashPaymentChild>();
            if (reimburse.Details.IsNotNull())
            {
                reimburse.Details.ForEach(x =>
                {
                    childModel.Add(new PettyCashPaymentChild
                    {
                        PCPCID = x.PCPCID,
                        PCPMID = reimburse.PCPMID,
                        PCRMID = x.PCRMID
                    });

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseRequisitionChild.Count > 0 && x.PCPCID > 0)
                    {
                        var existingModelData = existingPurchaseRequisitionChild.FirstOrDefault(y => y.PCPCID == x.PCPCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PCPMID = reimburse.PCPMID;
                        x.SetAdded();
                        SetPettyCashPaymentClaimChildNewId(x);
                    }
                });
                var willDeleted = existingPurchaseRequisitionChild.Where(x => !childModel.Select(y => y.PCPCID).Contains(x.PCPCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }
        public GridModel GetPettyCashPaymenteAllList(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
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
                    filter = $@" AND PCM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PCM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            string sql = $@"SELECT 
                                DISTINCT	                            
                                PCM.CreatedDate,
                                PCM.PCPMID,
								PCM.ReferenceNo,
								PCM.RequestDate,
								PCM.ApprovalStatusID,
								PCM.GrandTotal,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM PettyCashPaymentMaster PCM
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PCM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCM.PCPMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim}
                            LEFT JOIN (
                                        SELECT 
                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                        FROM 
                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
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
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PCM.PCPMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PCM.PCPMID
                                    LEFT JOIN (
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID  AND APSubmitDate.EmployeeID = {AppContexts.User.EmployeeID}
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
											   ) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PCM.PCPMID 
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public List<Attachments> GetAttachments(int id, string TableName)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='{TableName}' AND ReferenceID={id}";
            var attachment = MasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"]),
                    Description = data["Description"].ToString()
                });
            }
            return attachemntList;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PettyCashPaymentClaim);
            return comments.Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }


        public async Task<PettyCashPaymentMasterDto> GetPettyCashPaymentMaster(int PCPMID)
        {
            string sql = $@"SELECT ECM.*, 
                            VA.DepartmentID, 
                            VA.DepartmentName, 
                            VA.FullName AS EmployeeName, 
                            SV.SystemVariableCode AS ApprovalStatus, 
                            VA.ImagePath, 
                            VA.EmployeeCode, 
                            VA.DivisionID
                            ,VA.DivisionName 
                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ECM.GrandTotal AS PayableAmount
                            ,(SELECT Security.dbo.NumericToBDT(ECM.GrandTotal)) AmountInWords
                        FROM PettyCashPaymentMaster ECM
                        LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCPMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashPaymentClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.PCPMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PCPMID
                        WHERE ECM.PCPMID={PCPMID}";
            var reimburseClaimMaster = MasterRepo.GetModelData<PettyCashPaymentMasterDto>(sql);
            return reimburseClaimMaster;
        }

        public async Task<List<PettyCashPaymentChildDto>> GetPettyCashPaymentChild(int PCPMID)
        {
            string sql = $@"WITH RankedResults AS (
                            SELECT
                                A.*,
                                VA.DepartmentID,
                                VA.DepartmentName,
                                VA.FullName AS EmployeeName,
                                VA.ImagePath,
                                VA.EmployeeCode,
                                VA.DivisionID,
                                VA.DivisionName,
                                PC.PCPCID,
                                PC.PCPMID,
                                ROW_NUMBER() OVER (PARTITION BY PCPCID ORDER BY PCPCID) AS RowNum,
                                AP.ApprovalProcessID ClaimApprovalProcessID,
                                PCPM.ReferenceNo PaymentReferenceNo,PCPM.ApprovalStatusID PApprovalStatusID
                            FROM (
                                SELECT DISTINCT
                                    ECC.PCRCID,
                                    ECC.PCRMID,
                                    ECC.PCCID,
                                    PCRM.ReferenceNo ReimburseReferenceNo,
                                    ECC.ClaimTypeID,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN PCE.GrandTotal ELSE PCA.ResubmitTotalAmount END AS TotalAmount,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN PCE.ReferenceNo ELSE PCA.ReferenceNo END AS ClaimReferenceNo,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN PCE.EmployeeID ELSE PCA.EmployeeID END AS EmployeeID,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN 'Expense' ELSE 'Advance' END AS ClaimType,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN PCE.SubmitDate ELSE PCA.RequestDate END AS ClaimDate,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN PCE.PCEMID ELSE PCA.PCAMID END AS ClaimID,
                                    CASE WHEN ECC.ClaimTypeID = 1 THEN 28 ELSE 30 END AS ClaimAPTypeID
                                FROM PettyCashReimburseChild ECC
									LEFT JOIN PettyCashReimburseMaster PCRM ON ECC.PCRMID=PCRM.PCRMID
                                    LEFT JOIN PettyCashAdvanceMaster PCA ON PCA.PCAMID = ECC.PCCID AND ECC.ClaimTypeID = 2
                                    LEFT JOIN PettyCashExpenseMaster PCE ON PCE.PCEMID = ECC.PCCID AND ECC.ClaimTypeID = 1
                            ) A
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = A.EmployeeID
                            JOIN PettyCashPaymentChild PC ON PC.PCRMID = A.PCRMID
                            LEFT JOIN PettyCashPaymentMaster PCPM ON PC.PCPMID=PCPM.PCPMID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = A.ClaimID AND AP.APTypeID = ClaimAPTypeID
                            WHERE PC.PCPMID = {PCPMID}
                            )
                            SELECT * FROM RankedResults;";
                            //SELECT* FROM RankedResults WHERE RowNum = 1; ";

            var reimburseChilds = ChildRepo.GetDataModelCollection<PettyCashPaymentChildDto>(sql);
            return reimburseChilds;
        }


        public IEnumerable<Dictionary<string, object>> ReportForApprovalFeedback(int PCAMID, int ApTypeID)
        {
            //string sql = $@" EXEC Security..spRPTPettyCashAdvanceApprovalFeedback {PCAMID}";
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {PCAMID}, {ApTypeID}";
            var feedback = MasterRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public async Task<List<PettyCashFilteredData>> GetPettyCashApprovedReimburseClaimData(int InvoiceMasterID, int IPaymentMasterID, int PCPMID)
        {
            string sql = @$"SELECT *
                            FROM Accounts..viewGetAllPettyCashReimburseClaimForPayment WHERE (PCPMID is null OR PCPMID = {PCPMID}) ORDER BY PCRMID ASC";

            var data = MasterRepo.GetDataModelCollection<PettyCashFilteredData>(sql)
                            .Where(x => x.CWEmployeeID == AppContexts.User.EmployeeID).ToList();
            //.Where(x => PCPMID == 0 || (x != null && x.PCPMID == PCPMID)).ToList();
            return await Task.FromResult(data);


        }

        public async Task<List<Dictionary<string, object>>> GetAllExport(string WhereCondition, string FromDate, string ToDate)
        {
            string dateFilterConditionExp = string.Empty;
            string dateFilterConditionAdv = string.Empty;

            if (!string.IsNullOrEmpty(WhereCondition))
            {
                dateFilterConditionExp = $@" AND PCEM.CWID = '{WhereCondition.Split('-')[0]}'";
                dateFilterConditionAdv = $@" AND PCAM.CWID = '{WhereCondition.Split('-')[0]}'";

            }
            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                dateFilterConditionExp = $@" AND CAST(PCEM.CreatedDate as date) between '{FromDate}' and '{ToDate}'";
                dateFilterConditionAdv = $@" AND CAST(PCAM.CreatedDate as date) between '{FromDate}' and '{ToDate}'";

            }
            string sql = @$" WITH Payment_CTE AS(
	                            SELECT CONVERT( varchar,ROW_NUMBER() OVER(ORDER BY P.MASTERID)) SL ,
	                                                   P.[Employee ID],
	                                                   P.[Name of the employee],
	                                                   P.Dept,
	                                                   P.[Type of Expenses],
	                                                   P.[Submitted Amount],
	                                                   P.Deduction,
	                                                   P.[Approved Amount],
	                                                   P.[Refund Amount],
	                                                   P.[Adjustment Amount],
	                                                   P.[Payable Amount],
	                                                   P.[Cash out charge @ 1.25%],
	                                                   P.[Total with CashOut Charge],
	                                                   P.[Wallet No],
	                                                   CONVERT(varchar, '') [Receiving Date],
	                                                   CONVERT(varchar, '') [Paid on],
	                                                   P.[ERP Doc No]
	                                                FROM 
	                                                (SELECT PCEM.PCEMID MasterID,
		                                                VA.EmployeeCode 'Employee ID',
		                                                VA.FullName 'Name of the employee',
		                                                VA.DepartmentName 'Dept',
							                            'Expense' 'Type of Expenses',
		                                                PCEM.GrandTotal 'Submitted Amount',
		                                                0 Deduction,
		                                                PCEM.GrandTotal 'Approved Amount',
		                                                0 'Refund Amount',
		                                                0 'Adjustment Amount',
		                                                0 'Payable Amount',
		                                                0 'Cash out charge @ 1.25%',
		                                                0 'Total with CashOut Charge',
		                                                VA.WalletNumber 'Wallet No',
		                                                CAST(PCEM.CreatedDate as date) 'Receiving Date',
		                                                CAST(PCEM.DisbursementDate as date) 'Paid on',
		                                                PCEM.ReferenceNo 'ERP Doc No'
		
		                                                FROM {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..PettyCashExpenseMaster PCEM
		                                                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.EmployeeID = PCEM.EmployeeID
		                                                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..CustodianWallet WA ON WA.CWID = PCEM.CWID
		                                                WHERE PCEM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}
		                                                AND PCEM.IsDisbursement = 1 {dateFilterConditionExp}
                                                UNION ALL
                                                SELECT PCAM.PCAMID MasterID,
		                                                VA.EmployeeCode 'Employee ID',
		                                                VA.FullName 'Name of the employee',
		                                                VA.DepartmentName 'Dept',
		                                                'Advance' 'Type of Expenses',
		                                                PCAM.GrandTotal 'Submitted Amount',
		                                                0 Deduction,
		                                                PCAM.GrandTotal 'Approved Amount',
		                                                CASE WHEN (ISNULL(ReSubmitTotalAmount,0) > 0 AND PCAM.GrandTotal> ISNULL(ReSubmitTotalAmount,0)) THEN ABS(PCAM.GrandTotal- ISNULL(ReSubmitTotalAmount,0)) ELSE 0 END 'Refund Amount',
		                                                ISNULL(ReSubmitTotalAmount,0) 'Adjustment Amount',
		                                                CASE WHEN (ISNULL(ReSubmitTotalAmount,0) > 0 AND PCAM.GrandTotal < ISNULL(ReSubmitTotalAmount,0)) THEN ABS(PCAM.GrandTotal- ISNULL(ReSubmitTotalAmount,0)) 
			                                                 WHEN ISNULL(ReSubmitTotalAmount,0) = 0 THEN PCAM.GrandTotal ELSE 0 END 'Payable Amount',

		                                                CONVERT(DECIMAL(10,2),(ISNULL(CASE WHEN (ISNULL(ReSubmitTotalAmount,0) > 0 AND PCAM.GrandTotal < ISNULL(ReSubmitTotalAmount,0)) THEN ABS(PCAM.GrandTotal- ISNULL(ReSubmitTotalAmount,0)) 
			                                                 WHEN ISNULL(ReSubmitTotalAmount,0) = 0 THEN PCAM.GrandTotal ELSE 0 END,0) * 0.0125)) 'Cash out charge @ 1.25%',

		                                                CASE WHEN (ISNULL(ReSubmitTotalAmount,0) > 0 AND PCAM.GrandTotal < ISNULL(ReSubmitTotalAmount,0)) THEN ABS(PCAM.GrandTotal- ISNULL(ReSubmitTotalAmount,0)) 
			                                                 WHEN ISNULL(ReSubmitTotalAmount,0) = 0 THEN PCAM.GrandTotal ELSE 0 END +
			                                                 CONVERT(DECIMAL(10,2),(ISNULL(CASE WHEN (ISNULL(ReSubmitTotalAmount,0) > 0 AND PCAM.GrandTotal < ISNULL(ReSubmitTotalAmount,0)) THEN ABS(PCAM.GrandTotal- ISNULL(ReSubmitTotalAmount,0)) 
			                                                 WHEN ISNULL(ReSubmitTotalAmount,0) = 0 THEN PCAM.GrandTotal ELSE 0 END,0) * 0.0125))
			                                                 'Total with CashOut Charge',
		                                                VA.WalletNumber 'Wallet No',
		                                                CAST(PCAM.CreatedDate as date) 'Receiving Date',
		                                                CAST(PCAM.DisbursementDate as date) 'Paid on',
		                                                PCAM.ReferenceNo 'ERP Doc No'
		
		                                                FROM {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..PettyCashAdvanceMaster PCAM
		                                                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.EmployeeID = PCAM.EmployeeID
		                                                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..CustodianWallet WA ON WA.CWID = PCAM.CWID
		                                                WHERE PCAM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}
		                                                AND PCAM.IsDisbursement = 1
							                            AND PCAM.ResubmitApprovalStatusID = {(int)Util.ApprovalStatus.Approved} {dateFilterConditionAdv}
		                                                )P) 
                                                    SELECT * FROM Payment_CTE
                                                    UNION ALL
	                                                    SELECT '' SL ,
		                                                    ''[Employee ID],
		                                                    ''[Name of the employee],
		                                                    ''Dept,
		                                                    ''[Type of Expenses],
		                                                    SUM(P.[Submitted Amount]),
		                                                    SUM(P.Deduction),
		                                                    SUM(P.[Approved Amount]),
		                                                    SUM(P.[Refund Amount]),
		                                                    SUM(P.[Adjustment Amount]),
		                                                    SUM(P.[Payable Amount]),
		                                                    SUM(P.[Cash out charge @ 1.25%]),
		                                                    SUM(P.[Total with CashOut Charge]),
		                                                    '' [Wallet No],
		                                                    '' [Receiving Date],
		                                                    '' [Paid on],
		                                                    '' [ERP Doc No]
	                                                      FROM  Payment_CTE P";

            var result = MasterRepo.GetDataDictCollection(sql);
            return await Task.FromResult(result.ToList());
        }


    }
}
