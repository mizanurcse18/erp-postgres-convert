using SCM.DAL.Entities;
using SCM.Manager.Dto;
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
using SCM.Manager.Interfaces;

namespace SCM.Manager.Implementations
{
    public class InvoicePaymentManager : ManagerBase, IInvoicePaymentManager
    {
        private readonly IRepository<InvoicePaymentMaster> MasterRepo;
        private readonly IRepository<InvoicePaymentChild> ChildRepo;
        private readonly IRepository<InvoicePaymentMethod> PaymentRepo;
        public InvoicePaymentManager(IRepository<InvoicePaymentMaster> _masterRepo, IRepository<InvoicePaymentChild> _chilRepo, IRepository<InvoicePaymentMethod> _paymentRepo)
        {
            MasterRepo = _masterRepo;
            ChildRepo = _chilRepo;
            PaymentRepo = _paymentRepo;

        }
        public async Task<List<InvoiceFilteredData>> GetFilteredInvoiceList(DateTime FromDate, DateTime ToDate, string SupplierID, int InvoiceMasterID, int IPaymentMasterID, int paymentTypeId)
        {
            if (SupplierID == "undefined" || SupplierID=="false")
            {
                string sql = @$"SELECT *
                            FROM viewGetPendingInvoiceForPayment WHERE (IPMApprovalStatusID={(int)Util.ApprovalStatus.Rejected}  or IPaymentMasterID is null OR IPaymentMasterID = {IPaymentMasterID})";
                var data = MasterRepo.GetDataModelCollection<InvoiceFilteredData>(sql)
                       .Where(x => x.InvoiceDate.Date >= FromDate.Date && x.InvoiceDate.Date <= ToDate)
                       .Where(x => InvoiceMasterID == 0 || x.InvoiceMasterID == InvoiceMasterID).ToList();
                return await Task.FromResult(data);
            }
            else
            {
                //SupplierID = SupplierID.Substring(1, SupplierID.Length - 2);
                string sql = @$"SELECT *
                            FROM viewGetPendingInvoiceForPayment WHERE (IPMApprovalStatusID={(int)Util.ApprovalStatus.Rejected}  or IPaymentMasterID is null OR IPaymentMasterID = {IPaymentMasterID}) AND SupplierID in ({SupplierID})";
                var data = MasterRepo.GetDataModelCollection<InvoiceFilteredData>(sql)
                    .Where(x => x.InvoiceDate.Date >= FromDate.Date && x.InvoiceDate.Date <= ToDate)
                    .Where(x => InvoiceMasterID == 0 || x.InvoiceMasterID == InvoiceMasterID).ToList();
                return await Task.FromResult(data);
            }

            //var data = MasterRepo.GetDataModelCollection<InvoiceFilteredData>(sql)
            //    .Where(x => x.InvoiceDate.Date >= FromDate.Date && x.InvoiceDate.Date <= ToDate)
            //    //.Where(x => SupplierID == 0 || x.SupplierID == SupplierID)
            //    //.Where(x => paymentTypeId == 0 || x.PaymentModeID == paymentTypeId)
            //    .Where(x => InvoiceMasterID == 0 || x.InvoiceMasterID == InvoiceMasterID).ToList();
            //return await Task.FromResult(data);

            //if (FromDate != DateTime.MinValue)
            //{
            //    data = data.Where(x => x.InvoiceDate.Date >= FromDate && x.InvoiceDate.Date <= ToDate).ToList();
            //}
        }

        public async Task<List<InvoiceFilteredData>> GetFilteredInvoicePaymentList(int SupplierID, int InvoiceMasterID, int IPaymentMasterID, int TVPID)
        {
            string sql = @$"SELECT *
                            FROM viewGetAllInvoicePaymentDataForVettingPayment WHERE (TVPApprovalStatusID={(int)Util.ApprovalStatus.Rejected}  or TVPID is null OR TVPID = {TVPID})                             ";
            var data = MasterRepo.GetDataModelCollection<InvoiceFilteredData>(sql)
                .Where(x => SupplierID == 0 || x.SupplierID == SupplierID)
                .Where(x => TVPID == 0 || x.TVPID == TVPID)
                .Where(x => InvoiceMasterID == 0 || x.InvoiceMasterID == InvoiceMasterID).ToList();
            return await Task.FromResult(data);
        }

