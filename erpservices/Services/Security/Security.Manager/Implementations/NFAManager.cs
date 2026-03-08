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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Security.DAL;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Core.Util;

namespace Security.Manager
{
    public class NFAManager : ManagerBase, INFAManager
    {
        private readonly IRepository<NFAMaster> NFAMasterRepo;
        private readonly IRepository<NFAChild> NFAChildRepo;
        private readonly IRepository<NFAChildStrategic> NFAChildStrategicRepo;
        //readonly IModelAdapter Adapter;
        public NFAManager(IRepository<NFAMaster> nfaMasterRepo, IRepository<NFAChild> nfaChildRepo, IRepository<NFAChildStrategic> nfaChildStrategicRepo)
        {
            NFAMasterRepo = nfaMasterRepo;
            NFAChildRepo = nfaChildRepo;
            NFAChildStrategicRepo = nfaChildStrategicRepo;
        }

        public async Task<List<NFAMasterDto>> GetNFAList(string filterData)
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
                    filter = $@" AND NFA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND NFA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            NFA.*,
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
                            FROM NFAMaster NFA
                            LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = NFA.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = {(int)Util.ApprovalType.NFA}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
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
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ORDER BY CreatedDate desc";
            var nfas = NFAMasterRepo.GetDataModelCollection<NFAMasterDto>(sql);

            var testData = new EmailDtoCore
            {
                Subject = "Test"
            };
            await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return nfas;
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
                    filter = $@" AND NFA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND NFA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                                NFA.CreatedDate,
                                NFA.NFAID,
								NFA.ReferenceNo,
								NFA.NFADate,
								NFA.ApprovalStatusID,
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
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM NFAMaster NFA
                            LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = NFA.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = {(int)Util.ApprovalType.NFA}
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
                                    LEFT JOIN (
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID  AND APSubmitDate.EmployeeID = {AppContexts.User.EmployeeID}
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.NFA} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = NFA.NFAID 
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = NFAMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


        public GridModel GetListForGridNFADashboard(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"

            string sql = $@"SELECT 
                                DISTINCT	                            
                                NFA.CreatedDate,
                                NFA.NFAID,
								NFA.ReferenceNo,
								NFA.NFADate,
								NFA.ApprovalStatusID,
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
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM NFAMaster NFA
                            LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = NFA.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = {(int)Util.ApprovalType.NFA}
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
                                    LEFT JOIN (
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID  AND APSubmitDate.EmployeeID = {AppContexts.User.EmployeeID}
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.NFA} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = NFA.NFAID 
							WHERE NFA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}
                            ";
            var result = NFAMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }



