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
using Microsoft.EntityFrameworkCore;

namespace Accounts.Manager
{
    public class IOUOrExpensePaymentManager : ManagerBase, IIOUOrExpensePaymentManager
    {
        private readonly IRepository<IOUOrExpensePaymentMaster> MasterRepo;
        private readonly IRepository<IOUOrExpensePaymentChild> ChildRepo;
        private readonly IRepository<IOUMaster> IOUMasterRepo;
        private readonly IRepository<IOUChild> IOUChildRepo;
        private readonly IRepository<IOUOrExpensePaymentChild> IOUOrExpensePaymentChildRepo;
        public IOUOrExpensePaymentManager(IRepository<IOUOrExpensePaymentMaster> _masterRepo, IRepository<IOUOrExpensePaymentChild> _chilRepo, IRepository<IOUMaster> _iouMasterRepo, IRepository<IOUChild> _iouChildRepo, IRepository<IOUOrExpensePaymentChild> _iouOrExpensePaymentChildRepo)
        {
            MasterRepo = _masterRepo;
            ChildRepo = _chilRepo;
            IOUMasterRepo = _iouMasterRepo;
            IOUChildRepo = _iouChildRepo;
            IOUOrExpensePaymentChildRepo = _iouOrExpensePaymentChildRepo;
        }
        public async Task<List<ExpenseClaimFilterdData>> GetFilteredExpenseClaims(DateTime FromDate, DateTime ToDate, int DivisionID, int DepartmentID, int EmployeeID, int PaymentMasterID, string ReferenceNo)
        {
            string sql = @$"SELECT * FROM viewExpenseClaimPivot WHERE ClaimType='Expense' AND (PaymentMasterID IS NULL OR PaymentMasterID = {PaymentMasterID})";
            var data = MasterRepo.GetDataModelCollection<ExpenseClaimFilterdData>(sql)
                .Where(x => x.ClaimSubmitDate.Date >= FromDate.Date && x.ClaimSubmitDate.Date <= ToDate)
                .Where(x => DivisionID == 0 || x.DivisionID == DivisionID)
                .Where(x => DepartmentID == 0 || x.DepartmentID == DepartmentID)
                .Where(x => ReferenceNo == "undefined" || ReferenceNo == "null" || EF.Functions.Like(x.ReferenceNo, $"%{ReferenceNo}%"))
                .Where(x => EmployeeID == 0 || x.EmployeeID == EmployeeID).ToList();
            return await Task.FromResult(data);

        }

