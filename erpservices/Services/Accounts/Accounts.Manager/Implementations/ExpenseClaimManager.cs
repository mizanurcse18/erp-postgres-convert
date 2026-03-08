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
    public class ExpenseClaimManager : ManagerBase, IExpenseClaimManager
    {
        private readonly IRepository<ExpenseClaimMaster> ExpenseClaimMasterRepo;
        private readonly IRepository<ExpenseClaimChild> ExpenseClaimChildRepo;
        private readonly IRepository<IOUMaster> IOUMasterRepo;

        public ExpenseClaimManager(IRepository<ExpenseClaimMaster> expenseClaimMasterRepo, IRepository<ExpenseClaimChild> expenseClaimChildRepo, IRepository<IOUMaster> iouMasterRepo)
        {
            ExpenseClaimMasterRepo = expenseClaimMasterRepo;
            ExpenseClaimChildRepo = expenseClaimChildRepo;
            IOUMasterRepo = iouMasterRepo;
        }

        public async Task<ExpenseClaimMasterDto> GetExpenseClaim(int ECMasterID)
        {
            string sql = $@"SELECT DISTINCT ECM.*, 
                            VA.DepartmentID, 
                            VA.DepartmentName, 
                            VA.FullName AS EmployeeName, 
                            (Emp.EmployeeCode +'-'+ Emp.FullName) EmployeeDetails,
                            Emp.WalletNumber OnbehalfWallet,
                            VA.InternalDesignationName 'Designation',
                            VA.WalletNumber 'NagadWallet',
                            SV.SystemVariableCode AS ApprovalStatus, 
                            VA.ImagePath, 
                            VA.EmployeeCode, 
                            VA.DivisionID
                            ,VA.DivisionName 
                            ,IM.ReferenceNo AS IOUReferenceNo
                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,(ISNULL(IM.GrandTotal,0))- ECM.GrandTotal AS PayableAmount
                            ,AP.ApprovalProcessID
                        FROM ExpenseClaimMaster ECM
                        LEFT JOIN HRMS..Employee Emp ON Emp.EmployeeID=ECM.EmployeeID
                        LEFT JOIN IOUMaster IM ON ECM.IOUMasterID=IM.IOUMasterID
                        LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.ECMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.ECMasterID
                        WHERE ECM.ECMasterID={ECMasterID}";
            var expenseClaimMaster = ExpenseClaimMasterRepo.GetModelData<ExpenseClaimMasterDto>(sql);
            return expenseClaimMaster;
        }

        public async Task<List<ExpenseClaimChildDto>> GetExpenseClaimChild(int ECMasterID)
        {
            string sql = $@"SELECT ECC.*,SVp.SystemVariableDescription Purpose, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName
                        from ExpenseClaimChild ECC
                        LEFT JOIN ExpenseClaimMaster ECM ON ECM.ECMasterID = ECC.ECMasterID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = ECC.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVp ON SVp.SystemVariableID = ECC.PurposeID
                        WHERE ECC.ECMasterID={ECMasterID}";

            var expenseChilds = ExpenseClaimChildRepo.GetDataModelCollection<ExpenseClaimChildDto>(sql);
            expenseChilds.ForEach(x => x.Attachments = GetAttachments((int)x.ECChildID));
            return expenseChilds;
        }

        public Task RemoveExpenseClaim(int ECMasterID)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, string)> SaveChanges(ExpenseClaimDto expense)
        {
            var existingExpense = ExpenseClaimMasterRepo.Entities.Where(x => x.ECMasterID == expense.ECMasterID).SingleOrDefault();

            var validate = CheckApprovalValidation(expense);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsAutoApproved = false;
            bool IsResubmitted = false;

            var masterModel = new ExpenseClaimMaster
            {
                ReferenceKeyword = expense.ReferenceKeyword,
                IOUMasterID = expense.IOUMasterID,
                EmployeeID = expense.EmployeeID == 0 ? AppContexts.User.EmployeeID.Value : expense.EmployeeID,
                ApprovalStatusID = expense.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                IsOnBehalf = expense.IsOnBehalf,
                IsDraft = expense.IsDraft,
                GrandTotal = (decimal)expense.Details.Select(x => x.ExpenseClaimAmount).DefaultIfEmpty(0).Sum(),
                PaymentStatus = expense.IOUMasterID != 0 ? (IOUMasterRepo.Entities.SingleOrDefault(x => x.IOUMasterID == expense.IOUMasterID).GrandTotal > expense.Details.Select(x => x.ExpenseClaimAmount).DefaultIfEmpty(0).Sum() ? (int)Util.PaymentStatus.Payable : (int)Util.PaymentStatus.Receivable) : (int)Util.PaymentStatus.Receivable
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (expense.ECMasterID.IsZero() && existingExpense.IsNull())
                {
                    masterModel.ClaimSubmitDate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateExpenseClaimReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    //masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetExpenseClaimMasterNewId(masterModel);
                    expense.ECMasterID = masterModel.ECMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingExpense.CreatedBy;
                    masterModel.CreatedDate = existingExpense.CreatedDate;
                    masterModel.CreatedIP = existingExpense.CreatedIP;
                    masterModel.RowVersion = existingExpense.RowVersion;
                    masterModel.ECMasterID = expense.ECMasterID;
                    masterModel.ReferenceNo = existingExpense.ReferenceNo;
                    masterModel.ClaimSubmitDate = existingExpense.ClaimSubmitDate;
                    masterModel.IsOnBehalf = existingExpense.IsOnBehalf;
                    //masterModel.ApprovalStatusID = existingExpense.ApprovalStatusID;
                    masterModel.SetModified();
                }


                var childModel = GenerateExpenseClaimChildDto(expense).MapTo<List<ExpenseClaimChild>>();

                var months = $@"{string.Join(",", childModel.Select(x => x.ExpenseClaimDate.Month).Distinct())}";

                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                RemoveAttachments(expense);
                AddAttachments(expense);


                ExpenseClaimMasterRepo.Add(masterModel);
                ExpenseClaimChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingExpense.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.ExpenseApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Expense Claim Reference No:{masterModel.ReferenceNo}";
                        var obj = CreateApprovalProcessForLimit((int)masterModel.ECMasterID, Util.AutoExpenseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.ExpenseClaim, masterModel.GrandTotal, months);
                        ApprovalProcessID = obj.ApprovalProcessID;
                        IsAutoApproved = obj.IsAutoApproved;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            //UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            //    $"{Util.ExpenseApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Expense Claim Reference No:{masterModel.ReferenceNo}");

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

                if (IsAutoApproved)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.ECMasterID, (int)Util.ApprovalType.ExpenseClaim);
                }

                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                        //    await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMail(ApprovalProcessID, IsResubmitted, masterModel.ECMasterID, (int)Util.MailGroupSetup.ExpenseClaimInitiatedMail);
                }

            }

            await Task.CompletedTask;

            return (true, $"Expense Claim Submitted Successfully");
        }


        private List<ExpenseClaimDetails> GenerateExpenseClaimChildDto(ExpenseClaimDto expense)
        {
            var existingExpenseClaimChild = ExpenseClaimChildRepo.Entities.Where(x => x.ECMasterID == expense.ECMasterID).ToList();
            if (expense.Details.IsNotNull())
            {

                expense.Details.ForEach(x =>
                {
                    if (existingExpenseClaimChild.Count > 0 && x.ECChildID > 0)
                    {
                        var existingModelData = ExpenseClaimChildRepo.FirstOrDefault(y => y.ECChildID == x.ECChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.ECMasterID = expense.ECMasterID;
                        x.SetAdded();
                        SetExpenseClaimChildNewId(x);
                    }
                });

                var willDeleted = existingExpenseClaimChild.Where(x => !expense.Details.Select(y => y.ECChildID).Contains(x.ECChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    expense.Details.Add(new ExpenseClaimDetails
                    {
                        ECChildID = x.ECChildID,
                        ECMasterID = x.ECMasterID,
                        ExpenseClaimDate = x.ExpenseClaimDate,
                        PurposeID = x.PurposeID,
                        Details = x.Details,
                        ExpenseClaimAmount = x.ExpenseClaimAmount,
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
                        Attachments = GetAttachments((int)x.ECChildID)
                    });
                });
            }


            return expense.Details;
        }
        private void AddAttachments(ExpenseClaimDto expense)
        {
            int sl1 = 0;
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

                            string filename = $"ExpenseClaimChild-{DateTime.Now:ddMMyyHHmmss}-{sl1}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "ExpenseClaimChild\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.ECChildID, "ExpenseClaimChild", false, attachment.Size, 0, false, attachment.Description ?? "");

                            sl++;
                        }
                    }
                    sl1++;
                }

            }

        }
        private void RemoveAttachments(ExpenseClaimDto expense)
        {
            foreach (var details in expense.Details)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "ExpenseClaimChild";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.ECChildID, "ExpenseClaimChild", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='ExpenseClaimChild' AND ReferenceID={details.ECChildID}";
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
                            string folderName = "ExpenseClaimChild";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            File.Delete(str + "\\" + data.FileName);

                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.ECChildID, "ExpenseClaimChild", true, data.Size, 0, false, data.Description ?? "");

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
        private string GenerateExpenseClaimReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/EXP/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("ExpenseClaimReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        private void SetExpenseClaimMasterNewId(ExpenseClaimMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("ExpenseClaimMaster", AppContexts.User.CompanyID);
            master.ECMasterID = code.MaxNumber;
        }
        private void SetExpenseClaimChildNewId(ExpenseClaimDetails child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("ExpenseClaimChild", AppContexts.User.CompanyID);
            child.ECChildID = code.MaxNumber;
        }
        public GridModel GetExpenseClaimList(GridParameter parameters)
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
	                            ECM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable 
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,(CASE WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=22 THEN 'Payment Requested' 
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=0 THEN 'Payment Approved & Waiting for Settlement'
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,CASE WHEN ECM.ApprovalStatusID = 23 THEN  ExpenseApprovalDate ELSE NULL END ExpenseApprovalDate,FirstPaymentFeedbackRequestDate
								,ISNULL((CASE WHEN (ECM.ApprovalStatusID = 23 AND Pay.PaymentMasterID is not null) THEN (SELECT  CONVERT(VARCHAR(10), ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) / 86400 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) / 3600 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) % 3600 ) / 60 ))) ELSE NULL END),'') ClaimToPaymentTime
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                            FROM ExpenseClaimMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN (SELECT 
											EPM.ApprovalStatusID
											,EPC.IOUOrExpenseClaimID
											,EPM.IsSettlement
											,EPM.PaymentMasterID
											,FirstPaymentFeedbackRequestDate
										FROM Accounts..IOUOrExpensePaymentChild EPC
										JOIN Accounts..IOUOrExpensePaymentMaster EPM ON EPM.PaymentMasterID = EPC.PaymentMasterID
										LEFT JOIN (
										SELECT 
												Min(FeedbackRequestDate) FirstPaymentFeedbackRequestDate,ReferenceID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN(SELECT APTypeID,ReferenceID,ApprovalProcessID FROM Approval..ApprovalProcess )AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID	
											where APTypeID = 6 
											GROUP BY ReferenceID,ReferenceID
										)FF ON FF.ReferenceID = EPM.PaymentMasterID
										WHERE EPM.ApprovalStatusID <> 24 AND ClaimType = 'Expense') Pay on Pay.IOUOrExpenseClaimID=ECM.ECMasterID						    
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID ={AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.ECMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.ECMasterID
LEFT                                JOIN (SELECT DISTINCT
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
                                LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.ECMasterID
                                LEFT JOIN (
											SELECT 
												MAX(FeedbackSubmitDate) ExpenseApprovalDate,ReferenceID,AP.ApprovalProcessID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
											where APFeedbackID  = 5 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
											GROUP BY ReferenceID,AP.ApprovalProcessID
											)LFBSD ON LFBSD.ReferenceID = ECM.ECMasterID
                              WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND ISNULL(ECM.IsOnBehalf,0)=0 {filter}
                            ";
            var result = ExpenseClaimMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetExpenseClaimListForEmp(GridParameter parameters)
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
	                            ECM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable 
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,(CASE WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=22 THEN 'Payment Requested' 
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=0 THEN 'Payment Approved & Waiting for Settlement'
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,CASE WHEN ECM.ApprovalStatusID = 23 THEN  ExpenseApprovalDate ELSE NULL END ExpenseApprovalDate,FirstPaymentFeedbackRequestDate
								,ISNULL((CASE WHEN (ECM.ApprovalStatusID = 23 AND Pay.PaymentMasterID is not null) THEN (SELECT  CONVERT(VARCHAR(10), ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) / 86400 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) / 3600 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) % 3600 ) / 60 ))) ELSE NULL END),'') ClaimToPaymentTime
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                            FROM ExpenseClaimMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN (SELECT 
											EPM.ApprovalStatusID
											,EPC.IOUOrExpenseClaimID
											,EPM.IsSettlement
											,EPM.PaymentMasterID
											,FirstPaymentFeedbackRequestDate
										FROM Accounts..IOUOrExpensePaymentChild EPC
										JOIN Accounts..IOUOrExpensePaymentMaster EPM ON EPM.PaymentMasterID = EPC.PaymentMasterID
										LEFT JOIN (
										SELECT 
												Min(FeedbackRequestDate) FirstPaymentFeedbackRequestDate,ReferenceID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN(SELECT APTypeID,ReferenceID,ApprovalProcessID FROM Approval..ApprovalProcess )AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID	
											where APTypeID = 6 
											GROUP BY ReferenceID,ReferenceID
										)FF ON FF.ReferenceID = EPM.PaymentMasterID
										WHERE EPM.ApprovalStatusID <> 24 AND ClaimType = 'Expense') Pay on Pay.IOUOrExpenseClaimID=ECM.ECMasterID						    
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID ={AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.ECMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.ECMasterID
LEFT                                JOIN (SELECT DISTINCT
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
                                LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.ECMasterID
                                LEFT JOIN (
											SELECT 
												MAX(FeedbackSubmitDate) ExpenseApprovalDate,ReferenceID,AP.ApprovalProcessID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
											where APFeedbackID  = 5 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
											GROUP BY ReferenceID,AP.ApprovalProcessID
											)LFBSD ON LFBSD.ReferenceID = ECM.ECMasterID	
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND ISNULL(ECM.IsOnBehalf,0)=1 {filter}
                            ";
            var result = ExpenseClaimMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        private (bool, Dictionary<string, object>) CheckApprovalValidation(ExpenseClaimDto expense)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (expense.ECMasterID > 0 && expense.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM ExpenseClaimMaster ECM
                                 LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = ECM.ECMasterID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {expense.ApprovalProcessID}";
                var canReassesstment = ExpenseClaimMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)expense.ECMasterID, expense.ApprovalProcessID, (int)Util.ApprovalType.ExpenseClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.ExpenseClaim);
            return comments.Result;
        }
        private List<Attachments> GetAttachments(int ECChildID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='ExpenseClaimChild' AND ReferenceID={ECChildID}";
            var attachment = ExpenseClaimMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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

        public Task<decimal> GetIOUClaimAmount(int IOUMasterID)
        {
            var total = IOUMasterRepo.Entities.SingleOrDefault(x => x.IOUMasterID == IOUMasterID && x.ApprovalStatusID == (int)Util.ApprovalStatus.Approved).GrandTotal;
            return Task.FromResult(total);
        }

        public IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID)
        {
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {ReferenceID},{(int)Util.ApprovalType.ExpenseClaim}";
            var feedback = ExpenseClaimMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task UpdateExpenseClaimAfterReset(ExpenseClaimMasterDto ecm)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existingSCC = ExpenseClaimMasterRepo.Entities.Where(x => x.ECMasterID == ecm.ECMasterID).ToList();
                if (existingSCC.Count() > 0)
                {
                    foreach (var item in existingSCC)
                    {
                        item.CreatedBy = item.CreatedBy;
                        item.CreatedDate = item.CreatedDate;
                        item.CreatedIP = item.CreatedIP;
                        item.RowVersion = item.RowVersion;
                        item.ECMasterID = ecm.ECMasterID;
                        item.ReferenceNo = item.ReferenceNo;
                        item.ApprovalStatusID = 22;
                        item.SetModified();

                        SetAuditFields(item);
                        ExpenseClaimMasterRepo.Add(item);
                    }
                }

                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }


        #region Mail

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long ECMasterID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.ExpenseClaim, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)ECMasterID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        #endregion


        //public IEnumerable<Dictionary<string, object>> GetDivHeadBudget(int ECMasterID)
        //{
        //    string sql = $@" SELECT Map.DivisionID,Map.BudgetAmount--,SUM(Map.BudgetAmount-(PendingAmount+ApprovedAmount)) RemainingBalance
        //                            FROM 
        //                            HRMS..DivisionHeadMap Map
        //                            LEFT JOIN Accounts..ViewGetAllDepartmentWiseClaimData cData ON Map.EmployeeID=cData.DivHeadEmpID AND Map.DivisionID=cData.DivisionID
        //                            WHERE cData.DivHeadEmpID={AppContexts.User.EmployeeID} 
        //                            AND MonthName=FORMAT((SELECT top 1 ExpenseClaimDate FROM ExpenseClaimChild Where ECMasterID={ECMasterID}), 'MMMM') 
        //                            AND Year=FORMAT((SELECT top 1 ExpenseClaimDate FROM ExpenseClaimChild Where ECMasterID={ECMasterID}), 'yyyy')
        //                            GROUP BY Map.DivisionID,Map.BudgetAmount";
        //    var feedback = ExpenseClaimMasterRepo.GetDataDictCollection(sql);
        //    return feedback;
        //}
        public IEnumerable<Dictionary<string, object>> GetDivHeadBudgetDetails(int ECMasterID)
        {
            string sql = $@" SELECT * FROM ViewGetAllDepartmentWiseClaimData 
                                    WHERE (DivHeadEmpID={AppContexts.User.EmployeeID} OR ProxyEmployeeID={AppContexts.User.EmployeeID})
                                    AND MonthName=FORMAT((SELECT top 1 ExpenseClaimDate FROM ExpenseClaimChild Where ECMasterID={ECMasterID}), 'MMMM') 
                                    AND Year=FORMAT((SELECT top 1 ExpenseClaimDate FROM ExpenseClaimChild Where ECMasterID={ECMasterID}), 'yyyy')";
            var feedback = ExpenseClaimMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public GridModel GetAllExpenseClaims(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" WHERE CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" WHERE ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" WHERE ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" WHERE AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" WHERE AP.ApprovalProcessID IN 
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
	                            ECM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable 
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,(CASE WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=22 THEN 'Payment Requested' 
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=0 THEN 'Payment Approved & Waiting for Settlement'
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,CASE WHEN ECM.ApprovalStatusID = 23 THEN  ExpenseApprovalDate ELSE NULL END ExpenseApprovalDate,FirstPaymentFeedbackRequestDate
								,ISNULL((CASE WHEN (ECM.ApprovalStatusID = 23 AND Pay.PaymentMasterID is not null) THEN (SELECT  CONVERT(VARCHAR(10), ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) / 86400 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) / 3600 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) % 3600 ) / 60 ))) ELSE NULL END),'') ClaimToPaymentTime
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                            FROM ExpenseClaimMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN (SELECT 
											EPM.ApprovalStatusID
											,EPC.IOUOrExpenseClaimID
											,EPM.IsSettlement
											,EPM.PaymentMasterID
											,FirstPaymentFeedbackRequestDate
										FROM Accounts..IOUOrExpensePaymentChild EPC
										JOIN Accounts..IOUOrExpensePaymentMaster EPM ON EPM.PaymentMasterID = EPC.PaymentMasterID
										LEFT JOIN (
										SELECT 
												Min(FeedbackRequestDate) FirstPaymentFeedbackRequestDate,ReferenceID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN(SELECT APTypeID,ReferenceID,ApprovalProcessID FROM Approval..ApprovalProcess )AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID	
											where APTypeID = 6 
											GROUP BY ReferenceID,ReferenceID
										)FF ON FF.ReferenceID = EPM.PaymentMasterID
										WHERE EPM.ApprovalStatusID <> 24 AND ClaimType = 'Expense') Pay on Pay.IOUOrExpenseClaimID=ECM.ECMasterID						    
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID ={AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.ECMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.ECMasterID
LEFT                                JOIN (SELECT DISTINCT
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
                                LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.ECMasterID
                                LEFT JOIN (
											SELECT 
												MAX(FeedbackSubmitDate) ExpenseApprovalDate,ReferenceID,AP.ApprovalProcessID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
											where APFeedbackID  = 5 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
											GROUP BY ReferenceID,AP.ApprovalProcessID
											)LFBSD ON LFBSD.ReferenceID = ECM.ECMasterID
                               {filter}
                            ";
            var result = ExpenseClaimMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetExportAllExpenseClaims(string fromDate, string toDate)
        {
            string dateFilterCondition = string.Empty;
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                // = $@" AND CAST(M.CreatedDate as date) between IIF(('{fromDate}' is null OR '{fromDate}'=''),'2020-01-01',CAST('{fromDate}' as DATE)) and IIF(('{toDate}' is null OR '{toDate}'=''),CAST(Getdate() as date),CAST('{toDate}' as DATE))";
                dateFilterCondition = $@" WHERE CAST(ECM.CreatedDate as date) between CAST('{fromDate}' as DATE) and CAST('{toDate}' as DATE)";
            }
          
            string sql = $@"SELECT 
                                DISTINCT
                                 ECM.ECMasterID
	                            ,VA.FullName AS 'Created By'	
								,VA.EmployeeCode AS 'Creator ID'
								,VA.DepartmentName AS 'Department Name'
								,ECM.ReferenceNo AS 'Reference No'
                                ,ECM.GrandTotal 
                                ,FORMAT(CONVERT(DATETIME, ECM.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                ,FORMAT(CONVERT(DATETIME, ECM.UpdatedDate), 'M/d/yy hh:mm tt') AS 'Updated Date & Time'
                                ,FORMAT(CONVERT(DATETIME, ECM.ClaimSubmitDate), 'M/d/yy hh:mm tt') AS 'Claim Submit Date & Time'
                                ,(CASE WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=22 THEN 'Payment Requested' 
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=0 THEN 'Payment Approved & Waiting for Settlement'
									   WHEN ECM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
                                  ,FORMAT(CONVERT(DATETIME, ISNULL(FeedbackLastResponseDate, CommentSubmitDate)), 'M/d/yy hh:mm tt') AS 'Action Date'

                            FROM ExpenseClaimMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN (SELECT 
											EPM.ApprovalStatusID
											,EPC.IOUOrExpenseClaimID
											,EPM.IsSettlement
											,EPM.PaymentMasterID
											,FirstPaymentFeedbackRequestDate
										FROM Accounts..IOUOrExpensePaymentChild EPC
										JOIN Accounts..IOUOrExpensePaymentMaster EPM ON EPM.PaymentMasterID = EPC.PaymentMasterID
										LEFT JOIN (
										SELECT 
												Min(FeedbackRequestDate) FirstPaymentFeedbackRequestDate,ReferenceID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN(SELECT APTypeID,ReferenceID,ApprovalProcessID FROM Approval..ApprovalProcess )AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID	
											where APTypeID = 6 
											GROUP BY ReferenceID,ReferenceID
										)FF ON FF.ReferenceID = EPM.PaymentMasterID
										WHERE EPM.ApprovalStatusID <> 24 AND ClaimType = 'Expense') Pay on Pay.IOUOrExpenseClaimID=ECM.ECMasterID						    
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.ECMasterID AND AP.APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID ={AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.ECMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.ECMasterID
LEFT                                JOIN (SELECT DISTINCT
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
                                LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.ExpenseClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.ECMasterID
                                LEFT JOIN (
											SELECT 
												MAX(FeedbackSubmitDate) ExpenseApprovalDate,ReferenceID,AP.ApprovalProcessID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
											where APFeedbackID  = 5 AND APTypeID = {(int)Util.ApprovalType.ExpenseClaim}
											GROUP BY ReferenceID,AP.ApprovalProcessID
											)LFBSD ON LFBSD.ReferenceID = ECM.ECMasterID
                               {dateFilterCondition}
                            ";
            var data = ExpenseClaimMasterRepo.GetDataDictCollection(sql);
            return await Task.FromResult(data.ToList());
        }

    }
}
