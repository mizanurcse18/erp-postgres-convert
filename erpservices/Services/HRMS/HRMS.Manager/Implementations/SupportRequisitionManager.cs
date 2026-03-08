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
    public class SupportRequisitionManager : ManagerBase, ISupportRequisitionManager
    {

        private readonly IRepository<SupportRequisitionMaster> SupportRequisitionRepo;
        private readonly IRepository<AccessoriesRequisitionCategoryChild> AccessoriesRepo;
        private readonly IRepository<AssetRequisitionCategoryChild> AssetRepo;
        private readonly IRepository<AccessRequestCategoryChild> AccessRepo;
        public SupportRequisitionManager(IRepository<SupportRequisitionMaster> requestSupportRepo, IRepository<AccessoriesRequisitionCategoryChild> _accessoriesRepo, IRepository<AssetRequisitionCategoryChild> _assetRepo,
            IRepository<AccessRequestCategoryChild> _accessRepo)
        {
            SupportRequisitionRepo = requestSupportRepo;
            AccessoriesRepo = _accessoriesRepo;
            AssetRepo = _assetRepo;
            AccessRepo = _accessRepo;
        }

        public async Task<GridModel> GetListForGrid(GridParameter parameters)
        {
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
                    filter = $@" AND SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            if (!string.IsNullOrEmpty(parameters.AdditionalFilterData))
            {
                filter2 = $@" AND SupportCategoryID = {parameters.AdditionalFilterData}";
            }
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            SRM.SRMID
                                ,SRM.SupportCategoryID
                                ,SRM.RequestDate
                                ,SRM.ApprovalStatusID
                                ,SRM.ITRemarks
                                ,SRM.IsDraft
                                ,ISNULL(SRM.IsSettle,0) IsSettle
                                ,SRM.SettlementDate
                                ,SRM.ReferenceKeyword
                                ,SRM.ReferenceNo
                                ,SRM.CreatedBy
                                ,SRM.EmployeeID
                                ,SRM.CreatedDate
                                ,st.SystemVariableCode SupportType
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
                                ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            --,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                --,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND SRM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN CanSettle=0 AND SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (SRM.CreatedBy = {AppContexts.User.UserID} AND SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                                , COALESCE(SRM.SettlementRemarks,'') AS SettlementRemarks
                            FROM HRMS..SupportRequisitionMaster SRM
                            LEFT JOIN Security..SystemVariable st ON SRM.SupportCategoryID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = SRM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SRM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SRM.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SRM.SRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SRM.SRMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = SRM.SRMID
                                   LEFT JOIN (SELECT 
										AEF.APEmployeeFeedbackID,AEF.ApprovalProcessID,IsEditable CanSettle
									FROM 
										Approval..ApprovalEmployeeFeedback AEF
										LEFT JOIN 
											(
												SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
											) Prox ON Prox.ApprovalProcessID = AEF.ApprovalProcessID AND Prox.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
										LEFT JOIN (
												SELECT 
													AEF.EmployeeID,ApprovalProcessID 
												FROM 
													Approval..ApprovalEmployeeFeedback  AEF													
												WHERE SequenceNo = 1
											) RequestedEmployee ON RequestedEmployee.ApprovalProcessID = AEF.ApprovalProcessID
									WHERE (AEF.EmployeeID = {AppContexts.User.EmployeeID} or AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID}  OR 
										Prox.EmployeeID = (CASE WHEN RequestedEmployee.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END))
										)ForSettelment ON ForSettelment.ApprovalProcessID = AP.ApprovalProcessID
							WHERE ISNULL(SRM.EmployeeID,0)=0 AND (SRM.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter} {filter2}";
            var result = SupportRequisitionRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }
        public async Task<GridModel> GetAllListForGrid(GridParameter parameters)
        {
            string filter = "";
            string filter2 = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" WHERE CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" WHERE SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" WHERE SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            if (!string.IsNullOrEmpty(parameters.AdditionalFilterData))
            {
                filter2 = string.IsNullOrEmpty(filter) ? $@" Where SupportCategoryID = {parameters.AdditionalFilterData}" : $@" AND SupportCategoryID = {parameters.AdditionalFilterData}";
            }
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            SRM.SRMID
                                ,SRM.SupportCategoryID
                                ,SRM.RequestDate
                                ,SRM.ApprovalStatusID
                                ,SRM.ITRemarks
                                ,SRM.IsDraft
                                ,ISNULL(SRM.IsSettle,0) IsSettle
                                ,SRM.SettlementDate
                                ,SRM.ReferenceKeyword
                                ,SRM.ReferenceNo
                                ,SRM.CreatedBy
                                ,SRM.EmployeeID
                                ,SRM.CreatedDate
                                ,st.SystemVariableCode SupportType
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
                                ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            --,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                --,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND SRM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN CanSettle=0 AND SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (SRM.CreatedBy = {AppContexts.User.UserID} AND SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                                , COALESCE(SRM.SettlementRemarks,'') AS SettlementRemarks
                            FROM HRMS..SupportRequisitionMaster SRM
                            LEFT JOIN Security..SystemVariable st ON SRM.SupportCategoryID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = SRM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SRM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SRM.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SRM.SRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SRM.SRMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = SRM.SRMID
                                   LEFT JOIN (SELECT 
										AEF.APEmployeeFeedbackID,AEF.ApprovalProcessID,IsEditable CanSettle
									FROM 
										Approval..ApprovalEmployeeFeedback AEF
										LEFT JOIN 
											(
												SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
											) Prox ON Prox.ApprovalProcessID = AEF.ApprovalProcessID AND Prox.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
										LEFT JOIN (
												SELECT 
													AEF.EmployeeID,ApprovalProcessID 
												FROM 
													Approval..ApprovalEmployeeFeedback  AEF													
												WHERE SequenceNo = 1
											) RequestedEmployee ON RequestedEmployee.ApprovalProcessID = AEF.ApprovalProcessID
									WHERE (AEF.EmployeeID = {AppContexts.User.EmployeeID} or AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID}  OR 
										Prox.EmployeeID = (CASE WHEN RequestedEmployee.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END))
										)ForSettelment ON ForSettelment.ApprovalProcessID = AP.ApprovalProcessID
							--WHERE ISNULL(SRM.EmployeeID,0)=0 AND (SRM.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) 
                            {filter} {filter2}";
            var result = SupportRequisitionRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<GridModel> GetListForGridForEmp(GridParameter parameters)
        {
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
                    filter = $@" AND SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND SRM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            if (!string.IsNullOrEmpty(parameters.AdditionalFilterData))
            {
                filter2 = $@" AND SupportCategoryID = {parameters.AdditionalFilterData}";
            }
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            SRM.SRMID
                                ,SRM.SupportCategoryID
                                ,SRM.RequestDate
                                ,SRM.ApprovalStatusID
                                ,SRM.ITRemarks
                                ,SRM.IsDraft
                                ,ISNULL(SRM.IsSettle,0) IsSettle
                                ,SRM.SettlementDate
                                ,SRM.ReferenceKeyword
                                ,SRM.ReferenceNo
                                ,SRM.CreatedBy
                                ,SRM.CreatedDate
                                ,SRM.EmployeeID
                                ,st.SystemVariableCode SupportType
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
                                ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            --,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                --,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND SRM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (SRM.CreatedBy = U.UserID AND SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                            FROM HRMS..SupportRequisitionMaster SRM
                            LEFT JOIN Security..SystemVariable st ON SRM.SupportCategoryID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = SRM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SRM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SRM.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SRM.SRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SRM.SRMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = SRM.SRMID
                                   LEFT JOIN (SELECT 
										AEF.APEmployeeFeedbackID,AEF.ApprovalProcessID,IsEditable CanSettle
									FROM 
										Approval..ApprovalEmployeeFeedback AEF
										LEFT JOIN 
											(
												SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
											) Prox ON Prox.ApprovalProcessID = AEF.ApprovalProcessID AND Prox.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
										LEFT JOIN (
												SELECT 
													AEF.EmployeeID,ApprovalProcessID 
												FROM 
													Approval..ApprovalEmployeeFeedback  AEF													
												WHERE SequenceNo = 1
											) RequestedEmployee ON RequestedEmployee.ApprovalProcessID = AEF.ApprovalProcessID
									WHERE (AEF.EmployeeID = {AppContexts.User.EmployeeID} or AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID}  OR 
										Prox.EmployeeID = (CASE WHEN RequestedEmployee.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END))
										)ForSettelment ON ForSettelment.ApprovalProcessID = AP.ApprovalProcessID
							WHERE ISNULL(SRM.EmployeeID,0) <>0 AND (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter} {filter2}";
            var result = SupportRequisitionRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<(bool, string)> SaveChanges(SupportRequisitionDto srm)
        {
            var existingRSM = SupportRequisitionRepo.Entities.Where(x => x.SRMID == srm.SRMID).SingleOrDefault();

            if (srm.SRMID > 0 && (existingRSM.IsNullOrDbNull() || existingRSM.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Request Support.");
            }
            #region 

            #endregion

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (srm.SRMID > 0 && srm.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..SupportRequisitionMaster SRM
                            LEFT JOIN Security..Users U ON U.UserID = SRM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SRM.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SRM.SRMID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {srm.ApprovalProcessID}";
                var canReassesstment = SupportRequisitionRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Support Request once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Support Request once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)srm.SRMID, srm.ApprovalProcessID, (int)Util.ApprovalType.SupportRequisition);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new SupportRequisitionMaster
            {
                ReferenceKeyword = srm.ReferenceKeyword,
                ReferenceNo = srm.ReferenceNo,
                SupportCategoryID = srm.SupportCategoryID,
                BusinessJustification = srm.BusinessJustification,
                RequestDate = srm.RequestDate,
                ITRemomandation = srm.ITRemomandation,
                IsDraft = srm.IsDraft,
                IsOnBehalf = srm.IsOnBehalf,
                IsNewRequirements = srm.IsNewRequirements,
                IsReplacement = srm.IsReplacement,
                EmployeeID = srm.EmployeeID,
                ApprovalStatusID = srm.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (srm.SRMID.IsZero() && existingRSM.IsNull())
                {
                    masterModel.ReferenceNo = GenerateSupportRequisitionReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetRSMMasterNewId(masterModel);
                    srm.SRMID = (int)masterModel.SRMID;
                }
                else
                {
                    masterModel.IsOnBehalf = existingRSM.IsOnBehalf;
                    masterModel.CreatedBy = existingRSM.CreatedBy;
                    masterModel.CreatedDate = existingRSM.CreatedDate;
                    masterModel.CreatedIP = existingRSM.CreatedIP;
                    masterModel.RowVersion = existingRSM.RowVersion;
                    masterModel.SRMID = srm.SRMID;
                    masterModel.SetModified();
                }

                //var accessoriesItemDetailsModel = GenerateAccessoriesRequisitionItem(srm);
                var assetItemDetailsModel = GenerateAssetRequisitionItem(srm);
                var accessRequestMethodModel = GenerateAccessRequisitionMethodDto(srm).MapTo<List<AccessRequestCategoryChild>>();


                SetAuditFields(masterModel);
                //if (accessoriesItemDetailsModel.IsNotNull()) SetAuditFields(accessoriesItemDetailsModel);
                if (assetItemDetailsModel.IsNotNull()) SetAuditFields(assetItemDetailsModel);
                if (accessRequestMethodModel.IsNotNull()) SetAuditFields(accessRequestMethodModel);

                //RemoveAttachments(srm);
                //AddAttachments(srm);



                SupportRequisitionRepo.Add(masterModel);
                //AccessoriesRepo.AddRange(accessoriesItemDetailsModel);
                AssetRepo.AddRange(assetItemDetailsModel);
                AccessRepo.AddRange(accessRequestMethodModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingRSM.IsDraft && masterModel.IsModified))
                    {
                        int appPanelTypeId = masterModel.SupportCategoryID == 232 ? (int)Util.ApprovalPanel.SupportRequisition : (masterModel.SupportCategoryID == 233 ? (int)Util.ApprovalPanel.SupportRequisition_Accessories : (int)Util.ApprovalPanel.SupportRequisition_Asset);
                        string approvalTitle = $"{Util.ITSupportRequisitionApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Support Requisition Reference No:{masterModel.ReferenceNo}";
                        ApprovalProcessID = CreateApprovalProcessByAPTypeIDAndAPPanelID((int)masterModel.SRMID, Util.ITSupportRequisitionAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.SupportRequisition, appPanelTypeId);
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
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.SRMID, (int)Util.MailGroupSetup.RequisitionSupportInitiatedMail, (int)Util.ApprovalType.SupportRequisition);
                    //await SendMailFromRequestCreated(ApprovalProcessID, false, masterModel.SRMID, srm.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminSupportRequisitionEmployeeInitiatedMail : (int)Util.MailGroupSetup.AdminSupportRequisitionInitiatedMail, (int)Util.ApprovalType.AdminSupportRequest);
                }

            }
            await Task.CompletedTask;

            return (true, $"Support Requisition Submitted Successfully"); ;
        }
        private string GenerateSupportRequisitionReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/ARS/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("RequisitionSupportRefNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }
        //private List<AccessoriesRequisitionCategoryChild> GenerateAccessoriesRequisitionItem(SupportRequisitionDto IP)
        //{
        //    var existingRequestItem = AccessoriesRepo.GetAllList(x => x.SRMID == IP.SRMID);
        //    var itemModel = new List<AccessoriesRequisitionCategoryChild>();
        //    if (IP.AccessoriesItemDetails.IsNotNull())
        //    {
        //        IP.AccessoriesItemDetails.ForEach(x =>
        //        {
        //            itemModel.Add(new AccessoriesRequisitionCategoryChild
        //            {
        //                AccessoriesRCCID = x.AccessoriesRCCID,
        //                SRMID = IP.SRMID,
        //                ItemID = x.ItemID,
        //                Quantity = x.Quantity,
        //                Remarks = x.Remarks
        //            });

        //        });

        //        itemModel.ForEach(x =>
        //        {
        //            if (existingRequestItem.Count > 0 && x.AccessoriesRCCID > 0)
        //            {
        //                var existingModelData = existingRequestItem.FirstOrDefault(y => y.AccessoriesRCCID == x.AccessoriesRCCID);
        //                x.CreatedBy = existingModelData.CreatedBy;
        //                x.CreatedDate = existingModelData.CreatedDate;
        //                x.CreatedIP = existingModelData.CreatedIP;
        //                x.RowVersion = existingModelData.RowVersion;
        //                x.SetModified();
        //            }
        //            else
        //            {
        //                x.SRMID = IP.SRMID;
        //                x.SetAdded();
        //                SetAccessoriesItemNewId(x);
        //            }
        //        });

        //        var willDeleted = existingRequestItem.Where(x => !itemModel.Select(y => y.AccessoriesRCCID).Contains(x.AccessoriesRCCID)).ToList();
        //        willDeleted.ForEach(x =>
        //        {
        //            x.SetDeleted();
        //            itemModel.Add(x);
        //        });
        //    }

        //    return itemModel;
        //}
        private List<AssetRequisitionCategoryChild> GenerateAssetRequisitionItem(SupportRequisitionDto IP)
        {
            var existingRequestItem = AssetRepo.GetAllList(x => x.SRMID == IP.SRMID);
            var itemModel = new List<AssetRequisitionCategoryChild>();
            if (IP.AssetItemDetails.IsNotNull())
            {
                IP.AssetItemDetails.ForEach(x =>
                {
                    itemModel.Add(new AssetRequisitionCategoryChild
                    {
                        AssetRCID = x.AssetRCID,
                        SRMID = IP.SRMID,
                        ItemID = x.ItemID,
                        Quantity = x.Quantity,
                        Remarks = x.Remarks
                    });

                });

                itemModel.ForEach(x =>
                {
                    if (existingRequestItem.Count > 0 && x.AssetRCID > 0)
                    {
                        var existingModelData = existingRequestItem.FirstOrDefault(y => y.AssetRCID == x.AssetRCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.SRMID = IP.SRMID;
                        x.SetAdded();
                        SetAssetItemNewId(x);
                    }
                });

                var willDeleted = existingRequestItem.Where(x => !itemModel.Select(y => y.AssetRCID).Contains(x.AssetRCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    itemModel.Add(x);
                });
            }

            return itemModel;
        }

        private void SetAccessoriesItemNewId(AccessoriesRequisitionCategoryChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("AccessoriesRequisitionCategoryChild", AppContexts.User.CompanyID);
            child.AccessoriesRCCID = code.MaxNumber;
        }
        private void SetAssetItemNewId(AssetRequisitionCategoryChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("AssetRequisitionCategoryChild", AppContexts.User.CompanyID);
            child.AssetRCID = code.MaxNumber;
        }


        //GenerateSupportRequestRenovationORMaintenance
        private List<AccessRequestDetailsDto> GenerateAccessRequisitionMethodDto(SupportRequisitionDto srm)
        {
            var facilityMethodChild = AccessRepo.GetAllList(x => x.SRMID == srm.SRMID);
            if (srm.AccessRequestDetails.IsNotNull())
            {

                srm.AccessRequestDetails.ForEach(x =>
                {
                    if (facilityMethodChild.Count > 0 && x.AccessRCCID > 0)
                    {
                        var existingModelData = AccessRepo.FirstOrDefault(y => y.AccessRCCID == x.AccessRCCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.SRMID = srm.SRMID;
                        x.SetAdded();
                        SetAccessRequestMethodNewId(x);
                    }
                });

                var willDeleted = facilityMethodChild.Where(x => !srm.AccessRequestDetails.Select(y => y.AccessRCCID).Contains(x.AccessRCCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    srm.AccessRequestDetails.Add(new AccessRequestDetailsDto
                    {
                        AccessRCCID = x.AccessRCCID,
                        SRMID = x.SRMID,
                        CompanyID = x.CompanyID,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        CreatedIP = x.CreatedIP,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedDate = x.UpdatedDate,
                        UpdatedIP = x.UpdatedIP,
                        RowVersion = x.RowVersion,
                        ObjectState = x.ObjectState
                    });
                });
            }

            return srm.AccessRequestDetails;
        }


        private void SetAccessRequestMethodNewId(AccessRequestDetailsDto child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("AccessRequestCategoryChild", AppContexts.User.CompanyID);
            child.AccessRCCID = code.MaxNumber;
        }

        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.SupportRequisition);
            return comments.Result;
        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int SRMID, int mailGroup)
        {

            var mail = GetAPEmployeeEmailsWithMultiProxyParallal(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2.Where(x => x.IsNotNullOrEmpty()).ToList();
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.SupportRequisition, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, SRMID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private void SetRSMMasterNewId(SupportRequisitionMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("SupportRequisitionMaster", AppContexts.User.CompanyID);
            master.SRMID = code.MaxNumber;
        }


        public async Task<SupportRequisitionDto> GetSupportRequisition(int SRMID, int ApprovalProcessID)
        {
            string sql = $@"select DISTINCT 
                                 DU.SRMID
								,DU.SupportCategoryID
								,DU.BusinessJustification
								,DU.RequestDate
								,DU.ITRemomandation
								,DU.ApprovalStatusID
								,DU.ITRemarks
                                ,ISNULL(DU.EmployeeID,0) EmployeeID
								,DU.IsDraft
								,DU.CreatedBy
								,DU.CreatedDate
								,DU.IsSettle
								,DU.SettlementDate
								,DU.ReferenceKeyword
								,DU.ReferenceNo
                                ,DU.SettlementRemarks
                                ,DU.IsNewRequirements
								,DU.IsReplacement
                                ,VA.EmployeeCode
                                ,VA.FullName EmployeeName
                                ,VA.DivisionName
								,VA.DepartmentName
								,VA.WorkMobile
								,VA.WorkEmail
                                ,VA.DesignationName
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            --,ISNULL(APForwardInfoID,0) APForwardInfoID
								---,ISNULL(AEF.IsEditable,0) IsEditable
                                --,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                --,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,ST.SystemVariableCode SupportType
                                ,(Emp.EmployeeCode +'-'+ Emp.FullName) EmployeeDetails
                        from HRMS..SupportRequisitionMaster DU
                        LEFT JOIN HRMS..Employee Emp ON Emp.EmployeeID=DU.EmployeeID
                        LEFT JOIN HRMS..ViewALLEmployee EE ON EE.EmployeeID = DU.CreatedBy
						LEFT JOIN Security..SystemVariable ST ON ST.SystemVariableID = DU.SupportCategoryID
                        LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = DU.ApprovalStatusID
                        
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DU.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition}
										--GROUP BY ReferenceID 
                                        
										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.SRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DU.SRMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE DU.SRMID={SRMID}  --AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID} OR DU.EmployeeID={AppContexts.User.EmployeeID})";
            var rs = SupportRequisitionRepo.GetModelData<SupportRequisitionDto>(sql);
            return rs;
        }



        private List<Attachments> GetAttachments(int RSRMDID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='SupportRequisitionRenovationORMaintenanceDetails' AND ReferenceID={RSRMDID}";
            var attachment = AccessoriesRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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

        public async Task<SupportRequisitionDto> GetSupportRequisitionForReAssessment(int SRMID)
        {
            var model = SupportRequisitionRepo.Entities.Where(x => x.SRMID == SRMID).Select(y => new SupportRequisitionDto
            {
                SRMID = (int)y.SRMID,
                //DateOfResignation = y.DateOfResignation,
                //LastWorkingDay = y.LastWorkingDay,
                //IsCoreFunctional = y.IsCoreFunctional,
                //Description = y.Description,
                //EEIID = y.EEIID
            }).FirstOrDefault();


            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SupportRequisitionMaster' AND ReferenceID={SRMID}";
            var attachment = SupportRequisitionRepo.GetDataDictCollection(attachmentSql);
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

        public IEnumerable<Dictionary<string, object>> ReportForSupportRequisitionAttachments(int SRMID)
        {
            string sql = $@" EXEC Security..spRPTAttachmentList {0}";
            var attachemntList = SupportRequisitionRepo.GetDataDictCollection(sql);
            return attachemntList;
        }

        public Dictionary<string, object> ReportForSupportRequisitionMaster(int SRMID)
        {
            string sql = $@" EXEC HRMS..spRPTSupportRequisitionMaster {SRMID}";
            var masterData = SupportRequisitionRepo.GetData(sql);
            return masterData;
        }

        public IEnumerable<Dictionary<string, object>> ReportForRSMApprovalFeedback(int SRMID)
        {
            string sql = $@" EXEC Security..spRPTSupportRequisitionApprovalFeedback {SRMID}";
            var feedback = SupportRequisitionRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForRSM(int SRMID, int ApprovalProcessID)
        {
            //string sql = $@" EXEC Security..spRPTSupportRequisitionApprovalFeedback {SRMID}";
            string sql = $@"Declare
                            @NFAApprovalSequenceType int=55,
                            @EmployeeID int={AppContexts.User.EmployeeID}

                            SELECT 
		                    NFAApprovalSequenceType,
		                    AEF.EmployeeID,
		                    VE.FullName EmployeeName,
		                    ProxyEmployeeID,
		                    SequenceNo,
		                    ReferenceID,
		                    AP.APTypeID,
		                    AT.Name ApprovalTypeName,
		                    AEF.APFeedbackID,
		                    AEF.Particulars,
		                    AF.Name FeedbackName,
		                    FeedbackSubmitDate,				
		                    CASE 
		                    	WHEN AEF.APFeedbackID = 6 
		                    	THEN 'https://nagaderp.mynagad.com:7070/security/'+'/upload\images/approval\rejected.jpeg' 
		                    	WHEN AEF.APFeedbackID = 5  THEN 'https://nagaderp.mynagad.com:7070/security/'+Signtr.ImagePath 
		                    	ELSE NULL END SignatureImagePath,
		                    DivisionName,
		                    DepartmentName,
		                    DesignationName,
		                    SV.SystemVariableCode RSMApprovalSequenceTypeName,
		                    NFAApprovalSequenceType
	                        FROM 
	                        Approval..ApprovalEmployeeFeedback AEF 
	                        INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                        INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID
	                        LEFT JOIN Security..PersonImage Signtr ON Signtr.PersonID = VE.PersonID AND IsSignature = 1
	                        INNER JOIN Approval..ApprovalFeedback AF ON AF.APFeedbackID = AEF.APFeedbackID
	                        INNER JOIN Approval..ApprovalType AT ON AT.APTypeID = AP.APTypeID
	                        LEFT JOIN Security..SystemVariable SV ON AEF.NFAApprovalSequenceType = SV.SystemVariableID
	                        WHERE AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND ReferenceID = {SRMID} 
	                        AND NFAApprovalSequenceType <> 64 
                            AND ((@NFAApprovalSequenceType=55 AND @EmployeeID=(select EmployeeID from Approval..ApprovalEmployeeFeedback where ApprovalProcessID = {ApprovalProcessID} AND NFAApprovalSequenceType = @NFAApprovalSequenceType) and 1=1) 
                                    OR (AEF.EmployeeID={AppContexts.User.EmployeeID} OR ProxyEmployeeID={AppContexts.User.EmployeeID}))
	                        
	                        ORDER BY SequenceNo";
            var feedback = SupportRequisitionRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task RemoveSupportRequisition(int SRMID)
        {
            var supportRequisition = SupportRequisitionRepo.Entities.Where(x => x.SRMID == SRMID && x.IsDraft == true && x.CreatedBy == AppContexts.User.UserID).FirstOrDefault();
            if (supportRequisition.IsNullOrDbNull())
            {
                return;
            }
            var accessoriesItem = AccessoriesRepo.Entities.Where(x => x.SRMID == SRMID).ToList();
            var assetItem = AssetRepo.Entities.Where(x => x.SRMID == SRMID).ToList();
            var accessRequest = AccessRepo.Entities.Where(x => x.SRMID == SRMID).ToList();
            supportRequisition.SetDeleted();
            accessoriesItem.ForEach(x =>
            {
                x.SetDeleted();
            });
            assetItem.ForEach(x =>
            {
                x.SetDeleted();
            });
            accessRequest.ForEach(x =>
            {
                x.SetDeleted();
            });
            using (var unitOfWork = new UnitOfWork())
            {

                SupportRequisitionRepo.Add(supportRequisition);
                AccessoriesRepo.AddRange(accessoriesItem);
                AssetRepo.AddRange(assetItem);
                AccessRepo.AddRange(accessRequest);

                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public async Task<(bool, string)> SettleSupportRequisition(int SRMID, string SettlementRemarks)
        {
            //Can Settle
            string sqlCanSettle = $@"SELECT 
		                    distinct SRM.*,
                            AEF.ApprovalProcessID,
                            CASE WHEN SRM.ApprovalStatusID=23 AND ISNULL(SRM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
	                        FROM 
		                    HRMS..SupportRequisitionMaster SRM 
		                    INNER JOIN Approval..ApprovalProcess AP ON AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition} and AP.ReferenceID=SRM.SRMID
		                    INNER JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
		                    LEFT JOIN 
		                    	(
		                    		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
		                    	) Prox ON Prox.ApprovalProcessID = AEF.ApprovalProcessID AND Prox.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
		                    LEFT JOIN (
		                    		SELECT 
		                    			AEF.EmployeeID,ApprovalProcessID 
		                    		FROM 
		                    			Approval..ApprovalEmployeeFeedback  AEF													
		                    		WHERE SequenceNo = 1
		                    	) RequestedEmployee ON RequestedEmployee.ApprovalProcessID = AEF.ApprovalProcessID
	                        WHERE SRM.SRMID={SRMID} AND
	                        (AEF.EmployeeID = {AppContexts.User.EmployeeID} or AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID}  OR 
		                    Prox.EmployeeID = (CASE WHEN RequestedEmployee.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END))";
            var canSettled = SupportRequisitionRepo.GetModelData<SupportRequisitionDto>(sqlCanSettle);
            if (canSettled.IsNull() || canSettled.CanSettle == false)
            {
                return (false, $"Sorry, You are not permitted this settlement!");
            }

            var supportRequisition = SupportRequisitionRepo.Get(SRMID);
            if (supportRequisition.IsNullOrDbNull())
                return (false, "Requisition Support not found.");
            else if (supportRequisition.ApprovalStatusID == (int)Util.ApprovalStatus.Approved && (supportRequisition.IsSettle == false || supportRequisition.IsSettle == null))
            {


                supportRequisition.IsSettle = true;
                supportRequisition.SettlementDate = DateTime.Now;
                supportRequisition.SettlementRemarks = SettlementRemarks;
                supportRequisition.SetModified();

                using (var unitOfWork = new UnitOfWork())
                {
                    SupportRequisitionRepo.Add(supportRequisition);
                    unitOfWork.CommitChangesWithAudit();

                }
                //if (canSettled.SupportCategoryID == (int)Util.AdminSupportCategory.Vehicle)
                //{
                //    await SendMailToSettledReceiver(canSettled.ApprovalProcessID.ToString(), supportRequisition.SRMID, (int)Util.MailGroupSetup.AdminSupportRequisitionSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                //}
                //else
                //{
                //    await SendMailToSettledReceiver(canSettled.ApprovalProcessID.ToString(), supportRequisition.SRMID, (int)Util.MailGroupSetup.AdminSupportRequisitionWithoutVehicleSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                //}
            }

            return (true, "Requisition Support is settled."); ;

            //await Task.CompletedTask;
        }

        public async Task SendMailFromRequestCreated(string ApprovalProcessID, bool IsResubmitted, long MasterID, int mailGroup, int APTypeID)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxyAndSupervisor(ApprovalProcessID.ToInt()).Result;
            //List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)MasterID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        private async Task SendMailToSettledReceiver(string ApprovalProcessID, int SRMID, int MailGroupID, int APTypeID)
        {
            //var recieverMail = GetInitiatorEmployeeEmail(ApprovalProcessID.ToInt()).Result;
            var recieverMail = GetAPEmployeeEmailsForAllPanelMembers(ApprovalProcessID.ToInt()).Result;
            await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, MailGroupID, false, recieverMail.Item1, recieverMail.Item2, null, SRMID);
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
            var comments = SupportRequisitionRepo.GetDataDictCollection(sql);
            return comments;
        }

        public List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByRSMID(int id)
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
                                INNER JOIN HRMS..SupportRequisitionMaster AS D ON P.ReferenceID = D.SRMID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM Approval..ManualApprovalPanelEmployee a
		                                INNER JOIN HRMS..SupportRequisitionMaster b ON a.ReferenceID = b.SRMID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.SupportRequisition}
			                                AND APTypeID = {(int)Util.ApprovalType.SupportRequisition}
			                                --AND b.SRMID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.SupportRequisition}
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = SupportRequisitionRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForSupportRequisition()
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
                        LEFT JOIN SupportRequisitionMaster SRM on SRM.EmployeeID = Emp.EmployeeID  AND SRM.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND SRM.SRMID IS NULL";
            var listDict = SupportRequisitionRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<Dictionary<string, object>>> DownloadSupportRequisition()
        {
            string sql = $@"SELECT 
                            DISTINCT
	                             SRM.SRMID
								,SRM.SupportCategoryID
								,SRM.RequestDate
								,SRM.ApprovalStatusID
								,SRM.ITRemarks
								,SRM.IsDraft
								,SRM.CreatedBy
								,SRM.CreatedDate
								,SRM.IsSettle
								,SRM.SettlementDate
								,SRM.ReferenceKeyword
								,SRM.ReferenceNo
                                ,VA.DepartmentID
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
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,ST.SystemVariableCode SupportType
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                            FROM HRMS..SupportRequisitionMaster SRM
							JOIN Security..SystemVariable st ON st.SystemVariableID=SRM.SupportCategoryID
                            LEFT JOIN Security..Users U ON U.UserID = SRM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SRM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SRM.SRMID AND AP.APTypeID = {(int)Util.ApprovalType.SupportRequisition}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SRM.SRMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SRM.SRMID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.SupportRequisition} AND APFeedbackID = 2
								   )PendingAt ON  PendingAt.PendingReferenceID = SRM.SRMID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            )";
            var result = SupportRequisitionRepo.GetDataDictCollection(sql);
            return result.ToList();
        }

        // public async Task<List<AccessoriesItemDetailsDto>> GetAccessoriesItemDetails(int SRMID)
        // {
        //     string sql = $@"SELECT  c.*,
        //      VA.DepartmentID,
        //      VA.DepartmentName,
        //                     unit.UnitId UOM,
        //                     COALESCE(unit.UnitCode, '') UOMLabel,
        //      VA.FullName AS EmployeeName,
        //      SV.SystemVariableCode AS ApprovalStatus,
        //      VA.ImagePath,
        //      VA.EmployeeCode,
        //      VA.DivisionID,
        //      VA.DivisionName,
        //      --I.ItemCode+'-'+I.ItemName AS ItemName,
        //      I.ItemName,
        //I.InventoryTypeID
        //             FROM HRMS..AssetRequisitionCategoryChild c
        //LEFT JOIN Security..SupportRequisitionItem item ON c.ItemID = item.ItemId
        //LEFT JOIN Security..Unit unit ON  item.UnitId = unit.UnitId
        //                 LEFT JOIN SupportRequisitionmaster m ON m.SRMID = c.SRMID
        //                 LEFT JOIN Security..SupportRequisitionItem I ON I.ItemID = c.ItemID
        //                 LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
        //                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
        //                 LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
        //                 WHERE m.SRMID={SRMID}";
        //     var items = AccessoriesRepo.GetDataModelCollection<AccessoriesItemDetailsDto>(sql);
        //     return items;
        // }
        public async Task<List<AssetItemDetailsDto>> GetAssetItemDetails(int SRMID)
        {
            string sql = $@"SELECT  c.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
                            unit.UnitId UOM,
                            COALESCE(unit.UnitCode, '') UOMLabel,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName,
					        --I.ItemCode+'-'+I.ItemName AS ItemName,
					        I.ItemName,
							I.InventoryTypeID
                    FROM HRMS..AssetRequisitionCategoryChild c
							LEFT JOIN Security..SupportRequisitionItem item ON c.ItemID = item.ItemId
							LEFT JOIN Security..Unit unit ON  item.UnitId = unit.UnitId
                        LEFT JOIN SupportRequisitionmaster m ON m.SRMID = c.SRMID
                        LEFT JOIN Security..SupportRequisitionItem I ON I.ItemID = c.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        WHERE m.SRMID={SRMID}";
            var items = AssetRepo.GetDataModelCollection<AssetItemDetailsDto>(sql);
            return items;
        }


        public async Task<List<AccessRequestDetailsDto>> GetAccessDetails(int SRMID)
        {
            string sql = $@"SELECT  c.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName,
							sc.SystemVariableCode SupportCategoryName,
                            SRI.ItemName 'AccessTypesNeme'
                    FROM HRMS..AccessRequestCategoryChild c
						LEFT JOIN HRMS..SupportRequisitionmaster m ON m.SRMID = c.SRMID
						LEFT JOIN Security..SystemVariable sc on m.SupportCategoryID=sc.SystemVariableID
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        LEFT JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = c.AccessTypesIds
                        WHERE m.SRMID={SRMID}";
            //var facilities = FacilitiesRepo.GetDataModelCollection<FacilitiesDetailsDto>(sql);
            //return facilities;
            var facilities = AccessRepo.GetDataModelCollection<AccessRequestDetailsDto>(sql);
            return await Task.FromResult(facilities);
        }


        public async Task<List<Dictionary<string, object>>> GetAllSupportRequestListByWhereCondition(string whereCondition, string fromDate, string toDate)
        {
            string dateFilterCondition = string.Empty;
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                // = $@" AND CAST(M.CreatedDate as date) between IIF(('{fromDate}' is null OR '{fromDate}'=''),'2020-01-01',CAST('{fromDate}' as DATE)) and IIF(('{toDate}' is null OR '{toDate}'=''),CAST(Getdate() as date),CAST('{toDate}' as DATE))";
                dateFilterCondition = $@" AND CAST(M.CreatedDate as date) between CAST('{fromDate}' as DATE) and CAST('{toDate}' as DATE)";
            }

            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AccessRequest)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLVehicleSupportRequest";
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (select SRMID, 
                                	COUNT(SRMID) 'Item Count',
                                	STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AccessRequestCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.AccessTypesIds
                                	GROUP BY SRMID
                                	) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                    AND M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AccessRequest}
                                {dateFilterCondition}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AccessoriesRequisition)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLConsumbleGoodsSupportRequest";
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                 AND M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AccessoriesRequisition}
                                {dateFilterCondition}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AssetRequisition)
            {
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                 AND M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AssetRequisition}
                                {dateFilterCondition}";

                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }

            else
            {
                //string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";
                //string sql = $@"SELECT * FROM ViewALLEmployeeForExcel {where}";
                //var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                //return await Task.FromResult(data.ToList());
                // return await Task.FromResult(new List<Dictionary<string, object>>());
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (select SRMID, 
                                	COUNT(SRMID) 'Item Count',
                                	STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AccessRequestCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.AccessTypesIds
                                	GROUP BY SRMID
                                	
                                	UNION ALL
                                	
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                {dateFilterCondition}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }




        }

        public async Task<List<Dictionary<string, object>>> GetAllSupportRequestList(string whereCondition, string fromDate, string toDate)
        {
            string dateFilterCondition = string.Empty;
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                // = $@" AND CAST(M.CreatedDate as date) between IIF(('{fromDate}' is null OR '{fromDate}'=''),'2020-01-01',CAST('{fromDate}' as DATE)) and IIF(('{toDate}' is null OR '{toDate}'=''),CAST(Getdate() as date),CAST('{toDate}' as DATE))";
                dateFilterCondition = $@" AND CAST(M.CreatedDate as date) between CAST('{fromDate}' as DATE) and CAST('{toDate}' as DATE)";
            }

            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AccessRequest)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLVehicleSupportRequest";
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (select SRMID, 
                                	COUNT(SRMID) 'Item Count',
                                	STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AccessRequestCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.AccessTypesIds
                                	GROUP BY SRMID
                                	) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AccessRequest}
                                {dateFilterCondition}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AccessoriesRequisition)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLConsumbleGoodsSupportRequest";
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AccessoriesRequisition}
                                {dateFilterCondition}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.SupportRequisitionCategory.AssetRequisition)
            {
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                WHERE M.SupportCategoryID = {(int)Util.SupportRequisitionCategory.AssetRequisition}
                                {dateFilterCondition}";

                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }

            else
            {
                string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";
                string sql = $@"SELECT 
                                 ROW_NUMBER() OVER (ORDER BY M.SRMID) AS RowId
                                 ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',(''''+E.WorkMobile) AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                 ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                 ,M.ReferenceNo AS 'Reference No'
                                 ,SV.SystemVariableCode  AS 'Support Type',
                                 ItemDetails.[Item Count],
                                 ItemDetails.[Item Name]
                                 ,APP.FullName AS 'Response By (Admin)'
                                 ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                 ,M.BusinessJustification AS 'Business Justification'
                                ,M.ITRemomandation AS 'IT Remomandation'
                                 ,SVA.SystemVariableCode AS 'Approval Status'
                                 FROM SupportRequisitionMaster AS M
                                 JOIN Security..SystemVariable SV on M.SupportCategoryID=SV.SystemVariableID
                                 LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                 JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                 LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                 			from 
                                 			Approval..ApprovalProcess AP
                                 			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                 			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                 			where AP.APTypeID=37)APP ON M.SRMID=APP.ReferenceID
                                 LEFT JOIN (
                                 			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF(135) 
                                 			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                
                                 LEFT JOIN (Select * from (select SRMID, 
                                	COUNT(SRMID) 'Item Count',
                                	STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AccessRequestCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.AccessTypesIds
                                	GROUP BY SRMID
                                	
                                	UNION ALL
                                	
                                	select SRMID, 
                                		COUNT(SRMID) 'Item Count',
                                		STRING_AGG(SRI.ItemName,',') AS 'Item Name'
                                	from AssetRequisitionCategoryChild ARC
                                	INNER JOIN Security..SupportRequisitionItem SRI ON SRI.ItemID = ARC.ItemID
                                	GROUP BY SRMID) ItemDetails) ItemDetails ON ItemDetails.SRMID = M.SRMID
                                    {where}";
                var data = SupportRequisitionRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
        }

    }
}
