using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Core.Util;

namespace SCM.Manager
{
    public class InvoiceManager : ManagerBase, IInvoiceManager
    {
        private readonly IRepository<MaterialReceive> MaterialReceiveRepo;
        private readonly IRepository<MaterialReceiveChild> MaterialReceiveChildRepo;
        private readonly IRepository<InvoiceMaster> InvoiceMasterRepo;
        private readonly IRepository<InvoiceChild> InvoiceChildRepo;
        private readonly IRepository<InvoiceChildSCC> InvoiceChildSCCRepo;
        private readonly IRepository<PurchaseOrderMaster> PurchaseOrderMasterRepo;
        private readonly IRepository<PurchaseOrderChild> PurchaseOrderChildRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<QCChild> QCChildRepo;
        private readonly IRepository<QCMaster> QCMasterRepo;
        private readonly IRepository<SCCMaster> SCCMasterRepo;
        private readonly IRepository<SCCChild> SCCChildRepo;

        public InvoiceManager(IRepository<MaterialReceive> materialReceiveRepo, IRepository<MaterialReceiveChild> materialReceiveChildRepo,
            IRepository<InvoiceMaster> invoiceMasterRepo, IRepository<InvoiceChild> invoiceChildRepo, IRepository<PurchaseOrderMaster> purchaseOrderMasterRepo, IRepository<PurchaseOrderChild> purchaseOrderChildRepo, IRepository<Item> itemRepo, IRepository<QCChild> qcChildRepo, IRepository<QCMaster> qcMasterRepo
            , IRepository<InvoiceChildSCC> invoiceChildSCCRepo, IRepository<SCCMaster> sccMasterRepo, IRepository<SCCChild> sccChildRepo)
        {
            MaterialReceiveRepo = materialReceiveRepo;
            MaterialReceiveChildRepo = materialReceiveChildRepo;
            InvoiceMasterRepo = invoiceMasterRepo;
            InvoiceChildRepo = invoiceChildRepo;
            InvoiceChildSCCRepo = invoiceChildSCCRepo;
            PurchaseOrderMasterRepo = purchaseOrderMasterRepo;
            PurchaseOrderChildRepo = purchaseOrderChildRepo;
            ItemRepo = itemRepo;
            QCChildRepo = qcChildRepo;
            QCMasterRepo = qcMasterRepo;
            SCCMasterRepo = sccMasterRepo;
            SCCChildRepo = sccChildRepo;
        }
        #region Grid
        public GridModel GetListForGrid(GridParameter parameters)
        {
            string sql = $@"SELECT 
	                            MR.ReferenceNo GRNNo,
	                            MR.MRID,
	                            MR.MRDate,
	                            MR.WarehouseID,
	                            MR.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,
	                            SystemVariableCode InventoryTypeName,
								MR.CreatedDate,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID
                           FROM 
                           	MaterialReceive MR
                           	LEFT JOIN PurchaseOrderMaster PO ON MR.POMasterID = PO.POMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = MR.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = MR.WarehouseID
                           	LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID AND EntityTypeID =25
                           WHERE MR.ApprovalStatusID = 23  {parameters.ApprovalFilterData}";
            var result = MaterialReceiveRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetAdvanceInvoiceListForGrid(GridParameter parameters)
        {
            string sql = $@"SELECT 
	                            PO.ReferenceNo GRNNo,
	                            0 MRID,
	                            PO.PODate,
	                            PO.DeliveryLocation WarehouseID,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,
	                            SystemVariableCode InventoryTypeName,
								PO.CreatedDate,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,
                                ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                                ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID,
								IT.SystemVariableCode PaymentTerms,
								1 IsAdvanceInvoice,
								PR.ReferenceNo PRNo,
								PR.PRMasterID
                           FROM 
                           	PurchaseOrderMaster PO   
							LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                           	LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.PaymentTermsID AND EntityTypeID = 28
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN (
										SELECT 
											POMasterID,SUM(TotalPayableAmount + TaxAmount) InvoiceAmount 
										FROM 
											InvoiceMaster
											WHERE ApprovalStatusID <> 23
											GROUP BY POMasterID
							) Inv ON Inv.POMasterID = PO.POMasterID
                           WHERE PO.ApprovalStatusID = 23 AND Inv.POMasterID IS NULL AND IT.IsSystemGenerated = 1 {parameters.ApprovalFilterData}";

            // AND CONVERT(decimal, PO.TotalWithoutVatAmount) > CONVERT(decimal, ISNULL(Inv.InvoiceAmount, 0))
            var result = PurchaseOrderMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetRegularInvoiceListForGrid(GridParameter parameters)
        {
            string sql = $@"SELECT 	                            
	                            PO.PODate,
	                            PO.DeliveryLocation WarehouseID,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,	                            
								PO.CreatedDate,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,	
                                ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                                ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID,
								PR.ReferenceNo PRNo,
								PR.PRMasterID,
								ISNULL(MRTotalWithoutVatAmount,0) - ISNULL(TotalInvoiceAmount,0) DueAmount
                           FROM 
                           	PurchaseOrderMaster PO   
							LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
							INNER JOIN (
										   SELECT 
											 MRC.POMasterID,
											 SUM(MRC.TotalWithoutVatAmount) MRTotalWithoutVatAmount
										   FROM 
												MaterialReceive MRC
                                             INNER JOIN MaterialReceive MR ON MR.MRID = MRC.MRID
										   --LEFT JOIN InvoiceChild Inv ON INv.MRID = MRC.MRID
										   --LEFT JOIN InvoiceMaster IM ON IM.InvoiceMasterID = Inv.InvoiceMasterID AND IM.ApprovalStatusID <> 24
										    WHERE MR.ApprovalStatusID = 23
										   GROUP BY MRC.POMasterID
									)MRInv ON MRInv.POMasterID = PO.POMasterID
							LEFT JOIN (
								SELECT 
									  SUM(TotalPayableAmount)+SUM(TaxAmount) TotalInvoiceAmount,
									 POMasterID
								FROM 
									InvoiceMaster
									WHERE ApprovalStatusID <> 24
									GROUP BY POMasterID
							)Invoice ON Invoice.POMasterID = PO.POMasterID
                            LEFT JOIN (															
								SELECT DISTINCT
									MR.POMasterID
								FROM 
									MaterialReceive MR
									LEFT JOIN InvoiceChild IC ON MR.MRID = IC.MRID
									LEFT JOIN InvoiceMaster IM ON IC.InvoiceMasterID = IM.InvoiceMasterID
									WHERE IM.ApprovalStatusID <> 24
							)InM ON InM.POMasterID = PO.POMasterID
							WHERE CAST(ISNULL(MRTotalWithoutVatAmount,0) AS DECIMAL(18,0)) >= CAST(ISNULL(TotalInvoiceAmount,0) AS DECIMAL(18,0)) AND InM.POMasterID IS NULL  {parameters.ApprovalFilterData}";

            var result = MaterialReceiveRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetCreatedInvoiceListForGrid(GridParameter parameters)
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
                                ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                --,CASE WHEN ApprovalStatusID = 24 THEN CAST(1 as bit) WHEN EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,PR.PRMasterID
								,InventoryTypeID						
								,PO.ReferenceNo PONo
								,PR.ReferenceNo PRNo								
								,IT.SystemVariableCode InventoryTypeName
                                ,InType.SystemVariableCode InvoiceTypeName
                                ,SupplierName
                                ,Concat(PO.ReferenceNo, '',PR.ReferenceNo) POPRNo
                                ,Concat(IT.SystemVariableCode, '',InType.SystemVariableCode) InventoryAndInvoiceType
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,CASE WHEN IPayment.ApprovalStatusID=23 THEN 'Yes' ELSE 'No' END AS PaymentStatus
                            FROM InvoiceMaster IPM
							LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = IPM.POMasterID
							LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID

                            LEFT JOIN SCM..InvoicePaymentChild IPC ON IPC.POMasterID=IPM.POMasterID
							LEFT JOIN SCM..InvoicePaymentMaster IPayment ON IPayment.IPaymentMasterID=IPC.IPaymentMasterID AND IPayment.ApprovalStatusID<>24
                            
                            LEFT JOIN Supplier S ON S.SupplierID = IPM.SupplierID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable InType ON InType.SystemVariableID = IPM.InvoiceTypeID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IPM.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = IPM.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = IPM.InvoiceMasterID AND AP.APTypeID = {(int)Util.ApprovalType.Invoice}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.Invoice} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.Invoice} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IPM.InvoiceMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IPM.InvoiceMasterID
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
									        WHERE AEF.APTypeID = {(int)Util.ApprovalType.Invoice} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = IPM.InvoiceMasterID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = InvoiceMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public GridModel GetApprovedInvoiceListForGrid(GridParameter parameters)
        {
            // Approved Invoice List
            string sql = $@"SELECT 
                                DISTINCT
	                            IPM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode	                           
                                ,PR.PRMasterID
								,InventoryTypeID						
								,PO.ReferenceNo PONo
								,PR.ReferenceNo PRNo								
								,IT.SystemVariableCode InventoryTypeName
                                ,InType.SystemVariableCode InvoiceTypeName
                                ,SupplierName
                                ,Concat(PO.ReferenceNo, '',PR.ReferenceNo) POPRNo
                                ,Concat(IT.SystemVariableCode, '',InType.SystemVariableCode) InventoryAndInvoiceType
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,ISNULL(TM.TVMID, 0) TVMID
                                ,ISNULL(TM.ReferenceNo, '') TaxationVettingNo
	                            ,ISNULL(AP.ApprovalProcessID, 0) PRApprovalProcessID
	                            ,ISNULL(AP1.ApprovalProcessID, 0) POApprovalProcessID
                                
                            FROM InvoiceMaster IPM
							LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = IPM.POMasterID
							LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
                            LEFT JOIN Supplier S ON S.SupplierID = IPM.SupplierID
							LEFT JOIN Accounts..TaxationVettingMaster TM ON TM.InvoiceMasterID = IPM.InvoiceMasterID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID = PO.POMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PO}

                            LEFT JOIN Security..Users U ON U.UserID = IPM.CreatedBy
                            LEFT JOIN hrms..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                           LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID
						   LEFT JOIN Security..SystemVariable InType ON InType.SystemVariableID = IPM.InvoiceTypeID
						   Where IPM.ApprovalStatusID = {(int)ApprovalStatus.Approved}";
            var result = InvoiceMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        #endregion

        #region Master
        private string GetInvoiceMasterSQL(int InvoiceMasterID)
        {

            return @$"SELECT    I.*,
                                VA.DepartmentID, 
                                VA.DepartmentName, 
                                VA.FullName AS EmployeeName, 
                                SV.SystemVariableCode AS ApprovalStatus, 
                                VA.ImagePath, 
                                VA.EmployeeCode, 
                                VA.DivisionID, VA.DivisionName,
                                CASE  WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]   ({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit)    ELSE CAST(0 as bit) END IsReassessment,
							    CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned,
                                W.WarehouseName DeliveryLocationName,
                                WorkMobile,
	                            PO.ReferenceNo,
	                            PO.PODate PODate,
	                            PO.DeliveryLocation,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,
	                            IT.SystemVariableCode InventoryTypeName,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,
                                PO.GrandTotal,
                                PO.TotalVatAmount,
                                PO.TotalWithoutVatAmount,
                                PO.CreditDay,
								IM.BasePercent PreviousBasePercent,
								IM.BaseAmount PreviousBaseAmount,
								IM.TaxAmount PreviousTaxAmount,
								IM.TotalPayableAmount PreviousPaidAmount,
                                IM.AdvanceDeductionAmount PreviousAdvanceDeductionAmount,

                                ISNULL(AdvanceIM.AdvanceBasePercent,0) PreviousAdvanceBasePercent,
						        ISNULL(AdvanceIM.AdvanceBaseAmount,0) PreviousAdvanceBaseAmount,
						        ISNULL(AdvanceIM.AdvanceTaxAmount,0) PreviousAdvanceTaxAmount,
						        ISNULL(AdvanceIM.AdvanceTotalPayableAmount,0) PreviousAdvanceTotalPayableAmount,

                                C.CurrencyName,
								ITC.SystemVariableCode InvoiceTypeName,
                                PT.SystemVariableCode PaymentTermsName,
						        PT.NumericValue NumberOfDueDay,

                                ISNULL(AdvanceIM.AdvanceBaseAmount,0)-ISNULL(IM.AdvanceDeductionAmount,0) AdvanceDeductionAmount,
                                ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                                ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID

                           FROM
							InvoiceMaster I
						    LEFT JOIN PurchaseOrderMaster PO ON I.POMasterID=PO.POMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                            LEFT JOIN Security..Currency C ON C.CurrencyID=I.CurrencyID
                           	LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID AND EntityTypeID =25
							LEFT JOIN Security..SystemVariable ITC ON ITC.SystemVariableID = I.InvoiceTypeID AND ITC.EntityTypeID =29
                            LEFT JOIN  Security..SystemVariable PT ON PT.SystemVariableID=PO.PaymentTermsID AND  PT.EntityTypeID=28
							LEFT JOIN (SELECT 
										SUM(BasePercent) BasePercent,
										SUM(BaseAmount) BaseAmount,
										SUM(TaxAmount) TaxAmount,
										SUM(TotalPayableAmount) TotalPayableAmount,
                                        SUM(AdvanceDeductionAmount) AdvanceDeductionAmount,
										POMasterID 
										FROM InvoiceMaster
										WHERE InvoiceMasterID !={InvoiceMasterID} AND IsAdvanceInvoice = 0
										GROUP BY POMasterID) IM ON IM.POMasterID=PO.POMasterID
                            LEFT JOIN (
									SELECT 
										SUM(BasePercent) AdvanceBasePercent,
										SUM(BaseAmount) AdvanceBaseAmount,
										SUM(TaxAmount) AdvanceTaxAmount,
										SUM(TotalPayableAmount) AdvanceTotalPayableAmount,
										POMasterID 
										FROM InvoiceMaster 
										WHERE InvoiceMasterID !={InvoiceMasterID} AND IsAdvanceInvoice = 1
										GROUP BY POMasterID
							) AdvanceIM ON AdvanceIM.POMasterID=PO.POMasterID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = I.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID =     I.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID =     I.InvoiceMasterID AND AP.APTypeID = {(int)Util.ApprovalType.Invoice}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess APPO ON APPO.ReferenceID =  PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.Invoice}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.Invoice} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = I.InvoiceMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = I.InvoiceMasterID
							WHERE I.InvoiceMasterID ={InvoiceMasterID}";
        }
        public async Task<Dictionary<string, object>> GetInvoiceMasterDic(int InvoiceMasterID)
        {
            string sql = GetInvoiceMasterSQL(InvoiceMasterID);
            var master = InvoiceMasterRepo.GetData(sql);
            return master;
        }
        public async Task<Dictionary<string, object>> GetInvoiceMasterDicForTaxationVetting(int InvoiceMasterID)
        {
            string sql = GetInvoiceMasterSQLForTaxationVetting(InvoiceMasterID);
            var master = InvoiceMasterRepo.GetData(sql);
            return master;
        }
        private string GetInvoiceMasterSQLForTaxationVetting(int InvoiceMasterID)
        {

            return @$"SELECT    I.*,
                                VA.DepartmentID, 
                                VA.DepartmentName, 
                                VA.FullName AS EmployeeName, 
                                SV.SystemVariableCode AS ApprovalStatus, 
                                VA.ImagePath, 
                                VA.EmployeeCode, 
                                VA.DivisionID, VA.DivisionName,
                                CASE  WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]   ({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit)    ELSE CAST(0 as bit) END IsReassessment,
							    CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned,
                                W.WarehouseName DeliveryLocationName,
                                WorkMobile,
	                            PO.ReferenceNo,
	                            PO.PODate PODate,
	                            PO.DeliveryLocation,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,
	                            IT.SystemVariableCode InventoryTypeName,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,
                                PO.GrandTotal,
                                PO.TotalVatAmount,
                                PO.TotalWithoutVatAmount,

								PR.PRMasterID,
								PR.ReferenceNo PRNo,

								IM.BasePercent PreviousBasePercent,
								IM.BaseAmount PreviousBaseAmount,
								IM.TaxAmount PreviousTaxAmount,
								IM.TotalPayableAmount PreviousPaidAmount,
                                IM.AdvanceDeductionAmount PreviousAdvanceDeductionAmount,

                                ISNULL(AdvanceIM.AdvanceBasePercent,0) PreviousAdvanceBasePercent,
						        ISNULL(AdvanceIM.AdvanceBaseAmount,0) PreviousAdvanceBaseAmount,
						        ISNULL(AdvanceIM.AdvanceTaxAmount,0) PreviousAdvanceTaxAmount,
						        ISNULL(AdvanceIM.AdvanceTotalPayableAmount,0) PreviousAdvanceTotalPayableAmount,

                                C.CurrencyName,
								ITC.SystemVariableCode InvoiceTypeName,
                                PT.SystemVariableCode PaymentTermsName,
						        PT.NumericValue NumberOfDueDay,

                                ISNULL(AdvanceIM.AdvanceBaseAmount,0)-ISNULL(IM.AdvanceDeductionAmount,0) AdvanceDeductionAmount,
                        
	                            ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID,
	                            ISNULL(AP2.ApprovalProcessID, 0) POApprovalProcessID
                           FROM
							InvoiceMaster I
						    LEFT JOIN PurchaseOrderMaster PO ON I.POMasterID=PO.POMasterID
						    LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID=PO.PRMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                            LEFT JOIN Security..Currency C ON C.CurrencyID=I.CurrencyID
                           	LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID AND EntityTypeID =25
							LEFT JOIN Security..SystemVariable ITC ON ITC.SystemVariableID = I.InvoiceTypeID AND ITC.EntityTypeID =29
                            LEFT JOIN  Security..SystemVariable PT ON PT.SystemVariableID=PO.PaymentTermsID AND  PT.EntityTypeID=28
							LEFT JOIN (SELECT 
										SUM(BasePercent) BasePercent,
										SUM(BaseAmount) BaseAmount,
										SUM(TaxAmount) TaxAmount,
										SUM(TotalPayableAmount) TotalPayableAmount,
                                        SUM(AdvanceDeductionAmount) AdvanceDeductionAmount,
										POMasterID 
										FROM InvoiceMaster
										WHERE InvoiceMasterID !={InvoiceMasterID} AND IsAdvanceInvoice = 0
										GROUP BY POMasterID) IM ON IM.POMasterID=PO.POMasterID
                            LEFT JOIN (
									SELECT 
										SUM(BasePercent) AdvanceBasePercent,
										SUM(BaseAmount) AdvanceBaseAmount,
										SUM(TaxAmount) AdvanceTaxAmount,
										SUM(TotalPayableAmount) AdvanceTotalPayableAmount,
										POMasterID 
										FROM InvoiceMaster 
										WHERE InvoiceMasterID !={InvoiceMasterID} AND IsAdvanceInvoice = 1
										GROUP BY POMasterID
							) AdvanceIM ON AdvanceIM.POMasterID=PO.POMasterID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = I.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID =     I.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID =     I.InvoiceMasterID AND AP.APTypeID = {(int)Util.ApprovalType.Invoice} 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID =     PR.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP2 ON AP2.ReferenceID =     PO.POMasterID AND AP2.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.Invoice}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.Invoice} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = I.InvoiceMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = I.InvoiceMasterID
							WHERE I.InvoiceMasterID ={InvoiceMasterID}";
        }

        public async Task<PurchaseOrderMasterDto> GetMasterData(int POMasterID)
        {
            string sql = $@"SELECT
	                    PO.ReferenceNo,
	                    PO.PODate PODate,
	                    PO.DeliveryLocation,
	                    PO.SupplierID,
                        PO.CreditDay,
						S.SupplierName,
	                    WarehouseName Warehouse,
                        PO.CreatedDate POCreatedDate,
                        PO.POMasterID,
                        PO.GrandTotal,
                        PO.TotalVatAmount,
                        PO.TotalWithoutVatAmount,
                        PT.SystemVariableCode PaymentTermsName,
						PT.NumericValue NumberOfDueDay,

						0 BasePercent,
                        0 BaseAmount,
                        0 TaxAmount,
                        0 TotalPayableAmount,
						ISNULL(IM.BasePercent,0) PreviousBasePercent,
						ISNULL(IM.BaseAmount,0) PreviousBaseAmount,
						ISNULL(IM.TaxAmount,0) PreviousTaxAmount,
						ISNULL(IM.TotalPayableAmount,0) PreviousPaidAmount,
                        ISNULL(IM.AdvanceDeductionAmount,0) PreviousAdvanceDeductionAmount,
                
						ISNULL(AdvanceIM.AdvanceBasePercent,0) PreviousAdvanceBasePercent,
						ISNULL(AdvanceIM.AdvanceBaseAmount,0) PreviousAdvanceBaseAmount,
						ISNULL(AdvanceIM.AdvanceTaxAmount,0) PreviousAdvanceTaxAmount,
						ISNULL(AdvanceIM.AdvanceTotalPayableAmount,0) PreviousAdvanceTotalPayableAmount,

                        ISNULL(AdvanceIM.AdvanceBaseAmount,0)-ISNULL(IM.AdvanceDeductionAmount,0) AdvanceDeductionAmount,
                        ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,
                        ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                   FROM 
				    PurchaseOrderMaster PO
                   	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                   	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                    LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                    LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
					LEFT JOIN  Security..SystemVariable PT ON PT.SystemVariableID=PO.PaymentTermsID AND EntityTypeID=28
					LEFT JOIN (
									SELECT 
										SUM(BasePercent) BasePercent,
										SUM(BaseAmount) BaseAmount,
										SUM(TaxAmount) TaxAmount,
										SUM(TotalPayableAmount) TotalPayableAmount,
                                        SUM(AdvanceDeductionAmount) AdvanceDeductionAmount,
										POMasterID 
										FROM InvoiceMaster 
										WHERE ApprovalStatusID <> 24 AND IsAdvanceInvoice = 0
										GROUP BY POMasterID
							) IM ON IM.POMasterID=PO.POMasterID
							LEFT JOIN (
									SELECT 
										SUM(BasePercent) AdvanceBasePercent,
										SUM(BaseAmount) AdvanceBaseAmount,
										SUM(TaxAmount) AdvanceTaxAmount,
										SUM(TotalPayableAmount) AdvanceTotalPayableAmount,
										POMasterID 
										FROM InvoiceMaster 
										WHERE ApprovalStatusID <> 24 AND IsAdvanceInvoice = 1
										GROUP BY POMasterID
							) AdvanceIM ON AdvanceIM.POMasterID=PO.POMasterID
							WHERE PO.POMasterID = {POMasterID}";
            var master = await Task.Run(() => PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql));
            return master;
        }
        #endregion

        #region Advance
        public async Task<List<Dictionary<string, object>>> GetInvoiceChildListOfDict(int InvoiceMasterID)
        {
            string sql = $@"SELECT  
                            POC.ItemID,
                            POC.Qty ReceivedQty,
                            POC.Rate,
                            POC.Amount,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, 
                            I.ItemCode, 
                            POC.Description AS ItemDescription,
                            Unit.UnitCode
                            ,POC.Qty AS POQty
							,POC.VatPercent
							,POC.VATAmount
							,POC.TotalAmountIncludingVat
                    FROM PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN InvoiceMaster IM ON IM.POMasterID = POM.POMasterID
                        LEFT JOIN Item I ON I.ItemID=POC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
						LEFT JOIN PurchaseRequisitionChild PRC ON PRC.PRMasterID = POM.PRMasterID AND PRC.ItemID = POC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE IM.InvoiceMasterID={InvoiceMasterID}";
            var childs = InvoiceChildRepo.GetDataDictCollection(sql);
            return childs.ToList();
        }
        public async Task<List<PurchaseOrderChildDto>> GetChildDataAdvance(int POMasterID)
        {
            string sql = $@"SELECT  
                            POC.ItemID,
                            POC.Qty ReceivedQty,
                            POC.Rate,
                            POC.Amount,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, 
                            I.ItemCode, 
                            POC.Description AS ItemDescription,
                            Unit.UnitCode
                            ,POC.Qty AS POQty
							,POC.VatPercent
							,POC.VATAmount
							,POC.TotalAmountIncludingVat
                    FROM PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID=POC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
						LEFT JOIN PurchaseRequisitionChild PRC ON PRC.PRMasterID = POM.PRMasterID AND PRC.ItemID = POC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE POM.POMasterID={POMasterID}";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseOrderChildDto>(sql);
            return childs;
        }
        #endregion

        #region Regular
        public Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsByPO(int POMasterID)
        {

            var data = (from MR in MaterialReceiveRepo.GetAllList()
                        join IC in InvoiceChildRepo.GetAllList() on MR.MRID equals IC.MRID into MI
                        from IC in MI.DefaultIfEmpty()
                        join IM in InvoiceMasterRepo.GetAllList() on IC?.InvoiceMasterID equals IM.InvoiceMasterID into IMI
                        from IM in IMI.DefaultIfEmpty()
                        where MR.ApprovalStatusID == (int)Util.ApprovalStatus.Approved
                              && MR.POMasterID == POMasterID
                              && (IC == null|| (IM != null && IM.ApprovalStatusID == (int)Util.ApprovalStatus.Rejected))
                        select new MaterialReceiveDto
                        {
                            MRID = MR.MRID,
                            ReferenceNo = MR.ReferenceNo,
                            MRDate = MR.MRDate,
                            ChalanNo = MR.ChalanNo,
                            ChalanDate = MR.ChalanDate,
                            TotalVatAmount = MR.TotalVatAmount,
                            TotalReceivedAmount = MR.TotalReceivedAmount,
                            TotalWithoutVatAmount = MR.TotalWithoutVatAmount,
                            MRItemDetails = (from MRC in MaterialReceiveChildRepo.GetAllList()
                                             join MRM in MaterialReceiveRepo.GetAllList() on MRC.MRID equals MRM.MRID
                                             join PO in (from QC in QCChildRepo.GetAllList()
                                                         join POC in PurchaseOrderChildRepo.GetAllList() on new { QC.POCID, QC.ItemID } equals new { POC.POCID, POC.ItemID }
                                                         select new
                                                         {
                                                             QC.ItemID,
                                                             QC.QCCID,
                                                             POC.POMasterID,
                                                             POC.Description,
                                                             POC.VatPercent
                                                         }) on new { MRM.POMasterID, MRC.QCCID, MRC.ItemID } equals new { PO.POMasterID, PO.QCCID, PO.ItemID }
                                             join I in ItemRepo.GetAllList() on MRC.ItemID equals I.ItemID
                                             join U in GetAllUnits() on I.UnitID equals U.UnitID
                                             where MRM.MRID == MR.MRID
                                             select new MRItemDetails
                                             {
                                                 MRCID = MRC.MRCID,
                                                 ItemName = I.ItemCode + "-" + I.ItemName,
                                                 Description = PO.Description,
                                                 ReceiveQty = MRC.ReceiveQty,
                                                 UnitName = U.UnitCode,
                                                 ItemRate = MRC.ItemRate,
                                                 POVatPercent = PO.VatPercent,
                                                 VatAmount = MRC.VatAmount,
                                                 TotalAmount = MRC.TotalAmount,
                                                 TotalAmountIncludingVat = MRC.TotalAmountIncludingVat
                                             }).ToList()
                        }).ToList();
            return Task.Run(() =>
            {
                return data;
            });
        }
        public Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsByInvoiceID(int InvoiceMasterID)
        {
            var data = (from IC in InvoiceChildRepo.GetAllList()
                        join MR in MaterialReceiveRepo.GetAllList() on IC.MRID equals MR.MRID into MI
                        from MR in MI.DefaultIfEmpty()
                        join QCM in QCMasterRepo.GetAllList() on MR.QCMID equals QCM.QCMID
                        where IC.InvoiceMasterID == InvoiceMasterID
                        select new MaterialReceiveDto
                        {
                            MRID = MR.MRID,
                            QCMID = MR.QCMID,
                            ReferenceNo = MR.ReferenceNo,
                            QCMasterRefNo = QCM.ReferenceNo,
                            MRDate = MR.MRDate,
                            ChalanNo = MR.ChalanNo,
                            ChalanDate = MR.ChalanDate,
                            TotalVatAmount = MR.TotalVatAmount,
                            TotalReceivedAmount = MR.TotalReceivedAmount,
                            TotalWithoutVatAmount = MR.TotalWithoutVatAmount,
                            MRItemDetails = (from MRC in MaterialReceiveChildRepo.GetAllList()
                                             join MRM in MaterialReceiveRepo.GetAllList() on MRC.MRID equals MRM.MRID
                                             join PO in (from QC in QCChildRepo.GetAllList()
                                                         join POC in PurchaseOrderChildRepo.GetAllList() on new { QC.POCID, QC.ItemID } equals new { POC.POCID, POC.ItemID }
                                                         select new
                                                         {
                                                             QC.ItemID,
                                                             QC.QCCID,
                                                             POC.POMasterID,
                                                             POC.Description,
                                                             POC.VatPercent
                                                         }) on new { MRM.POMasterID, MRC.QCCID, MRC.ItemID } equals new { PO.POMasterID, PO.QCCID, PO.ItemID }
                                             join I in ItemRepo.GetAllList() on MRC.ItemID equals I.ItemID
                                             join U in GetAllUnits() on I.UnitID equals U.UnitID
                                             where MRM.MRID == MR.MRID
                                             select new MRItemDetails
                                             {
                                                 MRCID = MRC.MRCID,
                                                 ItemName = I.ItemCode + "-" + I.ItemName,
                                                 Description = PO.Description,
                                                 ReceiveQty = MRC.ReceiveQty,
                                                 UnitName = U.UnitCode,
                                                 ItemRate = MRC.ItemRate,
                                                 POVatPercent = PO.VatPercent,
                                                 VatAmount = MRC.VatAmount,
                                                 TotalAmount = MRC.TotalAmount,
                                                 TotalAmountIncludingVat = MRC.TotalAmountIncludingVat
                                             }).ToList()
                        }).ToList();
            return Task.Run(() =>
            {
                return data;
            });
        }
        public Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsForReassessmentAndView(int POMasterID, int InvoiceMasterID)
        {

            var data = (from MR in MaterialReceiveRepo.GetAllList()
                        join IC in InvoiceChildRepo.GetAllList().Where(x => x.InvoiceMasterID != InvoiceMasterID)
                        on MR.MRID equals IC.MRID into MI
                        from IC in MI.DefaultIfEmpty()
                        where MR.ApprovalStatusID == (int)Util.ApprovalStatus.Approved
                              && MR.POMasterID == POMasterID
                              && IC == null
                        select new MaterialReceiveDto
                        {
                            MRID = MR.MRID,
                            ReferenceNo = MR.ReferenceNo,
                            MRDate = MR.MRDate,
                            ChalanNo = MR.ChalanNo,
                            ChalanDate = MR.ChalanDate,
                            TotalVatAmount = MR.TotalVatAmount,
                            TotalReceivedAmount = MR.TotalReceivedAmount,
                            TotalWithoutVatAmount = MR.TotalWithoutVatAmount,
                            MRItemDetails = (from MRC in MaterialReceiveChildRepo.GetAllList()
                                             join MRM in MaterialReceiveRepo.GetAllList() on MRC.MRID equals MRM.MRID
                                             join PO in (from QC in QCChildRepo.GetAllList()
                                                         join POC in PurchaseOrderChildRepo.GetAllList() on new { QC.POCID, QC.ItemID } equals new { POC.POCID, POC.ItemID }
                                                         select new
                                                         {
                                                             QC.ItemID,
                                                             QC.QCCID,
                                                             POC.POMasterID,
                                                             POC.Description,
                                                             POC.VatPercent
                                                         }) on new { MRM.POMasterID, MRC.QCCID, MRC.ItemID } equals new { PO.POMasterID, PO.QCCID, PO.ItemID }
                                             join I in ItemRepo.GetAllList() on MRC.ItemID equals I.ItemID
                                             join U in GetAllUnits() on I.UnitID equals U.UnitID
                                             where MRM.MRID == MR.MRID
                                             select new MRItemDetails
                                             {
                                                 MRCID = MRC.MRCID,
                                                 ItemName = I.ItemCode + "-" + I.ItemName,
                                                 Description = PO.Description,
                                                 ReceiveQty = MRC.ReceiveQty,
                                                 UnitName = U.UnitCode,
                                                 ItemRate = MRC.ItemRate,
                                                 POVatPercent = PO.VatPercent,
                                                 VatAmount = MRC.VatAmount,
                                                 TotalAmount = MRC.TotalAmount,
                                                 TotalAmountIncludingVat = MRC.TotalAmountIncludingVat
                                             }).ToList()
                        }).ToList();
            return Task.Run(() =>
            {
                return data;
            });

        }
        public Task<List<ComboModel>> GetInvoiceChildList(int InvoiceMasterID)
        {
            return Task.Run(() =>
            {
                return (from IC in InvoiceChildRepo.GetAllList()
                        join MR in MaterialReceiveRepo.GetAllList() on IC.MRID equals MR.MRID
                        where IC.InvoiceMasterID == InvoiceMasterID
                        select new ComboModel { value = (int)MR.MRID, label = MR.ReferenceNo }).ToList();
            });
        }
        private List<UnitDto> GetAllUnits()
        {
            var unitSql = "SELECT * FROM Security..Unit";
            var unitDic = InvoiceMasterRepo.GetDataDictCollection(unitSql);
            var unitList = new List<UnitDto>();
            foreach (var unit in unitDic)
            {
                unitList.Add(new UnitDto
                {
                    UnitID = (int)unit["UnitID"],
                    UnitCode = unit["UnitCode"].ToString(),
                    UnitShortCode = unit["UnitShortCode"].ToString()

                });
            }
            return unitList;
        }
        #endregion

        #region Save Invoice & Releted Data
        public async Task<(bool, string)> SaveInvoice(InvoiceDto IV)
        {
            if (IV.InvoiceDate > DateTime.Now || IV.InvoiceReceiveDate > DateTime.Now || IV.MushakChalanDate > DateTime.Now)
            {
                return (false, "You can't select future date.");
            }
            var existingIV = InvoiceMasterRepo.Entities.Where(x => x.InvoiceMasterID == IV.InvoiceMasterID).SingleOrDefault();
            var removeList = RemoveAttachments(IV);

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (IV.InvoiceMasterID > 0 && IV.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT  {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                            FROM InvoiceMaster IV
                            LEFT JOIN Security..Users U ON U.UserID = IV.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = IV.InvoiceMasterID AND AP.APTypeID = {(int)Util.ApprovalType.Invoice}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 10 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 10 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IV.InvoiceMasterID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {IV.ApprovalProcessID}";
                var canReassesstment = InvoiceMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Invoice once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Invoice once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)IV.InvoiceMasterID, IV.ApprovalProcessID, (int)Util.ApprovalType.Invoice);
            }

            string ApprovalProcessID = "0";



            using (var unitOfWork = new UnitOfWork())
            {
                var masterModel = new InvoiceMaster
                {
                    InvoiceMasterID = IV.InvoiceMasterID,
                    ReferenceNo = GenerateInvoiceReference(IV.IsAdvanceInvoice ? "ADV" : "REG"),
                    InvoiceNo = IV.InvoiceNo,
                    InvoiceReceiveDate = IV.InvoiceReceiveDate,
                    InvoiceDate = IV.InvoiceDate,
                    POMasterID = IV.POMasterID,
                    AccountingDate = IV.AccountingDate,
                    InvoiceTypeID = IV.InvoiceTypeID,
                    CurrencyID = IV.CurrencyID,
                    IsAdvanceInvoice = IV.IsAdvanceInvoice,
                    InvoiceDescription = IV.InvoiceDescription,
                    SupplierID = IV.SupplierID,
                    CurrencyRate = IV.CurrencyRate,
                    ProjectNumber = IV.ProjectNumber,
                    ExternalID = IV.ExternalID,
                    BaseAmount = IV.BaseAmount,
                    BasePercent = IV.BasePercent,
                    TaxAmount = IV.TaxAmount,
                    TaxPercent = IV.TaxPercent,
                    TotalPayableAmount = IV.TotalPayableAmount,
                    AdvanceDeductionAmount = IV.AdvanceDeductionAmount,
                    GracePeriod = IV.GracePeriod,
                    MushakChalanNo = IV.MushakChalanNo,
                    MushakChalanDate = IV.MushakChalanDate
                };
                if (IV.InvoiceMasterID.IsZero() && existingIV.IsNull())
                {
                    masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetInvoiceMasterNewId(masterModel);
                    IV.InvoiceMasterID = (int)masterModel.InvoiceMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingIV.CreatedBy;
                    masterModel.CreatedDate = existingIV.CreatedDate;
                    masterModel.CreatedIP = existingIV.CreatedIP;
                    masterModel.RowVersion = existingIV.RowVersion;
                    masterModel.ApprovalStatusID = existingIV.ApprovalStatusID;
                    masterModel.SetModified();
                }
                var childModel = GenerateInvoiceChild(IV);
                var sccChilds = GenerateInvoiceChildSCC(IV);
                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                SetAuditFields(sccChilds);


                if (IV.Attachments.IsNotNull() && IV.Attachments.Count > 0)
                {
                    var vendorInvoiceAtt = IV.Attachments.Where(p => p.ParentFUID == (int)InvoiceDocumentType.VendorInvoice);
                    if (vendorInvoiceAtt.Count() == 0)
                    {
                        return (false, "Please Upload Vendor Invoice.");
                    }
                    var mushokChalanAtt = IV.Attachments.Where(x => x.ParentFUID == (int)InvoiceDocumentType.MusokChalan);
                    if (mushokChalanAtt.Count() == 0)
                    {
                        return (false, "Please Upload Musok Chalan.");
                    }

                    //var attachmentList = AddAttachments(IV.Attachments.Where(x => x.ID == 0).ToList());
                    var attachmentList = AddAttachments(IV.Attachments.ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            bool IsUpdated = attachemnt.ID > 0 ? true : false;
                            if (IsUpdated == false)
                                SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)IV.InvoiceMasterID, "InvoiceMaster", false, attachemnt.Size, attachemnt.ParentFUID, false, attachemnt.Description ?? "", IsUpdated);
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)IV.InvoiceMasterID, "InvoiceMaster", true, attachemnt.Size, attachemnt.ParentFUID, false, attachemnt.Description ?? "");
                        }
                    }
                }


                InvoiceMasterRepo.Add(masterModel);
                InvoiceChildRepo.AddRange(childModel);
                InvoiceChildSCCRepo.AddRange(sccChilds);

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.InvoiceApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Invoice No:{masterModel.ReferenceNo}";
                    var obj = CreateApprovalProcessForLimit((int)masterModel.InvoiceMasterID, Util.AutoInvoiceAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.Invoice, masterModel.TotalPayableAmount, "0");
                    ApprovalProcessID = obj.ApprovalProcessID;
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
                unitOfWork.CommitChangesWithAudit();

                if (ApprovalProcessID.ToInt() > 0)
                    //await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.InvoiceMasterID, (int)Util.MailGroupSetup.InvoiceInitiatedMail, (int)Util.ApprovalType.Invoice);

            }
            await Task.CompletedTask;

            return (true, $"Invoice Submitted Successfully");
        }
        private List<InvoiceChild> GenerateInvoiceChild(InvoiceDto invoice)
        {
            var childList = new List<InvoiceChild>();
            if (invoice.MaterialReceiveIDs.IsNotNull() && invoice.MaterialReceiveIDs.Count > 0)
            {
                foreach (var mr in invoice.MaterialReceiveIDs)
                {
                    childList.Add(new InvoiceChild
                    {
                        InvoiceMasterID = invoice.InvoiceMasterID,
                        MRID = mr
                    });
                }
                childList.ForEach(x => { x.SetAdded(); SetInvoiceChildNewId(x); });
                var removeList = InvoiceChildRepo.Entities.Where(x => x.InvoiceMasterID == invoice.InvoiceMasterID).ToList();
                removeList.ForEach(x =>
                {
                    x.SetDeleted();
                    childList.Add(x);
                });
            }
            return childList;
        }
        private List<InvoiceChildSCC> GenerateInvoiceChildSCC(InvoiceDto invoice)
        {
            var childList = new List<InvoiceChildSCC>();
            if (invoice.SCCIDs.IsNotNull() && invoice.SCCIDs.Count > 0)
            {
                foreach (var mr in invoice.SCCIDs)
                {
                    childList.Add(new InvoiceChildSCC
                    {
                        InvoiceMasterID = invoice.InvoiceMasterID,
                        SCCMID = mr
                    });
                }
                childList.ForEach(x => { x.SetAdded(); SetInvoiceChildNewId(x); });
                var removeList = InvoiceChildSCCRepo.Entities.Where(x => x.InvoiceMasterID == invoice.InvoiceMasterID).ToList();
                removeList.ForEach(x =>
                {
                    x.SetDeleted();
                    childList.Add(x);
                });
            }
            return childList;
        }
        private void SetInvoiceMasterNewId(InvoiceMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("InvoiceMaster", AppContexts.User.CompanyID);
            master.InvoiceMasterID = code.MaxNumber;
        }
        private void SetInvoiceChildNewId(InvoiceChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("InvoiceChild", AppContexts.User.CompanyID);
            child.InvoiceChildID = code.MaxNumber;
        }
        private void SetInvoiceChildNewId(InvoiceChildSCC child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("InvoiceChildSCC", AppContexts.User.CompanyID);
            child.InvoiceChildSCCID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private List<Attachment> AddAttachments(List<Attachment> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull() && attachment.ID == 0)
                    {
                        string filename = $"Invoice-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "Invoice\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        attachemntList.Add(new Attachment
                        {
                            FilePath = filePath,
                            OriginalName = Path.GetFileNameWithoutExtension(attachment.OriginalName),
                            FileName = filename,
                            Type = Path.GetExtension(attachment.OriginalName),
                            Size = attachment.Size,
                            Description = attachment.Description,
                            ParentFUID = attachment.ParentFUID
                        });

                        sl++;
                    }
                    else
                    {
                        attachemntList.Add(attachment);
                    }

                }
                return attachemntList;
            }
            return null;
        }
        private List<Attachment> RemoveAttachments(InvoiceDto PR)
        {
            if (PR.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='InvoiceMaster' AND ReferenceID={PR.InvoiceMasterID}";
                var prevAttachment = InvoiceMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !PR.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "Invoice";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        #endregion

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
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.Invoice);
            return comments.Result;
        }
        public IEnumerable<Dictionary<string, object>> InvoiceApprovalFeedback(int InvoiceMasterID)
        {
            string sql = $@" EXEC SCM..spRPTInvoiceApprovalFeedback {InvoiceMasterID}";
            var feedback = MaterialReceiveRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int InvoiceMasterID)
        {
            string attachmentSql = $@"SELECT F.*,CASE when S.SystemVariableCode IS NULL then '' Else S.SystemVariableCode END AS DocumentType 
                                            FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload F
                                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable S ON F.ParentFUID=S.SystemVariableID
                                            where F.TableName='InvoiceMaster' AND F.ReferenceID={InvoiceMasterID}";
            var attachment = InvoiceChildRepo.GetDataDictCollection(attachmentSql);
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
                    ParentFUID = (int)data["ParentFUID"],
                    DocumentType = data["DocumentType"].ToString(),
                    Description = data["Description"].ToString()
                });
            }
            return attachemntList;
        }
        private string GenerateInvoiceReference(string invoiceType)
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/INV/{invoiceType}/{ GenerateSystemCode("InovoiceRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        public GridModel GetRegularInvoiceSccListForGrid(GridParameter parameters)
        {
            string sql = $@"SELECT 	                            
	                            PO.PODate,
	                            PO.DeliveryLocation WarehouseID,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,	                            
								PO.CreatedDate,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,								
								PR.ReferenceNo PRNo,
								PR.PRMasterID,
								ISNULL(TotalAmount,0) - ISNULL(TotalInvoiceAmount,0) DueAmount
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                           FROM 
                           	PurchaseOrderMaster PO   
							LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
                           	LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                           	LEFT JOIN Warehouse W ON W.WarehouseID = PO.DeliveryLocation
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
							INNER JOIN (
										   SELECT 
											 SCC.POMasterID,
											 SUM(SCC.SCCAmount) TotalAmount
										   FROM 
												SCCMaster SCC
                                             INNER JOIN SCCChild SCCC ON SCCC.SCCMID = SCC.SCCMID
										  
										    WHERE SCC.ApprovalStatusID = 23
										   GROUP BY SCC.POMasterID
									)MRInv ON MRInv.POMasterID = PO.POMasterID
							LEFT JOIN (
								SELECT 
									  SUM(TotalPayableAmount)+SUM(TaxAmount) TotalInvoiceAmount,
									 POMasterID
								FROM 
									InvoiceMaster
									WHERE ApprovalStatusID <> 24
									GROUP BY POMasterID
							)Invoice ON Invoice.POMasterID = PO.POMasterID
                            LEFT JOIN (															
								SELECT DISTINCT
									MR.POMasterID
								FROM 
									MaterialReceive MR
									LEFT JOIN InvoiceChild IC ON MR.MRID = IC.MRID
									LEFT JOIN InvoiceMaster IM ON IC.InvoiceMasterID = IM.InvoiceMasterID
									WHERE IM.ApprovalStatusID <> 24
							)InM ON InM.POMasterID = PO.POMasterID
							WHERE CAST(ISNULL(TotalAmount,0) AS DECIMAL(18,0)) >= CAST(ISNULL(TotalInvoiceAmount,0) AS DECIMAL(18,0)) AND InM.POMasterID IS NULL  {parameters.ApprovalFilterData}";

            var result = MaterialReceiveRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public Task<List<SCCDto>> SCCReceiveMasterDetailsByPO(int POMasterID)
        {

            var data = (from SCC in SCCMasterRepo.GetAllList()
                            //join SCCC in SCCChildRepo.GetAllList() on SCC.SCCMID equals SCCC.SCCMID
                        join IC in InvoiceChildSCCRepo.GetAllList() on SCC.SCCMID equals IC.SCCMID into MI
                        from IC in MI.DefaultIfEmpty()
                        join IM in InvoiceMasterRepo.GetAllList() on IC?.InvoiceMasterID equals IM.InvoiceMasterID into IMI
                        from IM in IMI.DefaultIfEmpty()
                        where SCC.ApprovalStatusID == (int)Util.ApprovalStatus.Approved
                              && SCC.POMasterID == POMasterID
                              && (IC == null || (IM != null && IM.ApprovalStatusID == (int)Util.ApprovalStatus.Rejected))
                        select new SCCDto
                        {
                            SCCMID = SCC.SCCMID,
                            ReferenceNo = SCC.ReferenceNo,
                            InvoiceNoFromVendor = SCC.InvoiceNoFromVendor,
                            InvoiceDateFromVendor = SCC.InvoiceDateFromVendor,
                            //TotalVatAmount = Math.Round(SCCC.VatAmount, 4),
                            //TotalReceivedAmount = Math.Round((SCC.PaymentFixedOrPercentTotalAmount + SCCC.VatAmount), 4),
                            TotalWithoutVatAmount = Math.Round(SCC.PaymentFixedOrPercentTotalAmount, 4),
                            SccDate = SCC.CreatedDate,
                            SCCItemDetails = (from SCCC in SCCChildRepo.GetAllList()
                                              join SCCM in SCCMasterRepo.GetAllList() on SCCC.SCCMID equals SCCM.SCCMID
                                              join POC in PurchaseOrderChildRepo.GetAllList() on SCCC.POCID equals POC.POCID
                                              join I in ItemRepo.GetAllList() on SCCC.ItemID equals I.ItemID
                                              join U in GetAllUnits() on I.UnitID equals U.UnitID
                                              where SCCM.SCCMID == SCC.SCCMID
                                              select new SCCItemDetails
                                              {
                                                  SCCCID = SCCC.SCCCID,
                                                  ItemName = I.ItemCode + "-" + I.ItemName,
                                                  Description = POC.Description,
                                                  ReceivedQty = (int)SCCC.ReceivedQty,
                                                  UnitName = U.UnitCode,
                                                  ItemRate = SCCC.Rate,
                                                  POVatPercent = POC.VatPercent,
                                                  VatAmount = Math.Round(SCCC.VatAmount, 4),
                                                  TotalAmount = Math.Round(SCCC.TotalAmount, 4),
                                                  TotalAmountIncludingVat = Math.Round(SCCC.TotalAmountIncludingVat, 4)
                                              }).ToList(),
                            TotalVatAmount = (from SCCC in SCCChildRepo.GetAllList()
                                              join SCCM in SCCMasterRepo.GetAllList() on SCCC.SCCMID equals SCCM.SCCMID
                                              join POC in PurchaseOrderChildRepo.GetAllList() on SCCC.POCID equals POC.POCID
                                              join I in ItemRepo.GetAllList() on SCCC.ItemID equals I.ItemID
                                              join U in GetAllUnits() on I.UnitID equals U.UnitID
                                              where SCCM.SCCMID == SCC.SCCMID
                                              select SCCC.VatAmount).Sum()
                        }).ToList();

            return Task.Run(() =>
            {
                return data;
            });
        }

        public Task<List<SCCDto>> SCCMasterDetailsByInvoiceID(int InvoiceMasterID)
        {

            var data = (from SCC in SCCMasterRepo.GetAllList()
                        join SCCC in SCCChildRepo.GetAllList() on SCC.SCCMID equals SCCC.SCCMID
                        join IC in InvoiceChildSCCRepo.GetAllList() on new { SCC.SCCMID } equals new { IC.SCCMID }
                        where IC.InvoiceMasterID == InvoiceMasterID
                        select new SCCDto
                        {
                            SCCMID = SCC.SCCMID,
                            ReferenceNo = SCC.ReferenceNo,
                            InvoiceNoFromVendor = SCC.InvoiceNoFromVendor,
                            InvoiceDateFromVendor = SCC.InvoiceDateFromVendor,
                            TotalVatAmount = Math.Round(SCCC.VatAmount, 4),
                            TotalReceivedAmount = Math.Round(SCC.PaymentFixedOrPercentTotalAmount + SCCC.VatAmount, 4),
                            TotalWithoutVatAmount = Math.Round(SCC.PaymentFixedOrPercentTotalAmount, 4),
                            SccDate = SCC.CreatedDate,
                            SCCItemDetails = (from SCCC in SCCChildRepo.GetAllList()
                                              join SCCM in SCCMasterRepo.GetAllList() on SCCC.SCCMID equals SCCM.SCCMID
                                              join POC in PurchaseOrderChildRepo.GetAllList() on SCCC.POCID equals POC.POCID
                                              join I in ItemRepo.GetAllList() on SCCC.ItemID equals I.ItemID
                                              join U in GetAllUnits() on I.UnitID equals U.UnitID
                                              where SCCM.SCCMID == SCC.SCCMID
                                              select new SCCItemDetails
                                              {
                                                  SCCCID = SCCC.SCCCID,
                                                  ItemName = I.ItemCode + "-" + I.ItemName,
                                                  Description = POC.Description,
                                                  ReceivedQty = (int)SCCC.ReceivedQty,
                                                  UnitName = U.UnitCode,
                                                  ItemRate = Math.Round(SCCC.Rate, 4),
                                                  POVatPercent = POC.VatPercent,
                                                  VatAmount = Math.Round(SCCC.VatAmount, 4),
                                                  TotalAmount = Math.Round(SCCC.TotalAmount, 4),
                                                  TotalAmountIncludingVat = Math.Round(SCCC.TotalAmountIncludingVat, 4)
                                              }).ToList()
                        }).ToList();
            return Task.Run(() =>
            {
                return data;
            });

        }

        public int GetExistSccChild(int InvoiceMasterID)
        {
            var isExist = InvoiceChildSCCRepo.GetAllList().FirstOrDefault(x => x.InvoiceMasterID == InvoiceMasterID);
            int count = isExist.IsNotNull() ? 1 : 0;
            return count;
        }

        public Task<List<ComboModel>> GetSccInvoiceChildList(int InvoiceMasterID)
        {
            return Task.Run(() =>
            {
                return (from IC in InvoiceChildSCCRepo.GetAllList()
                        join MR in SCCMasterRepo.GetAllList() on IC.SCCMID equals MR.SCCMID
                        where IC.InvoiceMasterID == InvoiceMasterID
                        select new ComboModel { value = (int)MR.SCCMID, label = MR.ReferenceNo }).ToList();
            });
        }


    }
}
