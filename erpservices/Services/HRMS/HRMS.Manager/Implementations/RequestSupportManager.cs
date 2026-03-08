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
    public class RequestSupportManager : ManagerBase, IRequestSupportManager
    {

        private readonly IRepository<RequestSupportMaster> RequestSupportRepo;
        private readonly IRepository<RequestSupportItemDetails> ItemRepo;
        private readonly IRepository<RequestSupportVehicleDetails> VehicleRepo;
        private readonly IRepository<RequestSupportFacilitiesDetails> FacilitiesRepo;
        private readonly IRepository<RequestSupportRenovationORMaintenanceDetails> RenovationORMaintenanceRepo;
        public RequestSupportManager(IRepository<RequestSupportMaster> requestSupportRepo, IRepository<RequestSupportItemDetails> _itemRepo, IRepository<RequestSupportVehicleDetails> _vehicleRepo,
            IRepository<RequestSupportFacilitiesDetails> _facilitiesRepo, IRepository<RequestSupportRenovationORMaintenanceDetails> _renovationORMaintenanceRepo)
        {
            RequestSupportRepo = requestSupportRepo;
            ItemRepo = _itemRepo;
            VehicleRepo = _vehicleRepo;
            FacilitiesRepo = _facilitiesRepo;
            RenovationORMaintenanceRepo = _renovationORMaintenanceRepo;
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
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                filter2 = $@" AND SupportTypeID = {parameters.AdditionalFilterData}";
            }
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            RSM.RSMID
                                ,RSM.SupportTypeID
                                ,RSM.LocationOrFloor
                                ,RSM.NeededByDate
                                ,RSM.RemarksOrCommentsOrPurpose
                                ,RSM.ApprovalStatusID
                                ,RSM.AdminRemarks
                                ,RSM.IsDraft
                                ,ISNULL(RSM.IsSettle,0) IsSettle
                                ,RSM.SettlementDate
                                ,RSM.ReferenceKeyword
                                ,RSM.ReferenceNo
                                ,RSM.CreatedBy
                                ,RSM.EmployeeID
                                ,RSM.CreatedDate
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
                                ,FeedbackLastResponseDate
								,CommentSubmitDate
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND RSM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN CanSettle=0 AND RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (RSM.CreatedBy = U.UserID AND RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                                , COALESCE(RSM.SettlementRemarks,'') AS SettlementRemarks
                            FROM HRMS..RequestSupportMaster RSM
                            LEFT JOIN Security..SystemVariable st ON RSM.SupportTypeID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = RSM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = RSM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = RSM.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = RSM.RSMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = RSM.RSMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = RSM.RSMID
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
							WHERE ISNULL(RSM.EmployeeID,0)=0 AND (RSM.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter} {filter2}";
            var result = RequestSupportRepo.LoadGridModelOptimized(parameters, sql);
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
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                filter2 = $@" AND SupportTypeID = {parameters.AdditionalFilterData}";
            }
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            RSM.RSMID
                                ,RSM.SupportTypeID
                                ,RSM.LocationOrFloor
                                ,RSM.NeededByDate
                                ,RSM.RemarksOrCommentsOrPurpose
                                ,RSM.ApprovalStatusID
                                ,RSM.AdminRemarks
                                ,RSM.IsDraft
                                ,ISNULL(RSM.IsSettle,0) IsSettle
                                ,RSM.SettlementDate
                                ,RSM.ReferenceKeyword
                                ,RSM.ReferenceNo
                                ,RSM.CreatedBy
                                ,RSM.CreatedDate
                                ,RSM.EmployeeID
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
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND RSM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (RSM.CreatedBy = U.UserID AND RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                            FROM HRMS..RequestSupportMaster RSM
                            LEFT JOIN Security..SystemVariable st ON RSM.SupportTypeID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = RSM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = RSM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = RSM.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = RSM.RSMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = RSM.RSMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = RSM.RSMID
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
							WHERE ISNULL(RSM.EmployeeID,0) <>0 AND (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter} {filter2}";
            var result = RequestSupportRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<(bool, string)> SaveChanges(RequestSupportDto rsm)
        {
            var existingRSM = RequestSupportRepo.Entities.Where(x => x.RSMID == rsm.RSMID).SingleOrDefault();

            if (rsm.RSMID > 0 && (existingRSM.IsNullOrDbNull() || existingRSM.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Request Support.");
            }
            #region 

            #endregion

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (rsm.RSMID > 0 && rsm.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..RequestSupportMaster RSM
                            LEFT JOIN Security..Users U ON U.UserID = RSM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = RSM.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = RSM.RSMID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {rsm.ApprovalProcessID}";
                var canReassesstment = RequestSupportRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)rsm.RSMID, rsm.ApprovalProcessID, (int)Util.ApprovalType.AdminSupportRequest);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new RequestSupportMaster
            {
                ReferenceKeyword = rsm.ReferenceKeyword,
                ReferenceNo = rsm.ReferenceNo,
                SupportTypeID = rsm.SupportTypeID,
                LocationOrFloor = rsm.LocationOrFloor,
                NeededByDate = rsm.NeededByDate,
                RemarksOrCommentsOrPurpose = rsm.RemarksOrCommentsOrPurpose,
                IsDraft = rsm.IsDraft,
                EmployeeID = rsm.EmployeeID,
                ApprovalStatusID = rsm.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (rsm.RSMID.IsZero() && existingRSM.IsNull())
                {
                    masterModel.ReferenceNo = GenerateRequestSupportReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetRSMMasterNewId(masterModel);
                    rsm.RSMID = (int)masterModel.RSMID;
                }
                else
                {
                    masterModel.CreatedBy = existingRSM.CreatedBy;
                    masterModel.CreatedDate = existingRSM.CreatedDate;
                    masterModel.CreatedIP = existingRSM.CreatedIP;
                    masterModel.RowVersion = existingRSM.RowVersion;
                    masterModel.RSMID = rsm.RSMID;
                    masterModel.SetModified();
                }

                var itemDetailsModel = GenerateSupportRequestItem(rsm);
                var requestSupportVehicleMethodModel = GenerateVehicleSupportMethodDto(rsm).MapTo<List<RequestSupportVehicleDetails>>();
                var requestSupportFacilitiesMethodModel = GenerateFacilitySupportMethodDto(rsm).MapTo<List<RequestSupportFacilitiesDetails>>();
                var renovationORMaintenanceDetailsModel = GenerateSupportRequestRenovationORMaintenanceDto(rsm).MapTo<List<RequestSupportRenovationORMaintenanceDetails>>();


                SetAuditFields(masterModel);
                if (itemDetailsModel.IsNotNull()) SetAuditFields(itemDetailsModel);
                if (requestSupportVehicleMethodModel.IsNotNull()) SetAuditFields(requestSupportVehicleMethodModel);
                if (requestSupportFacilitiesMethodModel.IsNotNull()) SetAuditFields(requestSupportFacilitiesMethodModel);
                if (renovationORMaintenanceDetailsModel.IsNotNull()) SetAuditFields(renovationORMaintenanceDetailsModel);

                RemoveAttachments(rsm);
                AddAttachments(rsm);

                //if (rsm.Attachments.IsNotNull() && rsm.Attachments.Count > 0)
                //{

                //    var attachmentList = AddAttachments(rsm.Attachments.Where(x => x.ID == 0).ToList());

                //    //For Add new File
                //    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                //    {
                //        foreach (var attachemnt in attachmentList)
                //        {
                //            SetAttachmentNewId(attachemnt);
                //            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, rsm.RSMID, "RequestSupportMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                //        }
                //    }
                //    //For Remove Attachment                    
                //    if (removeList.IsNotNull() && removeList.Count > 0)
                //    {
                //        foreach (var attachemnt in removeList)
                //        {
                //            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, rsm.RSMID, "RequestSupportMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                //        }
                //    }
                //}

                RequestSupportRepo.Add(masterModel);
                ItemRepo.AddRange(itemDetailsModel);
                VehicleRepo.AddRange(requestSupportVehicleMethodModel);
                FacilitiesRepo.AddRange(requestSupportFacilitiesMethodModel);
                RenovationORMaintenanceRepo.AddRange(renovationORMaintenanceDetailsModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingRSM.IsDraft && masterModel.IsModified))
                    {

                        string approvalTitle = $"{Util.AdminSupportRequestApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Support Request Reference No:{masterModel.ReferenceNo}";
                        ApprovalProcessID = CreateApprovalProcessByAPTypeIDAndAPPanelID((int)masterModel.RSMID, Util.AutoAdminSupportRequestAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.AdminSupportRequest, (int)Util.ApprovalPanel.AdminSupportRequest);
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
                        //await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.RSMID, (int)Util.MailGroupSetup.AdminRequestSupportInitiatedMail, (int)Util.ApprovalType.AdminSupportRequest);
                        await SendMailFromRequestCreated(ApprovalProcessID, false, masterModel.RSMID, rsm.EmployeeID > 0 ? (int)Util.MailGroupSetup.AdminRequestSupportEmployeeInitiatedMail : (int)Util.MailGroupSetup.AdminRequestSupportInitiatedMail, (int)Util.ApprovalType.AdminSupportRequest);
                }

            }
            await Task.CompletedTask;

            return (true, $"Request Support Submitted Successfully"); ;
        }
        private string GenerateRequestSupportReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/ARS/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("AdminRequestSupportRefNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }
        private List<RequestSupportItemDetails> GenerateSupportRequestItem(RequestSupportDto IP)
        {
            var existingRequestItem = ItemRepo.GetAllList(x => x.RSMID == IP.RSMID);
            var itemModel = new List<RequestSupportItemDetails>();
            if (IP.ItemDetails.IsNotNull())
            {
                IP.ItemDetails.ForEach(x =>
                {
                    itemModel.Add(new RequestSupportItemDetails
                    {
                        RSIDID = x.RSIDID,
                        RSMID = IP.RSMID,
                        ItemID = x.ItemID,
                        Quantity = x.Quantity,
                        Remarks = x.Remarks
                    });

                });

                itemModel.ForEach(x =>
                {
                    if (existingRequestItem.Count > 0 && x.RSIDID > 0)
                    {
                        var existingModelData = existingRequestItem.FirstOrDefault(y => y.RSIDID == x.RSIDID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.RSMID = IP.RSMID;
                        x.SetAdded();
                        SetRequestItemNewId(x);
                    }
                });

                var willDeleted = existingRequestItem.Where(x => !itemModel.Select(y => y.RSIDID).Contains(x.RSIDID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    itemModel.Add(x);
                });
            }

            return itemModel;
        }
        private void SetRequestItemNewId(RequestSupportItemDetails child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("RequestSupportItemDetails", AppContexts.User.CompanyID);
            child.RSIDID = code.MaxNumber;
        }

        private List<VehicleDetailsDto> GenerateVehicleSupportMethodDto(RequestSupportDto rsm)
        {
            var vehicleMethodChild = VehicleRepo.GetAllList(x => x.RSMID == rsm.RSMID);
            if (rsm.VehicleDetails.IsNotNull())
            {

                rsm.VehicleDetails.ForEach(x =>
                {
                    if (vehicleMethodChild.Count > 0 && x.RSVDID > 0)
                    {
                        var existingModelData = VehicleRepo.FirstOrDefault(y => y.RSVDID == x.RSVDID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.RSMID = rsm.RSMID;
                        x.SetAdded();
                        SetVehicleMethodNewId(x);
                    }
                });

                var willDeleted = vehicleMethodChild.Where(x => !rsm.VehicleDetails.Select(y => y.RSVDID).Contains(x.RSVDID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    rsm.VehicleDetails.Add(new VehicleDetailsDto
                    {
                        RSVDID = x.RSVDID,
                        RSMID = x.RSMID,
                        TransportNeededFrom = x.TransportNeededFrom,
                        TransportNeededTo = x.TransportNeededTo,
                        TransportTypeID = x.TransportTypeID,
                        PersonQuantity = x.PersonQuantity,
                        TransportQuantity = (int)x.TransportQuantity,
                        StartTime = x.StartTime.ToString(),
                        EndTime = x.EndTime.ToString(),
                        Duration = x.Duration,
                        Remarks = x.Remarks,
                        FromDivisionID = (int)x.FromDivisionID,
                        FromDistrictID = (int)x.FromDistrictID,
                        FromThanaID = (int)x.FromThanaID,
                        FromArea = x.FromArea,
                        ToDivisionID = (int)x.ToDivisionID,
                        ToDistrictID = (int)x.ToDistrictID,
                        ToThanaID = (int)x.ToThanaID,
                        ToArea = x.ToArea,

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


            return rsm.VehicleDetails;
        }
        private void SetVehicleMethodNewId(VehicleDetailsDto child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("RequestSupportVehicleDetails", AppContexts.User.CompanyID);
            child.RSVDID = code.MaxNumber;
        }

        //GenerateSupportRequestRenovationORMaintenance
        private List<RenovationORMaintenanceDetailsDto> GenerateSupportRequestRenovationORMaintenanceDto(RequestSupportDto rsm)
        {
            var renovationMethodChild = RenovationORMaintenanceRepo.GetAllList(x => x.RSMID == rsm.RSMID);
            if (rsm.RenovationORMaintenanceDetails.IsNotNull())
            {

                rsm.RenovationORMaintenanceDetails.ForEach(x =>
                {
                    if (renovationMethodChild.Count > 0 && x.RSRMDID > 0)
                    {
                        var existingModelData = RenovationORMaintenanceRepo.FirstOrDefault(y => y.RSRMDID == x.RSRMDID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.RSMID = rsm.RSMID;
                        x.SetAdded();
                        SetRenovationMethodNewId(x);
                    }
                });

                var willDeleted = renovationMethodChild.Where(x => !rsm.RenovationORMaintenanceDetails.Select(y => y.RSRMDID).Contains(x.RSRMDID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    rsm.RenovationORMaintenanceDetails.Add(new RenovationORMaintenanceDetailsDto
                    {
                        RSRMDID = x.RSRMDID,
                        RSMID = x.RSMID,
                        RenoOrMainCategoryID = x.RenoOrMainCategoryID,
                        NeededByDate = x.NeededByDate,
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
                        Attachments = GetAttachments((int)x.RSRMDID)
                    });
                });
            }

            return rsm.RenovationORMaintenanceDetails;
        }

        private List<FacilitiesDetailsDto> GenerateFacilitySupportMethodDto(RequestSupportDto rsm)
        {
            var facilityMethodChild = FacilitiesRepo.GetAllList(x => x.RSMID == rsm.RSMID);
            if (rsm.FacilitiesDetails.IsNotNull())
            {

                rsm.FacilitiesDetails.ForEach(x =>
                {
                    if (facilityMethodChild.Count > 0 && x.RSFDID > 0)
                    {
                        var existingModelData = FacilitiesRepo.FirstOrDefault(y => y.RSFDID == x.RSFDID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.RSMID = rsm.RSMID;
                        x.SetAdded();
                        SetFacilityMethodNewId(x);
                    }
                });

                var willDeleted = facilityMethodChild.Where(x => !rsm.FacilitiesDetails.Select(y => y.RSFDID).Contains(x.RSFDID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    rsm.FacilitiesDetails.Add(new FacilitiesDetailsDto
                    {
                        RSFDID = x.RSFDID,
                        RSMID = x.RSMID,
                        SupportCategoryID = x.SupportCategoryID,
                        NeededByDate = x.NeededByDate,
                        Remarks = x.Remarks,
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

            return rsm.FacilitiesDetails;
        }


        private void SetFacilityMethodNewId(FacilitiesDetailsDto child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("RequestSupportFacilitiesDetails", AppContexts.User.CompanyID);
            child.RSFDID = code.MaxNumber;
        }
        private void SetRenovationMethodNewId(RenovationORMaintenanceDetailsDto child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("RequestSupportRenovationORMaintenanceDetails", AppContexts.User.CompanyID);
            child.RSRMDID = code.MaxNumber;
        }




        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.AdminSupportRequest);
            return comments.Result;
        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int RSMID, int mailGroup)
        {

            var mail = GetAPEmployeeEmailsWithMultiProxyParallal(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2.Where(x => x.IsNotNullOrEmpty()).ToList();
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.AdminSupportRequest, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, RSMID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private void SetRSMMasterNewId(RequestSupportMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("RequestSupportMaster", AppContexts.User.CompanyID);
            master.RSMID = code.MaxNumber;
        }

        private void AddAttachments(RequestSupportDto rsm)
        {
            foreach (var details in rsm.RenovationORMaintenanceDetails)
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

                            string filename = $"RenovationORMaintenance-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "RequestSupportRenovationORMaintenanceDetails\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.RSRMDID, "RequestSupportRenovationORMaintenanceDetails", false, attachment.Size, 0, false, attachment.Description ?? "");

                            sl++;
                        }
                    }
                }
            }

        }


        //private List<Attachments> RemoveAttachments(EmployeeAccessDeactivationDto ead)
        //{
        //    if (ead.Attachments.Count > 0)
        //    {
        //        var attachemntList = new List<Attachments>();
        //        string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeAccessDeactivation' AND ReferenceID={ead.EADID}";
        //        var prevAttachment = AccessDeactivationRepo.GetDataDictCollection(attachmentSql);

        //        foreach (var data in prevAttachment)
        //        {
        //            attachemntList.Add(new Attachments
        //            {
        //                FUID = (int)data["FUID"],
        //                FilePath = data["FilePath"].ToString(),
        //                OriginalName = data["OriginalName"].ToString(),
        //                FileName = data["FileName"].ToString(),
        //                Type = data["FileType"].ToString(),
        //                Size = Convert.ToDecimal(data["SizeInKB"]),

        //            });
        //        }
        //        var removeList = attachemntList.Where(x => !ead.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

        //        if (removeList.Count > 0)
        //        {
        //            foreach (var data in removeList)
        //            {
        //                string attachmentFolder = "upload\\attachments";
        //                string folderName = "EAD";
        //                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
        //                string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
        //                System.IO.File.Delete(str + "\\" + data.FileName);

        //            }

        //        }
        //        return removeList;
        //    }
        //    return null;
        //}

        private void RemoveAttachments(RequestSupportDto rsm)
        {
            foreach (var details in rsm.RenovationORMaintenanceDetails)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "RequestSupport";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.RSRMDID, "RequestSupportRenovationORMaintenanceDetails", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='RequestSupportRenovationORMaintenanceDetails' AND ReferenceID={details.RSRMDID}";
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
                            string folderName = "RequestSupportRenovationORMaintenanceDetails";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            //File.Delete(str + "\\" + data.FileName);
                            System.IO.File.Delete(str + "\\" + data.FileName);
                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.RSRMDID, "RequestSupportRenovationORMaintenanceDetails", true, data.Size, 0, false, data.Description ?? "");

                        }

                    }
                }
            }

        }


        public async Task<RequestSupportDto> GetRequestSupport(int RSMID, int ApprovalProcessID)
        {
            string sql = $@"select DISTINCT 
                                 DU.RSMID
								,DU.SupportTypeID
								,DU.LocationOrFloor
								,DU.NeededByDate
								,DU.RemarksOrCommentsOrPurpose
								,DU.ApprovalStatusID
								,DU.AdminRemarks
                                ,ISNULL(DU.EmployeeID,0) EmployeeID
								,DU.IsDraft
								,DU.CreatedBy
								,DU.CreatedDate
								,DU.IsSettle
								,DU.SettlementDate
								,DU.ReferenceKeyword
								,DU.ReferenceNo
                                ,DU.SettlementRemarks
                                ,loc.LocationName
                                ,VA.EmployeeCode
                                ,VA.FullName EmployeeName
                                ,VA.DivisionName
								,VA.DepartmentName
								,VA.WorkMobile
								,VA.WorkEmail
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
                        from HRMS..RequestSupportMaster DU
                        LEFT JOIN HRMS..Employee Emp ON Emp.EmployeeID=DU.EmployeeID
                        LEFT JOIN Security..Location loc on loc.LocationID=Du.LocationOrFloor
                        LEFT JOIN HRMS..ViewALLEmployee EE ON EE.EmployeeID = DU.CreatedBy
						LEFT JOIN Security..SystemVariable ST ON ST.SystemVariableID = DU.SupportTypeID
                        LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = DU.ApprovalStatusID
                        
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = DU.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
										--GROUP BY ReferenceID 
                                        
										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = DU.RSMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = DU.RSMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE DU.RSMID={RSMID}  --AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID} OR DU.EmployeeID={AppContexts.User.EmployeeID})";
            var rs = RequestSupportRepo.GetModelData<RequestSupportDto>(sql);
            return rs;
        }



        private List<Attachments> GetAttachments(int RSRMDID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='RequestSupportRenovationORMaintenanceDetails' AND ReferenceID={RSRMDID}";
            var attachment = RenovationORMaintenanceRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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

        public async Task<RequestSupportDto> GetRequestSupportForReAssessment(int RSMID)
        {
            var model = RequestSupportRepo.Entities.Where(x => x.RSMID == RSMID).Select(y => new RequestSupportDto
            {
                RSMID = (int)y.RSMID,
                //DateOfResignation = y.DateOfResignation,
                //LastWorkingDay = y.LastWorkingDay,
                //IsCoreFunctional = y.IsCoreFunctional,
                //Description = y.Description,
                //EEIID = y.EEIID
            }).FirstOrDefault();


            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='RequestSupportMaster' AND ReferenceID={RSMID}";
            var attachment = RequestSupportRepo.GetDataDictCollection(attachmentSql);
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

        public IEnumerable<Dictionary<string, object>> ReportForRequestSupportAttachments(int RSMID)
        {
            string sql = $@" EXEC Security..spRPTAttachmentList {0}";
            var attachemntList = RequestSupportRepo.GetDataDictCollection(sql);
            return attachemntList;
        }

        public Dictionary<string, object> ReportForRequestSupportMaster(int RSMID)
        {
            string sql = $@" EXEC HRMS..spRPTRequestSupportMaster {RSMID}";
            var masterData = RequestSupportRepo.GetData(sql);
            return masterData;
        }

        public IEnumerable<Dictionary<string, object>> ReportForRSMApprovalFeedback(int RSMID)
        {
            string sql = $@" EXEC Security..spRPTRequestSupportApprovalFeedback {RSMID}";
            var feedback = RequestSupportRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForRSM(int RSMID, int ApprovalProcessID)
        {
            //string sql = $@" EXEC Security..spRPTRequestSupportApprovalFeedback {RSMID}";
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
	                        WHERE AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND ReferenceID = {RSMID} 
	                        AND NFAApprovalSequenceType <> 64 
                            AND ((@NFAApprovalSequenceType=55 AND @EmployeeID=(select EmployeeID from Approval..ApprovalEmployeeFeedback where ApprovalProcessID = {ApprovalProcessID} AND NFAApprovalSequenceType = @NFAApprovalSequenceType) and 1=1) 
                                    OR (AEF.EmployeeID={AppContexts.User.EmployeeID} OR ProxyEmployeeID={AppContexts.User.EmployeeID}))
	                        
	                        ORDER BY SequenceNo";
            var feedback = RequestSupportRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task RemoveRequestSupport(int RSMID)
        {
            var requestSupport = RequestSupportRepo.Entities.Where(x => x.RSMID == RSMID && x.IsDraft == true && x.CreatedBy == AppContexts.User.UserID).FirstOrDefault();
            if (requestSupport.IsNullOrDbNull())
            {
                return;
            }
            var requestSupportItem = ItemRepo.Entities.Where(x => x.RSMID == RSMID).ToList();
            var requestSupportVehicle = VehicleRepo.Entities.Where(x => x.RSMID == RSMID).ToList();
            var requestSupportFacilities = FacilitiesRepo.Entities.Where(x => x.RSMID == RSMID).ToList();
            requestSupport.SetDeleted();
            requestSupportItem.ForEach(x =>
            {
                x.SetDeleted();
            });
            requestSupportVehicle.ForEach(x =>
            {
                x.SetDeleted();
            });
            requestSupportFacilities.ForEach(x =>
            {
                x.SetDeleted();
            });
            using (var unitOfWork = new UnitOfWork())
            {

                RequestSupportRepo.Add(requestSupport);
                ItemRepo.AddRange(requestSupportItem);
                VehicleRepo.AddRange(requestSupportVehicle);
                FacilitiesRepo.AddRange(requestSupportFacilities);

                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public async Task<(bool, string)> SettleRequestSupport(int RSMID, string SettlementRemarks)
        {
            //Can Settle
            string sqlCanSettle = $@"SELECT 
		                    distinct RSM.*,
                            AEF.ApprovalProcessID,
                            CASE WHEN RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
	                        FROM 
		                    HRMS..RequestSupportMaster RSM 
		                    INNER JOIN Approval..ApprovalProcess AP ON AP.APTypeID = 26 and AP.ReferenceID=RSm.RSMID
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
	                        WHERE RSM.RSMID={RSMID} AND
	                        (AEF.EmployeeID = {AppContexts.User.EmployeeID} or AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID}  OR 
		                    Prox.EmployeeID = (CASE WHEN RequestedEmployee.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END))";
            var canSettled = RequestSupportRepo.GetModelData<RequestSupportDto>(sqlCanSettle);
            if (canSettled.IsNull() || canSettled.CanSettle == false)
            {
                return (false, $"Sorry, You are not permitted this settlement!");
            }

            var requestSupport = RequestSupportRepo.Get(RSMID);
            if (requestSupport.IsNullOrDbNull())
                return (false, "Request Support not found.");
            else if (requestSupport.ApprovalStatusID == (int)Util.ApprovalStatus.Approved && (requestSupport.IsSettle == false || requestSupport.IsSettle == null))
            {


                requestSupport.IsSettle = true;
                requestSupport.SettlementDate = DateTime.Now;
                requestSupport.SettlementRemarks = SettlementRemarks;
                requestSupport.SetModified();

                using (var unitOfWork = new UnitOfWork())
                {
                    RequestSupportRepo.Add(requestSupport);
                    unitOfWork.CommitChangesWithAudit();

                }
                //if (canSettled.SupportTypeID == (int)Util.AdminSupportCategory.Vehicle)
                //{
                //    await SendMailToSettledReceiver(canSettled.ApprovalProcessID.ToString(), requestSupport.RSMID, (int)Util.MailGroupSetup.AdminRequestSupportSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                //}
                //else
                //{
                //    await SendMailToSettledReceiver(canSettled.ApprovalProcessID.ToString(), requestSupport.RSMID, (int)Util.MailGroupSetup.AdminRequestSupportWithoutVehicleSettlementFeedbackReceieve, (int)Util.ApprovalType.AdminSupportRequest);

                //}
            }

            return (true, "Request Support is settled."); ;

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

        private async Task SendMailToSettledReceiver(string ApprovalProcessID, int RSMID, int MailGroupID, int APTypeID)
        {
            //var recieverMail = GetInitiatorEmployeeEmail(ApprovalProcessID.ToInt()).Result;
            var recieverMail = GetAPEmployeeEmailsForAllPanelMembers(ApprovalProcessID.ToInt()).Result;
            await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, MailGroupID, false, recieverMail.Item1, recieverMail.Item2, null, RSMID);
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
            var comments = RequestSupportRepo.GetDataDictCollection(sql);
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
                                INNER JOIN HRMS..RequestSupportMaster AS D ON P.ReferenceID = D.RSMID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM Approval..ManualApprovalPanelEmployee a
		                                INNER JOIN HRMS..RequestSupportMaster b ON a.ReferenceID = b.RSMID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.DivisionClearance}
			                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
			                                --AND b.RSMID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = RequestSupportRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForRequestSupport()
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
                        LEFT JOIN RequestSupportMaster RSM on RSM.CreatedBy = Emp.EmployeeID  AND RSM.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND RSM.RSMID IS NULL";
            var listDict = RequestSupportRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<Dictionary<string, object>>> DownloadRequestSupport()
        {
            string sql = $@"SELECT 
                            DISTINCT
	                             RSM.RSMID
								,RSM.SupportTypeID
								,RSM.LocationOrFloor
								,RSM.NeededByDate
								,RSM.RemarksOrCommentsOrPurpose
								,RSM.ApprovalStatusID
								,RSM.AdminRemarks
								,RSM.IsDraft
								,RSM.CreatedBy
								,RSM.CreatedDate
								,RSM.IsSettle
								,RSM.SettlementDate
								,RSM.ReferenceKeyword
								,RSM.ReferenceNo
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
                            FROM HRMS..RequestSupportMaster RSM
							JOIN Security..SystemVariable st ON st.SystemVariableID=RSM.SupportTypeID
                            LEFT JOIN Security..Users U ON U.UserID = RSM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = RSM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = RSM.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = RSM.RSMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = RSM.RSMID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND APFeedbackID = 2
								   )PendingAt ON  PendingAt.PendingReferenceID = RSM.RSMID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            )";
            var result = RequestSupportRepo.GetDataDictCollection(sql);
            return result.ToList();
        }

        public async Task<List<ItemDetailsDto>> GetitemDetails(int RSMID)
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
                    FROM HRMS..RequestSupportItemDetails c
							LEFT JOIN Security..BusinessSupportItem item ON c.ItemID = item.ItemId
							LEFT JOIN Security..Unit unit ON  item.UnitId = unit.UnitId
                        LEFT JOIN RequestSupportmaster m ON m.RSMID = c.RSMID
                        LEFT JOIN Security..BusinessSupportItem I ON I.ItemID = c.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        WHERE m.RSMID={RSMID}";
            var items = ItemRepo.GetDataModelCollection<ItemDetailsDto>(sql);
            return items;
        }

        public async Task<List<VehicleDetailsDto>> GetVehicleDetails(int RSMID)
        {
            string sql = $@"SELECT  c.*,
                            --IIF(c.Duration = 0,'00:00',dbo.MinutesToDuration(c.Duration)) Total_HOUR
                            c.Duration Total_HOUR
                            ,ISNULL(CONVERT(VARCHAR(10), CAST(c.StartTime AS TIME), 0),'') ST_TIME
	                        ,ISNULL(CONVERT(VARCHAR(10), CAST(c.EndTime AS TIME), 0),'') END_TIME,
                            div.DivisionName FromDivisionName,
							dis.DistrictName FromDistrictName,
							tna.ThanaName FromThanaName,
							toDiv.DivisionName ToDivisionName,
							toDis.DistrictName ToDistrictName,
							toTna.ThanaName ToThanaName,
                            CASE WHEN ISNULL(c.IsOthers,0)=1 THEN c.Vehicle COLLATE SQL_Latin1_General_CP1_CI_AS ELSE vd.VehicleRegNo COLLATE SQL_Latin1_General_CP1_CI_AS END AS VehicleDetl,
							CASE WHEN ISNULL(c.IsOthers,0)=1 THEN c.Driver COLLATE SQL_Latin1_General_CP1_CI_AS ELSE dd.DriverName COLLATE SQL_Latin1_General_CP1_CI_AS END AS DriverDetl,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName,
                            sc.SystemVariableCode TransportType
                    FROM RequestSupportVehicleDetails c
                        LEFT JOIN Security..SystemVariable sc on c.TransportTypeID=sc.SystemVariableID
                        LEFT JOIN RequestSupportmaster m ON m.RSMID = c.RSMID
                        LEFT JOIN Security..Division div On c.FromDivisionID=div.DivisionID
						LEFT JOIN Security..District dis On c.FromDistrictID=dis.DistrictID
						LEFT JOIN Security..Thana tna On c.FromThanaID=tna.ThanaID
						LEFT JOIN Security..Division toDiv On c.ToDivisionID=toDiv.DivisionID
						LEFT JOIN Security..District toDis On c.ToDistrictID=toDis.DistrictID
						LEFT JOIN Security..Thana toTna On c.ToThanaID=toTna.ThanaID
                        --LEFT JOIN Security..VehicleDetails vd ON c.Vehicle COLLATE Latin1_General_CI_AS=vd.VehicleID AND ISNULL(c.IsOthers,0)=0
						--LEFT JOIN Security..DriverDetails dd ON c.Driver COLLATE Latin1_General_CI_AS=dd.DriverID AND ISNULL(c.IsOthers,0)=0
                        LEFT JOIN Security..VehicleDetails vd ON (ISNULL(c.IsOthers, 0) = 0 AND c.Vehicle COLLATE Latin1_General_CI_AS = CAST(vd.VehicleID AS NVARCHAR(MAX)))
                                       OR (ISNULL(c.IsOthers, 0) = 1 AND c.Vehicle = vd.VehicleRegNo COLLATE Latin1_General_CI_AS)
                        LEFT JOIN Security..DriverDetails dd ON (ISNULL(c.IsOthers, 0) = 0 AND c.Driver COLLATE Latin1_General_CI_AS = CAST(dd.DriverID AS NVARCHAR(MAX)))
                                       OR (ISNULL(c.IsOthers, 0) = 1 AND c.Driver = dd.DriverName COLLATE Latin1_General_CI_AS)
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        WHERE m.RSMID={RSMID}";
            var vehicle = VehicleRepo.GetDataModelCollection<VehicleDetailsDto>(sql);
            return vehicle;
        }

        public async Task<List<FacilitiesDetailsDto>> GetFacilitiesDetails(int RSMID)
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
							sc.SystemVariableCode SupportCategoryName
                    FROM RequestSupportFacilitiesDetails c
						LEFT JOIN Security..SystemVariable sc on c.SupportCategoryID=sc.SystemVariableID
                        LEFT JOIN RequestSupportmaster m ON m.RSMID = c.RSMID
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        WHERE m.RSMID={RSMID}";
            //var facilities = FacilitiesRepo.GetDataModelCollection<FacilitiesDetailsDto>(sql);
            //return facilities;
            var facilities = FacilitiesRepo.GetDataModelCollection<FacilitiesDetailsDto>(sql);
            return await Task.FromResult(facilities);
        }
        public async Task<List<RenovationORMaintenanceDetailsDto>> GetRenovationORMaintenanceDetails(int RSMID)
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
							sc.RenovationName RenoOrMainCategoryName
                    FROM RequestSupportRenovationORMaintenanceDetails c
						LEFT JOIN RenovationORMaintenanceCategory sc on c.RenoOrMainCategoryID=sc.ROMID
                        LEFT JOIN RequestSupportmaster m ON m.RSMID = c.RSMID
                        LEFT JOIN Security..Users U ON U.UserID = c.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = m.ApprovalStatusID
                        WHERE m.RSMID={RSMID}";
            //var facilities = FacilitiesRepo.GetDataModelCollection<FacilitiesDetailsDto>(sql);
            //return facilities;
            var facilities = FacilitiesRepo.GetDataModelCollection<RenovationORMaintenanceDetailsDto>(sql);
            facilities.ForEach(x => x.Attachments = GetAttachments((int)x.RSRMDID));
            return await Task.FromResult(facilities);
        }


        public async Task<List<Dictionary<string, object>>> GetAllSupportRequestListByWhereCondition(string whereCondition, string fromDate, string toDate)
        {
            string dateFilterCondition = string.Empty;
            if(!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                // = $@" AND CAST(M.CreatedDate as date) between IIF(('{fromDate}' is null OR '{fromDate}'=''),'2020-01-01',CAST('{fromDate}' as DATE)) and IIF(('{toDate}' is null OR '{toDate}'=''),CAST(Getdate() as date),CAST('{toDate}' as DATE))";
                dateFilterCondition = $@" AND CAST(M.CreatedDate as date) between CAST('{fromDate}' as DATE) and CAST('{toDate}' as DATE)";
            }

            if (whereCondition.ToInt() == (int)Util.AdminSupportCategory.Vehicle)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLVehicleSupportRequest";
                string sql = $@"SELECT 
                                ROW_NUMBER() OVER (ORDER BY V.RSVDID) AS RowId
                                ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',E.WorkMobile AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                ,M.ReferenceNo AS 'Reference No'
                                ,SV.SystemVariableCode  AS 'Support Type'
                                ,sc.SystemVariableCode  AS 'Vehicle Type'
                                ,V.TransportQuantity AS 'Number of Vehicle'
                                ,M.RemarksOrCommentsOrPurpose Purpose 
                                ,V.FromArea AS 'Location From (Area)'
                                ,V.ToArea AS 'Location To (Area)'
                                ,V.Remarks  AS 'Creator Remarks'
                                ,CASE WHEN ISNULL(V.IsOthers,0)=1 THEN V.Vehicle COLLATE SQL_Latin1_General_CP1_CI_AS ELSE vd.VehicleRegNo COLLATE SQL_Latin1_General_CP1_CI_AS END AS 'Vehicle No.'
                                ,CASE WHEN ISNULL(V.IsOthers,0)=1 THEN V.Driver COLLATE SQL_Latin1_General_CP1_CI_AS ELSE dd.DriverName COLLATE SQL_Latin1_General_CP1_CI_AS END AS 'Driver Name'
                                ,V.ContactNumber AS 'Driver Cell No.'
                                ,APP.FullName AS 'Response By (Admin)'
                                ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                ,M.AdminRemarks AS 'Admin Remarks'
                                ,SVA.SystemVariableCode AS 'Approval Status'
                                FROM RequestSupportMaster AS M
                                JOIN Security..SystemVariable SV on M.SupportTypeID=SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                JOIN RequestSupportVehicleDetails AS V ON M.RSMID = V.RSMID
                                LEFT JOIN Security..SystemVariable sc on V.TransportTypeID=sc.SystemVariableID
                                LEFT JOIN Security..VehicleDetails vd ON (ISNULL(V.IsOthers, 0) = 0 AND V.Vehicle COLLATE Latin1_General_CI_AS = CAST(vd.VehicleID AS NVARCHAR(MAX)))
                                                                       OR (ISNULL(V.IsOthers, 0) = 1 AND V.Vehicle = vd.VehicleRegNo COLLATE Latin1_General_CI_AS)
                                LEFT JOIN Security..DriverDetails dd ON (ISNULL(V.IsOthers, 0) = 0 AND V.Driver COLLATE Latin1_General_CI_AS = CAST(dd.DriverID AS NVARCHAR(MAX)))
                                                                       OR (ISNULL(V.IsOthers, 0) = 1 AND V.Driver = dd.DriverName COLLATE Latin1_General_CI_AS)
                                LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                			from 
                                			Approval..ApprovalProcess AP
                                			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                			where AP.APTypeID=26)APP ON M.RSMID=APP.ReferenceID
                                LEFT JOIN (
                                			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
                                			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                {dateFilterCondition}";
                var data = RequestSupportRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.AdminSupportCategory.ConsumbleGoods)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLConsumbleGoodsSupportRequest";
                string sql = $@"SELECT 
                                ROW_NUMBER() OVER (ORDER BY C.RSIDID) AS RowId
                                ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',E.WorkMobile AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                ,M.ReferenceNo AS 'Reference No'
                                ,Loc.LocationName AS 'Location Name'
                                ,SV.SystemVariableCode AS 'Support Type'
                                --,I.ItemCode+'-'+I.ItemName AS 'Item Name & Code'
                                ,I.ItemName AS 'Item Name & Code'
                                ,u.UnitCode UOM
                                ,C.Quantity
                                ,APP.FullName AS 'Response By (Admin)'
                                ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                ,C.Remarks AS 'Creator Remarks'
                                ,M.AdminRemarks AS 'Admin Remarks'
                                ,SVA.SystemVariableCode AS 'Approval Status'
                                
                                FROM RequestSupportMaster AS M
                                JOIN RequestSupportItemDetails AS C ON M.RSMID = C.RSMID
                                JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                JOIN Security..SystemVariable SV on M.SupportTypeID=SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                LEFT JOIN Security..Location loc on loc.LocationID=M.LocationOrFloor
                                LEFT JOIN Security..BusinessSupportItem I ON I.ItemID = C.ItemID
                                LEFT JOIN Security..Unit u ON u.UnitID=I.UnitID
                                LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                			from 
                                			Approval..ApprovalProcess AP
                                			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                			where AP.APTypeID=26)APP ON M.RSMID=APP.ReferenceID
                                LEFT JOIN (
                                			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
                                			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                {dateFilterCondition}";
                var data = RequestSupportRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            if (whereCondition.ToInt() == (int)Util.AdminSupportCategory.Facilities)
            {
                string sql = $@"SELECT 
                                ROW_NUMBER() OVER (ORDER BY F.RSFDID) AS RowId
                                ,E.FullName AS 'Created By',E.EmployeeCode AS 'Creator ID',E.DepartmentName AS 'Department',E.WorkMobile AS 'Contact Number',E.WorkEmail AS 'Email'
                                ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                ,M.ReferenceNo AS 'Reference No'
                                ,Loc.LocationName AS 'Location Name'
                                ,SV.SystemVariableCode  AS 'Support Type'
                                ,M.RemarksOrCommentsOrPurpose Purpose 
                                ,sc.SystemVariableCode AS 'Support Category Name'
                                ,FORMAT(CONVERT(DATETIME, F.NeededByDate), 'M/d/yy hh:mm tt') AS 'Needed By Date & Time'
                                ,F.Remarks AS 'Creator Remarks'
                                ,APP.FullName AS 'Response By (Admin)'
                                ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                ,M.AdminRemarks AS 'Admin Remarks'
                                ,SVA.SystemVariableCode AS 'Approval Status'
                                FROM RequestSupportMaster AS M
                                JOIN Security..SystemVariable SV on M.SupportTypeID=SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                LEFT JOIN Security..Location loc on loc.LocationID=M.LocationOrFloor
                                JOIN RequestSupportFacilitiesDetails AS F ON M.RSMID = F.RSMID
                                LEFT JOIN Security..SystemVariable sc on F.SupportCategoryID=sc.SystemVariableID
                                LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                			from 
                                			Approval..ApprovalProcess AP
                                			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                			where AP.APTypeID=26)APP ON M.RSMID=APP.ReferenceID
                                LEFT JOIN (
                                			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
                                			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                               {dateFilterCondition}";

                var data = RequestSupportRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }

            if (whereCondition.ToInt() == (int)Util.AdminSupportCategory.RenovationOrMaintenance)
            {
                //string sql = $@"SELECT * FROM HRMS..ViewALLConsumbleGoodsSupportRequest";
                string sql = $@"SELECT 
                                ROW_NUMBER() OVER (ORDER BY C.RSRMDID) AS RowId
                                ,E.FullName AS 'Creator',E.EmployeeCode AS 'Employee Code',E.DepartmentName AS 'Department',E.WorkMobile AS 'User Mobile No',E.WorkEmail AS 'Email ID'
                                ,FORMAT(CONVERT(DATETIME, M.CreatedDate), 'M/d/yy hh:mm tt') AS 'Created Date & Time'
                                ,M.ReferenceNo AS 'Reference No'
                                ,Loc.LocationName AS 'Location Name'
                                ,SV.SystemVariableCode AS 'Support Type'
                                ,I.RenovationName AS 'Renovation/Maintenance Category'
								,c.NeededByDate
                                ,APP.FullName AS 'Response By (Admin)'
                                ,FORMAT(CONVERT(DATETIME, APP.FeedbackLastResponseDate), 'M/d/yy hh:mm tt') AS 'Approved Date & Time'
                                ,C.Remarks AS 'Creator Remarks'
                                ,M.AdminRemarks AS 'Admin Remarks'
                                ,SVA.SystemVariableCode AS 'Approval Status'
                                
                                FROM RequestSupportMaster AS M
                                JOIN RequestSupportRenovationORMaintenanceDetails AS C ON M.RSMID = C.RSMID
                                JOIN HRMS..ViewALLEmployee E ON E.UserID=M.CreatedBy
                                JOIN Security..SystemVariable SV on M.SupportTypeID=SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable SVA ON SVA.SystemVariableID = M.ApprovalStatusID
                                LEFT JOIN Security..Location loc on loc.LocationID=M.LocationOrFloor
                                LEFT JOIN HRMS..RenovationORMaintenanceCategory I ON I.ROMID = C.RenoOrMainCategoryID
                                LEFT JOIN (select AP.ApprovalProcessID,AP.ReferenceID ,FE.FullName,AEF.FeedbackLastResponseDate
                                			from 
                                			Approval..ApprovalProcess AP
                                			LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID=AP.ApprovalProcessID AND ISNULL(AEF.IsEditable,0)=1
                                			LEFT JOIN HRMS..ViewALLEmployee FE ON FE.EmployeeID=AEF.EmployeeID
                                			where AP.APTypeID=26)APP ON M.RSMID=APP.ReferenceID
                                LEFT JOIN (
                                			SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
                                			)FE ON FE.ApprovalProcessID = APP.ApprovalProcessID
                                WHERE (E.EmployeeID = {AppContexts.User.EmployeeID} OR FE.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(FE.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})
                                {dateFilterCondition}";
                var data = RequestSupportRepo.GetDataDictCollection(sql);
                return await Task.FromResult(data.ToList());
            }
            else
            {
                return await Task.FromResult(new List<Dictionary<string, object>>());
            }

            //string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";
            //string sql = $@"SELECT * FROM ViewALLEmployeeForExcel {where}";
            //var data = RequestSupportRepo.GetDataDictCollection(sql);


        }

        public GridModel GetAllListForGrid(GridParameter parameters)
        {
            string filter = "";
            //string filter2 = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND RSM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            //if (!string.IsNullOrEmpty(parameters.AdditionalFilterData))
            //{
            //    filter2 = $@" AND SupportTypeID = {parameters.AdditionalFilterData}";
            //}
            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            RSM.RSMID
                                ,RSM.SupportTypeID
                                ,RSM.LocationOrFloor
                                ,RSM.NeededByDate
                                ,RSM.RemarksOrCommentsOrPurpose
                                ,RSM.ApprovalStatusID
                                ,RSM.AdminRemarks
                                ,RSM.IsDraft
                                ,ISNULL(RSM.IsSettle,0) IsSettle
                                ,RSM.SettlementDate
                                ,RSM.ReferenceKeyword
                                ,RSM.ReferenceNo
                                ,RSM.CreatedBy
                                ,RSM.EmployeeID
                                ,RSM.CreatedDate
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
                                ,FeedbackLastResponseDate
								,CommentSubmitDate
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND RSM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                --,CASE WHEN CanSettle=0 AND RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,CASE WHEN (RSM.CreatedBy = U.UserID AND RSM.ApprovalStatusID=23 AND ISNULL(RSM.IsSettle,0)=0) THEN 1 ELSE 0 END CanSettle
                                , COALESCE(RSM.SettlementRemarks,'') AS SettlementRemarks
                            FROM HRMS..RequestSupportMaster RSM
                            LEFT JOIN Security..SystemVariable st ON RSM.SupportTypeID=st.SystemVariableID
                            LEFT JOIN Security..Users U ON U.UserID = RSM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = RSM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = RSM.RSMID AND AP.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = RSM.RSMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = RSM.RSMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.AdminSupportRequest} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = RSM.RSMID
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
							WHERE ISNULL(RSM.EmployeeID,0)=0 AND (RSM.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter} ";
            var result = RequestSupportRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


    }
}