        public async Task<NFAMasterDto> GetNFA(int NFAID)
        {
            string sql = $@"select DISTINCT NFA.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned,WorkMobile, SV1.SystemVariableCode AS BudgetPlanCategoryName
                        from NFAMaster NFA
                        LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = NFA.ApprovalStatusID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON SV1.SystemVariableID = NFA.BudgetPlanCategoryID

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
                        WHERE NFA.NFAID={NFAID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            //var nfa = await NFAMasterRepo.GetAsync(NFAID);
            var nfa = NFAMasterRepo.GetModelData<NFAMasterDto>(sql);
            return nfa;
        }
        public async Task<List<NFAChildDto>> GetNFAChild(int NFAID)
        {
            string sql = $@"select NFA.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName
                        from NFAChild NFA
                        LEFT JOIN NFAMaster NM ON NM.NFAID = NFA.NFAMasterID
                        LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = NM.ApprovalStatusID
                        WHERE NFA.NFAMasterID={NFAID}";
            //var nfa = await NFAMasterRepo.GetAsync(NFAID);
            var nfaChilds = NFAChildRepo.GetDataModelCollection<NFAChildDto>(sql);
            return nfaChilds;
        }

        public async Task<List<NFAChildStrategicDto>> GetNFAChildStrategic(int NFAID)
        {
            string sql = $@"select NFA.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName, UNT.UnitCode, I.ItemName
                        from NFAChildStrategic NFA
                        LEFT JOIN NFAMaster NM ON NM.NFAID = NFA.NFAMasterID
                        LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = NM.ApprovalStatusID
                        LEFT JOIN scm..item I ON I.ItemID = NFA.ItemID
						LEFT JOIN Security..Unit UNT ON UNT.UnitID =  NFA.UOM
                        WHERE NFA.NFAMasterID={NFAID}";
            var nfaChilds = NFAChildRepo.GetDataModelCollection<NFAChildStrategicDto>(sql);
            return nfaChilds;
        }

        public async Task<(bool, string)> SaveChanges(NFADto nfa)
        {
            var existingNFA = NFAMasterRepo.Entities.Where(x => x.NFAID == nfa.NFAID).SingleOrDefault();
            var removeList = RemoveAttachments(nfa);

            if (nfa.NFAID > 0 && (existingNFA.IsNullOrDbNull() || existingNFA.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this NFA.");
            }

            foreach (var item in nfa.Attachments)
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

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (nfa.NFAID > 0 && nfa.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM NFAMaster NFA
                            LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = 2
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 2 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 2 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = NFA.NFAID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {nfa.ApprovalProcessID}";
                var canReassesstment = NFAMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit NFA once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit NFA once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback(nfa.NFAID, nfa.ApprovalProcessID, (int)Util.ApprovalType.NFA);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new NFAMaster
            {
                Subject = nfa.Subject,
                Preamble = nfa.Preamble,
                Description = nfa.Description,
                DescriptionImageURL = nfa.DescriptionImageURL,
                PriceAndCommercial = nfa.PriceAndCommercial,
                Solicitation = nfa.Solicitation,
                BudgetPlanRemarks = nfa.BudgetPlanRemarks,
                GrandTotal = (decimal)nfa.ItemDetails.Select(x => x.Total).DefaultIfEmpty(0).Sum(),
                TemplateID = nfa.TemplateID,
                ReferenceKeyword = nfa.ReferenceKeyword,
                IsDraft = nfa.IsDraft,
                ApprovalStatusID = nfa.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (nfa.NFAID.IsZero() && existingNFA.IsNull())
                {
                    masterModel.NFADate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateNFAReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    //masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetNFAMasterNewId(masterModel);
                    nfa.NFAID = masterModel.NFAID;
                }
                else
                {
                    masterModel.CreatedBy = existingNFA.CreatedBy;
                    masterModel.CreatedDate = existingNFA.CreatedDate;
                    masterModel.CreatedIP = existingNFA.CreatedIP;
                    masterModel.RowVersion = existingNFA.RowVersion;
                    masterModel.NFAID = nfa.NFAID;
                    masterModel.ReferenceNo = existingNFA.ReferenceNo;
                    masterModel.NFADate = existingNFA.NFADate;
                    masterModel.BudgetPlanRemarks = existingNFA.BudgetPlanRemarks;
                    //masterModel.ApprovalStatusID = existingNFA.ApprovalStatusID;
                    masterModel.SetModified();
                }

             

                var childModel = (dynamic)null;
                if (masterModel.TemplateID == (int)Util.NFAType.CreateNFA)
                {
                    childModel = GenerateNFAChild(nfa);
                }
                else
                {
                    childModel = GenerateNFAChildStrategic(nfa);
                }



                SetAuditFields(masterModel);
                SetAuditFields(childModel);



                if (nfa.Attachments.IsNotNull() && nfa.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(nfa.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, nfa.NFAID, "NFAMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, nfa.NFAID, "NFAMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                NFAMasterRepo.Add(masterModel);

                if (masterModel.TemplateID == (int)Util.NFAType.CreateNFA)
                {
                    NFAChildRepo.AddRange(childModel);
                }
                else
                {
                    NFAChildStrategicRepo.AddRange(childModel);
                }

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingNFA.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.NFAApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, NFA Reference No:{masterModel.ReferenceNo}";
                        //ApprovalProcessID = CreateApprovalProcess(masterModel.NFAID, Util.AutoNFAAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.NFA, (int)Util.ApprovalPanel.NFAApprovalPanel);
                        var obj = CreateApprovalProcessForLimitWithConfig((int)masterModel.NFAID, Util.AutoPOAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.NFA, masterModel.GrandTotal, "0");
                        ApprovalProcessID = obj.ApprovalProcessID;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            //UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            //    $"{Util.AutoLeaveAppTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}, Total Leave Request={applicationMaster.NoOfLeaveDays}");
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
                unitOfWork.CommitChangesWithAudit(); if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMail(ApprovalProcessID, IsResubmitted, masterModel.NFAID, (int)Util.MailGroupSetup.NFAInitiatedMail);
                }
            }
            await Task.CompletedTask;

            return (true, $"NFA Submitted Successfully");
        }

        private void SetNFAMasterNewId(NFAMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("NFAMaster", AppContexts.User.CompanyID);
            master.NFAID = code.MaxNumber;
        }

        private List<NFAChild> GenerateNFAChild(NFADto nfa)
        {
            var existingNFAChild = NFAChildRepo.Entities.Where(x => x.NFAMasterID == nfa.NFAID).ToList();
            var childModel = new List<NFAChild>();
            if (nfa.ItemDetails.IsNotNull())
            {
                nfa.ItemDetails.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.ItemName))
                    {
                        childModel.Add(new NFAChild
                        {
                            NFACID = x.NFACID,
                            NFAMasterID = nfa.NFAID,
                            ItemName = x.ItemName,
                            Description = x.Description,
                            Unit = x.Unit,
                            UnitType = x.UnitType,
                            UnitPrice = x.UnitPrice,
                            VatTaxStatus = x.VAT,
                            Vendor = x.Vendor,
                            TotalAmount = x.Total ?? 0,
                            Type = x.Type,
                            Duration = x.Duration,
                            CostType = x.CostType,
                            EstimatedBudgetAmount = x.EstimatedBudgetAmount,
                            AITPercent = x.AITPercent
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingNFAChild.Count > 0 && x.NFACID > 0)
                    {
                        var existingModelData = existingNFAChild.FirstOrDefault(y => y.NFACID == x.NFACID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.NFAMasterID = nfa.NFAID;
                        x.SetAdded();
                        SetNFAChildNewId(x);
                    }
                });

                var willDeleted = existingNFAChild.Where(x => !childModel.Select(y => y.NFACID).Contains(x.NFACID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }


            return childModel;
        }

        private void SetNFAChildNewId(NFAChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("NFAChild", AppContexts.User.CompanyID);
            child.NFACID = code.MaxNumber;
        }

        private string GenerateNFAReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("NFARefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
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
                        string filename = $"NFA-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "NFA\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachments> RemoveAttachments(NFADto nfa)
        {
            if (nfa.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='NFAMaster' AND ReferenceID={nfa.NFAID}";
                var prevAttachment = NFAMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !nfa.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "NFA";
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
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.NFA);
            return comments.Result;
        }
        public List<Attachments> GetAttachments(int NFAID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='NFAMaster' AND ReferenceID={NFAID}";
            var attachment = NFAMasterRepo.GetDataDictCollection(attachmentSql);
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
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }
        public async Task<NFADto> GetNFAForReAssessment(int NFAID)
        {

            var model = NFAMasterRepo.Entities.Where(x => x.NFAID == NFAID).Select(y => new NFADto
            {
                NFAID = y.NFAID,
                NFADate = y.NFADate,
                ReferenceNo = y.ReferenceNo,
                ReferenceKeyword = y.ReferenceKeyword,
                Subject = y.Subject,
                Preamble = y.Preamble,
                Description = y.Description,
                DescriptionImageURL = y.DescriptionImageURL,
                PriceAndCommercial = y.PriceAndCommercial,
                Solicitation = y.Solicitation,
                BudgetPlanRemarks = y.BudgetPlanRemarks,
                TemplateID = y.TemplateID,
                IsDraft = y.IsDraft,
                ItemDetails = NFAChildRepo.Entities.Where(z => z.NFAMasterID == NFAID).Select(a => new ItemDetails
                {
                    NFACID = a.NFACID,
                    ItemName = a.ItemName,
                    Description = a.Description,
                    Unit = a.Unit,
                    UnitType = a.UnitType,
                    UnitPrice = a.UnitPrice,
                    VAT = a.VatTaxStatus,
                    Vendor = a.Vendor,
                    Total = a.TotalAmount,
                    Duration = a.Duration,
                    Type = a.Type,
                    CostType = a.CostType,
                    EstimatedBudgetAmount = a.EstimatedBudgetAmount,
                    AITPercent = a.AITPercent,

                }).ToList()
            }).FirstOrDefault();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='NFAMaster' AND ReferenceID={NFAID}";
            var attachment = NFAMasterRepo.GetDataDictCollection(attachmentSql);
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
        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int NFAID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = mail.Item2;
            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.NFA, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, NFAID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        public IEnumerable<Dictionary<string, object>> ReportForNFAAttachments(int NFAID)
        {
            string sql = $@" EXEC Security..spRPTAttachmentList {NFAID}";
            var attachemntList = NFAMasterRepo.GetDataDictCollection(sql);
            return attachemntList;
        }

        public Dictionary<string, object> ReportForNFAMaster(int NFAID)
        {
            string sql = $@" EXEC Security..spRPTNFASCMMaster {NFAID}";
            var masterData = NFAMasterRepo.GetData(sql);
            return masterData;
        }

        public IEnumerable<Dictionary<string, object>> ReportForNFAChild(int NFAID)
        {
            string sql = $@" EXEC Security..spRPTNFASCM {NFAID}";
            var childList = NFAMasterRepo.GetDataDictCollection(sql);
            return childList;
        }


        public IEnumerable<Dictionary<string, object>> ReportForNFAApprovalFeedback(int NFAID)
        {
            string sql = $@" EXEC Security..spRPTNFAApprovalFeedback {NFAID}";
            var feedback = NFAMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public async Task RemoveNFA(int NFAID, int aprovalProcessId)
        {
            var nfaMaster = NFAMasterRepo.Entities.Where(x => x.NFAID == NFAID).FirstOrDefault();
            nfaMaster.SetDeleted();
            var mailData = new List<Dictionary<string, object>>();
            var data = new Dictionary<string, object>
                {
                    { "ReferenceNo", nfaMaster.ReferenceNo },
                    { "EmployeeName", AppContexts.User.FullName }
                };
            mailData.Add(data);
            var mail = GetAPEmployeeEmailsWithProxy(aprovalProcessId).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            BasicMail((int)Util.MailGroupSetup.NFARemoveMail, ToEmailAddress, false, CCEmailAddress, null, mailData);

            using (var unitOfWork = new UnitOfWork())
            {

                var nfaChild = NFAChildRepo.Entities.Where(x => x.NFAMasterID == NFAID).ToList();
                nfaChild.ForEach(x => x.SetDeleted());

                NFAMasterRepo.Add(nfaMaster);
                NFAChildRepo.AddRange(nfaChild);
                DeleteAllApprovalProcessRelatedData((int)ApprovalType.NFA, NFAID);
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
            var comments = NFAMasterRepo.GetDataDictCollection(sql);
            return comments;
        }



        #region NFA Strategic

        public async Task<(bool, string)> SaveChangesStrategicNFA(NFADto nfa)
        {
            var existingNFA = NFAMasterRepo.Entities.Where(x => x.NFAID == nfa.NFAID).SingleOrDefault();
            var removeList = RemoveAttachments(nfa);

            if (nfa.NFAID > 0 && (existingNFA.IsNullOrDbNull() || existingNFA.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this NFA.");
            }

            foreach (var item in nfa.Attachments)
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

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (nfa.NFAID > 0 && nfa.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM NFAMaster NFA
                            LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND AP.APTypeID = 2
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 2 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 2 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = NFA.NFAID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {nfa.ApprovalProcessID}";
                var canReassesstment = NFAMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit NFA once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit NFA once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback(nfa.NFAID, nfa.ApprovalProcessID, (int)Util.ApprovalType.NFA);
            }

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            var masterModel = new NFAMaster
            {
                Subject = nfa.Subject,
                Preamble = nfa.Preamble,
                Description = nfa.Description,
                DescriptionImageURL = nfa.DescriptionImageURL,
                PriceAndCommercial = nfa.PriceAndCommercial,
                Solicitation = nfa.Solicitation,
                BudgetPlanRemarks = nfa.BudgetPlanRemarks,
                GrandTotal = (decimal)nfa.ItemDetails.Select(x => x.Total).DefaultIfEmpty(0).Sum(),
                TemplateID = nfa.TemplateID,
                ReferenceKeyword = nfa.ReferenceKeyword,
                IsDraft = nfa.IsDraft,
                ApprovalStatusID = nfa.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (nfa.NFAID.IsZero() && existingNFA.IsNull())
                {
                    masterModel.NFADate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateNFAReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    //masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetNFAMasterNewId(masterModel);
                    nfa.NFAID = masterModel.NFAID;
                }
                else
                {
                    masterModel.CreatedBy = existingNFA.CreatedBy;
                    masterModel.CreatedDate = existingNFA.CreatedDate;
                    masterModel.CreatedIP = existingNFA.CreatedIP;
                    masterModel.RowVersion = existingNFA.RowVersion;
                    masterModel.NFAID = nfa.NFAID;
                    masterModel.ReferenceNo = existingNFA.ReferenceNo;
                    masterModel.NFADate = existingNFA.NFADate;
                    masterModel.BudgetPlanRemarks = existingNFA.BudgetPlanRemarks;
                    //masterModel.ApprovalStatusID = existingNFA.ApprovalStatusID;
                    masterModel.SetModified();
                }

                //if (!string.IsNullOrWhiteSpace(masterModel.DescriptionImageURL))
                //{
                //    string fileName = masterModel.DescriptionImageURL.Split("/").Last();
                //    string attachmentFolder = "upload\\NFADescriptionReportImage";
                //    string folderName = "NFA";
                //    IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                //    string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));
                //    File.Delete(str + "\\" + fileName);

                //}

                //if (!string.IsNullOrWhiteSpace(masterModel.Description))
                //{
                //    string filename = $"NFADescription-{DateTime.Now.ToString("ddMMyyHHmmss")}.png";
                //    masterModel.DescriptionImageURL = UploadUtil.SaveAttachmentInDisk(Extension.HtmlToImage(masterModel.Description), filename, "NFA\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""), "upload\\NFADescriptionReportImage");
                //}
                //else
                //{
                //    masterModel.DescriptionImageURL = "";
                //}

                var childModel = GenerateNFAChildStrategic(nfa);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);



                if (nfa.Attachments.IsNotNull() && nfa.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(nfa.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, nfa.NFAID, "NFAMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, nfa.NFAID, "NFAMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                NFAMasterRepo.Add(masterModel);
                NFAChildStrategicRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingNFA.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.NFAApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, NFA Reference No:{masterModel.ReferenceNo}";
                        ApprovalProcessID = CreateApprovalProcess(masterModel.NFAID, Util.AutoNFAAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.NFA, (int)Util.ApprovalPanel.NFAApprovalPanel);
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            //UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            //    $"{Util.AutoLeaveAppTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}, Total Leave Request={applicationMaster.NoOfLeaveDays}");
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
                unitOfWork.CommitChangesWithAudit(); if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMail(ApprovalProcessID, IsResubmitted, masterModel.NFAID, (int)Util.MailGroupSetup.NFAInitiatedMail);
                }
            }
            await Task.CompletedTask;

            return (true, $"NFA Submitted Successfully");
        }

        private List<NFAChildStrategic> GenerateNFAChildStrategic(NFADto nfa)
        {
            var existingNFAChild = NFAChildStrategicRepo.Entities.Where(x => x.NFAMasterID == nfa.NFAID).ToList();
            var childModel = new List<NFAChildStrategic>();
            if (nfa.ItemDetails.IsNotNull())
            {
                nfa.ItemDetails.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.ItemName))
                    {
                        childModel.Add(new NFAChildStrategic
                        {
                            NFACSID = x.NFACSID,
                            NFAMasterID = nfa.NFAID,
                            ItemID = x.ItemID,
                            Description = x.Description,
                            Qty = x.Qty,
                            UOM = x.UOM,
                            UnitPrice = (decimal)x.UnitPrice,
                            TotalAmount = x.Total ?? 0
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingNFAChild.Count > 0 && x.NFACSID > 0)
                    {
                        var existingModelData = existingNFAChild.FirstOrDefault(y => y.NFACSID == x.NFACSID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.NFAMasterID = nfa.NFAID;
                        x.SetAdded();
                        SetNFAChildStrategicNewId(x);
                    }
                });

                var willDeleted = existingNFAChild.Where(x => !childModel.Select(y => y.NFACSID).Contains(x.NFACSID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }


            return childModel;
        }

        private void SetNFAChildStrategicNewId(NFAChildStrategic child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("NFAChildStrategic", AppContexts.User.CompanyID);
            child.NFACSID = code.MaxNumber;
        }


        public async Task<NFADto> GetStrategicNFAForReAssessment(int NFAID)
        {



            //var model = NFAMasterRepo.Entities.Where(x => x.NFAID == NFAID).Select(y => new NFADto
            //{
            //    NFAID = y.NFAID,
            //    NFADate = y.NFADate,
            //    ReferenceNo = y.ReferenceNo,
            //    ReferenceKeyword = y.ReferenceKeyword,
            //    Subject = y.Subject,
            //    Preamble = y.Preamble,
            //    Description = y.Description,
            //    DescriptionImageURL = y.DescriptionImageURL,
            //    PriceAndCommercial = y.PriceAndCommercial,
            //    Solicitation = y.Solicitation,
            //    BudgetPlanRemarks = y.BudgetPlanRemarks,
            //    TemplateID = y.TemplateID,
            //    IsDraft = y.IsDraft,
            //    ItemDetails = NFAChildStrategicRepo.Entities.Where(z => z.NFAMasterID == NFAID).Select(a => new ItemDetails
            //    {
            //        NFACSID = a.NFACSID,
            //        ItemID = a.ItemID,
            //        Description = a.Description,
            //        Qty = a.Qty,
            //        UOM = a.UOM,
            //        UnitPrice = a.UnitPrice,
            //        Total = a.TotalAmount
            //    }).ToList()
            //}).FirstOrDefault();




            string sqlChildList = $@"SELECT a.NFAMasterID, a.NFACSID, a.ItemID, I.ItemName, a.Description, a.Qty, a.UOM, UNT.UnitCode, a.UnitPrice, a.TotalAmount
                        FROM NFAChildStrategic a
                        LEFT JOIN scm..item I ON I.ItemID = a.ItemID
                        LEFT JOIN Security..Unit UNT ON UNT.UnitID =  a.UOM
                        WHERE a.NFAMasterID = {NFAID}";

            //var stratefgicNFAChildList = NFAChildStrategicRepo.GetDataDictCollection(sqlChildList);
            //IEnumerable<Dictionary<string, object>> stratefgicNFAChildList = NFAChildStrategicRepo.GetDataDictCollection(sqlChildList);

            List<NFAChildStrategicDto> stratefgicNFAChildList = NFAChildStrategicRepo.GetDataModelCollection<NFAChildStrategicDto>(sqlChildList);
            var model = NFAMasterRepo.Entities.Where(x => x.NFAID == NFAID).Select(y => new NFADto
            {
                NFAID = y.NFAID,
                NFADate = y.NFADate,
                ReferenceNo = y.ReferenceNo,
                ReferenceKeyword = y.ReferenceKeyword,
                Subject = y.Subject,
                Preamble = y.Preamble,
                Description = y.Description,
                DescriptionImageURL = y.DescriptionImageURL,
                PriceAndCommercial = y.PriceAndCommercial,
                Solicitation = y.Solicitation,
                BudgetPlanRemarks = y.BudgetPlanRemarks,
                TemplateID = y.TemplateID,
                IsDraft = y.IsDraft,
                ItemDetails = stratefgicNFAChildList.Select(a => new ItemDetails
                {
                    NFACSID = a.NFACSID,
                    ItemID = a.ItemID,
                    ItemName = a.ItemName,
                    Description = a.Description,
                    Qty = a.Qty,
                    UOM = a.UOM,
                    UnitCode = a.UnitCode,
                    UnitPrice = a.UnitPrice,
                    Total = a.TotalAmount
                }).ToList()
            }).FirstOrDefault();




            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='NFAMaster' AND ReferenceID={NFAID}";
            var attachment = NFAMasterRepo.GetDataDictCollection(attachmentSql);
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

        #endregion



    }
}
