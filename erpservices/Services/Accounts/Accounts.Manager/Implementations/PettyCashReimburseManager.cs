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
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class PettyCashReimburseManager : ManagerBase, IPettyCashReimburseManager
    {
        private readonly IRepository<PettyCashReimburseMaster> PettyCashReimburseMasterRepo;
        private readonly IRepository<PettyCashReimburseChild> PettyCashReimburseChildRepo;

        public PettyCashReimburseManager(IRepository<PettyCashReimburseMaster> pettyCashReimburseMasterRepo, IRepository<PettyCashReimburseChild> pettyCashReimburseChildRepo)
        {
            PettyCashReimburseMasterRepo = pettyCashReimburseMasterRepo;
            PettyCashReimburseChildRepo = pettyCashReimburseChildRepo;
        }

        public async Task<PettyCashReimburseMasterDto> GetPettyCashReimburseMaster(int PCRMID)
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
                            ,VA.WorkMobile
                            --,1 ClaimTypeID
		                    --,'reimburse' ClaimType
                            --,IM.ReferenceNo AS IOUReferenceNo
                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ECM.GrandTotal AS PayableAmount
                            ,(SELECT Security.dbo.NumericToBDT(ECM.GrandTotal)) AmountInWords
                            ,AP.ApprovalProcessID
                        FROM PettyCashReimburseMaster ECM
                        LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCRMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim} 
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
										) EA ON EA.ReferenceID = ECM.PCRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PCRMID
                        WHERE ECM.PCRMID={PCRMID}";
            var reimburseClaimMaster = PettyCashReimburseMasterRepo.GetModelData<PettyCashReimburseMasterDto>(sql);
            return reimburseClaimMaster;
        }

        public async Task<List<PettyCashReimburseChildDto>> GetPettyCashReimburseChild(int PCRMID)
        {
            string sql = $@"SELECT A.*,VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName, AP.ApprovalProcessID ClaimApprovalProcessID FROM (SELECT ECC.PCRCID,ECC.PCRMID,ECC.PCCID,ECC.ClaimTypeID,CASE WHEN ECC.ClaimTypeID=1 THEN PCE.GrandTotal ELSE PCA.ResubmitTotalAmount END As TotalAmount
                        --,VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName
                        ,CASE WHEN ECC.ClaimTypeID=1 THEN PCE.ReferenceNo ELSE PCA.ReferenceNo END AS ReferenceNo
						,CASE WHEN ECC.ClaimTypeID=1 THEN PCE.EmployeeID ELSE PCA.EmployeeID END AS EmployeeID
						,CASE WHEN ECC.ClaimTypeID=1 THEN 'Expense' ELSE 'Advance' END AS ClaimType
						,CASE WHEN ECC.ClaimTypeID=1 THEN PCE.SubmitDate ELSE PCA.RequestDate END AS ClaimDate
                        ,CASE WHEN ECC.ClaimTypeID=1 THEN PCE.PCEMID ELSE PCA.PCAMID END AS ClaimID
                        ,CASE WHEN ECC.ClaimTypeID = 1 THEN 28 ELSE 30 END AS ClaimAPTypeID
                        from PettyCashReimburseChild ECC
                        LEFT JOIN PettyCashAdvanceMaster PCA ON PCA.PCAMID=ECC.PCCID AND ECC.ClaimTypeID=2
                        LEFT JOIN PettyCashExpenseMaster PCE ON PCE.PCEMID=ECC.PCCID AND ECC.ClaimTypeID=1
                        LEFT JOIN PettyCashReimburseMaster ECM ON ECM.PCRMID = ECC.PCRMID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = ECC.CreatedBy
                        --LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        WHERE ECC.PCRMID={PCRMID}) A
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = A.EmployeeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = A.ClaimID AND AP.APTypeID = ClaimAPTypeID";


            var reimburseChilds = PettyCashReimburseChildRepo.GetDataModelCollection<PettyCashReimburseChildDto>(sql);
            //reimburseChilds.ForEach(x => x.Attachments = GetAttachments((int)x.PCRCID));
            return reimburseChilds;
        }

        public Task RemovePettyCashReimburseClaim(int PCEMID)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, string)> SaveChanges(PettyCashReimburseClaimDto reimburse)
        {
            //var existingreimburse = PettyCashReimburseMasterRepo.Entities.Where(x => x.PCRMID == reimburse.PCRMID).SingleOrDefault();
            var existingreimburse = PettyCashReimburseMasterRepo.Get(reimburse.PCRMID);
            var validate = CheckApprovalValidation(reimburse);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit reimburse claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsAutoApproved = false;
            bool IsResubmitted = false;


            var removeList = RemoveAttachments(reimburse);
            using (var unitOfWork = new UnitOfWork())
            {
                var masterModel = new PettyCashReimburseMaster
                {
                    ReferenceKeyword = reimburse.ReferenceKeyword,
                    GrandTotal = (decimal)reimburse.GrandTotal,
                    IsDraft = reimburse.IsDraft,
                    ReimburseDate = reimburse.ReimburseDate,
                    ReferenceNo = GeneratePettyCashReimburseClaimReference() + (string.IsNullOrWhiteSpace(reimburse.ReferenceKeyword) ? "" : $"/{reimburse.ReferenceKeyword.ToUpper()}"),
                    ApprovalStatusID = reimburse.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                };
                if (reimburse.PCRMID.IsZero() && existingreimburse.IsNull())
                {
                    masterModel.SetAdded();
                    SetPettyCashReimburseClaimMasterNewId(masterModel);
                    reimburse.PCRMID = masterModel.PCRMID;
                }
                else
                {
                    masterModel.CreatedBy = existingreimburse.CreatedBy;
                    masterModel.CreatedDate = existingreimburse.CreatedDate;
                    masterModel.CreatedIP = existingreimburse.CreatedIP;
                    masterModel.RowVersion = existingreimburse.RowVersion;
                    masterModel.PCRMID = reimburse.PCRMID;
                    masterModel.ReferenceNo = existingreimburse.ReferenceNo;
                    masterModel.ReimburseDate = existingreimburse.ReimburseDate;
                    masterModel.SetModified();
                }
                var childModel = GeneratePettyCashReimburseChild(reimburse);
                var months = $@"{string.Join(",", childModel.Select(x => x.CreatedDate.Month).Distinct())}";
                SetAuditFields(masterModel);
                if (reimburse.Attachments.IsNotNull() && reimburse.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(reimburse.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)reimburse.PCRMID, "PettyCashReimburseMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)reimburse.PCRMID, "PettyCashReimburseMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                SetAuditFields(childModel);

                PettyCashReimburseMasterRepo.Add(masterModel);
                PettyCashReimburseChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingreimburse.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.PettyCashReimburseApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Reimburse Claim Reference No:{masterModel.ReferenceNo}";
                        //ApprovalProcessID = CreateApprovalProcessByAPTypeIDAndAPPanelID((int)masterModel.PCRMID, Util.AutoPettyCashReimburseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashReimburseClaim, (int)Util.ApprovalPanel.PettyCashReimburse);
                        var obj = CreateApprovalProcessForLimit((int)masterModel.PCRMID, Util.AutoPettyCashReimburseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashReimburseClaim, masterModel.GrandTotal, months);
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
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PCRMID, (int)Util.ApprovalType.PettyCashReimburseClaim);
                }

                if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved && !masterModel.IsDraft)
                    await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PCRMID, (int)Util.MailGroupSetup.PettyCashReimburseClaimInitiatedMail);


            }

            await Task.CompletedTask;

            return (true, $"Petty Cash reimburse Claim Submitted Successfully");
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
                        string filename = $"PCR-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PCR\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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

        private List<Attachments> RemoveAttachments(PettyCashReimburseClaimDto ead)
        {
            if (ead.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashReimburseMaster' AND ReferenceID={ead.PCRMID}";
                var prevAttachment = PettyCashReimburseMasterRepo.GetDataDictCollection(attachmentSql);

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

        private List<PettyCashReimburseChild> GeneratePettyCashReimburseChild(PettyCashReimburseClaimDto reimburse)
        {
            var existingPurchaseRequisitionChild = PettyCashReimburseChildRepo.GetAllList(x => x.PCRMID == reimburse.PCRMID);
            var childModel = new List<PettyCashReimburseChild>();
            if (reimburse.Details.IsNotNull())
            {
                reimburse.Details.ForEach(x =>
                {
                    childModel.Add(new PettyCashReimburseChild
                    {
                        PCRCID = x.PCRCID,
                        PCRMID = reimburse.PCRMID,
                        PCCID = x.ClaimID,
                        ClaimTypeID = x.ClaimTypeID
                    });

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseRequisitionChild.Count > 0 && x.PCRCID > 0)
                    {
                        var existingModelData = existingPurchaseRequisitionChild.FirstOrDefault(y => y.PCRCID == x.PCRCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PCRMID = reimburse.PCRMID;
                        x.SetAdded();
                        SetPettyCashReimburseClaimChildNewId(x);
                    }
                });

                var willDeleted = existingPurchaseRequisitionChild.Where(x => !childModel.Select(y => y.PCRCID).Contains(x.PCRCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private string GeneratePettyCashReimburseClaimReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/RM/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("PettyCashReimburseClaimReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        private void SetPettyCashReimburseClaimMasterNewId(PettyCashReimburseMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PettyCashReimburseMaster", AppContexts.User.CompanyID);
            master.PCRMID = code.MaxNumber;
        }
        private void SetPettyCashReimburseClaimChildNewId(PettyCashReimburseChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PettyCashReimburseChild", AppContexts.User.CompanyID);
            child.PCRCID = code.MaxNumber;
        }
        //private void SetPettyCashReimburseClaimChildNewId(PRChildItemDetails child)
        //{
        //    if (!child.IsAdded) return;
        //    var code = GenerateSystemCode("PettyCashReimburseChild", AppContexts.User.CompanyID);
        //    child.PCRCID = code.MaxNumber;
        //}
        public GridModel GetPettyCashReimburseClaimList(GridParameter parameters)
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
                    filter = $@" AND ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                                PCM.PCEMID,
								PCM.ReferenceNo,
								PCM.SubmitDate,
								PCM.ApprovalStatusID,
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
                            FROM PettyCashReimburseMaster PCM
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PCM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCM.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim}
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
										) EA ON EA.ReferenceID = PCM.PCEMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PCM.PCEMID
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
								   )PendingAt ON  PendingAt.PendingReferenceID = PCM.PCEMID 
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = PettyCashReimburseMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(PettyCashReimburseClaimDto reimburse)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (reimburse.PCRMID > 0 && reimburse.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM PettyCashReimburseMaster ECM
                                 LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCRMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashReimburseClaim}
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
            			) EA ON EA.ReferenceID = ECM.PCRMID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {reimburse.ApprovalProcessID}";
                var canReassesstment = PettyCashReimburseMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)reimburse.PCRMID, reimburse.ApprovalProcessID, (int)Util.ApprovalType.PettyCashReimburseClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PettyCashReimburseClaim);
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

        //public Task<decimal> GetIOUClaimAmount(int IOUMasterID)
        //{
        //    var total = IOUMasterRepo.Entities.SingleOrDefault(x => x.IOUMasterID == IOUMasterID && x.ApprovalStatusID == (int)Util.ApprovalStatus.Approved).GrandTotal;
        //    return Task.FromResult(total);
        //}

        #region Mail

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long PCEMID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.PettyCashReimburseClaim, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)PCEMID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        #endregion


        public IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID)
        {
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {ReferenceID},{(int)Util.ApprovalType.PettyCashReimburseClaim}";
            var feedback = PettyCashReimburseMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public List<Attachments> GetAttachments(int id, string TableName)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='{TableName}' AND ReferenceID={id}";
            var attachment = PettyCashReimburseMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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



        public GridModel GetPettyCashReimburseAllList(GridParameter parameters)
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
                            FROM PettyCashReimburseMaster PCM
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
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = PettyCashReimburseMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<List<PettyCashFilteredData>> GetPettyCashReimburseClaimData(int InvoiceMasterID, int IPaymentMasterID, int PCRMID)
        {
            string sql = @$"SELECT *
                            FROM Accounts..viewGetAllPettyCashDisburseClaim WHERE (PCRMID is null OR PCRMID = {PCRMID})";


            var data = PettyCashReimburseMasterRepo.GetDataModelCollection<PettyCashFilteredData>(sql)
                            .Where(x => x.CWEmployeeID == AppContexts.User.EmployeeID)
                            .Where(x => PCRMID == 0 || (x != null && x.PCRMID == PCRMID)).ToList();
            return await Task.FromResult(data);


            //var data = PettyCashReimburseMasterRepo.GetDataModelCollection<PettyCashFilteredData>(sql)
            //    .Where(x => PCRMID == 0 || x.PCRMID == PCRMID).ToList();
            ////.Where(x => InvoiceMasterID == 0 || x.InvoiceMasterID == InvoiceMasterID).ToList();
            //return await Task.FromResult(data);

        }





    }
}
