using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.Manager.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class DocumentUploadManager : ManagerBase, IDocumentUploadManager
    {

        private readonly IRepository<DocumentUpload> DocumentUploadRepo;
        private readonly IRepository<DocumentUploadResponse> DocumentUploadResponseRepo;
        public DocumentUploadManager(IRepository<DocumentUpload> accessDeactivationRepo, IRepository<DocumentUploadResponse> documentUploadResponse)
        {
            DocumentUploadRepo = accessDeactivationRepo;
            DocumentUploadResponseRepo = documentUploadResponse;
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
            string sql = $@"SELECT 
                            DISTINCT
	                             DU.DUID
								,DU.EmployeeID
								,DU.TINNumber
								,DU.DocumentTypeID
								,DU.IncomeYear
								--,DU.AssessmentYear
								,DU.RegSlNo
								,DU.TaxZone
								,DU.TaxCircle
								,DU.TaxUnit
								,DU.PayableAmount
								,DU.PaidAmount
								,DU.SubmissionDate
								,DU.ApprovalStatusID
								,DU.IsDraft
                                ,DU.IsUploaded
                                ,DU.CreatedDate
                                ,DU.ApiResponse
                                ,FY.YearDescription+IY.YearDescription TaxYearDetails
								,DU.TINNumber+DU.RegSlNo+DU.TaxZone+DU.TaxCircle+DU.TaxUnit TINDetails
								,CONCAT(DU.PayableAmount, DU.PaidAmount) AmountDetails
                                ,FY.YearDescription FinancialYear
								,IY.YearDescription AssessmentYear
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
								,EE.DepartmentID EEDepartmentID
	                            ,EE.DepartmentName EEDepartmentName
	                            ,EE.FullName AS EEEmployeeName,EE.EmployeeCode AS EEEmployeeCode, EE.DivisionID EEDivisionID, EE.DivisionName EEDivisionName
	                            ,EE.DesignationName
								,EE.DateOfJoining
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
								,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,DU.CreatedBy
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM HRMS..DocumentUpload DU
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=DU.IncomeYear
							LEFT JOIN Security..FinancialYear IY ON IY.FinancialYearID=DU.AssessmentYear
							LEFT JOIN HRMS..ViewALLEmployeeRegularJoin EE ON EE.EmployeeID = DU.EmployeeID
                            LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = DU.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DU.DUID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
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
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.DUID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DU.DUID
                                    LEFT JOIN ( 
                                            SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                
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
                                    LEFT JOIN (
								      SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										   DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = DU.DUID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            OR DU.EmployeeID={AppContexts.User.EmployeeID}) {filter}";
            var result = DocumentUploadRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }
        public async Task<(bool, string)> SaveChanges(DocumentUploadDto documentUpload)
        {
            
            var existFinnancialYear = DocumentUploadRepo.Entities.Where(x => x.DocumentTypeID == documentUpload.DocumentTypeID && x.EmployeeID == AppContexts.User.EmployeeID && x.IncomeYear == documentUpload.IncomeYear && x.ApprovalStatusID != (int)Util.ApprovalStatus.Rejected && x.DUID != documentUpload.DUID);
            if (existFinnancialYear.IsNotNull() && existFinnancialYear.Count() > 0)
            {
                return (false, "Already exist Tax-Card this financial year.");
            }
            var existingDU = DocumentUploadRepo.Entities.Where(x => x.DUID == documentUpload.DUID).SingleOrDefault();

            foreach (var item in documentUpload.Attachments)
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


                    string err = CheckFileExtensionsForAttachment(ext, item.OriginalName);
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

            var removeList = RemoveAttachments(documentUpload);
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (documentUpload.DUID > 0 && documentUpload.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..DocumentUpload DU
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = DU.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.Default)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = DU.DUID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.DUID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {documentUpload.ApprovalProcessID}";
                var canReassesstment = DocumentUploadRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit DU once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit DU once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback(documentUpload.DUID, documentUpload.ApprovalProcessID, (int)Util.ApprovalType.EmployeeeDocumentUpload);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new DocumentUpload
            {
                DUID = documentUpload.DUID,
                EmployeeID = (int)AppContexts.User.EmployeeID,
                TINNumber = documentUpload.TINNumber,
                DocumentTypeID = documentUpload.DocumentTypeID,
                IncomeYear = documentUpload.IncomeYear,
                AssessmentYear = documentUpload.AssessmentYear,
                RegSlNo = documentUpload.RegSlNo,
                TaxZone = documentUpload.TaxZone,
                TaxCircle = documentUpload.TaxCircle,
                TaxUnit = documentUpload.TaxUnit,
                PayableAmount = documentUpload.PayableAmount,
                PaidAmount = documentUpload.PaidAmount,
                SubmissionDate = documentUpload.SubmissionDate,
                IsDraft = documentUpload.IsDraft,
                ApprovalStatusID = documentUpload.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (documentUpload.DUID.IsZero() && existingDU.IsNull())
                {
                    masterModel.SetAdded();
                    SetDUMasterNewId(masterModel);
                    documentUpload.DUID = (int)masterModel.DUID;
                }
                else
                {
                    masterModel.CreatedBy = existingDU.CreatedBy;
                    masterModel.CreatedDate = existingDU.CreatedDate;
                    masterModel.CreatedIP = existingDU.CreatedIP;
                    masterModel.RowVersion = existingDU.RowVersion;
                    masterModel.SetModified();
                }

                SetAuditFields(masterModel);

                if (documentUpload.Attachments.IsNotNull() && documentUpload.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(documentUpload.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, documentUpload.DUID, "DocumentUpload", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, documentUpload.DUID, "DocumentUpload", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                DocumentUploadRepo.Add(masterModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingDU.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.DocumentUploadApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, TIN No:{masterModel.TINNumber}";
                        //var obj = CreateApprovalProcessForLimit((int)masterModel.DUID, Util.AutoDocumentUploadAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.EmployeeeDocumentUpload, masterModel.PaidAmount, "0");
                        //ApprovalProcessID = obj.ApprovalProcessID;

                        ApprovalProcessID = CreateApprovalProcessByAPTypeIDAndAPPanelID((int)masterModel.DUID, Util.AutoDocumentUploadAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.EmployeeeDocumentUpload, (int)Util.ApprovalPanel.EmployeeDocumentUploadApproval);

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
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.DUID, (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail, (int)Util.ApprovalType.EmployeeeDocumentUpload);
                }
            }
            await Task.CompletedTask;

            return (true, $"Document Upload Submitted Successfully"); ;
        }

        private List<Attachments> RemoveAttachments(DocumentUploadDto ead)
        {
            if (ead.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentUpload' AND ReferenceID={ead.DUID}";
                var prevAttachment = DocumentUploadRepo.GetDataDictCollection(attachmentSql);

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
                        string folderName = "DU";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.EmployeeeDocumentUpload);
            return comments.Result;
        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int DUID, int mailGroup)
        {

            var mail = GetAPEmployeeEmailsWithMultiProxyParallal(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2;
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.EmployeeeDocumentUpload, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, DUID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private void SetDUMasterNewId(DocumentUpload master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("DocumentUpload", AppContexts.User.CompanyID);
            master.DUID = code.MaxNumber;
        }

        private void SetDUChildNewId(DocumentUploadResponse child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("DocumentUploadResponse", AppContexts.User.CompanyID);
            child.DURID = code.MaxNumber;
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
                        string filename = $"DU-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "DU\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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

        public async Task<DocumentUploadDto> GetDocumentUpload(int DUID, int ApprovalProcessID)
        {
            string sql = $@"select DISTINCT DU.DUID
								,DU.EmployeeID
                                ,EE.EmployeeCode
								,DU.TINNumber
                                ,FY.YearDescription IncomeYearTitle
								,AY.YearDescription AssessmentYearTitle
								,DU.RegSlNo
								,DU.TaxZone 
								,DU.TaxCircle
								,DU.TaxUnit
								,DU.PayableAmount
								,DU.PaidAmount
                                ,DU.SubmissionDate
	                            ,DU.ApprovalStatusID
								,EE.FullName EmployeeName
                                ,EE.DivisionName
								,EE.DepartmentName
								,EE.WorkMobile
								,EE.WorkEmail
	                            ,DU.CreatedDate
                                ,DU.CreatedBy
                                ,DU.IsDraft
                                ,DU.IncomeYear
                                ,DU.AssessmentYear
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            --,ISNULL(APForwardInfoID,0) APForwardInfoID
								---,ISNULL(AEF.IsEditable,0) IsEditable
                                --,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                --,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
								,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,DU.CreatedBy

								
                        from HRMS..DocumentUpload DU
						LEFT JOIN HRMS..ViewALLEmployee EE ON EE.EmployeeID = DU.EmployeeID
                        LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = DU.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear FY ON FY.FinancialYearID = DU.IncomeYear
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear AY ON AY.FinancialYearID = DU.AssessmentYear
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DU.DUID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
										--GROUP BY ReferenceID 
                                        
										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.DUID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DU.DUID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE DU.DUID={DUID}  --AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID} OR DU.EmployeeID={AppContexts.User.EmployeeID})";
            var du = DocumentUploadRepo.GetModelData<DocumentUploadDto>(sql);
            return du;
        }
        public List<Attachments> GetAttachments(int DUID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentUpload' AND ReferenceID={DUID}";
            var attachment = DocumentUploadRepo.GetDataDictCollection(attachmentSql);
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

        public async Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID)
        {
            return GetApprovalForwardingMembers(ReferenceID, APTypeID, APPanelID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedFirstMember(aprovalProcessId).Result;
        }

        public async Task<DocumentUploadDto> GetDocumentUploadForReAssessment(int DUID)
        {
            var model = DocumentUploadRepo.Entities.Where(x => x.DUID == DUID).Select(y => new DocumentUploadDto
            {
                DUID = (int)y.DUID,
                //DateOfResignation = y.DateOfResignation,
                //LastWorkingDay = y.LastWorkingDay,
                //IsCoreFunctional = y.IsCoreFunctional,
                //Description = y.Description,
                //EEIID = y.EEIID
            }).FirstOrDefault();


            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DocumentUpload' AND ReferenceID={DUID}";
            var attachment = DocumentUploadRepo.GetDataDictCollection(attachmentSql);
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
            model.Attachments = attachemntList;

            return await Task.FromResult(model);
        }

        public IEnumerable<Dictionary<string, object>> ReportForDocumentUploadAttachments(int DUID)
        {
            string sql = $@" EXEC Security..spRPTAttachmentList {0}";
            var attachemntList = DocumentUploadRepo.GetDataDictCollection(sql);
            return attachemntList;
        }

        public Dictionary<string, object> ReportForDocumentUploadMaster(int DUID)
        {
            string sql = $@" EXEC HRMS..spRPTDocumentUpload {DUID}";
            var masterData = DocumentUploadRepo.GetData(sql);
            return masterData;
        }

        public IEnumerable<Dictionary<string, object>> ReportForDUApprovalFeedback(int DUID)
        {
            string sql = $@" EXEC {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..spRPTDocumentUploadApprovalFeedback {DUID}";
            var feedback = DocumentUploadRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task RemoveDocumentUpload(int DUID, int aprovalProcessId)
        {
            var accessDeactivation = DocumentUploadRepo.Entities.Where(x => x.DUID == DUID).FirstOrDefault();
            accessDeactivation.SetDeleted();
            var mailData = new List<Dictionary<string, object>>();
            var data = new Dictionary<string, object>
                {
                    //{ "ReferenceNo", accessDeactivation.ReferenceNo },
                    { "EmployeeName", AppContexts.User.FullName }
                };
            mailData.Add(data);
            var mail = GetAPEmployeeEmailsWithProxy(aprovalProcessId).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            BasicMail((int)Util.MailGroupSetup.NFARemoveMail, ToEmailAddress, false, CCEmailAddress, null, mailData);

            using (var unitOfWork = new UnitOfWork())
            {


                DocumentUploadRepo.Add(accessDeactivation);
                DeleteAllApprovalProcessRelatedData((int)ApprovalType.EmployeeeDocumentUpload, DUID);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID)
        {
            string sql = $@"SELECT 
	                             FullName,
	                             APForwardEmployeeComment,
	                             CAST(CommentSubmitDate as Date) CommentSubmitDate,
	                             DesignationName,
	                             DepartmentName
                            FROM 
	                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo AEF 
	                            INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                            INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID	
                            WHERE AP.APTypeID = {APTypeID} AND ReferenceID = {ReferenceID} AND CommentSubmitDate IS NOT NULL
                            ORDER BY APForwardInfoID asc";
            var comments = DocumentUploadRepo.GetDataDictCollection(sql);
            return comments;
        }

        public List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByDUID(int id)
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
                                INNER JOIN HRMS..DocumentUpload AS D ON P.ReferenceID = D.DUID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM Approval..ManualApprovalPanelEmployee a
		                                INNER JOIN HRMS..DocumentUpload b ON a.ReferenceID = b.DUID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.DivisionClearance}
			                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
			                                --AND b.DUID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = DocumentUploadRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForDocumentUpload()
        {
            string sql = @$"SELECT 
                                E.EmployeeID as value,EmployeeCode,CONCAT(EmployeeCode,'-',FullName) label,WorkEmail,WorkMobile,DivisionName ,DepartmentName
                            ,EmpStats.SystemVariableCode EmployeeStatus,Emp.EmployeeTypeID
                        FROM
                        Employee E
                        INNER JOIN Employment Emp ON emp.EmployeeID = E.EmployeeID AND IsCurrent = 1
                        INNER JOIN Division D on D.DivisionID = Emp.DivisionID
                        INNER JOIN Department Dept on Dept.DepartmentID = Emp.DepartmentID
                        LEFT JOIN Security..SystemVariable EmpStats ON EmpStats.SystemVariableID = Emp.EmployeeTypeID
                        LEFT JOIN DocumentUpload DU on DU.EmployeeID = Emp.EmployeeID  AND DU.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND DU.DUID IS NULL";
            var listDict = DocumentUploadRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAllHODDocumentUploadList()
        {
            string sql = $@"SELECT 
                            DISTINCT
                                 ROW_NUMBER() OVER(ORDER BY DU.CreatedDate ASC) AutoGenRowNum,
	                             DU.DUID
								,DU.EmployeeID
								,DU.TINNumber
								,DU.DocumentTypeID
								,DU.IncomeYear
								--,DU.AssessmentYear
								,DU.RegSlNo
								,DU.TaxZone
								,DU.TaxCircle
								,DU.TaxUnit
								,DU.PayableAmount
								,DU.PaidAmount
								,DU.SubmissionDate
								,DU.ApprovalStatusID
								,DU.IsDraft
                                ,DU.IsUploaded
                                ,DU.CreatedDate
                                ,DU.ApiResponse
                                ,FY.YearDescription+IY.YearDescription TaxYearDetails
								,DU.TINNumber+DU.RegSlNo+DU.TaxZone+DU.TaxCircle+DU.TaxUnit TINDetails
								,CONCAT(DU.PayableAmount, DU.PaidAmount) AmountDetails
                                ,FY.YearDescription FinancialYear
								,IY.YearDescription AssessmentYear
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
								,EE.DepartmentID EEDepartmentID
	                            ,EE.DepartmentName EEDepartmentName
	                            ,EE.FullName AS EEEmployeeName,EE.EmployeeCode AS EEEmployeeCode, EE.DivisionID EEDivisionID, EE.DivisionName EEDivisionName
	                            ,EE.DesignationName
								,EE.DateOfJoining
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
								,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,DU.CreatedBy
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM HRMS..DocumentUpload DU
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=DU.IncomeYear
							LEFT JOIN Security..FinancialYear IY ON IY.FinancialYearID=DU.AssessmentYear
							LEFT JOIN HRMS..ViewALLEmployeeRegularJoin EE ON EE.EmployeeID = DU.EmployeeID
                            LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = DU.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DU.DUID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
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
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.DUID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DU.DUID
                                    LEFT JOIN ( 
                                            SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                
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
                                    LEFT JOIN (
								      SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										   DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = DU.DUID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            OR DU.EmployeeID={AppContexts.User.EmployeeID})";
            var listDict = DocumentUploadRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public IEnumerable<Dictionary<string, object>> ReportForEADApprovalFeedback(int DUID)
        {
            throw new NotImplementedException();
        }

        public void UpdateDocumentUploadStatus(DocumentUploadResponseDto documentUploadResponseDto)
        {
            var existingDocumentUpload = DocumentUploadRepo.Get(documentUploadResponseDto.DUID);
            if (existingDocumentUpload.IsNotNull())
            {
                existingDocumentUpload.SetModified();
                existingDocumentUpload.IsUploaded = documentUploadResponseDto.ApiStatus == 200 ? true : false;
                existingDocumentUpload.ApiResponse = documentUploadResponseDto.ApiResponse.ToString();

                DocumentUploadResponse failResponse = new DocumentUploadResponse();
                if(documentUploadResponseDto.ApiStatus != 200)
                {
                    failResponse.SetAdded();
                    failResponse.DUID = documentUploadResponseDto.DUID;
                    failResponse.ApiStatus = documentUploadResponseDto.ApiStatus.ToString();
                    failResponse.ApiResponse = documentUploadResponseDto.ApiResponse.ToString();
                    //SetDUChildNewId(failResponse);
                }

                SetAuditFields(existingDocumentUpload);
                SetAuditFields(failResponse);

                using (var unitOfWork = new UnitOfWork())
                {
                    DocumentUploadRepo.Add(existingDocumentUpload);
                    DocumentUploadResponseRepo.Add(failResponse);
                    unitOfWork.CommitChangesWithAudit();
                }
            }

        }

        public string CheckFileExtensionsForAttachment(string ext, string fileName)
        {
            string error = "";
            string fileExt = fileName.IsNullOrEmpty() ? "" : System.IO.Path.GetExtension(fileName).Remove(0, 1);
            if (!AttachmentExtensionsForDocumentUpload.Contains(ext) && (fileExt.IsNotNullOrEmpty() && !AttachmentExtensionsForDocumentUpload.Contains(fileExt)))
            {
                error = "Uploaded file extension is not allowed.";
            }
            return error;
        }

    }
}
