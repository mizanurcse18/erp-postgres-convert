using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace Approval.Manager.Implementations
{
    public class DocumentApprovalManager : ManagerBase, IDocumentApprovalManager
    {

        private readonly IRepository<DocumentApprovalMaster> DocumentApprovalRepo;
        public DocumentApprovalManager(IRepository<DocumentApprovalMaster> documentApprovalMasterRepo)
        {
            DocumentApprovalRepo = documentApprovalMasterRepo;
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
                        string filename = $"DocumentApproval-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "DocumentApproval\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(DocumentApprovalDto DocumentApproval)
        {
            if (DocumentApproval.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentApprovalMaster' AND ReferenceID={DocumentApproval.DAMID}";
                var prevAttachment = DocumentApprovalRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !DocumentApproval.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "DocumentApproval";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            else
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentApprovalMaster' AND ReferenceID={DocumentApproval.DAMID}";
                var prevAttachment = DocumentApprovalRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList;

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "DocumentApproval";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

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

        public async Task<(bool, string)> SaveChanges(DocumentApprovalDto DocumentApproval)
        {
            var existingDA = DocumentApprovalRepo.Entities.Where(x => x.DAMID == DocumentApproval.DAMID).SingleOrDefault();
            if (DocumentApproval.DAMID > 0 && (existingDA.IsNullOrDbNull() || existingDA.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Document Approval.");
            }

            foreach (var item in DocumentApproval.Attachments)
            {
                string ext = "";
                bool fileValid = false;
                string fileValidError = "";
                if (item.FUID > 0)
                {
                    ext = item.Type.Remove(0, 1);
                }
                else
                {
                    string result = item.AttachedFile.Split(',')[1];

                    var bytes = System.Convert.FromBase64String(result);
                    fileValid = UploadUtil.IsFileValidForDocument(bytes, item.AttachedFile);


                    string err = CheckValidFileExtensionsForAttachment(ext, item.OriginalName);
                    if (fileValid == false)
                    {
                        fileValidError = "Uploaded file extension is not allowed.";
                    }
                    if (!fileValidError.IsNullOrEmpty())
                    {
                        err = fileValidError;
                    }
                    if (!err.IsNullOrEmpty())
                    {
                        return (false, err);
                    }
                }
            }

            var removeList = RemoveAttachments(DocumentApproval);


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (DocumentApproval.DAMID > 0 && DocumentApproval.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM DocumentApprovalMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.DAMID AND AP.APTypeID = {(int)Util.ApprovalType.DocumentApproval}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.DocumentApproval} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.DocumentApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.DAMID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {DocumentApproval.ApprovalProcessID}";
                var canReassesstment = DocumentApprovalRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit DocumentApproval once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit DocumentApproval once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)DocumentApproval.DAMID, DocumentApproval.ApprovalProcessID, (int)Util.ApprovalType.DocumentApproval, AppContexts.GetDatabaseName(ConnectionName.Default));
            }

            string ApprovalProcessID = "0";
            var masterModel = new DocumentApprovalMaster
            {
                RequestDate = DocumentApproval.RequestDate,
                TemplateID = DocumentApproval.TemplateID,
                TemplateBody = DocumentApproval.TemplateBody,
                //ApprovalStatusID = (int)ApprovalStatus.Pending,
                ReferenceKeyword = DocumentApproval.ReferenceKeyword,
                IsDraft = DocumentApproval.IsDraft,
                ApprovalStatusID = DocumentApproval.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };


            using (var unitOfWork = new UnitOfWork())
            {
                if (DocumentApproval.DAMID.IsZero() && existingDA.IsNull())
                {
                    masterModel.ReferenceNo = GenerateDocumentApprovalReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetDocumentApprovalNewId(masterModel);
                    DocumentApproval.DAMID = (int)masterModel.DAMID;

                }
                else
                {
                    masterModel.CreatedBy = existingDA.CreatedBy;
                    masterModel.CreatedDate = existingDA.CreatedDate;
                    masterModel.CreatedIP = existingDA.CreatedIP;
                    masterModel.RowVersion = existingDA.RowVersion;
                    masterModel.DAMID = DocumentApproval.DAMID;
                    masterModel.ReferenceNo = existingDA.ReferenceNo;
                    masterModel.SetModified();

                    
                }

                SetAuditFields(masterModel);

                if (DocumentApproval.Attachments.IsNotNull() && DocumentApproval.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(DocumentApproval.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DocumentApproval.DAMID, "DocumentApprovalMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }
                //For Remove Attachment                    
                if (removeList.IsNotNull() && removeList.Count > 0)
                {
                    foreach (var attachemnt in removeList)
                    {
                        SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DocumentApproval.DAMID, "DocumentApprovalMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                    }
                }
                if (masterModel.IsAdded || (masterModel.IsDraft && masterModel.IsModified))
                {
                        if (DocumentApproval.DocumentApprovalApprovalPanelList.IsNotNull() && DocumentApproval.DocumentApprovalApprovalPanelList.Count > 0)
                        {
                            DeleteManualApprovalPanel((int)masterModel.DAMID, AppContexts.GetDatabaseName(ConnectionName.Default), (int)Util.ApprovalType.DocumentApproval);
                            foreach (var item in DocumentApproval.DocumentApprovalApprovalPanelList)
                            {
                                SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.DocumentApproval, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.DocumentApproval, (int)masterModel.DAMID, AppContexts.GetDatabaseName(ConnectionName.Default));

                            }
                        }
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
                        ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                    }
                }

                DocumentApprovalRepo.Add(masterModel);
                unitOfWork.CommitChangesWithAudit();

                //if (masterModel.IsAdded)
                //{

                if (!masterModel.IsDraft && DocumentApproval.ApprovalProcessID == 0)
                {
                    string approvalTitle = $"{Util.DocumentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, DocumentApproval Reference No:{masterModel.ReferenceNo}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.DAMID, Util.AutoDocumentApprovalAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.DocumentApproval, (int)Util.ApprovalPanel.DocumentApproval, context, AppContexts.GetDatabaseName(ConnectionName.Default));
                    ApprovalProcessID = obj.ApprovalProcessID;
                    
                }


                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        //await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.DAMID, (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail, (int)Util.ApprovalType.DocumentApproval);
                        await SendMailFromRequestCreated(ApprovalProcessID, false, masterModel.DAMID, (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail, (int)Util.ApprovalType.DocumentApproval);
                }
            }
            await Task.CompletedTask;

            return (true, $"DocumentApproval Submitted Successfully");
        }
        public async Task SendMailFromRequestCreated(string ApprovalProcessID, bool IsResubmitted, long MasterID, int mailGroup, int APTypeID)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxyAndSupervisor(ApprovalProcessID.ToInt()).Result;
            //List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)MasterID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }




        public async Task<(bool, string)> SaveChangesForHR(DocumentApprovalDto DocumentApproval)
        {
            var existingDA = DocumentApprovalRepo.Entities.Where(x => x.DAMID == DocumentApproval.DAMID).SingleOrDefault();
            if (DocumentApproval.DAMID > 0 && (existingDA.IsNullOrDbNull() || existingDA.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Document Approval.");
            }


            foreach (var item in DocumentApproval.Attachments)
            {
                string ext = "";
                bool fileValid = false;
                string fileValidError = "";
                if (item.FUID > 0)
                {
                    ext = item.Type.Remove(0, 1);
                }
                else
                {
                    string result = item.AttachedFile.Split(',')[1];

                    var bytes = System.Convert.FromBase64String(result);
                    fileValid = UploadUtil.IsFileValidForDocument(bytes, item.AttachedFile);


                    string err = CheckValidFileExtensionsForAttachment(ext, item.OriginalName);
                    if (fileValid == false)
                    {
                        fileValidError = "Uploaded file extension is not allowed.";
                    }
                    if (!fileValidError.IsNullOrEmpty())
                    {
                        err = fileValidError;
                    }
                    if (!err.IsNullOrEmpty())
                    {
                        return (false, err);
                    }
                }
            }

            var removeList = RemoveAttachments(DocumentApproval);


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (DocumentApproval.DAMID > 0 && DocumentApproval.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM DocumentApprovalMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.DAMID AND AP.APTypeID = {(int)Util.ApprovalType.HRSupportDocApproval}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.HRSupportDocApproval} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.HRSupportDocApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.DAMID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {DocumentApproval.ApprovalProcessID}";
                var canReassesstment = DocumentApprovalRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit DocumentApproval once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit DocumentApproval once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)DocumentApproval.DAMID, DocumentApproval.ApprovalProcessID, (int)Util.ApprovalType.HRSupportDocApproval, AppContexts.GetDatabaseName(ConnectionName.Default));
            }

            string ApprovalProcessID = "0";
            var masterModel = new DocumentApprovalMaster
            {
                RequestDate = DocumentApproval.RequestDate,
                TemplateID = DocumentApproval.TemplateID,
                TemplateBody = DocumentApproval.TemplateBody,
                //ApprovalStatusID = (int)ApprovalStatus.Pending,
                ReferenceKeyword = DocumentApproval.ReferenceKeyword,
                IsDraft = DocumentApproval.IsDraft,
                ApprovalStatusID = DocumentApproval.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };


            using (var unitOfWork = new UnitOfWork())
            {
                if (DocumentApproval.DAMID.IsZero() && existingDA.IsNull())
                {
                    masterModel.ReferenceNo = GenerateDocumentApprovalReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetDocumentApprovalNewId(masterModel);
                    DocumentApproval.DAMID = (int)masterModel.DAMID;

                }
                else
                {
                    masterModel.CreatedBy = existingDA.CreatedBy;
                    masterModel.CreatedDate = existingDA.CreatedDate;
                    masterModel.CreatedIP = existingDA.CreatedIP;
                    masterModel.RowVersion = existingDA.RowVersion;
                    masterModel.DAMID = DocumentApproval.DAMID;
                    masterModel.ReferenceNo = existingDA.ReferenceNo;
                    masterModel.SetModified();


                }

                SetAuditFields(masterModel);

                if (DocumentApproval.Attachments.IsNotNull() && DocumentApproval.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(DocumentApproval.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DocumentApproval.DAMID, "DocumentApprovalMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }
                //For Remove Attachment                    
                if (removeList.IsNotNull() && removeList.Count > 0)
                {
                    foreach (var attachemnt in removeList)
                    {
                        SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DocumentApproval.DAMID, "DocumentApprovalMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                    }
                }

                DocumentApprovalRepo.Add(masterModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingDA.IsDraft && masterModel.IsModified))
                    {

                        string approvalTitle = $"{Util.DocumentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, DocumentApproval Reference No:{masterModel.ReferenceNo}";
                        var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

                        ApprovalProcessID = CreateApprovalProcessForDocumentApprovalHR((int)masterModel.DAMID, Util.AutoDocumentApprovalAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.HRSupportDocApproval, (int)Util.ApprovalPanel.HRSupportDocApproval, context, AppContexts.GetDatabaseName(ConnectionName.Default));

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
                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }
                

                unitOfWork.CommitChangesWithAudit();

                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        //await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.DAMID, (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail, (int)Util.ApprovalType.DocumentApproval);
                        await SendMailFromRequestCreated(ApprovalProcessID, false, masterModel.DAMID, (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail, (int)Util.ApprovalType.DocumentApproval);
                }
            }
            await Task.CompletedTask;

            return (true, $"DocumentApproval Submitted Successfully");
        }






        private void SetDocumentApprovalNewId(DocumentApprovalMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("DocumentApprovalMaster", AppContexts.User.CompanyID);
            master.DAMID = code.MaxNumber;
        }



        private string GenerateDocumentApprovalReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/DocumentApproval/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("DocumentApprovalMasterRefNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }
        
        public GridModel GetDocumentApprovalList(GridParameter parameters)
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
                    filter = $@" AND DocumentApproval.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND DocumentApproval.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string hrFilter = "";
            int apTypeID = (int)Util.ApprovalType.DocumentApproval;
            if (parameters.AdditionalFilterData == "hr")
            {
                hrFilter = $@"DAT.CategoryType = {(int)Util.DocApprovalCategory.HR} AND";
                
                apTypeID = (int)Util.ApprovalType.HRSupportDocApproval;
            }
            else
            {
                hrFilter = $@"DAT.CategoryType <> {(int)Util.DocApprovalCategory.HR} AND";
            }
            string sql = $@"SELECT DISTINCT
	                            DocumentApproval.*,
                                DAT.DATName TemplateName
	                            ,VA.DepartmentID
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
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,(SELECT AEF.EmployeeCode,EmployeeName,DepartmentName FROM Approval..viewApprovalEmployeeFeedback AEF WHERE AEF.APTypeID = {apTypeID} AND AEF.ReferenceID = DocumentApproval.DAMID AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
                                FOR JSON PATH) PendingAt
                            FROM DocumentApprovalMaster DocumentApproval
                            LEFT JOIN Security..Users U ON U.UserID = DocumentApproval.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = DocumentApproval.ApprovalStatusID
                            LEFT JOIN DocumentApprovalTemplate DAT ON DocumentApproval.TemplateID = DAT.DATID
                            
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DocumentApproval.DAMID AND AP.APTypeID = {apTypeID}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable,IsSCM
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable ,0 IsSCM 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {apTypeID} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {apTypeID} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DocumentApproval.DAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DocumentApproval.DAMID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														Approval..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
														) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                    LEFT JOIN 
								        (
								            SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								        )							
						                F ON F.ApprovalProcessID = Ap.ApprovalProcessID
							WHERE {hrFilter} (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                                    ";
            var result = DocumentApprovalRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }



        public async Task<DocumentApprovalMasterDto> GetDocumentApprovalMaster(int DAMID)
        {
            int apTypeID = (int)Util.ApprovalType.DocumentApproval;
            DocumentApprovalMaster dam = DocumentApprovalRepo.Entities.Where(x => x.DAMID == DAMID).FirstOrDefault();
            if(dam.IsNotNull() && (dam.TemplateID > 0))
            {
                apTypeID = (int)Util.ApprovalType.HRSupportDocApproval;
            }


            string sql = $@"SELECT DocumentApproval.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,VA.WorkMobile
                            ,DAT.DATName TemplateName
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from DocumentApprovalMaster DocumentApproval
                        LEFT JOIN Security..Users U ON U.UserID = DocumentApproval.CreatedBy						
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = DocumentApproval.ApprovalStatusID
                        LEFT JOIN DocumentApprovalTemplate DAT ON DocumentApproval.TemplateID = DAT.DATID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DocumentApproval.DAMID AND AP.APTypeID = {apTypeID}
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {apTypeID}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {apTypeID} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DocumentApproval.DAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DocumentApproval.DAMID
                                    LEFT JOIN 
								        (
								            SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								        )							
						                F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE DocumentApproval.DAMID={DAMID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = DocumentApprovalRepo.GetModelData<DocumentApprovalMasterDto>(sql);
            return master;
        }



        public async Task<List<ManualApprovalPanelEmployeeDto>> GetDocumentApprovalApprovalPanelDefault(int DAMID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.EmployeeCode+'-'+Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={DAMID}  AND APE.APTypeID=13";

            var maps = DocumentApprovalRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return maps;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.DocumentApproval);
            return comments.Result;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalCommentHR(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.HRSupportDocApproval);
            return comments.Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID)
        {
            return GetApprovalForwardingMembers(ReferenceID, APTypeID, APPanelID).Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberListApprovalService(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID, AppContexts.GetDatabaseName(ConnectionName.Default)).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberListApprovalService(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId, AppContexts.GetDatabaseName(ConnectionName.Default)).Result;
        }

        public IEnumerable<Dictionary<string, object>> ReportForDocumentApprovalFeedback(int DAMID)
        {
            string sql = $@" EXEC Approval..spRPTDocumentApprovalApprovalFeedback {DAMID}";
            var feedback = DocumentApprovalRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> ReportForDocumentApprovalFeedbackHR(int DAMID)
        {
            string sql = $@" EXEC Approval..spRPTDocumentApprovalForHRApprovalFeedback {DAMID}";
            var feedback = DocumentApprovalRepo.GetDataDictCollection(sql);
            return feedback;
        }
        
        public List<Attachments> GetAttachments(int DAMID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentApprovalMaster' AND ReferenceID={DAMID}";
            var attachment = DocumentApprovalRepo.GetDataDictCollection(attachmentSql);
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
        
        public List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByTemplateID(int id)
        {
            string sql = $@"SELECT P.MAPPanelEmployeeID
	                                ,P.EmployeeID
	                                ,P.APPanelID
	                                ,P.SequenceNo
	                                ,P.ProxyEmployeeID
	                                ,P.IsProxyEmployeeEnabled
	                                ,P.NFAApprovalSequenceType
	                                ,P.IsEditable
	                                ,P.IsSCM
	                                ,P.IsMultiProxy
	                                ,P.APTypeID
	                                ,P.ReferenceID
	                                ,P.CompanyID
	                                ,P.CreatedBy
	                                ,P.CreatedDate
	                                ,P.CreatedIP
	                                ,P.UpdatedBy
	                                ,P.UpdatedDate
	                                ,P.UpdatedIP
	                                ,P.ROWVERSION
	                                ,E.EmployeeCode+'-'+E.FullName AS EmployeeName
	                                ,E.EmployeeCode
	                                ,SV.SystemVariableCode AS NFAApprovalSequenceTypeName
                                    ,VE1.EmployeeCode+'-'+VE1.FullName ProxyApprovalPanelEmployeeName
                                FROM ManualApprovalPanelEmployee AS P
                                INNER JOIN DocumentApprovalMaster AS D ON P.ReferenceID = D.DAMID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM ManualApprovalPanelEmployee a
		                                INNER JOIN DocumentApprovalMaster b ON a.ReferenceID = b.DAMID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.DocumentApproval}
			                                AND APTypeID = {(int)Util.ApprovalType.DocumentApproval}
			                                AND b.TemplateID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.DocumentApproval}
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = DocumentApprovalRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }
        public List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int DAMID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM DocumentApprovalMaster DocumentApproval 
                        LEFT JOIN PurchaseRequisitionMaster PR ON DocumentApproval.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE DocumentApproval.DAMID={DAMID}";

            var pr = DocumentApprovalRepo.GetModelData<DocumentApprovalMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.DocumentApproval, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            if (pr.EmployeeID != AppContexts.User.EmployeeID)
            {
                ManualApprovalPanelEmployeeDto seq2 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.DocumentApproval, SequenceNo = 2, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Prepared", NFAApprovalSequenceType = 55, IsSystemGenerated = true };
                apList.Add(seq2);
            }
            return apList;
        }
        public void DeleteDocumentApproval(int id)
        {
            using var unitOfWork = new UnitOfWork();
            var doc = DocumentApprovalRepo.Entities.Where(x => x.DAMID == id && x.IsDraft == true && x.CreatedBy == AppContexts.User.UserID).FirstOrDefault();
            if (doc.IsNullOrDbNull())
            {
                return;
            }
            //var doc = DocumentApprovalRepo.Entities.SingleOrDefault(x => x.DAMID == id);
            doc.SetDeleted();
            DocumentApprovalRepo.Add(doc);

            unitOfWork.CommitChangesWithAudit();
        }

    }



}
