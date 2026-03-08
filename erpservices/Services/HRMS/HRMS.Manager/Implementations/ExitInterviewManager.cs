using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class ExitInterviewManager : ManagerBase, IExitInterviewManager
    {

        private readonly IRepository<EmployeeExitInterview> ExitInterviewRepo;
        public ExitInterviewManager(IRepository<EmployeeExitInterview> EmployeeExitInterviewRepo)
        {
            ExitInterviewRepo = EmployeeExitInterviewRepo;
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
                        string filename = $"ExitInterview-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "ExitInterview\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(ExitInterviewDto ExitInterview)
        {
            if (ExitInterview.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeExitInterview' AND ReferenceID={ExitInterview.EEIID}";
                var prevAttachment = ExitInterviewRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !ExitInterview.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "ExitInterview";
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
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeExitInterview' AND ReferenceID={ExitInterview.EEIID}";
                var prevAttachment = ExitInterviewRepo.GetDataDictCollection(attachmentSql);

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
                        string folderName = "ExitInterview";
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

        public async Task<(bool, string)> SaveChanges(ExitInterviewDto ExitInterview)
        {
            
            if(ExitInterview.EmployeeID.IsZero())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }
            #region Find Employee

            string sqlExtEmp = @$"SELECT 
	                            FullName
	                            ,Employeecode
	                            ,DepartmentName
	                            ,DivisionName
	                            FROM 
	                        Employee AS emp
	                        LEFT JOIN Employment empl ON empl.EmployeeID = emp.EmployeeID AND IsCurrent = 1
	                        LEFT JOIN Department Dep ON Dep.DepartmentID = empl.DepartmentID
	                        LEFT JOIN Division Div on Div.DivisionID = empl.DivisionID
	                        WHERE Emp.EmployeeID = {ExitInterview.EmployeeID}";

            var exitEmployee = ExitInterviewRepo.GetModelData<EmployeeDto>(sqlExtEmp);
            
            if (exitEmployee.IsNull())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }

            #endregion
            foreach (var item in ExitInterview.Attachments)
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

            var existingDA = ExitInterviewRepo.Entities.Where(x => x.EEIID == ExitInterview.EEIID).SingleOrDefault();

            var removeList = RemoveAttachments(ExitInterview);


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (ExitInterview.EEIID > 0 && ExitInterview.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM EmployeeExitInterview MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.EEIID AND AP.APTypeID = {(int)Util.ApprovalType.ExitInterview}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.ExitInterview} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.ExitInterview} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.EEIID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {ExitInterview.ApprovalProcessID}";
                var canReassesstment = ExitInterviewRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit ExitInterview once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit ExitInterview once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)ExitInterview.EEIID, ExitInterview.ApprovalProcessID, (int)Util.ApprovalType.ExitInterview, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
            }

            string ApprovalProcessID = "0";
            var masterModel = new EmployeeExitInterview
            {
                //TemplateID = ExitInterview.TemplateID,
                EmployeeID = ExitInterview.EmployeeID,
                InterviewDetails = ExitInterview.InterviewDetails,
                IsDraft = ExitInterview.IsDraft,
                ApprovalStatusID = ExitInterview.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };


            using (var unitOfWork = new UnitOfWork())
            {
                if (ExitInterview.EEIID.IsZero() && existingDA.IsNull())
                {
                    //masterModel.ReferenceNo = GenerateExitInterviewReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetExitInterviewNewId(masterModel);
                    ExitInterview.EEIID = (int)masterModel.EEIID;

                }
                else
                {
                    masterModel.CreatedBy = existingDA.CreatedBy;
                    masterModel.CreatedDate = existingDA.CreatedDate;
                    masterModel.CreatedIP = existingDA.CreatedIP;
                    masterModel.RowVersion = existingDA.RowVersion;
                    masterModel.EEIID = ExitInterview.EEIID;
                   // masterModel.ReferenceNo = existingDA.ReferenceNo;
                    masterModel.SetModified();

                    
                }

                SetAuditFields(masterModel);

                if (ExitInterview.Attachments.IsNotNull() && ExitInterview.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(ExitInterview.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)ExitInterview.EEIID, "EmployeeExitInterview", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }
                //For Remove Attachment                    
                if (removeList.IsNotNull() && removeList.Count > 0)
                {
                    foreach (var attachemnt in removeList)
                    {
                        SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)ExitInterview.EEIID, "EmployeeExitInterview", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                    }
                }
                if (masterModel.IsAdded || (masterModel.IsDraft && masterModel.IsModified))
                {
                    if (ExitInterview.ExitInterviewApprovalPanelList.IsNotNull() && ExitInterview.ExitInterviewApprovalPanelList.Count > 0)
                    {
                        DeleteManualApprovalPanel((int)masterModel.EEIID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext),(int)Util.ApprovalType.ExitInterview);
                        foreach (var item in ExitInterview.ExitInterviewApprovalPanelList)
                        {
                            string MAPPanelEmployeeID = SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.ExitInterview, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.ExitInterview, (int)masterModel.EEIID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                            if (item.ManualMultipleProxyDetails.IsNotNull() && item.ManualMultipleProxyDetails.Count > 0)
                                foreach (var subItem in item.ManualMultipleProxyDetails)
                                {
                                    SaveManualApprovalPanelMultiProxy(Convert.ToInt32(MAPPanelEmployeeID), (int)Util.ApprovalPanel.ExitInterview, subItem.DivisionID, subItem.DepartmentID, subItem.EmployeeID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));

                                }
                        }




                        //foreach (var item in ExitInterview.ExitInterviewApprovalPanelList)
                        //{
                        //   SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.ExitInterview, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.ExitInterview, (int)masterModel.EEIID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));

                        //}
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

                ExitInterviewRepo.Add(masterModel);

                unitOfWork.CommitChangesWithAudit();

                //if (masterModel.IsAdded)
                //{

                if (!masterModel.IsDraft && ExitInterview.ApprovalProcessID == 0)
                {
                    string approvalTitle = $"{Util.ExitInterviewTitle} {exitEmployee.FullName}-{exitEmployee.EmployeeCode}||{exitEmployee.DivisionName}||{exitEmployee.DepartmentName}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.EEIID, Util.AutoExitInterviewAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.ExitInterview, (int)Util.ApprovalPanel.ExitInterview, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                }

                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.EEIID, (int)Util.MailGroupSetup.ExitInterviewInitiatedMail, (int)Util.ApprovalType.ExitInterview);
                }
            }
            await Task.CompletedTask;

            return (true, $"ExitInterview Submitted Successfully");
        }

        private void SetExitInterviewNewId(EmployeeExitInterview master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("EmployeeExitInterview", AppContexts.User.CompanyID);
            master.EEIID = code.MaxNumber;
        }



        private string GenerateExitInterviewReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/ExitInterview/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("EmployeeExitInterviewRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        private string GenerateRTVReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/RTV/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("EmployeeExitInterviewRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        public GridModel GetExitInterviewList(GridParameter parameters)
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
                    filter = $@" AND ExitInterview.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ExitInterview.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string sql = $@"SELECT DISTINCT
	                            ExitInterview.*
                                --,DAT.DATName TemplateName
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
                                ,(SELECT AEF.EmployeeCode,EmployeeName,DepartmentName FROM Approval..viewApprovalEmployeeFeedback AEF WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExitInterview} AND AEF.ReferenceID = ExitInterview.EEIID AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
                                FOR JSON PATH) PendingAt
                                ,ExtEmp.FullName ExitEmpName
								,ExtEmp.EmployeeCode ExitEmpCode
                                ,ExtEmp.DivisionName ExitEmpDivisionName
							    ,ExtEmp.DepartmentName ExitEmpDepartmentName
                                ,ExtEmp.WorkEmail ExitEmpWorkEmail
							    ,ExtEmp.WorkMobile ExitEmpWorkMobile
                                ,case when ExitInterview.EmployeeID = {AppContexts.User.EmployeeID} then cast(1 as bit) else cast(0 as bit) end canModify
                            FROM EmployeeExitInterview ExitInterview
                            LEFT JOIN Security..Users U ON U.UserID = ExitInterview.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ExitInterview.ApprovalStatusID
                            --LEFT JOIN ExitInterviewTemplate DAT ON ExitInterview.TemplateID = DAT.DATID
                            LEFT JOIN HRMS..ViewALLEmployee ExtEmp ON ExtEmp.EmployeeID = ExitInterview.EmployeeID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ExitInterview.EEIID AND AP.APTypeID = {(int)Util.ApprovalType.ExitInterview}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExitInterview} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExitInterview} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ExitInterview.EEIID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ExitInterview.EEIID
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
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                                    ";
            var result = ExitInterviewRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }



        public async Task<ExitInterviewMasterDto> GetExitInterviewMaster(int EEIID)
        {
            string sql = $@"SELECT ExitInterview.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,VA.WorkMobile
                            ,VA.WorkEmail
                            ,ExitInterview.InterviewDetails as TemplateBody
                            --,DAT.DATName TemplateName
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ExtEmp.FullName ExitEmpName
							,ExtEmp.EmployeeCode ExitEmpCode
                            ,ExtEmp.DivisionName ExitEmpDivisionName
							,ExtEmp.DepartmentName ExitEmpDepartmentName
                            ,ExtEmp.WorkEmail ExitEmpWorkEmail
							,ExtEmp.WorkMobile ExitEmpWorkMobile
                            ,case when ExitInterview.EmployeeID = {AppContexts.User.EmployeeID} then cast(1 as bit) else cast(0 as bit) end canModify
                        from EmployeeExitInterview ExitInterview
                        LEFT JOIN Security..Users U ON U.UserID = ExitInterview.CreatedBy						
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ExitInterview.ApprovalStatusID
                        --LEFT JOIN ExitInterviewTemplate DAT ON ExitInterview.TemplateID = DAT.DATID
                        LEFT JOIN HRMS..ViewALLEmployee ExtEmp ON ExtEmp.EmployeeID = ExitInterview.EmployeeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ExitInterview.EEIID AND AP.APTypeID = {(int)Util.ApprovalType.ExitInterview}
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExitInterview}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExitInterview} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ExitInterview.EEIID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ExitInterview.EEIID
                                    LEFT JOIN 
								        (
								            SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								        )							
						                F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE ExitInterview.EEIID={EEIID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = ExitInterviewRepo.GetModelData<ExitInterviewMasterDto>(sql);
            return master;
        }



        public async Task<List<ManualApprovalPanelEmployeeDto>> GetExitInterviewApprovalPanelDefault(int EEIID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.EmployeeCode+'-'+Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={EEIID}  AND APE.APTypeID={(int)Util.ApprovalType.ExitInterview}";

            var maps = ExitInterviewRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);
            maps.ForEach(x =>
            {
                if (x.IsMultiProxy)
                {
                    string proxySql = @$"select mpp.EmployeeID,mpp.APPanelID, ve.DivisionID,ve.DepartmentID,ve.EmployeeCode, ve.FullName EmployeeName 
                                    from Approval..ManualApprovalPanelProxyEmployee mpp
                                    left join HRMS..ViewALLEmployee ve on ve.EmployeeID = mpp.EmployeeID
                                    where mpp.MAPPanelEmployeeID={x.MAPPanelEmployeeID}";
                    x.ManualMultipleProxyDetails = ExitInterviewRepo.GetDataModelCollection<ManualMultipleProxyDetailsDto>(proxySql);
                }
            });
            return maps;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.ExitInterview);
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
            return GetApprovalForwardingMembers(ApprovalProcessID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberListApprovalService(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)).Result;
        }

        public IEnumerable<Dictionary<string, object>> ReportForExitInterviewFeedback(int EEIID)
        {
            string sql = $@" EXEC HRMS..spRPTExitInterviewApprovalFeedback {EEIID}";
            var feedback = ExitInterviewRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public List<Attachment> GetAttachments(int EEIID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeExitInterview' AND ReferenceID={EEIID}";
            var attachment = ExitInterviewRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachment>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachment
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
                                FROM Approval..ManualApprovalPanelEmployee AS P
                                INNER JOIN HRMS.dbo.EmployeeExitInterview AS D ON P.ReferenceID = D.EEIID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM Approval..ManualApprovalPanelEmployee a
		                                INNER JOIN HRMS.dbo.EmployeeExitInterview b ON a.ReferenceID = b.EEIID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.ExitInterview}
			                                AND APTypeID = {(int)Util.ApprovalType.ExitInterview}
			                                --AND b.TemplateID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.ExitInterview}
											AND SequenceNo > 1
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = ExitInterviewRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }
        public List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int EEIID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM EmployeeExitInterview ExitInterview 
                        LEFT JOIN PurchaseRequisitionMaster PR ON ExitInterview.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE ExitInterview.EEIID={EEIID}";

            var pr = ExitInterviewRepo.GetModelData<ExitInterviewMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.ExitInterview, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            if (pr.EmployeeID != AppContexts.User.EmployeeID)
            {
                ManualApprovalPanelEmployeeDto seq2 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.ExitInterview, SequenceNo = 2, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Prepared", NFAApprovalSequenceType = 55, IsSystemGenerated = true };
                apList.Add(seq2);
            }
            return apList;
        }

        public async Task Delete(int id)
        {
            

            ExitInterviewDto ExitInterview = new ExitInterviewDto();
            ExitInterview.EEIID = id;
            var removeList = RemoveAttachments(ExitInterview);



            using (var unitOfWork = new UnitOfWork())
            {

                var ent = ExitInterviewRepo.Entities.Where(x => x.EEIID == id).FirstOrDefault();

                ent.SetDeleted();
                ExitInterviewRepo.Add(ent);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        public async Task<ExitInterviewTemplateDto> GetExitInterviewTemplate(int DATID)
        {
            string sql = $@"SELECT D.*,SV.SystemVariableCode CategoryTypeName
                        from {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..DocumentApprovalTemplate D
                        LEFT JOIN Security..SystemVariable SV ON D.CategoryType = SV.SystemVariableID
                        WHERE D.DATID={DATID}";
            var master = ExitInterviewRepo.GetModelData<ExitInterviewTemplateDto>(sql);
            return master;
        }

    }



}
