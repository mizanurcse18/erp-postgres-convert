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
    public class EmployeeAccessDeactivationManager : ManagerBase, IEmployeeAccessDeactivationManager
    {

        private readonly IRepository<EmployeeAccessDeactivation> AccessDeactivationRepo;
        public EmployeeAccessDeactivationManager(IRepository<EmployeeAccessDeactivation> accessDeactivationRepo)
        {
            AccessDeactivationRepo = accessDeactivationRepo;
        }

        public async Task<GridModel> GetListForGrid(GridParameter parameters)
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
                    filter = $@" AND EAD.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND EAD.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                                 EAD.EADID
            	                ,EAD.DateOfResignation
            	                ,EAD.LastWorkingDay
            	                ,EAD.IsCoreFunctional
            	                ,EAD.ApprovalStatusID
            	                ,EAD.IsSentForDivisionClearance
            	                ,EAD.DivisionClearanceApprovalStatusID
            	                ,EAD.SentForDivisionClearanceDate
                                ,CASE WHEN isnull(EAD.IsSentForDivisionClearance,0)=0 then 'No' Else 'Yes' End AS DivClearenceStatus
            	                ,EAD.IsDraft
            	                ,EAD.Description
            	                ,EAD.EEIID
                                ,EAD.CreatedDate
                                ,VA.DepartmentID
                                ,VA.DepartmentName
                                ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
            	                ,EE.DepartmentID EEDepartmentID
                                ,EE.DepartmentName EEDepartmentName
                                ,EE.FullName AS EEEmployeeName,EE.EmployeeCode AS EEEmployeeCode, EE.DivisionID EEDivisionID, EE.DivisionName EEDivisionName
                                ,SV.SystemVariableCode AS ApprovalStatus
                                ,VA.ImagePath
                                ,VA.EmployeeCode
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployeeParallel]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0),AEF.APEmployeeFeedbackID)) AS Bit) IsCurrentAPEmployee
                                ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                                ,ISNULL(APForwardInfoID,0) APForwardInfoID
            	                ,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
            	                ,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,EAD.CreatedBy
                                ,CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy
                                ,Particulars
                                ,Particulars+' '+(CASE WHEN APSubmitDate.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END) ParticularsWtihProxy
                                 FROM HRMS..EmployeeAccessDeactivation EAD
                                -- JOIN EmployeeExitInterview EI ON EI.EEIID=EAD.EEIID --AND EI.ApprovalStatusID=23
                                LEFT JOIN HRMS..ViewALLEmployeeRegularJoin EE ON EE.EmployeeID = EAD.EmployeeID
                                 LEFT JOIN Security..Users U ON U.UserID = EAD.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = EAD.ApprovalStatusID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
                                 LEFT JOIN (
                                             SELECT 
                                                   APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy, Particulars
                                             FROM 
                                                 Approval.dbo.functionJoinListAEFParallel({AppContexts.User.EmployeeID})
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
            			--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation} 
            			--GROUP BY ReferenceID

            			--UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = EAD.EADID

                                             LEFT JOIN(
            			SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
            			FROM 
            				Approval..ApprovalEmployeeFeedbackRemarks AEFR 
            				INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
            			WHERE APFeedbackID = 11 --Returned
            			GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
            		) Rej ON Rej.ReferenceID = EAD.EADID
                                         LEFT JOIN ( 
                                                 SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDateParallel({AppContexts.User.EmployeeID})                                
            		)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID AND APSubmitDate.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
            		LEFT JOIN (
            						SELECT 
            						   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
            						FROM 
            							Approval..ApprovalForwardInfo  
            						WHERE 
            							EmployeeID = {AppContexts.User.EmployeeID} 
            						GROUP BY ApprovalProcessID

            							) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                         --LEFT JOIN (
            	  --     SELECT 
            			--    AEF.EmployeeCode PendingEmployeeCode,
            			--    EmployeeName PendingEmployeeName,
            			--    DepartmentName PendingDepartmentName,
            			--    AEF.ReferenceID PendingReferenceID
            		 --   FROM 
            			--    Approval..viewApprovalEmployeeFeedback AEF 
            		 --   WHERE AEF.APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
            	  -- )PendingAt ON  PendingAt.PendingReferenceID = EAD.EADID
            WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                                 OR EAD.EmployeeID={AppContexts.User.EmployeeID}) {filter}";


            var result = AccessDeactivationRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }
        public async Task<GridModel> GetDivClearenceListForGrid(GridParameter parameters)
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
                    filter = $@" AND EAD.DivisionClearanceApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND EAD.DivisionClearanceApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                             EAD.EADID
								,EAD.DateOfResignation
								,EAD.LastWorkingDay
								,EAD.IsCoreFunctional
								,EAD.ApprovalStatusID
								,EAD.IsSentForDivisionClearance
								,EAD.DivisionClearanceApprovalStatusID
								,EAD.SentForDivisionClearanceDate
								,EAD.IsDraft
								,EAD.Description
								,EAD.EEIID
                                ,EAD.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
								,EE.DepartmentID EEDepartmentID
	                            ,EE.DepartmentName EEDepartmentName
	                            ,EE.FullName AS EEEmployeeName,EE.EmployeeCode AS EEEmployeeCode, EE.DivisionID EEDivisionID, EE.DivisionName EEDivisionName
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
								,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM HRMS..EmployeeAccessDeactivation EAD
							-- JOIN EmployeeExitInterview EI ON EI.EEIID=EAD.EEIID --AND EI.ApprovalStatusID=23
							LEFT JOIN HRMS..ViewALLEmployeeRegularJoin EE ON EE.EmployeeID = EAD.EmployeeID
                            LEFT JOIN Security..Users U ON U.UserID = EAD.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = EAD.DivisionClearanceApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.DivisionClearance}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.DivisionClearance} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.DivisionClearance} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EADID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = EAD.EADID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.DivisionClearance} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = EAD.EADID
							WHERE isnull(EAD.IsSentForDivisionClearance,0)=1 AND (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter}";
            var result = AccessDeactivationRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<(bool, string)> SaveChanges(EmployeeAccessDeactivationDto ead)
        {
            var existingEAD = AccessDeactivationRepo.Entities.Where(x => x.EADID == ead.EADID).SingleOrDefault();
            var removeList = RemoveAttachments(ead);

            if (ead.EmployeeID.IsZero())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }

            #region Exit Employee Validation

       //     string sqlExtEmp = @$"SELECT
       //                         FullName
       //                         ,EmployeeCode
       //                         ,DepartmentName
       //                         ,DivisionName
       //                         FROM
							//EmployeeExitInterview ExtEmp
       //                     INNER JOIN Employee emp ON Emp.EmployeeID = ExtEmp.EmployeeID
       //                     LEFT JOIN Employment empl ON empl.EmployeeID = emp.EmployeeID AND IsCurrent = 1
       //                     LEFT JOIN Department Dep ON Dep.DepartmentID = empl.DepartmentID
       //                     LEFT JOIN Division Div on Div.DivisionID = empl.DivisionID
							//WHERE ExtEmp.EEIID = {ead.EEIID}";

            string sqlExtEmp = $@"Select FullName,EmployeeCode,DepartmentName,DivisionName from HRMS..ViewALLEmployee where EmployeeID={ead.EmployeeID}";

            var exitEmployee = AccessDeactivationRepo.GetModelData<EmployeeDto>(sqlExtEmp);

            if (exitEmployee.IsNull())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }

            //if (exitEmployee.EmployeeID.IsZero())
            //{
            //     exitEmployee = AccessDeactivationRepo.GetModelData<EmployeeDto>(sqlEmp);
            //}

            #endregion

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (ead.EADID > 0 && ead.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..EmployeeAccessDeactivation EAD
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = EAD.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.Default)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EADID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {ead.ApprovalProcessID}";
                var canReassesstment = AccessDeactivationRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit EAD once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit EAD once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback(ead.EADID, ead.ApprovalProcessID, (int)Util.ApprovalType.AccessDeactivation);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new EmployeeAccessDeactivation
            {
                EEIID = ead.EEIID,
                EmployeeID = ead.EmployeeID,
                DateOfResignation = ead.DateOfResignation,
                Description = ead.Description,
                LastWorkingDay = ead.LastWorkingDay,
                IsDraft = ead.IsDraft,
                //IsCoreFunctional = ead.IsCoreFunctional,
                ApprovalStatusID = ead.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (ead.EADID.IsZero() && existingEAD.IsNull())
                {
                    masterModel.SetAdded();
                    SetEADMasterNewId(masterModel);
                    ead.EADID = (int)masterModel.EADID;
                }
                else
                {
                    masterModel.CreatedBy = existingEAD.CreatedBy;
                    masterModel.CreatedDate = existingEAD.CreatedDate;
                    masterModel.CreatedIP = existingEAD.CreatedIP;
                    masterModel.RowVersion = existingEAD.RowVersion;
                    masterModel.EADID = ead.EADID;
                    masterModel.EEIID = ead.EEIID;
                    masterModel.EmployeeID = ead.EmployeeID;
                    masterModel.DateOfResignation = existingEAD.DateOfResignation;
                    masterModel.LastWorkingDay = existingEAD.LastWorkingDay;
                    //masterModel.Description = existingEAD.Description;
                    masterModel.SetModified();
                }

                SetAuditFields(masterModel);

                if (ead.Attachments.IsNotNull() && ead.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(ead.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, ead.EADID, "EmployeeAccessDeactivation", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, ead.EADID, "EmployeeAccessDeactivation", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                AccessDeactivationRepo.Add(masterModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingEAD.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.EADApprovalTitle} {exitEmployee.FullName}-{exitEmployee.EmployeeCode}||{exitEmployee.DivisionName}||{exitEmployee.DepartmentName}";
                        ApprovalProcessID = CreateApprovalProcessParallel((int)masterModel.EADID, Util.AutoEADAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.AccessDeactivation, (int)Util.ApprovalPanel.AccessDeactivation);
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessFeedbackForParallel((int)approvalProcessFeedBack["ApprovalProcessID"],
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
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        await SendMail(ApprovalProcessID, IsResubmitted, (int)masterModel.EADID, (int)Util.MailGroupSetup.EmployeeAccessDeactivationInitiatedMail);
                }
            }
            await Task.CompletedTask;

            return (true, $"EAD Submitted Successfully"); ;
        }

        public async Task<(bool, string)> SaveChangesDivisionClearence(EmployeeAccessDeactivationDto ead)
        {
            var existingEAD = AccessDeactivationRepo.Entities.Where(x => x.EADID == ead.EADID).SingleOrDefault();

            if (ead.EmployeeID.IsZero())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }

            #region Exit Employee Validation

            //     string sqlExtEmp = @$"SELECT
            //                         FullName
            //                         ,EmployeeCode
            //                         ,DepartmentName
            //                         ,DivisionName
            //                         FROM
            //EmployeeExitInterview ExtEmp
            //                     INNER JOIN Employee emp ON Emp.EmployeeID = ExtEmp.EmployeeID
            //                     LEFT JOIN Employment empl ON empl.EmployeeID = emp.EmployeeID AND IsCurrent = 1
            //                     LEFT JOIN Department Dep ON Dep.DepartmentID = empl.DepartmentID
            //                     LEFT JOIN Division Div on Div.DivisionID = empl.DivisionID
            //WHERE ExtEmp.EEIID = {ead.EEIID}";

            string sqlExtEmp = $@"Select FullName,EmployeeCode,DepartmentName,DivisionName from ViewALLEmployee where EmployeeID={ead.EmployeeID}";

            var exitEmployee = AccessDeactivationRepo.GetModelData<EmployeeDto>(sqlExtEmp);

            if (exitEmployee.IsNull())
            {
                return (false, $"Sorry, Exit Employee not found!");
            }

            #endregion


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (ead.EADID > 0 && ead.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM HRMS..EmployeeAccessDeactivation EAD
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = EAD.CreatedBy
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.Default)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.DivisionClearance}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.DivisionClearance} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EADID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {ead.ApprovalProcessID}";
                var canReassesstment = AccessDeactivationRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit EAD once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit EAD once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback(ead.EADID, ead.ApprovalProcessID, (int)Util.ApprovalType.DivisionClearance);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;


            using (var unitOfWork = new UnitOfWork())
            {



                existingEAD.IsSentForDivisionClearance = true;
                existingEAD.SentForDivisionClearanceDate = existingEAD.IsDraftForDivClearence ? existingEAD.SentForDivisionClearanceDate : DateTime.Now;
                existingEAD.IsDraftForDivClearence = ead.IsDraftForDivClearence ? ead.IsDraftForDivClearence : false;
                existingEAD.DivisionClearanceApprovalStatusID = ead.IsDraftForDivClearence ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending;


                existingEAD.SetModified();

                SetAuditFields(existingEAD);


                if (ead.ApprovalProcessID == 0)
                {
                    if (ead.DivisionClearenceApprovalPanelList.IsNotNull() && ead.DivisionClearenceApprovalPanelList.Count > 0)
                    {
                        DeleteManualApprovalPanel((int)existingEAD.EADID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext),(int)Util.ApprovalType.DivisionClearance);
                        foreach (var item in ead.DivisionClearenceApprovalPanelList)
                        {
                            SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.DivisionClearance, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.DivisionClearance, (int)existingEAD.EADID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));

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

                AccessDeactivationRepo.Add(existingEAD);

                unitOfWork.CommitChangesWithAudit();

                if (!existingEAD.IsDraftForDivClearence && ead.ApprovalProcessID == 0)
                {
                    string approvalTitle = $"{Util.DivisionClearenceTitle} {exitEmployee.FullName}-{exitEmployee.EmployeeCode}||{exitEmployee.DivisionName}||{exitEmployee.DepartmentName}, DivisionClearance Reference No:{""}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)existingEAD.EADID, Util.DivisionClearenceTitle, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.DivisionClearance, (int)Util.ApprovalPanel.DivisionClearance, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                }
                if (!existingEAD.IsDraftForDivClearence)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        await SendMailDC(ApprovalProcessID, IsResubmitted, (int)existingEAD.EADID, (int)Util.MailGroupSetup.DivisionClearenceInitiatedMail);
                }
            }
            await Task.CompletedTask;

            return (true, $"Send Division Successfully"); ;
        }

        private List<Attachments> RemoveAttachments(EmployeeAccessDeactivationDto ead)
        {
            if (ead.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeAccessDeactivation' AND ReferenceID={ead.EADID}";
                var prevAttachment = AccessDeactivationRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !ead.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "EAD";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.AccessDeactivation);
            return comments.Result;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalCommentDivClearence(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.DivisionClearance);
            return comments.Result;
        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int EADID, int mailGroup)
        {

            var mail = GetAPEmployeeEmailsWithMultiProxyParallal(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2.Where(x=> x.IsNotNullOrEmpty()).ToList();
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.AccessDeactivation, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, EADID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private async Task SendMailDC(string ApprovalProcessID, bool IsResubmitted, int EADID, int mailGroup)
        {

            var mail = GetAPEmployeeEmailsWithMultiProxyParallal(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = mail.Item1;
            List<string> CCEmailAddress = mail.Item2;
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.DivisionClearance, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, EADID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private void SetEADMasterNewId(EmployeeAccessDeactivation master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("EmployeeAccessDeactivation", AppContexts.User.CompanyID);
            master.EADID = code.MaxNumber;
        }

        private List<Attachments> AddAttachments(List<Attachments> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"EAD-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "EAD\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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

        public async Task<EmployeeAccessDeactivationDto> GetAccessDeactivation(int EADID, int ApprovalProcessID)
        {
            string sql = $@"SELECT DISTINCT EAD.EADID
	                ,EAD.DateOfResignation
	                ,EAD.LastWorkingDay
	                ,EAD.IsCoreFunctional
	                ,EAD.ApprovalStatusID
	                ,EAD.IsSentForDivisionClearance
	                ,EAD.DivisionClearanceApprovalStatusID
	                ,EAD.SentForDivisionClearanceDate
	                ,EAD.IsDraft
	                ,EAD.EEIID
	                ,EAD.Description
	                ,EAD.CreatedDate
	                ,EAD.CreatedBy
	                ,VA.DepartmentID
	                ,VA.DepartmentName
	                ,VA.FullName AS EmployeeName
	                ,SV.SystemVariableCode AS ApprovalStatus
	                ,VA.ImagePath
	                ,VA.EmployeeCode
	                ,VA.DivisionID
	                ,VA.DivisionName
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
	                ,EAD.EmployeeID
	                ,VA.WorkMobile
	                ,EE.FullName EEFullName
					,EE.DesignationName EEDesignationName
					,EE.WorkEmail EEEmail
	                ,EE.EmployeeCode EEEmployeeCode
	                ,EE.DepartmentName EEDepartmentName
	                ,EE.DivisionName EEDivisionName
	                ,EE.WorkMobile EEMobile
					,EE.DateOfJoining
					,EE.EmployeeStatus EmployeeType
                    ,EE.ImagePath EmpImagePath
                    ,Sup.SupervisorFullName
                    ,CONCAT(DATEDIFF(YEAR, EE.DateOfJoining, EAD.LastWorkingDay) - (CASE 
							    WHEN (MONTH(EAD.LastWorkingDay) < MONTH(EE.DateOfJoining)) OR 
							         (MONTH(EAD.LastWorkingDay) = MONTH(EE.DateOfJoining) AND DAY(EAD.LastWorkingDay) < DAY(EE.DateOfJoining)) 
							    THEN 1 
							    ELSE 0 
							END),' years ',(DATEDIFF(MONTH, EE.DateOfJoining, EAD.LastWorkingDay) % 12) - (DATEDIFF(DAY, EE.DateOfJoining, EAD.LastWorkingDay) % 30 / 30),' months ',(DATEDIFF(DAY, EE.DateOfJoining, EAD.LastWorkingDay) % 30),' days '
							)AS Tenure
	                ,(
		                CASE 
			                WHEN isnull(EAD.IsCoreFunctional, 0) = 0
				                THEN '72'
			                ELSE '71'
			                END
		                ) AS FunctionalOrNot
                FROM HRMS..EmployeeAccessDeactivation EAD
                LEFT JOIN HRMS..ViewALLEmployee EE ON EE.EmployeeID = EAD.EmployeeID
				LEFT JOIN HRMS..ViewEmployeeSupervisorMap Sup ON Sup.EmployeeID = EAD.EmployeeID
                LEFT JOIN Security..Users U ON U.UserID = EAD.CreatedBy
                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext) }..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext) }..SystemVariable SV ON SV.SystemVariableID = EAD.ApprovalStatusID
                LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID
	                AND AP.APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
                LEFT JOIN (
	                SELECT COUNT(cntr) EditableCount
		                ,ReferenceID
	                FROM (
		                --SELECT 
		                --	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
		                --FROM 
		                --	Approval..ApprovalEmployeeFeedback  AEF
		                --	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
		                --where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
		                --GROUP BY ReferenceID 
		                --UNION ALL
		                SELECT COUNT(APEmployeeFeedbackID) Cntr
			                ,ReferenceID
		                FROM Approval..ApprovalEmployeeFeedback AEF
		                LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
		                WHERE SequenceNo = 1
			                AND APFeedbackID = 2
			                AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
			                AND EmployeeID = {AppContexts.User.EmployeeID}
		                GROUP BY ReferenceID
		                ) V
	                GROUP BY ReferenceID
	                ) EA ON EA.ReferenceID = EAD.EADID
               LEFT JOIN (
                        	SELECT AP.ApprovalProcessID
                        		,COUNT(ISNULL(APFeedbackID, 0)) Cntr
                        		,AP.ReferenceID
                        	FROM Approval..ApprovalEmployeeFeedback AEFR
                        	INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
                        	WHERE APFeedbackID = 11 --Returned
                        	GROUP BY AP.ApprovalProcessID
                        		,AP.ReferenceID
                        	) Rej ON Rej.ReferenceID = EAD.EADID
                LEFT JOIN (
	                SELECT ApprovalProcessID
		                ,EmployeeID
		                ,ProxyEmployeeID
	                FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID})
	                ) F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                WHERE EAD.EADID = {EADID}
	                AND (
		                VA.EmployeeID = {AppContexts.User.EmployeeID}
		                OR F.EmployeeID = {AppContexts.User.EmployeeID}
		                OR ISNULL(F.ProxyEmployeeID, 0) = {AppContexts.User.EmployeeID}
		                OR EAD.EmployeeID = {AppContexts.User.EmployeeID}
		                )";
            //var nfa = await AccessDeactivationRepo.GetAsync(EADID);
            var ead = AccessDeactivationRepo.GetModelData<EmployeeAccessDeactivationDto>(sql);
            return ead;
        }


        public async Task<EmployeeAccessDeactivationDto> GetAccessDeactivationDivisionClearence(int EADID, int ApprovalProcessID)
        {
            string sql = $@"select DISTINCT EAD.EADID,EAD.DateOfResignation,EAD.LastWorkingDay,EAD.IsCoreFunctional,EAD.ApprovalStatusID,EAD.IsSentForDivisionClearance,EAD.DivisionClearanceApprovalStatusID,EAD.IsDraftForDivClearence
                            ,EAD.SentForDivisionClearanceDate,EAD.IsDraft,EAD.EEIID,EAD.Description,EAD.CreatedDate,EAD.CreatedBy, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned,VA.WorkMobile,EAD.EmployeeID
                            ,EE.FullName EEFullName,EE.EmployeeCode EEEmployeeCode,EE.DesignationName EEDesignationName,EE.DivisionName EEDivisionName,EE.DepartmentName EEDepartmentName,EE.WorkEmail EEEmail,
							EE.WorkMobile EEMobile
                            ,(CASE WHEN EAD.IsCoreFunctional=1 then '71' WHEN EAD.IsCoreFunctional=0 then '72' Else NULL END) AS FunctionalOrNot
                            ,APAD.ApprovalProcessID ADApprovalProcessID
                            --I.EEIID value,
                            --CONCAT(EE.EmployeeCode,'-', EE.FullName) label,
                        from HRMS..EmployeeAccessDeactivation EAD
                        -- LEFT JOIN HRMS..EmployeeExitInterview I ON EAD.EEIID=I.EEIID 
						LEFT JOIN HRMS..ViewALLEmployee EE ON EE.EmployeeID = EAD.EmployeeID
                        LEFT JOIN Security..Users U ON U.UserID = EAD.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = EAD.DivisionClearanceApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.DivisionClearance} 
                        LEFT JOIN Approval..ApprovalProcess APAD ON APAD.ReferenceID = EAD.EADID AND APAD.APTypeID = {(int)Util.ApprovalType.AccessDeactivation} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										--SELECT 
										--	COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										--FROM 
										--	Approval..ApprovalEmployeeFeedback  AEF
										--	LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
										--GROUP BY ReferenceID 
                                        
										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.DivisionClearance} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EADID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = EAD.EADID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE EAD.EADID={EADID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            //var nfa = await AccessDeactivationRepo.GetAsync(EADID);
            var nfa = AccessDeactivationRepo.GetModelData<EmployeeAccessDeactivationDto>(sql);
            return nfa;
        }
        public List<Attachments> GetAttachments(int EADID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeAccessDeactivation' AND ReferenceID={EADID}";
            var attachment = AccessDeactivationRepo.GetDataDictCollection(attachmentSql);
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
                    Description = data["Description"].ToString(),
                    TableName = data["TableName"].ToString(),
                    ReferenceId = (int)data["ReferenceID"]
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

        public async Task<EmployeeAccessDeactivationDto> GetAccessDeactivationForReAssessment(int EADID)
        {
            var model = AccessDeactivationRepo.Entities.Where(x => x.EADID == EADID).Select(y => new EmployeeAccessDeactivationDto
            {
                EADID = (int)y.EADID,
                DateOfResignation = y.DateOfResignation,
                LastWorkingDay = y.LastWorkingDay,
                IsCoreFunctional = y.IsCoreFunctional,
                Description = y.Description,
                EEIID = y.EEIID
            }).FirstOrDefault();


            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeAccessDeactivation' AND ReferenceID={EADID}";
            var attachment = AccessDeactivationRepo.GetDataDictCollection(attachmentSql);
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

        public IEnumerable<Dictionary<string, object>> ReportForAccessDeactivationAttachments(int RefID, string TableName)
        {
            string sql = $@" EXEC Security..spRPTAttachmentListForAll {RefID},{TableName}";
            var attachemntList = AccessDeactivationRepo.GetDataDictCollection(sql);
            return attachemntList;
        }

        public Dictionary<string, object> ReportForAccessDeactivationMaster(int EADID)
        {
            string sql = $@" EXEC HRMS..spRPTEmployeeAccessDeactivation {EADID}";
            var masterData = AccessDeactivationRepo.GetData(sql);
            return masterData;
        }

        public IEnumerable<Dictionary<string, object>> ReportForEADApprovalFeedback(int EADID)
        {
            string sql = $@" EXEC Security..spRPTAccessDeactivationApprovalFeedback {EADID}";
            var feedback = AccessDeactivationRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForEAD(int EADID,int ApprovalProcessID)
        {
            //string sql = $@" EXEC Security..spRPTAccessDeactivationApprovalFeedback {EADID}";
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
		                    SV.SystemVariableCode EADApprovalSequenceTypeName,
		                    NFAApprovalSequenceType
	                        FROM 
	                        Approval..ApprovalEmployeeFeedback AEF 
	                        INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                        INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID
	                        LEFT JOIN Security..PersonImage Signtr ON Signtr.PersonID = VE.PersonID AND IsSignature = 1
	                        INNER JOIN Approval..ApprovalFeedback AF ON AF.APFeedbackID = AEF.APFeedbackID
	                        INNER JOIN Approval..ApprovalType AT ON AT.APTypeID = AP.APTypeID
	                        LEFT JOIN Security..SystemVariable SV ON AEF.NFAApprovalSequenceType = SV.SystemVariableID
	                        WHERE AP.APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND ReferenceID = {EADID} 
	                        AND NFAApprovalSequenceType <> 64 
                            AND ((@NFAApprovalSequenceType=55 AND @EmployeeID=(select EmployeeID from Approval..ApprovalEmployeeFeedback where ApprovalProcessID = {ApprovalProcessID} AND NFAApprovalSequenceType = @NFAApprovalSequenceType) and 1=1) 
                                    OR (AEF.EmployeeID={AppContexts.User.EmployeeID} OR ProxyEmployeeID={AppContexts.User.EmployeeID}))
	                        
	                        ORDER BY SequenceNo";
            var feedback = AccessDeactivationRepo.GetDataDictCollection(sql);
            return feedback;
        }


        
        public IEnumerable<Dictionary<string, object>> ReportForDivClearenceApprovalFeedback(int EADID)
        {
            string sql = $@" EXEC Security..spRPTDivisionClearenceApprovalFeedback {EADID}";
            var feedback = AccessDeactivationRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task RemoveAccessDeactivation(int EADID, int aprovalProcessId)
        {
            var accessDeactivation = AccessDeactivationRepo.Entities.Where(x => x.EADID == EADID).FirstOrDefault();
            accessDeactivation.SetDeleted();
            var mailData = new List<Dictionary<string, object>>();
            var data = new Dictionary<string, object>
                {
                    //{ "ReferenceNo", accessDeactivation.ReferenceNo },
                    { "EmployeeName", AppContexts.User.FullName }
                };
            mailData.Add(data);
            var mail = GetAPEmployeeEmailsWithProxy(aprovalProcessId).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            BasicMail((int)Util.MailGroupSetup.NFARemoveMail, ToEmailAddress, false, CCEmailAddress, null, mailData);

            using (var unitOfWork = new UnitOfWork())
            {


                AccessDeactivationRepo.Add(accessDeactivation);
                DeleteAllApprovalProcessRelatedData((int)ApprovalType.AccessDeactivation, EADID);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
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
            var comments = AccessDeactivationRepo.GetDataDictCollection(sql);
            return comments;
        }



        //GetDivisionClearenceApprovalPanelDefault
        public async Task<List<ManualApprovalPanelEmployeeDto>> GetDivisionClearenceApprovalPanelDefault(int EADID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.EmployeeCode+'-'+Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={EADID}  AND APE.APTypeID={(int)Util.ApprovalType.DivisionClearance}";

            var maps = AccessDeactivationRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return maps;
        }

        public List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByEADID(int id)
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
                                INNER JOIN HRMS..EmployeeAccessDeactivation AS D ON P.ReferenceID = D.EADID
                                LEFT OUTER JOIN HRMS.dbo.ViewALLEmployee AS E ON P.EmployeeID = E.EmployeeID
						        LEFT JOIN HRMS..ViewALLEmployee VE1 ON P.ProxyEmployeeID = VE1.EmployeeID
                                LEFT OUTER JOIN Security.dbo.SystemVariable AS SV ON P.NFAApprovalSequenceType = SV.SystemVariableID
                                WHERE ReferenceID IN (
		                                SELECT MAX(ReferenceID) refID
		                                FROM Approval..ManualApprovalPanelEmployee a
		                                INNER JOIN HRMS..EmployeeAccessDeactivation b ON a.ReferenceID = b.EADID
		                                WHERE a.APPanelID = {(int)Util.ApprovalPanel.DivisionClearance}
			                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
			                                --AND b.EADID = {id}
		                                )
	                                AND APTypeID = {(int)Util.ApprovalType.DivisionClearance}
                                ORDER BY P.ReferenceID
	                                ,P.SequenceNo";

            var list = AccessDeactivationRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return list;
        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForAccessDeactivation()
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
                        LEFT JOIN EmployeeAccessDeactivation EAD on EAD.EmployeeID = Emp.EmployeeID  AND EAD.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND EAD.EADID IS NULL";
            var listDict = AccessDeactivationRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<Dictionary<string, object>>> DownloadAccessDeactivation()
        {
            string sql = $@"SELECT 
                            DISTINCT
	                             EAD.EADID
								,EAD.DateOfResignation
								,EAD.LastWorkingDay
								,EAD.IsCoreFunctional
								,EAD.ApprovalStatusID
								,EAD.IsSentForDivisionClearance
								,EAD.DivisionClearanceApprovalStatusID
								,EAD.SentForDivisionClearanceDate
                                ,CASE WHEN isnull(EAD.IsSentForDivisionClearance,0)=0 then 'No' Else 'Yes' End AS DivClearenceStatus
								,EAD.IsDraft
								,EAD.Description
								,EAD.EEIID
                                ,EAD.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
								,EE.DepartmentID EEDepartmentID
	                            ,EE.DepartmentName EEDepartmentName
	                            ,EE.FullName AS EEEmployeeName,EE.EmployeeCode AS EEEmployeeCode, EE.DivisionID EEDivisionID, EE.DivisionName EEDivisionName
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
								,EE.FullName+EE.EmployeeCode+EE.DepartmentName EEEmployeeWithDepartment
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,EAD.CreatedBy
                                --,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM HRMS..EmployeeAccessDeactivation EAD
							-- JOIN EmployeeExitInterview EI ON EI.EEIID=EAD.EEIID --AND EI.ApprovalStatusID=23
							LEFT JOIN HRMS..ViewALLEmployeeRegularJoin EE ON EE.EmployeeID = EAD.EmployeeID
                            LEFT JOIN Security..Users U ON U.UserID = EAD.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = EAD.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = EAD.EADID AND AP.APTypeID = {(int)Util.ApprovalType.AccessDeactivation}
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
										--where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation} 
										--GROUP BY ReferenceID

										--UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = EAD.EADID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = EAD.EADID
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
                                    --LEFT JOIN (
								  --     SELECT 
										--    AEF.EmployeeCode PendingEmployeeCode,
										--    EmployeeName PendingEmployeeName,
										--    DepartmentName PendingDepartmentName,
										--    AEF.ReferenceID PendingReferenceID
									 --   FROM 
										--    Approval..viewApprovalEmployeeFeedback AEF 
									 --   WHERE AEF.APTypeID = {(int)Util.ApprovalType.AccessDeactivation} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  -- )PendingAt ON  PendingAt.PendingReferenceID = EAD.EADID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            OR EAD.EmployeeID={AppContexts.User.EmployeeID})";
            var result = AccessDeactivationRepo.GetDataDictCollection(sql);
            return result.ToList();
        }
    }
}