        public async Task<InvoicePaymentMasterDto> GetMaster(int IPaymentMasterID)
        {
            string sql = $@"SELECT IPM.*, 
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
                            ,WorkMobile
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,(SELECT Security.dbo.NumericToBDT(IM.TotalPayableAmount)) AmountInWords
                        FROM --Accounts..TaxationVettingPayment TVP
						 Accounts..TaxationVettingMaster TVM --ON TVP.TVMID=TVM.TVMID
						JOIN SCM..InvoiceMaster IM ON IM.InvoiceMasterID=TVM.InvoiceMasterID
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = IM.POMasterID
						LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						LEFT JOIN SCM..InvoicePaymentChild IPC ON IPC.TVMID=TVM.TVMID
						LEFT JOIN InvoicePaymentMaster IPM ON IPM.IPaymentMasterID = IPC.IPaymentMasterID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IPM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = IPM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = IPM.IPaymentMasterID AND AP.APTypeID IN ({(int)ApprovalType.InvoicePayment})
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)ApprovalType.InvoicePayment}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID IN ({(int)ApprovalType.InvoicePayment}) AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IPM.IPaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IPM.IPaymentMasterID
                        WHERE IPM.IPaymentMasterID={IPaymentMasterID}";
            var paymentMaster = MasterRepo.GetModelData<InvoicePaymentMasterDto>(sql);
            return paymentMaster;
        }
        public async Task<List<PaymentMethodsDetailsDto>> GetPaymentMethodDetails(int IPaymentMasterID)
        {
            string sql = $@"SELECT PM.*
	                                ,b.BankName
	                                ,CONCAT (
		                                cb.BankName
		                                ,'-'
		                                ,c.BranchName
		                                ,'-'
		                                ,c.AccountNo
		                                ) CBDetails
	                                ,CONCAT (
		                                Cb.BankName
		                                ,'-'
		                                ,c.BranchName
		                                ,'-'
		                                ,c.AccountNo
		                                ) label
	                                ,PM.ChequeBookID value
	                                ,s.SupplierName
                                    ,s.BankName SupplierBank
	                                ,s.BankAccountName SupplierBankAccName
	                                ,s.BankAccountNumber SupplierAccNumber
	                                ,s.BankBranch SuplierBranch
	                                ,s.BINNumber SupplierBINNo
	                                ,s.RoutingNumber SupplierRoutingNo
	                                ,s.SwiftCode SupplierSwiftCode
                                FROM SCM..InvoicePaymentMethod PM
                                LEFT JOIN Accounts..Bank b ON PM.BankID = b.BankID
                                LEFT JOIN Accounts..ChequeBook c ON PM.ChequeBookID = c.CBID
                                LEFT JOIN Accounts..Bank cb ON cb.BankID = c.BankID
                                LEFT JOIN SCM..Supplier s ON s.SupplierID = PM.SupplierID
                        WHERE PM.IPaymentMasterID={IPaymentMasterID}";
            var paymentMaster = PaymentRepo.GetDataModelCollection<PaymentMethodsDetailsDto>(sql);
            paymentMaster.ForEach(x => x.Attachments = GetAttachments((int)x.PaymentMethodID));
            return await Task.FromResult(paymentMaster);
        }

        public GridModel GetListForGrid(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
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
                    filter = $@" AND IPM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND IPM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                    //case "Bank Payment":
                    //    filter = $@" AND IPM.PaidBy = {(int)Util.PaymentType.Bank}";
                    //    break;
                    //case "E-Money":
                    //    filter = $@" AND IPM.PaidBy = {(int)Util.PaymentType.EMoney}";
                    //    break;
                    //default:
                    //    break;
            }
            switch (parameters.AdditionalFilterData)
            {
                case "Bank Payment":
                    filter2 = $@" AND IPM.PaidBy = {(int)Util.PaymentType.Bank}";
                    break;
                case "E-Money Payment":
                    filter2 = $@" AND IPM.PaidBy = {(int)Util.PaymentType.EMoney}";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
                                DISTINCT
	                            IPM.*,
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
                                --,CASE WHEN ApprovalStatusID = 24 THEN CAST(1 as bit) WHEN EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN IPM.PaidBy=1 THEN 'Bank Payment' Else 'E-Money Payment' END AS PaymentType
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM InvoicePaymentMaster IPM
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IPM.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = IPM.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = IPM.IPaymentMasterID AND AP.APTypeID = {(int)Util.ApprovalType.InvoicePayment}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.InvoicePayment} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.InvoicePayment} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IPM.IPaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IPM.IPaymentMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.InvoicePayment} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = IPM.IPaymentMasterID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID} ) {filter} {filter2}
                            ";
            var result = MasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<List<InvoiceFilteredData>> GetChildList(int IPaymentMasterID)
        {
            string sql = $@"SELECT * FROM viewGetInvoicePaymentChild
                        WHERE IPaymentMasterID={IPaymentMasterID}";

            var childs = ChildRepo.GetDataModelCollection<InvoiceFilteredData>(sql);
            return childs;
        }

        public async Task<(bool, string)> SaveChanges(InvoicePaymentDto invoiceDto)
        {
            var existingMaster = MasterRepo.Get(invoiceDto.IPaymentMasterID);

            //var existingPaymentMethod = PaymentRepo.Entities.Where(x => x.IPaymentMasterID == invoiceDto.IPaymentMasterID).SingleOrDefault();

            var validate = CheckApprovalValidation(invoiceDto.IPaymentMasterID, invoiceDto.ApprovalProcessID, (int)ApprovalType.InvoicePayment);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit invoiceDto sattlement once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool InvoicePayment = false;
            bool IsAutoApproved = false;
            bool IsResubmitted = false;

            using (var unitOfWork = new UnitOfWork())
            {
                var masterModel = new InvoicePaymentMaster
                {
                    IPaymentMasterID = invoiceDto.IPaymentMasterID,
                    ReferenceNo = GenerateMasterReference("INV/PAYMENT") + (string.IsNullOrWhiteSpace(invoiceDto.ReferenceKeyword) ? "" : $"/{invoiceDto.ReferenceKeyword.ToUpper()}"),
                    //ApprovalStatusID = (int)ApprovalStatus.Pending,
                    PaymentDate = invoiceDto.PaymentDate,
                    PaidBy = invoiceDto.PaidBy,
                    ReferenceKeyword = invoiceDto.ReferenceKeyword,
                    GrandTotal = invoiceDto.Details.Select(x => x.TotalPayableAmount).DefaultIfEmpty(0).Sum(),
                    IsException = invoiceDto.IsException,
                    IsDraft = invoiceDto.IsDraft,
                    ApprovalStatusID = invoiceDto.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                };
                if (invoiceDto.IPaymentMasterID.IsZero() && existingMaster.IsNull())
                {
                    masterModel.SetAdded();
                    SetInvoicePaymentMasterNewId(masterModel);
                    invoiceDto.IPaymentMasterID = masterModel.IPaymentMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingMaster.CreatedBy;
                    masterModel.CreatedDate = existingMaster.CreatedDate;
                    masterModel.CreatedIP = existingMaster.CreatedIP;
                    masterModel.RowVersion = existingMaster.RowVersion;
                    masterModel.IPaymentMasterID = existingMaster.IPaymentMasterID;
                    masterModel.ReferenceNo = existingMaster.ReferenceNo;
                    masterModel.SetModified();
                }
                var childModel = GenerateInvoicePaymentChild(invoiceDto);

                var paymentMethodModel = GeneratePaymentMethodDto(invoiceDto).MapTo<List<InvoicePaymentMethod>>();

                

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                if (paymentMethodModel.IsNotNull()) SetAuditFields(paymentMethodModel);
                RemoveAttachments(invoiceDto);
                AddAttachments(invoiceDto);
                



                MasterRepo.Add(masterModel);
                ChildRepo.AddRange(childModel);
                if (paymentMethodModel.IsNotNull()) PaymentRepo.AddRange(paymentMethodModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingMaster.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{InvoicePaymentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Invoice PaymentNo:{masterModel.ReferenceNo}";
                        var returnObj = CreateApprovalProcessForLimit((int)masterModel.IPaymentMasterID, AutoInvoicePaymentDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)ApprovalType.InvoicePayment, masterModel.GrandTotal, invoiceDto.PaymentDate.Month.ToString(), masterModel.IsException);
                        ApprovalProcessID = returnObj.ApprovalProcessID;
                        IsAutoApproved = returnObj.IsAutoApproved;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                                $"{InvoicePaymentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Invoice Payment No:{masterModel.ReferenceNo}");
                            UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)ApprovalFeedback.Approved,
                                $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);
                            InvoicePayment = true;
                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }

                UpdateCBCModel(paymentMethodModel);

                unitOfWork.CommitChangesWithAudit();

                if (IsAutoApproved && !masterModel.IsDraft)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.IPaymentMasterID, (int)ApprovalType.InvoicePayment);
                }

                if (!masterModel.IsDraft && ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        await SendMailFromManagerBase(ApprovalProcessID, IsResubmitted, masterModel.IPaymentMasterID, (int)Util.MailGroupSetup.InvoicePaymentInitiatedMail, (int)ApprovalType.InvoicePayment);
                }
            }

            await Task.CompletedTask;


            return (true, $"Invoice Payment Submitted Successfully");
        }

        private void UpdateCBCModel(List<InvoicePaymentMethod> paymentMethodModel)
        {

            foreach (InvoicePaymentMethod payMethod in paymentMethodModel)
            {
                ChequeBookLeafUpdate(Convert.ToInt32(payMethod.ChequeBookID), payMethod.LeafNo);
            }
        }

        private List<PaymentMethodsDetailsDto> GeneratePaymentMethodDto(InvoicePaymentDto invoiceDto)
        {
            var invoicePaymentMethodChild = PaymentRepo.GetAllList(x => x.IPaymentMasterID == invoiceDto.IPaymentMasterID);
            if (invoiceDto.PaymentDetails.IsNotNull())
            {

                invoiceDto.PaymentDetails.ForEach(x =>
                {
                    if (invoicePaymentMethodChild.Count > 0 && x.PaymentMethodID > 0)
                    {
                        var existingModelData = PaymentRepo.FirstOrDefault(y => y.PaymentMethodID == x.PaymentMethodID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.IPaymentMasterID = invoiceDto.IPaymentMasterID;
                        x.SetAdded();
                        SetInvoicePaymentMethodNewId(x);
                    }
                });

                var willDeleted = invoicePaymentMethodChild.Where(x => !invoiceDto.PaymentDetails.Select(y => y.PaymentMethodID).Contains(x.PaymentMethodID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    invoiceDto.PaymentDetails.Add(new PaymentMethodsDetailsDto
                    {
                        PaymentMethodID = x.PaymentMethodID,
                        IPaymentMasterID = x.IPaymentMasterID,
                        CategoryID = x.CategoryID,
                        BankID = x.BankID,
                        VendorBankName = x.VendorBankName,
                        BranchName = x.BranchName,
                        AccountNo = x.AccountNo,
                        RoutingNo = x.RoutingNo,
                        SwiftCode = x.SwiftCode,
                        ChequeBookID = x.ChequeBookID,
                        LeafNo = x.LeafNo,
                        NetPayableAmount = x.NetPayableAmount,
                        SupplierID = x.SupplierID,

                        CompanyID = x.CompanyID,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        CreatedIP = x.CreatedIP,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedDate = x.UpdatedDate,
                        UpdatedIP = x.UpdatedIP,
                        RowVersion = x.RowVersion,

                        ObjectState = x.ObjectState,
                        Attachments = GetAttachments((int)x.PaymentMethodID)
                    });
                });
            }


            return invoiceDto.PaymentDetails;
        }
        private void SetInvoicePaymentMethodNewId(PaymentMethodsDetailsDto child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("InvoicePaymentMethod", AppContexts.User.CompanyID);
            child.PaymentMethodID = code.MaxNumber;
        }
        private void AddAttachments(InvoicePaymentDto invoiceDto)
        {
            if (invoiceDto.PaymentDetails.IsNotNull())
                foreach (var details in invoiceDto.PaymentDetails)
                {
                    if (details.Attachments.IsNotNull())
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

                                    string filename = $"InvoicePaymentMethod-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                                    var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                                    var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "InvoicePaymentMethod\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                                    // To Add Into DB

                                    SetAttachmentNewId(attachment);
                                    SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.PaymentMethodID, "InvoicePaymentMethod", false, attachment.Size, 0, false, attachment.Description ?? "");

                                    sl++;
                                }
                            }
                        }
                    }

                }

        }
        private void RemoveAttachments(InvoicePaymentDto invoiceDto)
        {
            if (invoiceDto.PaymentDetails.IsNotNull())
                foreach (var details in invoiceDto.PaymentDetails)
                {
                    if (details.IsDeleted)
                    {
                        if (details.Attachments.IsNotNull())
                            foreach (var data in details.Attachments)
                            {
                                // To Remove Physical Files

                                string attachmentFolder = "upload\\attachments";
                                string folderName = "InvoicePaymentMethod";
                                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                                string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                                File.Delete(str + "\\" + data.FileName);

                                // To Remove From DB

                                SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PaymentMethodID, "InvoicePaymentMethod", true, data.Size, 0, false, data.Description ?? "");

                            }
                    }
                    else
                    {
                        var attachemntList = new List<Attachments>();
                        string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='InvoicePaymentMethod' AND ReferenceID={details.PaymentMethodID}";
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
                                string folderName = "InvoicePaymentMethod";
                                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                                string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                                File.Delete(str + "\\" + data.FileName);

                                // To Remove From DB

                                SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PaymentMethodID, "InvoicePaymentMethod", true, data.Size, 0, false, data.Description ?? "");

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
        //public List<Attachments> GetAttachments(int TVMID)
        //{
        //    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TaxationVettingMaster' AND ReferenceID={TVMID}";
        //    var attachment = TaxationVettingMasterRepo.GetDataDictCollection(attachmentSql);
        //    var attachemntList = new List<Attachments>();

        //    foreach (var data in attachment)
        //    {
        //        attachemntList.Add(new Attachments
        //        {
        //            FUID = (int)data["FUID"],
        //            AID = data["FUID"].ToString(),
        //            FilePath = data["FilePath"].ToString(),
        //            OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
        //            FileName = data["FileName"].ToString(),
        //            Type = data["FileType"].ToString(),
        //            Size = Convert.ToDecimal(data["SizeInKB"]),
        //            Description = data["Description"].ToString()
        //        });
        //    }
        //    return attachemntList;
        //}

        private List<InvoicePaymentChild> GenerateInvoicePaymentChild(InvoicePaymentDto IP)
        {
            var existingPurchaseRequisitionChild = ChildRepo.GetAllList(x => x.IPaymentMasterID == IP.IPaymentMasterID);
            var childModel = new List<InvoicePaymentChild>();
            if (IP.Details.IsNotNull())
            {
                IP.Details.ForEach(x =>
                {
                    childModel.Add(new InvoicePaymentChild
                    {
                        PaymentChildID = x.PaymentChildID,
                        IPaymentMasterID = IP.IPaymentMasterID,
                        TVMID = x.TVMID,
                        POMasterID = x.POMasterID,
                        SupplierID = x.SupplierID,
                        WarehouseID = x.WarehouseID,
                        InvoiceAmount = x.InvoiceAmount,
                        PostingDate = DateTime.Today,
                        ReceivingDate = DateTime.Today,
                        CustomDeduction = x.CustomDeduction,
                        NetPayableAmount = x.NetPayableAmount
                    });

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseRequisitionChild.Count > 0 && x.PaymentChildID > 0)
                    {
                        var existingModelData = existingPurchaseRequisitionChild.FirstOrDefault(y => y.PaymentChildID == x.PaymentChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.IPaymentMasterID = IP.IPaymentMasterID;
                        x.SetAdded();
                        SetInvoicePaymentChildNewId(x);
                    }
                });

                var willDeleted = existingPurchaseRequisitionChild.Where(x => !childModel.Select(y => y.PaymentChildID).Contains(x.PaymentChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private string GenerateMasterReference(string intial)
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{intial}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("ExpenseOrIOUPaymentSattlementReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        private void SetInvoicePaymentMasterNewId(InvoicePaymentMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("InvoicePaymentMaster", AppContexts.User.CompanyID);
            master.IPaymentMasterID = code.MaxNumber;
        }
        private void SetInvoicePaymentMethodNewId(InvoicePaymentMethod masterPaymentMethod)
        {
            if (!masterPaymentMethod.IsAdded) return;
            var code = GenerateSystemCode("InvoicePaymentMethod", AppContexts.User.CompanyID);
            masterPaymentMethod.PaymentMethodID = code.MaxNumber;
        }

        private void SetInvoicePaymentChildNewId(InvoicePaymentChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("InvoicePaymentChild", AppContexts.User.CompanyID);
            child.PaymentChildID = code.MaxNumber;
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(long IPaymentMasterID, int ApprovalProcessID, int ApprovalType)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (IPaymentMasterID > 0 && ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM InvoicePaymentMaster IPM
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IPM.CreatedBy
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = IPM.IPaymentMasterID AND AP.APTypeID = {ApprovalType}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {ApprovalType} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
            				LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {ApprovalType} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = IPM.IPaymentMasterID                                       
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)IPaymentMasterID, ApprovalProcessID, ApprovalType);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        public List<InvoicePaymentMasterDto> GetMasterList(string filterData)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (filterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND IPM.ApprovalStatusID = {(int)ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND IPM.ApprovalStatusID = {(int)ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
	                            IPM.*,
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
                                ,AP.APTypeID
                            FROM InvoicePaymentMaster IPM
                            LEFT JOIN Security..Users U ON U.UserID = IPM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = IPM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = IPM.IPaymentMasterID AND AP.APTypeID IN ({(int)ApprovalType.InvoicePayment},{(int)ApprovalType.IOUPayment})
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID IN ({(int)ApprovalType.InvoicePayment},{(int)ApprovalType.IOUPayment})
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID IN ({(int)ApprovalType.InvoicePayment},{(int)ApprovalType.IOUPayment}) AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IPM.IPaymentMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IPM.IPaymentMasterID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ORDER BY CreatedDate desc"; ;
            var invoiceDtoClaims = MasterRepo.GetDataModelCollection<InvoicePaymentMasterDto>(sql);

            return invoiceDtoClaims;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            //string APTypeIDs = $@"{(int)ApprovalType.InvoicePayment}";
            var comments = GetApprovalComments(approvalProcessID, (int)ApprovalType.InvoicePayment);
            return comments.Result;
        }
        private List<Attachments> GetAttachments(int PaymentChildID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='InvoicePaymentMethod' AND ReferenceID={PaymentChildID}";
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
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }

        #region Mail

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long IPaymentMasterID, int mailGroup, int APTypeID)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)IPaymentMasterID, 0);
        }
        #endregion

        public IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID)
        {
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {ReferenceID},{(int)Util.ApprovalType.InvoicePayment}";
            var feedback = MasterRepo.GetDataDictCollection(sql);
            return feedback;
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
            var comments = MasterRepo.GetDataDictCollection(sql);
            return comments;
        }
    }
}
