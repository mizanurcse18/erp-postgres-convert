using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace SCM.Manager.Implementations
{
    public class MaterialReceiveManager : ManagerBase, IMaterialReceiveManager
    {

        private readonly IRepository<MaterialReceive> MaterialReceiveRepo;
        private readonly IRepository<MaterialReceiveChild> MaterialReceiveChildRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<QCMaster> QCMasterRepo;
        private readonly IRepository<InventoryTransaction> InventoryTransactionRepo;
        public MaterialReceiveManager(IRepository<MaterialReceive> materialReceiveMasterRepo, IRepository<MaterialReceiveChild> materialReceiveChildRepo, IRepository<Item> itemRepo, IRepository<InventoryTransaction> inventoryTransactionRepo, IRepository<QCMaster> qcMasterRepo)
        {
            MaterialReceiveRepo = materialReceiveMasterRepo;
            MaterialReceiveChildRepo = materialReceiveChildRepo;
            ItemRepo = itemRepo;
            InventoryTransactionRepo = inventoryTransactionRepo;
            QCMasterRepo = qcMasterRepo;
        }

        private void SetMaterialReceiveNewId(MaterialReceive master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("MaterialReceive", AppContexts.User.CompanyID);
            master.MRID = code.MaxNumber;
        }

        private void SetInventoryTransactionNewId(InventoryTransaction tran)
        {
            //if (!tran.IsAdded) return;
            var code = GenerateSystemCode("InventoryTransaction", AppContexts.User.CompanyID);
            tran.ITID = code.MaxNumber;
        }

        private List<MaterialReceiveChild> GenerateMaterialReceiveChild(MaterialReceiveDto MR)
        {
            var existingMaterialReceiveChild = MaterialReceiveChildRepo.Entities.Where(x => x.MRID == MR.MRID).ToList();
            var childModel = new List<MaterialReceiveChild>();
            if (MR.MRItemDetails.IsNotNull())
            {
                MR.MRItemDetails.ForEach(x =>
                {
                    if (x.ReceiveQty > 0)
                    {
                        childModel.Add(new MaterialReceiveChild
                        {
                            MRCID = x.MRCID,
                            MRID = MR.MRID,
                            //POCID = x.POCID,
                            ItemID = x.ItemID,
                            ReceiveQty = x.ReceiveQty,
                            ItemRate = x.ItemRate,
                            TotalAmount = x.TotalAmount,
                            QCCID = x.QCCID,
                            TotalAmountIncludingVat = x.TotalAmountIncludingVat,
                            VatAmount = x.VatAmount
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingMaterialReceiveChild.Count > 0 && x.MRCID > 0)
                    {
                        var existingModelData = existingMaterialReceiveChild.FirstOrDefault(y => y.MRCID == x.MRCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.MRID = MR.MRID;
                        x.SetAdded();
                        SetMaterialReceiveChildNewId(x);
                    }
                });

                var willDeleted = existingMaterialReceiveChild.Where(x => !childModel.Select(y => y.MRCID).Contains(x.MRCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private List<InventoryTransaction> GenerateInventoryTransaction(MaterialReceiveDto MR)
        {

            var existingInTranChild = InventoryTransactionRepo.Entities.Where(x => x.MRID == MR.MRID).ToList();
            var childModel = new List<InventoryTransaction>();
            if (MR.MRItemDetails.IsNotNull())
            {
                MR.MRItemDetails.ForEach(x =>
                {

                    if (x.ReceiveQty > 0)
                    {
                        childModel.Add(new InventoryTransaction
                        {
                            WarehouseID = MR.WarehouseID,
                            PRMasterID = MR.PRMasterID,
                            POMasterID = MR.POMasterID,
                            MRID = MR.MRID,
                            TransferID = 0,
                            StoreRequestID = 0,
                            ItemID = x.ItemID,
                            BatchNo = 0,
                            TransactionQty = x.ReceiveQty,
                            ItemRate = x.ItemRate,
                            InOrOut = "IN",
                            IsTransfer = false
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingInTranChild.Count > 0 && x.ITID > 0)
                    {
                        var existingModelData = existingInTranChild.FirstOrDefault(y => y.ITID == x.ITID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.MRID = MR.MRID;
                        x.SetAdded();
                        SetInventoryTransactionNewId(x);
                    }
                });

                var willDeleted = existingInTranChild.Where(x => !childModel.Select(y => y.ITID).Contains(x.ITID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private void SetMaterialReceiveChildNewId(MaterialReceiveChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("MaterialReceiveChild", AppContexts.User.CompanyID);
            child.MRCID = code.MaxNumber;
        }

        private string GenerateMaterialReceiveReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/GRN/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("MaterialReceiveRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        public GridModel GetMaterialReceiveList(GridParameter parameters)
        {
            string filter = "";
            //string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            //where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT 
                            DISTINCT
	                            MR.CreatedDate
								,MR.MRID
								,MR.MRDate
								,MR.ApprovalStatusID
								,MR.ReferenceNo
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,ISNULL(PO.ReferenceNo, '') AS PONo
                                ,ISNULL(PR.ReferenceNo, '') AS PRNo
                                ,S.SupplierName
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                --,ISNULL(CountQC, 0) CountQC
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,PO.POMasterID
                                ,PR.PRMasterID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                                ,ISNULL(APPR.ApprovalProcessID, 0) PRApprovalProcessID

                            FROM MaterialReceive MR
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID

                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN Approval..ApprovalProcess APPR ON APPR.ReferenceID = PO.PRMasterID AND APPR.APTypeID = {(int)Util.ApprovalType.PR} 

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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
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
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.GRN} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = MR.MRID WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = MaterialReceiveRepo.LoadGridModelOptimized(parameters, sql);

            return result;
        }

        public GridModel GetAllGRNList(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT 
                            DISTINCT
	                            MR.CreatedDate
								,MR.MRID
								,MR.MRDate
								,MR.ApprovalStatusID
								,MR.ReferenceNo
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,S.SupplierName
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                --,ISNULL(CountQC, 0) CountQC
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,PO.POMasterID
                                ,PR.PRMasterID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                                ,ISNULL(APPR.ApprovalProcessID, 0) PRApprovalProcessID
                            FROM MaterialReceive MR
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID

                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN Approval..ApprovalProcess APPR ON APPR.ReferenceID = PO.PRMasterID AND APPR.APTypeID = {(int)Util.ApprovalType.PR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
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
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.GRN} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = MR.MRID
                                    {where} {filter}
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = MaterialReceiveRepo.LoadGridModelOptimized(parameters, sql);

            return result;
        }


        public async Task<GridModel> GetMaterialReceiveListForService(GridParameter parameters)
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
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            MR.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
								,PO.InventoryTypeID
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,S.SupplierName
                            FROM MaterialReceive MR
							LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID

                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID

                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
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
							WHERE  PO.InventoryTypeID = {(int)Util.InventoryType.Services} AND (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";

            var result = MaterialReceiveRepo.LoadGridModel(parameters, sql);
            return result;
        }


        public async Task<MaterialReceiveMasterDto> GetMaterialReceiveMasterForService(int MRID)
        {
            string sql = $@"SELECT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,(SELECT Security.dbo.NumericToBDT(MR.TotalReceivedAmount)) AmountInWords
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                        from MaterialReceive MR
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON MR.POMasterID = MR.POMasterID
						LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
                        WHERE MR.MRID={MRID} AND  PO.InventoryTypeID = {(int)Util.InventoryType.Services}";
            var master = MaterialReceiveRepo.GetModelData<MaterialReceiveMasterDto>(sql);
            return master;
        }
        

        public async Task<MaterialReceiveMasterDto> GetMaterialReceiveMasterFromAllList(int MRID)
        {
            string sql = $@"SELECT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,(SELECT Security.dbo.NumericToBDT(MR.TotalReceivedAmount)) AmountInWords
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
							,QC.ReferenceNo AS QCNo
                            ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                        from MaterialReceive MR
						LEFT JOIN QCMaster QC ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
						LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
                        LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
                        WHERE MR.MRID={MRID}";
            var master = MaterialReceiveRepo.GetModelData<MaterialReceiveMasterDto>(sql);
            return master;
        }

        public async Task<MaterialReceiveMasterDto> GetMaterialReceiveMaster(int MRID)
        {
            string sql = $@"SELECT DISTINCT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,(SELECT Security.dbo.NumericToBDT(MR.TotalReceivedAmount)) AmountInWords
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
							,QC.ReferenceNo AS QCNo
                            ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                        from MaterialReceive MR
						LEFT JOIN QCMaster QC ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
						LEFT JOIN Supplier S ON MR.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
                        LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.GRN}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE MR.MRID={MRID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = MaterialReceiveRepo.GetModelData<MaterialReceiveMasterDto>(sql);
            return master;
        }

        public async Task<List<MaterialReceiveChildDto>> GetMaterialReceiveChild(int MRID)
        {
            string sql = $@"SELECT  MRC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, I.ItemCode,Unit.UnitCode
					        ,ISNULL(PO.ReceivedQty,0) AS GRNReceivedQty
                            ,POC.Qty AS POQty
							,QCC.QCCNote
							,POC.Description AS PODescription
							,POC.VatPercent POVatPercent
                    FROM MaterialReceiveChild MRC
                        LEFT JOIN MaterialReceive MRM ON MRM.MRID = MRC.MRID
						LEFT JOIN (	SELECT MRC1.ItemID,SUM(MRC1.ReceiveQty) ReceivedQty FROM MaterialReceiveChild MRC1
									JOIN MaterialReceive MR1 ON MRC1.MRID=MR1.MRID 
									LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = MR1.POMasterID
									LEFT JOIN Security..Unit U ON U.UnitID = POC.UOM
									WHERE MR1.ApprovalStatusID	<> 24 AND MR1.MRID!={MRID}
									GROUP BY MRC1.ItemID) PO ON MRC.ItemID=PO.ItemID
                        LEFT JOIN Item I ON I.ItemID=MRC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
						LEFT JOIN QCChild QCC ON QCC.QCCID=MRC.QCCID AND MRC.QCCID = QCC.QCCID
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = MRM.POMasterID AND POC.ItemID = MRC.ItemID AND POC.POCID = QCC.POCID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
                        WHERE MRC.MRID={MRID}";
            var childs = MaterialReceiveChildRepo.GetDataModelCollection<MaterialReceiveChildDto>(sql);
            return childs;
        }

        public async Task<List<ManualApprovalPanelEmployeeDto>> GetGRNApprovalPanelDefault(int MRID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={MRID} AND APE.APTypeID ={(int)Util.ApprovalType.GRN}";

            var maps = QCMasterRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return maps;
        }


        public async Task<List<MaterialReceiveChildDto>> GetMaterialReceiveChildForService(int MRID)
        {
            string sql = $@"SELECT  MRC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, I.ItemCode,Unit.UnitCode,
					        ISNULL(PO.ReceivedQty,0) AS ReceivedQty,
					        ISNULL(PO.UnReceived,0) - ISNULL(MRC.ReceiveQty, 0) AS BalanceQty
                            ,POC.Qty AS POQty
                    FROM MaterialReceiveChild MRC
                        LEFT JOIN MaterialReceive MRM ON MRM.MRID = MRC.MRID
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = MRM.POMasterID
						LEFT JOIN (	SELECT MRC1.ItemID, U.UnitCode,SUM(MRC1.ReceiveQty) ReceivedQty,SUM(POC.Qty) - SUM(MRC1.ReceiveQty) UnReceived FROM MaterialReceiveChild MRC1
									JOIN MaterialReceive MR1 ON MRC1.MRID=MR1.MRID 
									LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = MR1.POMasterID
									LEFT JOIN Security..Unit U ON U.UnitID = POC.UOM
									WHERE MR1.ApprovalStatusID	<> 24 AND MR1.MRID!={MRID}
									GROUP BY MRC1.ItemID, U.UnitCode) PO ON MRC.ItemID=PO.ItemID
                        LEFT JOIN Item I ON I.ItemID=MRC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = MRM.POMasterID AND POC.ItemID = MRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
                        WHERE MRC.MRID={MRID} AND  POM.InventoryTypeID = {(int)Util.InventoryType.Services}";
            var childs = MaterialReceiveChildRepo.GetDataModelCollection<MaterialReceiveChildDto>(sql);
            return childs;
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
                        string filename = $"MaterialReceive-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "MaterialReceive\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(MaterialReceiveDto MR)
        {
            if (MR.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialReceive' AND ReferenceID={MR.MRID}";
                var prevAttachment = MaterialReceiveRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !MR.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "MaterialReceive";
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

        public async Task<(bool, string)> SaveChanges(MaterialReceiveDto MR)
        {
            if (MR.MRDate > DateTime.Now)
            {
                return (false, "You can't select future date.");
            }

            var existingMR = MaterialReceiveRepo.Entities.Where(x => x.MRID == MR.MRID).SingleOrDefault();

            if (MR.MRID > 0 && (existingMR.IsNullOrDbNull() || existingMR.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this GRN.");
            }

            foreach (var item in MR.Attachments)
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

            var removeList = RemoveAttachments(MR);

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (MR.MRID > 0 && MR.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM MaterialReceive MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRID AND AP.APTypeID = {(int)Util.ApprovalType.GRN}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.GRN} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.GRN} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {MR.ApprovalProcessID}";
                var canReassesstment = MaterialReceiveRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit GRN once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit GRN once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)MR.MRID, MR.ApprovalProcessID, (int)Util.ApprovalType.GRN);
            }

            string ApprovalProcessID = "0";
            //decimal rcvQty = (decimal)MR.MRItemDetails.Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum();
            //decimal rcvAmt = (decimal)MR.MRItemDetails.Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum() * (decimal)MR.MRItemDetails.Select(x => x.ItemRate).DefaultIfEmpty(0).Sum();
            //decimal rcvRate = rcvAmt / rcvQty;



            var masterModel = new MaterialReceive
            {
                WarehouseID = MR.WarehouseID,
                SupplierID = MR.SupplierID,
                PRMasterID = MR.PRMasterID,
                POMasterID = MR.POMasterID,
                QCMID = MR.QCMID,
                TotalReceivedQty = (decimal)MR.MRItemDetails.Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum(),
                TotalReceivedAmount = (decimal)MR.MRItemDetails.Select(x => x.TotalAmountIncludingVat).DefaultIfEmpty(0).Sum(),
                TotalReceivedAvgRate = (decimal)MR.MRItemDetails.Select(x => x.TotalAmountIncludingVat).DefaultIfEmpty(0).Sum() / (decimal)MR.MRItemDetails.Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum(),
                ReferenceKeyword = MR.ReferenceKeyword,
                ChalanNo = MR.ChalanNo,
                ChalanDate = MR.ChalanDate,
                IsDraft = MR.IsDraft,

                TotalWithoutVatAmount = (decimal)MR.MRItemDetails.Select(x => x.TotalAmount).DefaultIfEmpty(0).Sum(),
                TotalVatAmount = (decimal)MR.MRItemDetails.Select(x => x.VatAmount).DefaultIfEmpty(0).Sum(),

                ApprovalStatusID = MR.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };


            using (var unitOfWork = new UnitOfWork())
            {
                if (MR.MRID.IsZero() && existingMR.IsNull())
                {
                    masterModel.MRDate = DateTime.Now;
                    //masterModel.ReferenceNo = GenerateMaterialReceiveReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.ReferenceNo = MR.GRNNo;
                    masterModel.SetAdded();
                    SetMaterialReceiveNewId(masterModel);
                    MR.MRID = (int)masterModel.MRID;
                }
                else
                {
                    masterModel.CreatedBy = existingMR.CreatedBy;
                    masterModel.CreatedDate = existingMR.CreatedDate;
                    masterModel.CreatedIP = existingMR.CreatedIP;
                    masterModel.RowVersion = existingMR.RowVersion;
                    masterModel.MRID = MR.MRID;
                    masterModel.ReferenceNo = existingMR.ReferenceNo;
                    masterModel.MRDate = existingMR.MRDate;
                    masterModel.BudgetPlanRemarks = existingMR.BudgetPlanRemarks;
                    masterModel.ApprovalStatusID = existingMR.ApprovalStatusID;
                    masterModel.SetModified();
                }
                var childModel = GenerateMaterialReceiveChild(MR);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                if (MR.Attachments.IsNotNull() && MR.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(MR.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)MR.MRID, "MaterialReceive", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)MR.MRID, "MaterialReceive", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.GRNApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, GRN Reference No:{masterModel.ReferenceNo}";


                    if (MR.GRNApprovalPanelList.IsNotNull() && MR.GRNApprovalPanelList.Count > 0)
                    {
                        foreach (var item in MR.GRNApprovalPanelList)
                        {
                            SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.GRNBelowTheLimit, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.GRN, (int)masterModel.MRID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
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


                MaterialReceiveRepo.Add(masterModel);
                MaterialReceiveChildRepo.AddRange(childModel);
                unitOfWork.CommitChangesWithAudit();


                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.GRNApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, GRN Reference No:{masterModel.ReferenceNo}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.MRID, Util.AutoGRNAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.GRN, (int)Util.ApprovalPanel.GRNBelowTheLimit, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                }

                if (ApprovalProcessID.ToInt() > 0)
                    // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.MRID, (int)Util.MailGroupSetup.GRNInitiatedMail, (int)Util.ApprovalType.GRN);
            }
            await Task.CompletedTask;

            return (true, $"GRN Submitted Successfully");
        }


        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.GRN);
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
            var comments = MaterialReceiveRepo.GetDataDictCollection(sql);
            return comments;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }


        public IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int MRID)
        {
            string sql = $@" EXEC SCM..spRPTGRNApprovalFeedback {MRID}";
            var feedback = MaterialReceiveRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int MRID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialReceive' AND ReferenceID={MRID}";
            var attachment = MaterialReceiveRepo.GetDataDictCollection(attachmentSql);
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

    }
}
