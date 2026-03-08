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
    public class TaxationVettingPaymentManager : ManagerBase, ITaxationVettingPaymentManager
    {
        private readonly IRepository<TaxationVettingPayment> TaxationVettingPaymentRepo;
        private readonly IRepository<TaxationVettingPaymentChild> ChildRepo;
        private readonly IRepository<TaxationVettingPaymentMethod> PaymentRepo;
        private readonly IRepository<ChequeBookChild> ChequeBookChildRepo;

        //readonly IModelAdapter Adapter;
        public TaxationVettingPaymentManager(IRepository<TaxationVettingPayment> taxationVettingPaymentRepo, IRepository<TaxationVettingPaymentChild> _chilRepo, IRepository<TaxationVettingPaymentMethod> _paymentRepo, IRepository<ChequeBookChild> chequeBookChildRepo)
        {
            TaxationVettingPaymentRepo = taxationVettingPaymentRepo;
            ChildRepo = _chilRepo;
            PaymentRepo = _paymentRepo;
            ChequeBookChildRepo = chequeBookChildRepo;
        }

        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.TaxationVettingPayment);
            return comments.Result;
        }
        public IEnumerable<Dictionary<string, object>> TaxationVettingApprovalFeedback(int TVMID)
        {
            string sql = $@" EXEC Accounts..spRPTTaxationVettingApprovalFeedback {TVMID}";
            var feedback = TaxationVettingPaymentRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int tvpid, string TableName)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='{TableName}' AND ReferenceID={tvpid}";
            var attachment = TaxationVettingPaymentRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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

        private string GenerateTaxationVettingPaymentReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/TVP/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("TaxationVettingPaymentRefNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }
        public GridModel GetListForGrid(GridParameter parameters)
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
                    filter = $@" AND TVP.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND TVP.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string sql = $@" SELECT 
                                DISTINCT
                                 TVP.TVPID
	                            ,TVP.ReferenceNo
								,TVP.ServicePeriod
								,TVP.NetPayableAmount
								,TVP.Purpose
                                ,TVP.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName
                                ,VA.DivisionID
                                ,VA.DivisionName
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
                                ,ISNULL(VA.FullName+VA.EmployeeCode+VA.DepartmentName,'') EmployeeWithDepartment
                                ,SVP.SystemVariableCode AS PaymentMode
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                          FROM TaxationVettingPayment TVP
                            LEFT JOIN Security..Users U ON U.UserID = TVP.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = TVP.ApprovalStatusID
                            LEFT JOIN Security..SystemVariable SVP ON SVP.SystemVariableID = TVP.PaymentModeID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = TVP.TVPID AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = TVP.TVPID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = TVP.TVPID
                                    LEFT JOIN ( 
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                   
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
									        WHERE AEF.APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = TVP.TVPID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}";
            var result = TaxationVettingPaymentRepo.LoadGridModel(parameters, sql);
            return result;
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
            var comments = TaxationVettingPaymentRepo.GetDataDictCollection(sql);
            return comments;
        }
        public async Task<Dictionary<string, object>> TaxationVettingAndInvoiceInfo(int tvmid)
        {
            string query = @$"SELECT TVM.TVMID
                            	,TVM.InvoiceMasterID
                            	,TVM.PRMasterID
                            	,TVM.POMasterID
                            	,TVM.VATRebatableID
                            	,TVM.VATRebatablePercent
                            	,TVM.VATRebatableAmount
                            	,TVM.VDSRateID
                            	,TVM.VDSRatePercent
                            	,TVM.VDSAmount
                            	,TVM.TDSMethodID
                            	,TVM.TDSRate
                            	,TVM.TDSAmount
                            	,TVM.ReferenceNo
                            	,TVM.ReferenceKeyword
                            	,TVM.ApprovalStatusID
                            	,TVM.GrandTotal
                            	,TVM.Remarks
                            	,TVM.TVMDate TaxationVettingDate
                            	,TVM.TVMDate
                            	,TVM.TDSRateID
                            	,I.ReferenceNo AS InvoiceNo
                            	,I.InvoiceDate
                            	,I.IsAdvanceInvoice
                            	,PO.PODate
                            	,PO.DeliveryLocation
                            	,PO.SupplierID
                            	,S.SupplierName
                            	,PO.ReferenceNo AS PONo
                            	,PO.CreatedDate AS POCreatedDate
                            	,PO.TotalVatAmount
                            	,PO.TotalWithoutVatAmount
                            	,PR.ReferenceNo AS PRNo
                                ,PR.CreatedDate AS PRCreatedDate
                            	,ITC.SystemVariableCode AS InvoiceTypeName
                            	,PT.SystemVariableCode AS PaymentTermsName
                            	,PT.NumericValue AS NumberOfDueDay
                            	,TD.SystemVariableCode AS TDSMethodName
                                ,TD.SystemVariableID AS TDSMethodID
                            	,VDS.SectionOrServiceCode AS VDSCode
                            	,TDS.SectionOrServiceCode AS TDSCode
                            	,YN.SystemVariableCode AS YesNoName
                                ,I.BaseAmount
	                            ,I.TaxAmount
	                            ,I.TotalPayableAmount
	                            ,I.AdvanceDeductionAmount
                            FROM --TaxationVettingMaster AS TVM
                            SCM..InvoicePaymentMaster IPM
						    JOIN SCM..InvoicePaymentChild IPC ON IPM.IPaymentMasterID=IPC.IPaymentMasterID
							JOIN TaxationVettingMaster TVM ON IPC.TVMID=TVM.TVMID
                            LEFT OUTER JOIN SCM.dbo.InvoiceMaster AS I ON I.InvoiceMasterID = TVM.InvoiceMasterID
                            LEFT OUTER JOIN SCM.dbo.PurchaseOrderMaster AS PO ON TVM.POMasterID = PO.POMasterID
                            LEFT OUTER JOIN SCM.dbo.PurchaseRequisitionMaster AS PR ON TVM.PRMasterID = PR.PRMasterID
                            LEFT OUTER JOIN SCM.dbo.Supplier AS S ON S.SupplierID = PO.SupplierID
                            LEFT OUTER JOIN Security.dbo.SystemVariable AS ITC ON ITC.SystemVariableID = I.InvoiceTypeID
                            	AND ITC.EntityTypeID = 29
                            LEFT OUTER JOIN Security.dbo.SystemVariable AS PT ON PT.SystemVariableID = PO.PaymentTermsID
                            	AND PT.EntityTypeID = 28
                            LEFT OUTER JOIN Security.dbo.SystemVariable AS TD ON TD.SystemVariableID = TVM.TDSMethodID
                            LEFT OUTER JOIN Security.dbo.SystemVariable AS YN ON YN.SystemVariableID = TVM.VATRebatableID
                            LEFT OUTER JOIN VatTaxDeductionSource AS VDS ON VDS.VTDSID = TVM.VDSRateID
                            LEFT OUTER JOIN VatTaxDeductionSource AS TDS ON TDS.VTDSID = TVM.TDSRateID
                            WHERE (TVM.TVMID = {tvmid})";
            return await Task.Run(() => TaxationVettingPaymentRepo.GetData(query));
        }

        public async Task<(bool, string)> SaveChanges(TaxationVettingPaymentDto model)
        {
            var existingMaster = TaxationVettingPaymentRepo.Entities.Where(x => x.TVPID == model.TVPID).SingleOrDefault();
            var removeList = RemoveAttachments(model);
            var removeListMethod = RemoveAttachmentsMethod(model);

            //var existingPaymentMethod = PaymentRepo.Entities.Where(x => x.TVPID == model.TVPID).ToList();

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (model.TVPID > 0 && model.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT  {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                            FROM TaxationVettingPayment TVP
                            LEFT JOIN Security..Users U ON U.UserID = TVP.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = TVP.TVPID AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 9 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = TVP.TVPID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {model.ApprovalProcessID}";
                var canReassesstment = TaxationVettingPaymentRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Taxation Vetting once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Taxation Vetting once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)model.TVPID, model.ApprovalProcessID, (int)Util.ApprovalType.TaxationVettingPayment);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;


            using (var unitOfWork = new UnitOfWork())
            {
                model.ApprovalStatusID = model.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending;
                var masterModel = model.MapTo<TaxationVettingPayment>();

                if (model.TVPID.IsZero() && existingMaster.IsNull())
                {
                    masterModel.TVPDate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateTaxationVettingPaymentReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetTaxationVettingPaymentNewId(masterModel);
                    model.TVPID = (int)masterModel.TVPID;
                }
                else
                {
                    masterModel.CreatedBy = existingMaster.CreatedBy;
                    masterModel.CreatedDate = existingMaster.CreatedDate;
                    masterModel.CreatedIP = existingMaster.CreatedIP;
                    masterModel.RowVersion = existingMaster.RowVersion;
                    masterModel.ReferenceNo = existingMaster.ReferenceNo;
                    masterModel.TVPDate = existingMaster.TVPDate;
                    masterModel.ReferenceKeyword = existingMaster.ReferenceKeyword;
                    masterModel.SetModified();
                }


                var childModel = GenerateTaxationPaymentChild(model);
                var paymentMethodModel = GenerateTaxationPaymentMethodModel(model).MapTo<List<TaxationVettingPaymentMethod>>();

                //var chequeBookChildModel = GenerateCBCModel(paymentMethodModel);
                

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                SetAuditFields(paymentMethodModel);
                //SetAuditFields(chequeBookChildModel);


                //RemoveAttachments(model);
                //AddAttachments(model);

                if (model.Attachments.IsNotNull() && model.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(model.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.TVPID, "TaxationVettingPayment", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)model.TVPID, "TaxationVettingPayment", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                var list = model.IsBankTransfer != 80 ? model.PaymentDetails.Where(x => x.Attachments.Count > 0) : new List<PaymentMethodsDetailsDto>();

                //var list = model.PaymentDetails.Where(x => x.Attachments.Count > 0);
                foreach (var item in list)
                {
                    if (item.Attachments.IsNotNull() && item.Attachments.Count > 0)
                    {

                        var attachmentList = AddAttachmentsMethod(item.Attachments.Where(x => x.ID == 0).ToList());

                        //For Add new File
                        if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                        {
                            foreach (var attachemnt in attachmentList)
                            {
                                SetAttachmentNewIdMethod(attachemnt);
                                SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)item.PaymentMethodID, "TaxationVettingPaymentMethod", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                            }
                        }
                        //For Remove Attachment                    
                        if (removeList.IsNotNull() && removeList.Count > 0)
                        {
                            foreach (var attachemnt in removeList)
                            {
                                SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)item.PaymentMethodID, "TaxationVettingPaymentMethod", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                            }
                        }
                    }
                }



                TaxationVettingPaymentRepo.Add(masterModel);
                PaymentRepo.AddRange(paymentMethodModel);
                ChildRepo.AddRange(childModel);
                //ChequeBookChildRepo.AddRange(chequeBookChildModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingMaster.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.TaxationVettingPaymentApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Taxation Vetting Payment No:{masterModel.ReferenceNo}";
                        ApprovalProcessID = CreateApprovalProcess((int)masterModel.TVPID, Util.AutoTaxationVettingpaymentDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.TaxationVettingPayment, (int)Util.ApprovalPanel.TaxationVettingPayment);
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
                            IsResubmitted = true;
                        }
                    }
                }

                UpdateCBCModel(paymentMethodModel);

                unitOfWork.CommitChangesWithAudit();
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        await SendMail(ApprovalProcessID, IsResubmitted, (int)masterModel.TVPID, (int)Util.MailGroupSetup.TaxationPaymentInitiatedMail);
                }

            }
            await Task.CompletedTask;

            return (true, $"Taxation Vetting Payment Submitted Successfully");
        }


        private void UpdateCBCModel(List<TaxationVettingPaymentMethod> paymentMethodModel)
        {

            foreach (TaxationVettingPaymentMethod payMethod in paymentMethodModel)
            {
                ChequeBookLeafUpdate(Convert.ToInt32(payMethod.ChequeBookID), payMethod.LeafNo);
            }
        }

        //private List<ChequeBookChild> GenerateCBCModel(List<TaxationVettingPaymentMethod> paymentMethodModel)
        //{
        //    List<ChequeBookChild> chequeBookChildList = new List<ChequeBookChild>();

        //    foreach (TaxationVettingPaymentMethod payMethod in paymentMethodModel)
        //    {
        //        var existingCBChild = ChequeBookChildRepo.Entities.Where(x => x.CBID == payMethod.ChequeBookID && x.LeafNo == payMethod.LeafNo).ToList();
        //        ChequeBookChild obj = new ChequeBookChild();
        //        obj.CBCID = existingCBChild[0].CBCID;
        //        obj.CBID = Convert.ToInt32(payMethod.ChequeBookID);
        //        obj.LeafNo = payMethod.LeafNo;
        //        obj.IsActiveLeaf = existingCBChild[0].IsActiveLeaf;
        //        obj.IsUsed = true;
        //        obj.CreatedBy = existingCBChild[0].CreatedBy;
        //        obj.CreatedDate = existingCBChild[0].CreatedDate;
        //        obj.CreatedIP = existingCBChild[0].CreatedIP;
        //        obj.RowVersion = existingCBChild[0].RowVersion;
        //        obj.SetModified();

        //        chequeBookChildList.Add(obj);
        //    }
        //    return chequeBookChildList;
        //}

        private void SetTaxationPaymentMethodNewId(PaymentMethodsDetailsDto paymentMethod)
        {
            if (!paymentMethod.IsAdded) return;
            var code = GenerateSystemCode("TaxationVettingPaymentMethod", AppContexts.User.CompanyID);
            paymentMethod.PaymentMethodID = code.MaxNumber;
        }
        private List<TaxationVettingPaymentChild> GenerateTaxationPaymentChild(TaxationVettingPaymentDto TP)
        {
            var existingTaxationVettingPaymentChild = ChildRepo.Entities.Where(x => x.TVPID == TP.TVPID).ToList();
            var childModel = new List<TaxationVettingPaymentChild>();
            if (TP.Details.IsNotNull())
            {
                TP.Details.ForEach(x =>
                {
                    childModel.Add(new TaxationVettingPaymentChild
                    {
                        TVPChildID = x.TVPChildID,
                        TVPID = TP.TVPID,
                        IPaymentMasterID = x.IPaymentMasterID,
                        POMasterID = x.POMasterID,
                        SupplierID = x.SupplierID
                    });

                });

                childModel.ForEach(x =>
                {
                    if (existingTaxationVettingPaymentChild.Count > 0 && x.TVPChildID > 0)
                    {
                        var existingModelData = existingTaxationVettingPaymentChild.FirstOrDefault(y => y.TVPChildID == x.TVPChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.TVPID = TP.TVPID;
                        x.SetAdded();
                        SetTaxationPaymentChildNewId(x);
                    }
                });

                var willDeleted = existingTaxationVettingPaymentChild.Where(x => !childModel.Select(y => y.TVPChildID).Contains(x.TVPChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }


        //GenerateTaxationPaymentMethodModel
        private List<PaymentMethodsDetailsDto> GenerateTaxationPaymentMethodModel(TaxationVettingPaymentDto TP)
        {
            var existingTaxationVettingPaymentMethod = PaymentRepo.Entities.Where(x => x.TVPID == TP.TVPID).ToList();
            //var paymentModel = new List<TaxationVettingPaymentDto>();
            if (TP.PaymentDetails.IsNotNull())
            {
                TP.PaymentDetails.ForEach(x =>
                {
                    if (existingTaxationVettingPaymentMethod.Count > 0 && x.PaymentMethodID > 0)
                    {
                        var existingModelData = existingTaxationVettingPaymentMethod.FirstOrDefault(y => y.PaymentMethodID == x.PaymentMethodID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.TVPID = TP.TVPID;
                        x.SetAdded();
                        SetTaxationPaymentMethodNewId(x);
                    }
                });


                var willDeleted = existingTaxationVettingPaymentMethod.Where(x => !TP.PaymentDetails.Select(y => y.PaymentMethodID).Contains(x.PaymentMethodID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    TP.PaymentDetails.Add(new PaymentMethodsDetailsDto
                    {
                        PaymentMethodID = x.PaymentMethodID,
                        TVPID = x.TVPID,
                        CategoryID = x.CategoryID,
                        BankID = x.BankID,
                        VendorBankName = x.VendorBankName,
                        BranchName = x.BranchName,
                        AccountNo = x.AccountNo,
                        RoutingNo = x.RoutingNo,
                        SwiftCode = x.SwiftCode,
                        ChequeBookID = x.ChequeBookID,
                        LeafNo = x.LeafNo,
                        Amount = x.Amount,
                        CompanyID = x.CompanyID,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        CreatedIP = x.CreatedIP,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedDate = x.UpdatedDate,
                        UpdatedIP = x.UpdatedIP,
                        RowVersion = x.RowVersion,
                        ObjectState = x.ObjectState,
                        Attachments = GetAttachments((int)x.PaymentMethodID, "TaxationVettingPaymentMethod")
                    });
                });

            }

            return TP.PaymentDetails;
        }


        private void SetTaxationPaymentChildNewId(TaxationVettingPaymentChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("TaxationVettingPaymentChild", AppContexts.User.CompanyID);
            child.TVPChildID = code.MaxNumber;
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
                        string filename = $"TaxationVettingPayment-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "TaxationVettingPayment\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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


        private List<Attachments> AddAttachmentsMethod(List<Attachments> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"TaxationVettingPaymentMethod-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "TaxationVettingPaymentMethod\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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

        private List<Attachment> RemoveAttachments(TaxationVettingPaymentDto TVP)
        {
            if (TVP.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TaxationVettingPayment' AND ReferenceID={TVP.TVPID}";
                var prevAttachment = TaxationVettingPaymentRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !TVP.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "TaxationVettingPayment";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }


        private List<Attachment> RemoveAttachmentsMethod(TaxationVettingPaymentDto TVP)
        {
            var list = TVP.IsBankTransfer != 80 ? TVP.PaymentDetails.Where(x => x.Attachments.Count > 0) : new List<PaymentMethodsDetailsDto>();
            foreach(var item in list)
            {
                if (item.Attachments.Count > 0)
                {
                    var attachemntList = new List<Attachment>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TaxationVettingPaymentMethod' AND ReferenceID={item.PaymentMethodID}";
                    var prevAttachment = TaxationVettingPaymentRepo.GetDataDictCollection(attachmentSql);

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
                    var removeList = attachemntList.Where(x => !item.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                    if (removeList.Count > 0)
                    {
                        foreach (var data in removeList)
                        {
                            string attachmentFolder = "upload\\attachments";
                            string folderName = "TaxationVettingPaymentMethod";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            System.IO.File.Delete(str + "\\" + data.FileName);

                        }

                    }
                    return removeList;
                }
            }
            
            return null;
        }

        private void SetTaxationVettingPaymentNewId(TaxationVettingPayment master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("TaxationVettingPayment", AppContexts.User.CompanyID);
            master.TVPID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private void SetAttachmentNewIdMethod(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int RefID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = mail.Item2;
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.TaxationVettingPayment, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, RefID, 0);
        }

        public Task<TaxationVettingPaymentDto> GetTaxationVettingPayment(int tvpid)
        {
            string sql = $@"SELECT TVP.*
                        	,VA.DepartmentID
                        	,VA.DepartmentName
                        	,VA.FullName AS EmployeeName
                        	,VA.DivisionID
                        	,VA.DivisionName
                        	,SV.SystemVariableCode AS ApprovalStatus
                        	,VA.ImagePath
                        	,VA.EmployeeCode
                            ,VA.WorkMobile
                        	,CAST((
                        			SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID}, ISNULL(AP.ApprovalProcessID, 0))
                        			) AS BIT) IsCurrentAPEmployee
                        	,CASE 
                        		WHEN (
                        				SELECT Approval.dbo.[fnIsAPCreator]({AppContexts.User.EmployeeID}, ISNULL(AP.ApprovalProcessID, 0))
                        				) = 1
                        			AND EditableCount > 0
                        			THEN CAST(1 AS BIT)
                        		ELSE CAST(0 AS BIT)
                        		END IsReassessment
                        	,CASE 
                        		WHEN ISNULL(Cntr, 0) > 0
                        			THEN CAST(1 AS BIT)
                        		ELSE CAST(0 AS BIT)
                        		END IsReturned
                        	,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        FROM TaxationVettingPayment TVP
                        LEFT JOIN Security..Users U ON U.UserID = TVP.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = TVP.ApprovalStatusID
                        LEFT JOIN Security..SystemVariable SVP ON SVP.SystemVariableID = TVP.PaymentModeID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = TVP.TVPID
                        	AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
                        LEFT JOIN (
                        	SELECT COUNT(cntr) EditableCount
                        		,ReferenceID
                        	FROM (
                        		SELECT COUNT(APEmployeeFeedbackID) Cntr
                        			,ReferenceID
                        		FROM Approval..ApprovalEmployeeFeedback AEF
                        		LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
                        		WHERE SequenceNo = 2
                        			AND APFeedbackID = 2
                        			AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
                        		GROUP BY ReferenceID
                        		
                        		UNION ALL
                        		
                        		SELECT COUNT(APEmployeeFeedbackID) Cntr
                        			,ReferenceID
                        		FROM Approval..ApprovalEmployeeFeedback AEF
                        		LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
                        		WHERE SequenceNo = 1
                        			AND APFeedbackID = 2
                        			AND APTypeID = {(int)Util.ApprovalType.TaxationVettingPayment}
                        			AND EmployeeID = {AppContexts.User.EmployeeID}
                        		GROUP BY ReferenceID
                        		) V
                        	GROUP BY ReferenceID
                        	) EA ON EA.ReferenceID = TVP.TVPID
                        LEFT JOIN (
                        	SELECT AP.ApprovalProcessID
                        		,COUNT(ISNULL(APFeedbackID, 0)) Cntr
                        		,AP.ReferenceID
                        	FROM Approval..ApprovalEmployeeFeedbackRemarks AEFR
                        	INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
                        	WHERE APFeedbackID = 11 --Returned
                        	GROUP BY AP.ApprovalProcessID
                        		,AP.ReferenceID
                        	) Rej ON Rej.ReferenceID = TVP.TVPID
                        WHERE TVP.TVPID = {tvpid}";
            var master = TaxationVettingPaymentRepo.GetModelData<TaxationVettingPaymentDto>(sql);
            return Task.Run(() => master);
        }

        public IEnumerable<Dictionary<string, object>> GetChildList(int TVPID)
        {
            string query = $@"SELECT * from SCM..viewGetAllTaxationPaymentChildData
                            WHERE TVPID={TVPID}";
            var ChildList = TaxationVettingPaymentRepo.GetDataDictCollection(query);
            return ChildList;
        }

        public async Task<List<PaymentMethodsDetailsDto>> GetPaymentMethodDetails(int TVPID)
        {
            var finalList = new List<PaymentMethodsDetailsDto>();
            string sql = $@"SELECT PM.*,b.BankName, cb.BankName ChequeBookBank,CONCAT(cb.BankName,'-',c.BranchName,'-',c.AccountNo) CBDetails FROM Accounts..TaxationVettingPaymentMethod PM
                            LEFT JOIN Accounts..Bank b on PM.BankID=b.BankID
                            LEFT JOIN Accounts..ChequeBook c on PM.ChequeBookID=c.CBID
                            LEFT JOIN Accounts..Bank cb on cb.BankID=c.BankID
                        WHERE PM.TVPID={TVPID}";
            var list = TaxationVettingPaymentRepo.GetDataModelCollection<PaymentMethodsDetailsDto>(sql);

            foreach(var x in list)
            {
                finalList.Add(new PaymentMethodsDetailsDto
                {
                    PaymentMethodID = x.PaymentMethodID,
                    TVPID = x.TVPID,
                    CategoryID = x.CategoryID,
                    BankID = x.BankID,
                    BankName = x.BankName,
                    VendorBankName = x.VendorBankName,
                    LeafNo = x.LeafNo,
                    Amount = x.Amount,
                    BranchName = x.BranchName,
                    AccountNo = x.AccountNo,
                    RoutingNo = x.RoutingNo,
                    SwiftCode = x.SwiftCode,
                    ChequeBookID = x.ChequeBookID,
                    FromOrTo = x.FromOrTo,
                    ChequeBookBank = x.ChequeBookBank,
                    CBDetails = x.CBDetails,
                    Attachments = GetAttachments((int)x.PaymentMethodID, "TaxationVettingPaymentMethod")
                });
            }

            return finalList;
        }
        public IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID)
        {
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {ReferenceID},{(int)Util.ApprovalType.TaxationVettingPayment}";
            var feedback = TaxationVettingPaymentRepo.GetDataDictCollection(sql);
            return feedback;
        }


    }
}
