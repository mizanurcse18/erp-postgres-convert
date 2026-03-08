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
    public class PettyCashExpenseClaimManager : ManagerBase, IPettyCashExpenseClaimManager
    {
        private readonly IRepository<PettyCashExpenseMaster> PettyCashExpenseMasterRepo;
        private readonly IRepository<PettyCashExpenseChild> PettyCashExpenseChildRepo;
        private readonly IRepository<IOUMaster> IOUMasterRepo;

        public PettyCashExpenseClaimManager(IRepository<PettyCashExpenseMaster> pettyCashExpenseMasterRepo, IRepository<PettyCashExpenseChild> pettyCashExpenseChildRepo, IRepository<IOUMaster> iouMasterRepo)
        {
            PettyCashExpenseMasterRepo = pettyCashExpenseMasterRepo;
            PettyCashExpenseChildRepo = pettyCashExpenseChildRepo;
            IOUMasterRepo = iouMasterRepo;
        }

        public async Task<PettyCashExpenseMasterDto> GetPettyCashExpenseClaim(int PCEMID)
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
                            ,1 ClaimTypeID
		                    ,'Expense' ClaimType
                            --,IM.ReferenceNo AS IOUReferenceNo
                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ECM.GrandTotal AS PayableAmount
                            ,(SELECT Security.dbo.NumericToBDT(ECM.GrandTotal)) AmountInWords
                        FROM PettyCashExpenseMaster ECM
                        --LEFT JOIN IOUMaster IM ON ECM.IOUMasterID=IM.IOUMasterID
                        LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.PCEMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PCEMID
                        WHERE ECM.PCEMID={PCEMID}";
            var expenseClaimMaster = PettyCashExpenseMasterRepo.GetModelData<PettyCashExpenseMasterDto>(sql);
            return expenseClaimMaster;
        }

        public async Task<List<PettyCashExpenseChildDto>> GetPettyCashExpenseClaimChild(int PCEMID)
        {
            string sql = $@"SELECT ECC.*,SVp.SystemVariableDescription Purpose, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName
                        from PettyCashExpenseChild ECC
                        LEFT JOIN PettyCashExpenseMaster ECM ON ECM.PCEMID = ECC.PCEMID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = ECC.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVp ON SVp.SystemVariableID = ECC.PurposeID
                        WHERE ECC.PCEMID={PCEMID}";

            var expenseChilds = PettyCashExpenseChildRepo.GetDataModelCollection<PettyCashExpenseChildDto>(sql);
            expenseChilds.ForEach(x => x.Attachments = GetAttachments((int)x.PCECID));
            return expenseChilds;
        }

        public Task RemovePettyCashExpenseClaim(int PCEMID)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, string)> SaveChanges(PettyCashExpenseClaimDto expense)
        {
            var existingExpense = PettyCashExpenseMasterRepo.Entities.Where(x => x.PCEMID == expense.PCEMID).SingleOrDefault();

            var validate = CheckApprovalValidation(expense);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsAutoApproved = false;
            bool IsResubmitted = false;

            long CWID = long.TryParse(GetCustodianWalletByEmployeeID(AppContexts.User.EmployeeID.Value).Result?.FirstOrDefault()?.GetValueOrDefault("CWID")?.ToString(), out long result) ? result : 0;

            var masterModel = new PettyCashExpenseMaster
            {
                ReferenceKeyword = expense.ReferenceKeyword,
                EmployeeID = AppContexts.User.EmployeeID.Value,
                GrandTotal = (decimal)expense.Details.Select(x => x.ExpenseAmount).DefaultIfEmpty(0).Sum(),
                IsDraft = expense.IsDraft,
                ApprovalStatusID = expense.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (expense.PCEMID.IsZero() && existingExpense.IsNull())
                {
                    masterModel.SubmitDate = DateTime.Now;
                    masterModel.ReferenceNo = GeneratePettyCashExpenseClaimReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.CWID = CWID;
                    masterModel.SetAdded();
                    SetPettyCashExpenseClaimMasterNewId(masterModel);
                    expense.PCEMID = masterModel.PCEMID;
                }
                else
                {
                    masterModel.CreatedBy = existingExpense.CreatedBy;
                    masterModel.CreatedDate = existingExpense.CreatedDate;
                    masterModel.CreatedIP = existingExpense.CreatedIP;
                    masterModel.RowVersion = existingExpense.RowVersion;
                    masterModel.PCEMID = expense.PCEMID;
                    masterModel.ReferenceNo = existingExpense.ReferenceNo;
                    masterModel.SubmitDate = existingExpense.SubmitDate;
                    masterModel.CWID = existingExpense.CWID;
                    masterModel.SetModified();
                }


                var childModel = GeneratePettyCashExpenseChildDto(expense).MapTo<List<PettyCashExpenseChild>>();

                var months = $@"{string.Join(",", childModel.Select(x => x.ExpenseDate.Month).Distinct())}";

                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                RemoveAttachments(expense);
                AddAttachments(expense);


                PettyCashExpenseMasterRepo.Add(masterModel);
                PettyCashExpenseChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingExpense.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.PettyCashExpenseApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Expense Claim Reference No:{masterModel.ReferenceNo}";
                        //var obj = CreateApprovalProcessForLimit((int)masterModel.PCEMID, Util.AutoExpenseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashExpenseClaim, masterModel.GrandTotal, months);
                        ApprovalProcessID = CreateApprovalProcessForDynamic((int)masterModel.PCEMID, Util.AutoPettyCashExpenseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)ApprovalType.PettyCashExpenseClaim, masterModel.GrandTotal);
                        //ApprovalProcessID = obj.ApprovalProcessID;
                        //IsAutoApproved = obj.IsAutoApproved;
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
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PCEMID, (int)Util.ApprovalType.PettyCashExpenseClaim);
                }

                if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved && !masterModel.IsDraft)
                    await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PCEMID, (int)Util.MailGroupSetup.PettyCashExpenseClaimInitiatedMail);

            }

            await Task.CompletedTask;

            return (true, $"Petty Cash Expense Claim Submitted Successfully");
        }


        private List<PettyCashExpenseClaimDetails> GeneratePettyCashExpenseChildDto(PettyCashExpenseClaimDto expense)
        {
            var existingPettyCashExpenseClaimChild = PettyCashExpenseChildRepo.Entities.Where(x => x.PCEMID == expense.PCEMID).ToList();
            if (expense.Details.IsNotNull())
            {

                expense.Details.ForEach(x =>
                {
                    if (existingPettyCashExpenseClaimChild.Count > 0 && x.PCECID > 0)
                    {
                        var existingModelData = PettyCashExpenseChildRepo.FirstOrDefault(y => y.PCECID == x.PCECID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PCEMID = expense.PCEMID;
                        x.SetAdded();
                        SetPettyCashExpenseClaimChildNewId(x);
                    }
                });

                var willDeleted = existingPettyCashExpenseClaimChild.Where(x => !expense.Details.Select(y => y.PCECID).Contains(x.PCECID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    expense.Details.Add(new PettyCashExpenseClaimDetails
                    {
                        PCECID = x.PCECID,
                        PCEMID = x.PCEMID,
                        ExpenseDate = x.ExpenseDate,
                        PurposeID = x.PurposeID,
                        Details = x.Details,
                        ExpenseAmount = x.ExpenseAmount,
                        Remarks = x.Remarks,
                        CompanyID = x.CompanyID,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        CreatedIP = x.CreatedIP,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedDate = x.UpdatedDate,
                        UpdatedIP = x.UpdatedIP,
                        RowVersion = x.RowVersion,
                        ObjectState = x.ObjectState,
                        Attachments = GetAttachments((int)x.PCECID)
                    });
                });
            }


            return expense.Details;
        }



        private void AddAttachments(PettyCashExpenseClaimDto expense)
        {
            foreach (var details in expense.Details)
            {
                var attachemntList = details.Attachments.Where(x => x.ID == 0).ToList();
                if (attachemntList.IsNotNull() && attachemntList.Count > 0)
                {
                    int sl = 0;
                    foreach (var attachment in attachemntList)
                    {
                        if (attachment.AttachedFile.IsNotNull())
                        {
                            // To Add Physical Files

                            string filename = $"PettyCashExpenseChild-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PettyCashExpenseChild\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.PCECID, "PettyCashExpenseChild", false, attachment.Size, 0, false, attachment.Description ?? "");

                            sl++;
                        }
                    }
                }
            }

        }
        private void RemoveAttachments(PettyCashExpenseClaimDto expense)
        {
            foreach (var details in expense.Details)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PettyCashExpenseChild";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCECID, "PettyCashExpenseChild", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashExpenseChild' AND ReferenceID={details.PCECID}";
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
                    var removeFiles = attachemntList.Where(x => !details.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                    if (removeFiles.Count > 0)
                    {
                        foreach (var data in removeFiles)
                        {
                            // To Remove Physical Files

                            string attachmentFolder = "upload\\attachments";
                            string folderName = "PettyCashExpenseChild";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            File.Delete(str + "\\" + data.FileName);

                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCECID, "PettyCashExpenseChild", true, data.Size, 0, false, data.Description ?? "");

                        }

                    }
                }
            }

        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private string GeneratePettyCashExpenseClaimReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/EXP/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("PettyCashExpenseClaimReferenceNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }
        private void SetPettyCashExpenseClaimMasterNewId(PettyCashExpenseMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PettyCashExpenseMaster", AppContexts.User.CompanyID);
            master.PCEMID = code.MaxNumber;
        }
        private void SetPettyCashExpenseClaimChildNewId(PettyCashExpenseClaimDetails child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PettyCashExpenseChild", AppContexts.User.CompanyID);
            child.PCECID = code.MaxNumber;
        }
        public GridModel GetPettyCashExpenseClaimList(GridParameter parameters)
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
                                ,ISNULL(CS.EntityTypeName, '') ClaimStatusName
                            FROM PettyCashExpenseMaster PCM
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PCM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCM.ApprovalStatusID
                            LEFT JOIN Security..SystemVariable CS ON CS.SystemVariableID=PCM.ClaimStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCM.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PCM.PCEMID 
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = PettyCashExpenseMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(PettyCashExpenseClaimDto expense)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (expense.PCEMID > 0 && expense.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM PettyCashExpenseMaster ECM
                                 LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = ECM.PCEMID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {expense.ApprovalProcessID}";
                var canReassesstment = PettyCashExpenseMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)expense.PCEMID, expense.ApprovalProcessID, (int)Util.ApprovalType.PettyCashExpenseClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PettyCashExpenseClaim);
            return comments.Result;
        }
        private List<Attachments> GetAttachments(int PCECID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='PettyCashExpenseChild' AND ReferenceID={PCECID}";
            var attachment = PettyCashExpenseMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.PettyCashExpenseClaim, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)PCEMID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        #endregion


        public IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID)
        {
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {ReferenceID},{(int)Util.ApprovalType.PettyCashExpenseClaim}";
            var feedback = PettyCashExpenseMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public List<Attachments> GetAttachments(int id, string TableName)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='{TableName}' AND ReferenceID={id}";
            var attachment = PettyCashExpenseMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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



        public GridModel GetPettyCashExpenseAndAdvanceList(GridParameter parameters)
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
                            FROM PettyCashExpenseMaster PCM
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PCM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCM.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PCM.PCEMID 
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND ApprovalStatusID={(int)Util.ApprovalStatus.Approved} {filter}
                            ";
            var result = PettyCashExpenseMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }





    }
}