        public async Task<IOUOrExpensePaymentMasterDto> GetMaster(int PaymentMasterID)
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
                            ,AP.APTypeID
                        FROM IOUOrExpensePaymentMaster ECM
                        LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PaymentMasterID AND AP.APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment})
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.ExpensePayment}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment}) AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.PaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PaymentMasterID
                        WHERE ECM.PaymentMasterID={PaymentMasterID}";
            var expenseClaimMaster = MasterRepo.GetModelData<IOUOrExpensePaymentMasterDto>(sql);
            return expenseClaimMaster;
        }

        public async Task<List<ExpenseClaimFilterdData>> GetChildList(int PaymentMasterID)
        {
            string sql = $@"SELECT 
	                                M.ECMasterID
	                                ,ECM.ReferenceNo 
	                                ,Emp.EmployeeCode
	                                ,FullName
	                                ,DepartmentName
	                                ,DivisionName
	                                ,FoodAndEntertainment
	                                ,Conveyance
	                                ,Stationeries
	                                ,PettyCashReimbursement
	                                ,ForeignTravel
	                                ,LocalTravel
                                    ,TransferAllowance
	                                ,FoodAndEntertainment+Conveyance+ Stationeries+PettyCashreimbursement+ForeignTravel+LocalTravel+TransferAllowance TotalAmount
	                                ,ECM.ClaimSubmitDate
	                                ,Emp.DepartmentID
	                                ,DivisionID
	                                ,Emp.EmployeeID
	                                ,emp.ImagePath
	                                ,IOUM.IOUMasterID
	                                ,IOUM.ReferenceNo IOUReferenceNo
	                                ,IOUM.GrandTotal IOUAmount	
	                                ,AccountPayableAmountToEmployee = ISNULL(ECM.GrandTotal, 0) - ISNULL(IOUM.GrandTotal, 0)
                           FROM (
                           	SELECT ECMasterID
                           		,ISNULL([Food & Entertainment], 0) [FoodAndEntertainment]
                           		,ISNULL([Conveyance], 0) [Conveyance]
                           		,ISNULL([Stationeries], 0) [Stationeries]
                           		,ISNULL([Petty Cash Reimbursement], 0) [PettyCashReimbursement]
                           		,ISNULL([Foreign Travel], 0) [ForeignTravel]
                           		,ISNULL([Local Travel], 0) [LocalTravel]
                           		,ISNULL([TransferAllowance], 0) [TransferAllowance]
                           	FROM (
                           		SELECT ECMasterID
                           			,Prps.SystemVariableCode Purpose
                           			,ISNULL(ExpenseClaimAmount, 0) ExpenseClaimAmount
                           		FROM ExpenseClaimChild ECC
                           		INNER JOIN Security..SystemVariable Prps ON Prps.SystemVariableID = ECC.PurposeID
                           			AND EntityTypeID = 21
                           		) A
                           	PIVOT(SUM(ExpenseClaimAmount) FOR Purpose IN (
                           				[Food & Entertainment]
                           				,[Conveyance]
                           				,[Stationeries]
                           				,[Petty Cash reimbursement]
                           				,[Foreign Travel]
                           				,[Local Travel]
                           				,[TransferAllowance]
                           				)) PIV
                           	) M
                        INNER JOIN IOUOrExpensePaymentChild IEPC ON M.ECMasterID = IEPC.IOUOrExpenseClaimID 
                        INNER JOIN ExpenseClaimMaster ECM ON M.ECMasterID = ECm.ECMasterID
                        INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = ECM.EmployeeID
                        LEFT JOIN IOUMaster IOUM ON IOUM.IOUMasterID = ECM.IOUMasterID
                        JOIN Accounts..IOUOrExpensePaymentMaster EPM on EPM.PaymentMasterID=IEPC.PaymentMasterID --and EPM.ApprovalStatusID <>24 
                        WHERE IEPC.PaymentMasterID={PaymentMasterID}";

            var expenseChilds = ChildRepo.GetDataModelCollection<ExpenseClaimFilterdData>(sql);
            return expenseChilds;
        }

        private List<IOUOrExpensePaymentChild> GenerateChild(IOUOrExpensePaymentDto PO)
        {
            var existingPurchaseOrderChild = IOUOrExpensePaymentChildRepo.Entities.Where(x => x.PaymentMasterID == PO.PaymentMasterID).ToList();
            var childModel = new List<IOUOrExpensePaymentChild>();
            if (PO.Details.IsNotNull())
            {
                PO.Details.ForEach(x =>
                {
                    childModel.Add(new IOUOrExpensePaymentChild
                    {
                        PaymentChildID = x.PaymentChildID
                        ,
                        PaymentMasterID = x.PaymentMasterID
                        ,
                        EmployeeID = x.EmployeeID
                        ,
                        DepartmentID = x.DepartmentID
                        ,
                        IOUOrExpenseClaimID = x.ECMasterID,
                        GLID = x.GLID
                        ,
                        ApprovedAmount = x.AccountPayableAmountToEmployee
                        ,
                        TotalAmount = x.TotalAmount
                        ,
                        ReceivingDate = DateTime.Today,
                        PostingDate = DateTime.Today
                        ,
                        PaymentStatus = x.PaymentStatus
                        ,
                        CompanyID = x.CompanyID
                        ,
                        CreatedBy = x.CreatedBy
                        ,
                        CreatedDate = x.CreatedDate
                        ,
                        CreatedIP = x.CreatedIP
                        ,
                        UpdatedBy = x.UpdatedBy
                        ,
                        UpdatedDate = x.UpdatedDate
                        ,
                        UpdatedIP = x.UpdatedIP
                        ,
                        RowVersion = x.RowVersion
                    });

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseOrderChild.Count > 0 && x.PaymentChildID > 0)
                    {
                        var existingModelData = existingPurchaseOrderChild.FirstOrDefault(y => y.PaymentChildID == x.PaymentChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PaymentMasterID = PO.PaymentMasterID;
                        x.SetAdded();
                        SetIOUOrExpensePaymentChildNewId(x);
                    }
                });

                var willDeleted = existingPurchaseOrderChild.Where(x => !childModel.Select(y => y.PaymentChildID).Contains(x.PaymentChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        public async Task<(bool, string)> SaveChanges(IOUOrExpensePaymentDto expenseDto)
        {
            var existingExpense = MasterRepo.Entities.Where(x => x.PaymentMasterID == expenseDto.PaymentMasterID).SingleOrDefault();

            //var validate = CheckApprovalValidation(expenseDto.PaymentMasterID, expenseDto.ApprovalProcessID, (int)Util.ApprovalType.ExpensePayment);

            //if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense sattlement once it processed from approval panel");

            //var approvalProcessFeedBack = validate.Item2;



            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (expenseDto.PaymentMasterID > 0 && expenseDto.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM Accounts..IOUOrExpensePaymentMaster PurchaseRequisition
                            LEFT JOIN Security..Users U ON U.UserID = PurchaseRequisition.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PurchaseRequisition.PaymentMasterID AND AP.APTypeID =  {(int)Util.ApprovalType.ExpensePayment}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.ExpensePayment} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.ExpensePayment} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PurchaseRequisition.PaymentMasterID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {expenseDto.ApprovalProcessID}";
                var canReassesstment = IOUMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit IOU or Expense Payment once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit IOU or Expense Payment once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)expenseDto.PaymentMasterID, expenseDto.ApprovalProcessID, (int)Util.ApprovalType.ExpensePayment);
            }


            string ApprovalProcessID = "0";
            bool ExpensePayment = false;
            bool IsAutoApproved = false;
            bool IsResubmitted = false;

            var masterModel = new IOUOrExpensePaymentMaster
            {
                ApprovalStatusID = (int)ApprovalStatus.Pending,
                PaymentDate = expenseDto.PaymentDate,
                ReferenceKeyword = expenseDto.ReferenceKeyword,
                GrandTotal = (decimal)expenseDto.Details.Select(x => x.TotalAmount).DefaultIfEmpty(0).Sum(),
                ClaimType = Util.ClaimSattlementType.Expense.ToString(),
                IsException = expenseDto.IsException
            };

            using (var unitOfWork = new UnitOfWork())
            {

                if (expenseDto.PaymentMasterID.IsZero() && existingExpense.IsNull())
                {

                    masterModel.ReferenceNo = GenerateExpenseOrIOUClaimReference("PSEXP") + (string.IsNullOrWhiteSpace(expenseDto.ReferenceKeyword) ? "" : $"/{expenseDto.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetIOUOrExpensePaymentMasterNewId(masterModel);
                    expenseDto.PaymentMasterID = (int)masterModel.PaymentMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingExpense.CreatedBy;
                    masterModel.CreatedDate = existingExpense.CreatedDate;
                    masterModel.CreatedIP = existingExpense.CreatedIP;
                    masterModel.RowVersion = existingExpense.RowVersion;
                    masterModel.PaymentMasterID = expenseDto.PaymentMasterID;
                    masterModel.ReferenceNo = existingExpense.ReferenceNo;
                    masterModel.SetModified();
                }


                //var childModel = expenseDto.Details.Select(x => new IOUOrExpensePaymentChild
                //{
                //    PaymentMasterID = expenseDto.PaymentMasterID,
                //    EmployeeID = x.EmployeeID,
                //    DepartmentID = x.DepartmentID,
                //    IOUOrExpenseClaimID = x.ECMasterID,
                //    ApprovedAmount = masterModel.GrandTotal,
                //    PostingDate = DateTime.Today,
                //    ReceivingDate = DateTime.Today,
                //    TotalAmount = masterModel.GrandTotal
                //}).ToList();

                //childModel.ForEach(x =>
                //{
                //    x.SetAdded();
                //    SetIOUOrExpensePaymentChildNewId(x);
                //});
                var childModel = GenerateChild(expenseDto);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                MasterRepo.Add(masterModel);
                ChildRepo.AddRange(childModel);

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.ExpenseSattlementApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Expense Settlement  Reference No:{masterModel.ReferenceNo}";
                    var returnObj = CreateApprovalProcessForLimit((int)masterModel.PaymentMasterID, Util.AutoExpenseSattlementDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.ExpensePayment, masterModel.GrandTotal, expenseDto.PaymentDate.Month.ToString(), masterModel.IsException);
                    ApprovalProcessID = returnObj.ApprovalProcessID;
                    IsAutoApproved = returnObj.IsAutoApproved;
                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            $"{Util.ExpenseSattlementApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Expense Sattlement Reference No:{masterModel.ReferenceNo}");
                        UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                            (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                            $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                            (int)approvalProcessFeedBack["APTypeID"],
                            (int)approvalProcessFeedBack["ReferenceID"], 0);
                        ExpensePayment = true;
                        ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (IsAutoApproved)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PaymentMasterID, (int)Util.ApprovalType.ExpensePayment);
                }

                if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                    //    //    await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PaymentMasterID, (int)Util.MailGroupSetup.ExpensePaymentInitiatedMail, (int)Util.ApprovalType.ExpensePayment);
            }

            await Task.CompletedTask;


            return (true, $"Expense Payment Submitted Successfully");
        }

        private string GenerateExpenseOrIOUClaimReference(string intial)
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{intial}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("ExpenseOrIOUPaymentSattlementReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        private void SetIOUOrExpensePaymentMasterNewId(IOUOrExpensePaymentMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("IOUOrExpensePaymentMaster", AppContexts.User.CompanyID);
            master.PaymentMasterID = code.MaxNumber;
        }

        private void SetIOUOrExpensePaymentChildNewId(IOUOrExpensePaymentChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("IOUOrExpensePaymentChild", AppContexts.User.CompanyID);
            child.PaymentChildID = code.MaxNumber;
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(long PaymentMasterID, int ApprovalProcessID, int ApprovalType)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (PaymentMasterID > 0 && ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM IOUOrExpensePaymentMaster IEM
                                 LEFT JOIN Security..Users U ON U.UserID = IEM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PaymentMasterID AND AP.APTypeID = {ApprovalType}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {ApprovalType} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {ApprovalType} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = ECM.PaymentMasterID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {ApprovalProcessID}";
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)PaymentMasterID, ApprovalProcessID, ApprovalType);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }
        public GridModel GetMasterList(GridParameter parameters)
        //public List<IOUOrExpensePaymentMasterDto> GetMasterList(string filterData)
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
            string sql = $@"SELECT DISTINCT
	                            ECM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,(CASE WHEN ECM.ApprovalStatusID=23 AND isnull(ECM.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable                                
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,AP.APTypeID
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CONVERT(nvarchar(30),Cast(PaymentDate as Date), 113)+' '+ECM.ClaimType+' '+Cast(ECM.GrandTotal as nvarchar(20)) PaymentInfo
								,ISNULL(CONVERT(nvarchar(30),Cast(SettlementDate as Date), 113)+' '+ECM.SettlementRefNo,'') SettlementInfo
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,'' PaymentApprovalToSettlementTime
                            FROM IOUOrExpensePaymentMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PaymentMasterID AND AP.APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment})
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment})
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment}) AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.PaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PaymentMasterID
                            LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment}) AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.PaymentMasterID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public GridModel GetMasterApprovedHistoryList(GridParameter parameters)
        {
            string filter = $@" ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
            string sql = $@"SELECT DISTINCT
	                            ECM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,(CASE WHEN ECM.ApprovalStatusID=23 AND isnull(ECM.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable                                
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,AP.APTypeID
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CONVERT(nvarchar(30),Cast(PaymentDate as Date), 113)+' '+ECM.ClaimType+' '+Cast(ECM.GrandTotal as nvarchar(20)) PaymentInfo
								,ISNULL(CONVERT(nvarchar(30),Cast(SettlementDate as Date), 113)+' '+ECM.SettlementRefNo,'') SettlementInfo
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,'' PaymentApprovalToSettlementTime
                            FROM IOUOrExpensePaymentMaster ECM
                            LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ECM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ECM.PaymentMasterID AND AP.APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment})
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment})
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment}) AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ECM.PaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ECM.PaymentMasterID
                            LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID IN ({(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.IOUPayment}) AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ECM.PaymentMasterID
							WHERE {filter}
                            ";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            string APTypeIDs = $@"{(int)Util.ApprovalType.ExpensePayment},{(int)Util.ApprovalType.ExpensePayment}";
            var comments = GetApprovalComments(approvalProcessID, APTypeIDs);
            return comments.Result;
        }

        public IEnumerable<Dictionary<string, object>> ExpensePaymentApprovalFeedback(int TVMID)
        {
            string sql = $@" EXEC Accounts..spRPTExpensePaymentApprovalFeedback {TVMID}";
            var feedback = ChildRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> IOUPaymentApprovalFeedback(int TVMID)
        {
            string sql = $@" EXEC Accounts..spRPTIOUPaymentApprovalFeedback {TVMID}";
            var feedback = ChildRepo.GetDataDictCollection(sql);
            return feedback;
        }
        private List<Attachments> GetAttachments(int PaymentChildID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='IOUOrExpensePaymentChild' AND ReferenceID={PaymentChildID}";
            var attachment = MasterRepo.GetDataDictCollection(attachmentSql);
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
            var comments = ChildRepo.GetDataDictCollection(sql);
            return comments;
        }
        public async Task<List<IOUClaimFilterdData>> GetFilteredIOUClaims(DateTime FromDate, DateTime ToDate, int DivisionID, int DepartmentID, int EmployeeID, int PaymentMasterID)
        {
            string sql = @$"SELECT * FROM viewIOUClaim WHERE ClaimType='IOU' AND (PaymentMasterID IS NULL OR PaymentApprovalStatusID = {(int)Util.ApprovalStatus.Rejected} OR PaymentMasterID = {PaymentMasterID})";
            var data = MasterRepo.GetDataModelCollection<IOUClaimFilterdData>(sql)
           .Where(x => x.SettlementDate.Date >= FromDate.Date && x.SettlementDate.Date <= ToDate)
           .Where(x => DivisionID == 0 || x.DivisionID == DivisionID)
           .Where(x => DepartmentID == 0 || x.DepartmentID == DepartmentID)
           .Where(x => EmployeeID == 0 || x.EmployeeID == EmployeeID).ToList();
            return await Task.FromResult(data);
            //else
            //{
            //    string sql = @$"SELECT * FROM viewIOUClaim WHERE IOUApprovalStatusID = 23 AND (PaymentMasterID IS NULL OR PaymentApprovalStatusID = 24)
            //                    UNION  
            //                    SELECT * FROM viewIOUClaim WHERE IOUApprovalStatusID = 23 AND (PaymentMasterID IS NOT NULL AND PaymentApprovalStatusID <> 24) AND ClaimType = 'IOU' AND PaymentMasterID = {PaymentMasterID}";
            //    var data = MasterRepo.GetDataModelCollection<IOUClaimFilterdData>(sql)
            //   .Where(x => x.SettlementDate.Date >= FromDate.Date && x.SettlementDate.Date <= ToDate)
            //   .Where(x => DivisionID == 0 || x.DivisionID == DivisionID)
            //   .Where(x => DepartmentID == 0 || x.DepartmentID == DepartmentID)
            //   .Where(x => EmployeeID == 0 || x.EmployeeID == EmployeeID).ToList();
            //    return await Task.FromResult(data);
            //}
        }

        public async Task<(bool, string)> SaveIOUChanges(IOUPaymentDto iouDto)
        {
            var existing = MasterRepo.Entities.Where(x => x.PaymentMasterID == iouDto.PaymentMasterID).SingleOrDefault();

            //var validate = CheckApprovalValidation(iouDto.PaymentMasterID, iouDto.ApprovalProcessID, (int)Util.ApprovalType.IOUPayment);

            //if (validate.Item1 == false) return (false, $"Sorry, You can not edit IOU sattlement once it processed from approval panel");

            //var approvalProcessFeedBack = validate.Item2;
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (iouDto.PaymentMasterID > 0 && iouDto.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM Accounts..IOUOrExpensePaymentMaster PurchaseRequisition
                            LEFT JOIN Security..Users U ON U.UserID = PurchaseRequisition.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PurchaseRequisition.PaymentMasterID AND AP.APTypeID =  {(int)Util.ApprovalType.IOUPayment}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.IOUPayment} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.IOUPayment} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PurchaseRequisition.PaymentMasterID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {iouDto.ApprovalProcessID}";
                var canReassesstment = IOUMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit IOU or Expense Payment once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit IOU or Expense Payment once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)iouDto.PaymentMasterID, iouDto.ApprovalProcessID, (int)Util.ApprovalType.IOUPayment);
            }
            string ApprovalProcessID = "0";
            bool IOUPayment = false;
            bool IsResubmitted = false;
            bool IsAutoApproved = false;

            var masterModel = new IOUOrExpensePaymentMaster
            {

                PaymentDate = iouDto.PaymentDate,
                ReferenceKeyword = iouDto.ReferenceKeyword,
                GrandTotal = (decimal)iouDto.Details.Select(x => x.GrandTotal).DefaultIfEmpty(0).Sum(),
                ClaimType = Util.ClaimSattlementType.IOU.ToString()
            };

            using (var unitOfWork = new UnitOfWork())
            {

                if (iouDto.PaymentMasterID.IsZero() && existing.IsNull())
                {
                    masterModel.ReferenceNo = GenerateExpenseOrIOUClaimReference("PSIOU") + (string.IsNullOrWhiteSpace(iouDto.ReferenceKeyword) ? "" : $"/{iouDto.ReferenceKeyword.ToUpper()}");
                    masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetIOUOrExpensePaymentMasterNewId(masterModel);

                }
                else
                {
                    masterModel.CreatedBy = existing.CreatedBy;
                    masterModel.CreatedDate = existing.CreatedDate;
                    masterModel.CreatedIP = existing.CreatedIP;
                    masterModel.RowVersion = existing.RowVersion;
                    masterModel.PaymentMasterID = iouDto.PaymentMasterID;
                    masterModel.ReferenceNo = existing.ReferenceNo;
                    masterModel.ApprovalStatusID = existing.ApprovalStatusID;
                    masterModel.SetModified();
                }
                iouDto.PaymentMasterID = masterModel.PaymentMasterID;
                iouDto.GrandTotal = masterModel.GrandTotal;
                var childModel = GenerateIOUPaymentChildModel(iouDto);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                MasterRepo.Add(masterModel);
                ChildRepo.AddRange(childModel);

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.IOUSattlementApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, IOU Settlement  Reference No:{masterModel.ReferenceNo}";
                    var returnObj = CreateApprovalProcessForLimit((int)masterModel.PaymentMasterID, Util.AutoIOUSattlementDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.IOUPayment, masterModel.GrandTotal, masterModel.PaymentDate.Month.ToString());
                    ApprovalProcessID = returnObj.ApprovalProcessID;
                    IsAutoApproved = returnObj.IsAutoApproved;
                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            $"{Util.IOUSattlementApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, IOU Sattlement Reference No:{masterModel.ReferenceNo}");
                        UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                            (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                            $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                            (int)approvalProcessFeedBack["APTypeID"],
                            (int)approvalProcessFeedBack["ReferenceID"], 0);
                        IOUPayment = true;
                        ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (IsAutoApproved)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PaymentMasterID, (int)Util.ApprovalType.IOUPayment);
                }

                if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                    //    await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PaymentMasterID, (int)Util.MailGroupSetup.IOUPaymentInitiatedMail, (int)Util.ApprovalType.IOUPayment);
            }

            await Task.CompletedTask;


            return (true, $"IOU Settlement  Submitted Successfully");
        }
        public async Task<List<IOUClaimFilterdData>> GetChildIOUList(int PaymentMasterID)
        {
            string sql = $@"SELECT 
                                Payment.PaymentChildID
								,PM.ReferenceNo
                            	,M.IOUMasterID
                            	,M.ReferenceNo
                            	,M.SettlementDate
                            	,Emp.EmployeeCode
                            	,FullName
                            	,DepartmentName
                            	,DivisionName
                            	,Emp.DepartmentID
                            	,DivisionID
                            	,Emp.EmployeeID
                            	,emp.ImagePath
                            	,M.GrandTotal
                            FROM 
                            IOUMaster M
                            INNER JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = M.EmployeeID
                            INNER JOIN IOUOrExpensePaymentChild Payment ON Payment.IOUOrExpenseClaimID = M.IOUMasterID 
                            INNER JOIN IOUOrExpensePaymentMaster PM ON PM.PaymentMasterID = Payment.PaymentMasterID  
                            WHERE PM.PaymentMasterID={PaymentMasterID}";

            var iouChilds = ChildRepo.GetDataModelCollection<IOUClaimFilterdData>(sql);
            return iouChilds;
        }
        public IEnumerable<Dictionary<string, object>> GetIOUApprovalComment(int approvalProcessID)
        {
            string APTypeIDs = $@"{(int)Util.ApprovalType.IOUPayment},{(int)Util.ApprovalType.IOUPayment}";
            var comments = GetApprovalComments(approvalProcessID, APTypeIDs);
            return comments.Result;
        }

        public async Task<(bool, string)> CreateIOUOrExpPaymentSettlement(IOUOrExpensePaymentDto dto)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var settlePayment = MasterRepo.Entities.Where(x => x.PaymentMasterID == dto.PaymentMasterID).FirstOrDefault();

                settlePayment.IsSettlement = true;
                settlePayment.SettlementDate = dto.SettlementDate.AddHours(6);
                settlePayment.SettledBy = AppContexts.User.UserID;
                settlePayment.SettlementRefNo = dto.SettlementRefNo;
                settlePayment.SetModified();
                SetAuditFields(settlePayment);
                MasterRepo.Add(settlePayment);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
            return (true, $"IOUOrExpense Settlement Submitted Successfully"); ;
        }

        private List<IOUOrExpensePaymentChild> GenerateIOUPaymentChildModel(IOUPaymentDto iouDto)
        {
            var childList = new List<IOUOrExpensePaymentChild>();
            if (iouDto.Details.IsNotNull())
            {
                childList = iouDto.Details.Select(x => new IOUOrExpensePaymentChild
                {
                    PaymentChildID = x.PaymentChildID,
                    PaymentMasterID = iouDto.PaymentMasterID,
                    EmployeeID = x.EmployeeID,
                    DepartmentID = x.DepartmentID,
                    IOUOrExpenseClaimID = x.IOUMasterID,
                    ApprovedAmount = iouDto.GrandTotal,
                    PostingDate = DateTime.Today,
                    ReceivingDate = DateTime.Today,
                    TotalAmount = iouDto.GrandTotal
                }).ToList();
                var existsInfo = ChildRepo.Entities.Where(x => x.PaymentMasterID == iouDto.PaymentMasterID).ToList();

                childList.ForEach(x =>
                {
                    if (existsInfo.Count > 0 && x.PaymentChildID > 0)
                    {
                        var existingModelData = existsInfo.FirstOrDefault(y => y.PaymentChildID == x.PaymentChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PaymentMasterID = iouDto.PaymentMasterID;
                        x.SetAdded();
                        SetIOUOrExpensePaymentChildNewId(x);
                    }
                });

                var willDeleted = existsInfo.Where(x => !childList.Select(y => y.PaymentChildID).Contains(x.PaymentChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childList.Add(x);
                });

            }

            return childList;
        }

        #region Mail

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long PaymentMasterID, int mailGroup, int APTypeID)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)PaymentMasterID, 0);
        }
        #endregion

    }
}
