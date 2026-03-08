using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
	public class APViewManager : ManagerBase, IAPViewManager
	{

		private readonly IRepository<ApprovalPanelEmployee> ApprovalPanelEmployeeRepo;
		public APViewManager(IRepository<ApprovalPanelEmployee> appRepo)
		{
			ApprovalPanelEmployeeRepo = appRepo;
		}

		public async Task<IEnumerable<Dictionary<string, object>>> GetAPViewListDic(int APTypeID, int ReferenceID)
		{
			string sql1 = "";
			if (APTypeID == (int)Util.ApprovalType.PR)
			{
				sql1 = $@"SELECT DISTINCT PR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName,WorkMobile
                            ,(SELECT Security.dbo.NumericToBDT(PR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(QPR.QAmount,0) TotalQuotedAmount
                            ,MR.ReferenceNo AS MRReferenceNo
                            ,MR.MRMasterID
                            ,ISNULL(AP1.ApprovalProcessID,0) MRApprovalProcessID
                        from {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseRequisitionMaster PR
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Warehouse W ON PR.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {APTypeID} 
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..MaterialRequisitionMaster MR ON MR.MRMasterID = PR.MRMasterID
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = MR.MRMasterID AND AP1.APTypeID = {APTypeID}

                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {APTypeID}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {APTypeID} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PR.PRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PR.PRMasterID
                          LEFT JOIN (
										SELECT SUM(Amount) QAmount,PRMasterID FROM {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseRequisitionQuotation
										GROUP BY PRMasterID
									)QPR ON QPR.PRMasterID = PR.PRMasterID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE PR.PRMasterID={ReferenceID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.MR)
			{
				sql1 = $@"SELECT DISTINCT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE WHEN(SELECT Approval.dbo.[fnIsAPCreator]( { AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                        , CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                        , W.WarehouseName DeliveryLocationName, WorkMobile
                        ,(SELECT Security.dbo.NumericToBDT(MR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..MaterialRequisitionMaster MR
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Warehouse W ON MR.DeliveryLocation = W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                        LEFT JOIN { AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN { AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = { (int)Util.ApprovalType.MR}
                LEFT JOIN(
                                SELECT COUNT(cntr) EditableCount, ReferenceID FROM

                                (
                                SELECT
                                    COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
                                FROM

                                    Approval..ApprovalEmployeeFeedback AEF

                                    LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID

                                where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = { (int)Util.ApprovalType.MR}
                GROUP BY ReferenceID

                UNION ALL

                SELECT

                                            COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
                                        FROM

                                            Approval..ApprovalEmployeeFeedback AEF
                                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID

                                        where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = { (int)Util.ApprovalType.MR } AND EmployeeID = { AppContexts.User.EmployeeID }

                                        GROUP BY ReferenceID

										)V
                                        GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRMasterID

                                        LEFT JOIN(
                                        SELECT AP.ApprovalProcessID, COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID
                                         FROM

                                            Approval..ApprovalEmployeeFeedbackRemarks AEFR
                                            INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID

                                        WHERE APFeedbackID = 11--Returned
                                       GROUP BY AP.ApprovalProcessID,AP.ReferenceID
									) Rej ON Rej.ReferenceID = MR.MRMasterID
                                LEFT JOIN
                                (
                                    SELECT ApprovalProcessID, EmployeeID, ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( { AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID


                        WHERE MR.MRMasterID ={ ReferenceID}
                AND(VA.EmployeeID =  { AppContexts.User.EmployeeID}
                OR F.EmployeeID =  { AppContexts.User.EmployeeID}
                OR ISNULL(F.ProxyEmployeeID ,0) =  { AppContexts.User.EmployeeID})";
			}

			else if (APTypeID == (int)Util.ApprovalType.PO)
			{
				sql1 = $@"select PO.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
							,S.SupplierName
                            ,PR.ReferenceNo AS PRReferenceNo
                            ,PR.PRDate
                            ,VA1.FullName AS PREmployeeName
                            , ISNULL(S.RegisteredAddress, '') AS SupplierAddress
                            ,S.PhoneNumber AS SupplierContact
                            ,(SELECT Security.dbo.NumericToBDT(PO.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,PR.BudgetPlanRemarks AS PRBudgetPlanRemarks
                        from {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseOrderMaster PO
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseRequisitionMaster PR ON PO.PRMasterID=PR.PRMasterID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Supplier S ON PO.SupplierID = S.SupplierID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                                LEFT JOIN
                                (
                                    SELECT ApprovalProcessID, EmployeeID, ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( { AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE PO.POMasterID={ReferenceID} AND(VA.EmployeeID =  { AppContexts.User.EmployeeID}
                OR F.EmployeeID =  { AppContexts.User.EmployeeID}
                OR ISNULL(F.ProxyEmployeeID ,0) =  { AppContexts.User.EmployeeID})";
			}

			else if (APTypeID == (int)Util.ApprovalType.EmployeeProfileApproval)
			{
				sql1 = $@"SELECT P.*
                            , VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..EmployeeProfileApproval P 
                            LEFT JOIN HRMS..ViewAllEmployee VA ON VA.PersonID=P.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = P.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = P.EPAID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} 
                            LEFT JOIN (
							    SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
							    (
							    SELECT 
								    COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
							    FROM 
								    Approval..ApprovalEmployeeFeedback  AEF
								    LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
							    where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval}
							    GROUP BY ReferenceID 

							    UNION ALL

							    SELECT 
								    COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
							    FROM 
								    Approval..ApprovalEmployeeFeedback AEF
								    LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
							    where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
							    GROUP BY ReferenceID

							    )V
							    GROUP BY ReferenceID
							    ) EA ON EA.ReferenceID = P.EPAID

                                LEFT JOIN(
							    SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
							    FROM 
								    Approval..ApprovalEmployeeFeedbackRemarks AEFR 
								    INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
							    WHERE APFeedbackID = 11 --Returned
							    GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
						    ) Rej ON Rej.ReferenceID = P.EPAID
LEFT                        JOIN
                                (
                                    SELECT ApprovalProcessID, EmployeeID, ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( { AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            WHERE P.EPAID = {ReferenceID} AND(VA.EmployeeID =  { AppContexts.User.EmployeeID}
                OR F.EmployeeID =  { AppContexts.User.EmployeeID}
                OR ISNULL(F.ProxyEmployeeID ,0) =  { AppContexts.User.EmployeeID})";
			}

			else if (APTypeID == (int)Util.ApprovalType.QC)
			{
				sql1 = $@"SELECT DISTINCT QC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,ISNULL(MR.MRID, 0) MRID
                            ,ISNULL(RM.RTVMID,0) AS RTVMID
                            ,(SELECT REPLACE(QC.ReferenceNo, 'QC', 'GRN' )) as GRNNo

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                        from {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..QCMaster QC
                        LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseOrderMaster PO ON PO.POMasterID = QC.POMasterID
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Supplier S ON QC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..MaterialReceive MR ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QC.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..RTVMaster RM ON QC.QCMID = RM.QCMID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = QC.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.QC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = QC.QCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = QC.QCMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE QC.QCMID={ReferenceID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}

			else if (APTypeID == (int)Util.ApprovalType.GRN)
			{
				sql1 = $@"SELECT DISTINCT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
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
                        from {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..MaterialReceive MR
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..QCMaster QC ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..PurchaseOrderMaster PO ON PO.POMasterID = MR.POMasterID
						LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Supplier S ON MR.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SCMContext)}..Warehouse W ON PO.DeliveryLocation=W.WarehouseID
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
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE MR.MRID={ReferenceID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.DocumentApproval)
			{
				sql1 = $@"SELECT DocumentApproval.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,VA.WorkMobile
                            ,DAT.DATName TemplateName
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from {AppContexts.GetDatabaseName(ConnectionName.Default)}..DocumentApprovalMaster DocumentApproval
                        LEFT JOIN Security..Users U ON U.UserID = DocumentApproval.CreatedBy						
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = DocumentApproval.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.Default)}..DocumentApprovalTemplate DAT ON DocumentApproval.TemplateID = DAT.DATID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DocumentApproval.DAMID AND AP.APTypeID = {(int)Util.ApprovalType.DocumentApproval}
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.DocumentApproval}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.DocumentApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DocumentApproval.DAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DocumentApproval.DAMID
                                    LEFT JOIN 
								        (
								            SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								        )							
						                F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE DocumentApproval.DAMID={ReferenceID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.NFA)
			{
				sql1 = $@"select DISTINCT NFA.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned,WorkMobile
                        from {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..NFAMaster NFA
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = NFA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = NFA.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = {(int)Util.ApprovalType.NFA} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.NFA}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.NFA} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = NFA.NFAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = NFA.NFAID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE NFA.NFAID={ReferenceID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.LeaveApplication)
			{
				sql1 = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
                                E.EmployeeID,
								E.FullName, 
								E.EmployeeCode,
								E.DepartmentName,
								E.DesignationName,
								E.DivisionName,
								E.ImagePath,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
                                ELA.CancellationStatus,
                                ELA.Remarks,
								(CE.EmployeeCode +'-'+CE.FullName) CancelledBy,
		                        ISNULL (BE.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ISNULL(APForward.APForwardInfoID,0) APForwardInfoID
	                        FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..EmployeeLeaveApplication ELA
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee  E  ON ELA.EmployeeID=E.EmployeeID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..viewEmployeeUserMapData  CE  ON ELA.CancelledBy=CE.UserID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee BE ON ELA.BackupEmployeeID=BE.EmployeeID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 1 AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 1 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                            WHERE ELA.EmployeeLeaveAID={ReferenceID}  AND (E.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.MicroSite)
			{
				sql1 = $@"SELECT DISTINCT MSM.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,WorkMobile
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,M.NameEN ParentNameEN
                            ,M.NameBN ParentNameBN
                            ,SV1.SystemVariableCode AS ParentType

                        from {AppContexts.GetDatabaseName(ConnectionName.MicroSiteContext)}..MicroSiteMaster MSM
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = MSM.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.MicroSiteContext)}..Menu M ON M.MenuID = MSM.ParentID
                        LEFT JOIN Security..SystemVariable SV1 ON SV1.SystemVariableID = M.TypeID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = MSM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MSM.MicroSiteMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MicroSite} 

                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.MicroSite}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MicroSite} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MSM.MicroSiteMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MSM.MicroSiteMasterID
                           LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE MSM.MicroSiteMasterID={ReferenceID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			else if (APTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload)
			{
				sql1 = $@"SELECT DISTINCT MSM.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,WorkMobile
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            --,M.NameEN ParentNameEN
                            --,M.NameBN ParentNameBN
                            --,SV1.SystemVariableCode AS ParentType

                        from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..DocumentUpload MSM
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = MSM.CreatedBy
                        --LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.MicroSiteContext)}..Menu M ON M.MenuID = MSM.ParentID
                        --LEFT JOIN Security..SystemVariable SV1 ON SV1.SystemVariableID = M.TypeID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = MSM.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MSM.DUID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} 

                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeeDocumentUpload} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MSM.DUID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MSM.DUID
                           LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE MSM.DUID={ReferenceID} AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
			}
			if (sql1.IsNotNullOrEmpty())
			{
				var listDict1 = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql1);
				if (listDict1.Count() == 0)
				{
					var list = new List<Dictionary<string, object>>();
					return await Task.FromResult(list);
				}
			}

			string sql = $@"SELECT AEF.* FROM viewApprovalEmployeeFeedback AEF WHERE AEF.APTypeID={APTypeID} AND AEF.ReferenceID={ReferenceID} Order by SequenceNo ASC";
			var listDict = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);

			return await Task.FromResult(listDict);
		}

		public async Task<IEnumerable<Dictionary<string, object>>> GetAPViewForAll(int APTypeID, int ReferenceID)
		{


			if (APTypeID != (int)Util.ApprovalType.LeaveApplication && APTypeID != (int)Util.ApprovalType.NFA && APTypeID != (int)Util.ApprovalType.PO && APTypeID != (int)Util.ApprovalType.QC && APTypeID != (int)Util.ApprovalType.GRN)
			{
				var list = new List<Dictionary<string, object>>();
				return await Task.FromResult(list);
			}
			string sql = $@"SELECT AEF.* FROM viewApprovalEmployeeFeedback AEF WHERE AEF.APTypeID={APTypeID} AND AEF.ReferenceID={ReferenceID} Order by SequenceNo ASC";
			var listDict = ApprovalPanelEmployeeRepo.GetDataDictCollection(sql);
				
			return await Task.FromResult(listDict);
		}
	}
}
