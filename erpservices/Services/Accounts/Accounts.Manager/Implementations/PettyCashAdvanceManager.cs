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
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class PettyCashAdvanceManager : ManagerBase, IPettyCashAdvanceManager
    {
        private readonly IRepository<PettyCashAdvanceMaster> PettyCashAdvanceMasterRepo;
        private readonly IRepository<PettyCashAdvanceChild> PettyCashAdvanceChildRepo;
        //readonly IModelAdapter Adapter;
        public PettyCashAdvanceManager(IRepository<PettyCashAdvanceMaster> pettyCashAdvanceMasterRepo, IRepository<PettyCashAdvanceChild> pettyCashAdvanceChildRepo)
        {
            PettyCashAdvanceMasterRepo = pettyCashAdvanceMasterRepo;
            PettyCashAdvanceChildRepo = pettyCashAdvanceChildRepo;
        }

        public async Task<List<PettyCashAdvanceMasterDto>> GetPettyCashAdvanceList()
        {
            string sql = $@"SELECT PA.* FROM PettyCashAdvanceMaster PA
                            ORDER BY CreatedDate desc";
            var ious = PettyCashAdvanceMasterRepo.GetDataModelCollection<PettyCashAdvanceMasterDto>(sql);
            return ious;
        }

        public async Task<PettyCashAdvanceMasterDto> GetPettyCashAdvance(int PCAMID)
        {
            string sql = $@"select PA.*
                            ,2 ClaimTypeID
		                    ,'Expense' ClaimType, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                        from PettyCashAdvanceMaster PA
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PA.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PA.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PA.PCAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PA.PCAMID
                        WHERE PA.PCAMID={PCAMID}";
            var pa = PettyCashAdvanceMasterRepo.GetModelData<PettyCashAdvanceMasterDto>(sql);
            return pa;
        }
        //GetPettyCashAdvanceResubmitChild
        public async Task<List<PettyCashAdvanceChildDto>> GetPettyCashAdvanceResubmitChild(int PCAMID)
        {
            string sql = $@"SELECT * FROM PettyCashAdvanceChild PAC
                        WHERE PAC.PCAMID={PCAMID}";
            //var pa = await PettyCashAdvanceMasterRepo.GetAsync(PCAMID);
            var iouChilds = PettyCashAdvanceChildRepo.GetDataModelCollection<PettyCashAdvanceChildDto>(sql);
            iouChilds.ForEach(x => x.Attachments = GetResubmitAttachments((int)x.PCACID));
            return iouChilds;
        }
        public async Task<List<PettyCashAdvanceChildDto>> GetPettyCashAdvanceChild(int PCAMID)
        {
            string sql = $@"SELECT * FROM PettyCashAdvanceChild PAC
                        WHERE PAC.PCAMID={PCAMID}";
            //var pa = await PettyCashAdvanceMasterRepo.GetAsync(PCAMID);
            var iouChilds = PettyCashAdvanceChildRepo.GetDataModelCollection<PettyCashAdvanceChildDto>(sql);
            iouChilds.ForEach(x => x.Attachments = GetAttachments((int)x.PCACID));
            return iouChilds;
        }
        //GetResubmitAttachments
        private List<Attachments> GetResubmitAttachments(int PCACID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='PettyCashAdvanceResubmitChild' AND ReferenceID={PCACID}";
            var attachment = PettyCashAdvanceMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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
        private List<Attachments> GetAttachments(int PCACID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='PettyCashAdvanceChild' AND ReferenceID={PCACID}";
            var attachment = PettyCashAdvanceMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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
        public GridModel GetPettyCashAdvanceClaimList(GridParameter parameters)
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
                    filter = $@" AND PCA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PCA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            PCA.*
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
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND PCA.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(PCA.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                ,CASE WHEN CanSettle=0 AND PCA.ApprovalStatusID=23 AND ISNULL(PCA.IsSettlement,0)=0 THEN 1 ELSE 0 END CanSettle
                                ,ISNULL(CS.EntityTypeName, '') ClaimStatusName
                            FROM Accounts..PettyCashAdvanceMaster PCA
                            LEFT JOIN Security..Users U ON U.UserID = PCA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable CS ON CS.SystemVariableID=PCA.ClaimStatusID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCA.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCA.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim}
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
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PCA.PCAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PCA.PCAMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = PCA.PCAMID
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
							WHERE (PCA.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})                            
                             {filter}";
            var result = PettyCashAdvanceMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }
        private (bool, Dictionary<string, object>) CheckApprovalValidation(PettyCashAdvanceDto pa)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (pa.master.PCAMID > 0 && pa.master.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM PettyCashAdvanceMaster PAM
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PAM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PAM.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = PAM.PCAMID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {pa.master.ApprovalProcessID}";
                var canReassesstment = PettyCashAdvanceMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)pa.master.PCAMID, pa.master.ApprovalProcessID, (int)Util.ApprovalType.PettyCashAdvanceClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        private (bool, Dictionary<string, object>) ResubmitCheckApprovalValidation(PettyCashAdvanceDto pa)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (pa.master.PCAMID > 0 && pa.master.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM PettyCashAdvanceMaster PAM
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PAM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PAM.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = PAM.PCAMID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {pa.master.ApprovalProcessID}";
                var canReassesstment = PettyCashAdvanceMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)pa.master.PCAMID, pa.master.ApprovalProcessID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }
        public async Task<(bool, string)> SaveChanges(PettyCashAdvanceDto pa)
        {

            var existingPA = PettyCashAdvanceMasterRepo.Entities.Where(x => x.PCAMID == pa.master.PCAMID).SingleOrDefault();

            var validate = CheckApprovalValidation(pa);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            bool IsAutoApproved = false;

            long CWID = long.TryParse(GetCustodianWalletByEmployeeID(AppContexts.User.EmployeeID.Value).Result?.FirstOrDefault()?.GetValueOrDefault("CWID")?.ToString(), out long result) ? result : 0;


            var masterModel = new PettyCashAdvanceMaster
            {
                EmployeeID = AppContexts.User.EmployeeID.Value,
                RequestDate = pa.master.RequestDate,
                ReferenceNo = pa.master.ReferenceNo,
                ReferenceKeyword = pa.master.ReferenceKeyword,
                CWID = CWID,
                ApprovalStatusID = pa.master.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                IsDraft = pa.master.IsDraft,
                GrandTotal = (decimal)pa.childs.Select(x => x.AdvanceAmount).DefaultIfEmpty(0).Sum()
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (pa.master.PCAMID.IsZero() && existingPA.IsNull())
                {
                    masterModel.ReferenceNo = GeneratePettyCashAdvanceReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetPettyCashAdvanceMasterNewId(masterModel);
                    pa.master.PCAMID = masterModel.PCAMID;
                }
                else
                {
                    masterModel.CreatedBy = existingPA.CreatedBy;
                    masterModel.CreatedDate = existingPA.CreatedDate;
                    masterModel.CreatedIP = existingPA.CreatedIP;
                    masterModel.RowVersion = existingPA.RowVersion;
                    masterModel.PCAMID = pa.master.PCAMID;
                    masterModel.ReferenceNo = existingPA.ReferenceNo;
                    masterModel.RequestDate = existingPA.RequestDate;
                    masterModel.SetModified();
                }


                var childModel = GeneratePettyCashAdvanceChild(pa).MapTo<List<PettyCashAdvanceChild>>();

                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                RemoveAttachments(pa);
                AddAttachments(pa);


                PettyCashAdvanceMasterRepo.Add(masterModel);
                PettyCashAdvanceChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingPA.IsDraft && masterModel.IsModified))
                    {

                        string approvalTitle = $"{Util.PettyCashAdvanceApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, PA Reference No:{masterModel.ReferenceNo}";
                        ApprovalProcessID = CreateApprovalProcessForDynamic((int)masterModel.PCAMID, Util.AutoPettyCashAdvanceAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)ApprovalType.PettyCashAdvanceClaim, masterModel.GrandTotal);

                        //var returnObj = CreateApprovalProcessForLimit((int)masterModel.PCAMID, Util.AutoPettyCashAdvanceAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashAdvanceClaim, masterModel.GrandTotal, masterModel.RequestDate.Month.ToString());
                        //ApprovalProcessID = returnObj.ApprovalProcessID;
                        //IsAutoApproved = returnObj.IsAutoApproved;

                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            $"{Util.PettyCashAdvanceApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, PA Reference No:{masterModel.ReferenceNo}");
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

                unitOfWork.CommitChangesWithAudit();

                if (IsAutoApproved && !masterModel.IsDraft)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.PCAMID, (int)Util.ApprovalType.PettyCashAdvanceClaim);
                }
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                        await SendMail(ApprovalProcessID, IsResubmitted, masterModel.PCAMID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.MailGroupSetup.PettyCashAdvanceClaimInitiatedMail);

                }

            }
            await Task.CompletedTask;

            return (true, $"Petty Cash Advance Submitted Successfully"); ;
        }
        private void ResubmitAddAttachments(PettyCashAdvanceDto pa)
        {
            foreach (var details in pa.childs)
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

                            string filename = $"PettyCashAdvanceResubmitChild-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PettyCashAdvanceResubmitChild\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.PCACID, "PettyCashAdvanceResubmitChild", false, attachment.Size, 0, false, attachment.Description ?? "");

                            sl++;
                        }
                    }
                }
            }

        }
        private void AddAttachments(PettyCashAdvanceDto pa)
        {
            foreach (var details in pa.childs)
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

                            string filename = $"PettyCashAdvanceChild-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PettyCashAdvanceChild\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.PCACID, "PettyCashAdvanceChild", false, attachment.Size, 0, false, attachment.Description ?? "");

                            sl++;
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


        private void ResubmitRemoveAttachments(PettyCashAdvanceDto pa)
        {
            foreach (var details in pa.childs)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PettyCashAdvanceResubmitChild";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCACID, "PettyCashAdvanceResubmitChild", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashAdvanceResubmitChild' AND ReferenceID={details.PCACID}";
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
                            string folderName = "PettyCashAdvanceChild";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            File.Delete(str + "\\" + data.FileName);

                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCACID, "PettyCashAdvanceChild", true, data.Size, 0, false, data.Description ?? "");

                        }

                    }
                }
            }

        }
        private void RemoveAttachments(PettyCashAdvanceDto pa)
        {
            foreach (var details in pa.childs)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PettyCashAdvanceChild";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCACID, "PettyCashAdvanceChild", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashAdvanceChild' AND ReferenceID={details.PCACID}";
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
                            string folderName = "PettyCashAdvanceChild";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            File.Delete(str + "\\" + data.FileName);

                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.PCACID, "PettyCashAdvanceChild", true, data.Size, 0, false, data.Description ?? "");

                        }

                    }
                }
            }

        }

        private void SetPettyCashAdvanceMasterNewId(PettyCashAdvanceMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PettyCashAdvanceMaster", AppContexts.User.CompanyID);
            master.PCAMID = code.MaxNumber;
        }

        private List<PChildItemDetails> GeneratePettyCashAdvanceChild(PettyCashAdvanceDto pa)
        {
            var existingPettyCashAdvanceChild = PettyCashAdvanceChildRepo.Entities.Where(x => x.PCAMID == pa.master.PCAMID).ToList();
            if (pa.childs.IsNotNull())
            {

                pa.childs.ForEach(x =>
                {
                    if (existingPettyCashAdvanceChild.Count > 0 && x.PCACID > 0)
                    {
                        var existingModelData = PettyCashAdvanceChildRepo.FirstOrDefault(y => y.PCACID == x.PCACID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PCAMID = pa.master.PCAMID;
                        x.SetAdded();
                        SetPettyCashAdvanceChildNewId(x);
                    }
                });

                var willDeleted = existingPettyCashAdvanceChild.Where(x => !pa.childs.Select(y => y.PCACID).Contains(x.PCACID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    pa.childs.Add(new PChildItemDetails
                    {
                        PCACID = x.PCACID,
                        PCAMID = x.PCAMID,
                        Details = x.Details,
                        ProjectName = x.ProjectName,
                        AdvanceAmount = x.AdvanceAmount,
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
                        Attachments = GetAttachments((int)x.PCACID)
                    });
                });
            }


            return pa.childs;
        }

        private List<PChildItemDetails> GeneratePettyCashAdvanceResubmitChild(PettyCashAdvanceDto pa)
        {
            var existingPettyCashAdvanceChild = PettyCashAdvanceChildRepo.Entities.Where(x => x.PCAMID == pa.master.PCAMID).ToList();
            if (pa.childs.IsNotNull())
            {

                pa.childs.ForEach(x =>
                {
                    if (existingPettyCashAdvanceChild.Count > 0 && x.PCACID > 0)
                    {
                        var existingModelData = PettyCashAdvanceChildRepo.FirstOrDefault(y => y.PCACID == x.PCACID);
                        x.ResubmitAmount = x.ResubmitAmount;
                        x.ResubmitRemarks = x.ResubmitRemarks;
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                });
            }
            return pa.childs;
        }


        private void SetPettyCashAdvanceChildNewId(PChildItemDetails child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PettyCashAdvanceChild", AppContexts.User.CompanyID);
            child.PCACID = code.MaxNumber;
        }

        private string GeneratePettyCashAdvanceReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{GenerateSystemCode("IOUReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }


        public async Task RemovePettyCashAdvanceMaster(int PCAMID, int ApprovalProcessID)
        {
            var advPettyCash = PettyCashAdvanceMasterRepo.Entities.Where(x => x.PCAMID == PCAMID && x.IsDraft == true && x.CreatedBy == AppContexts.User.UserID).FirstOrDefault();
            if (advPettyCash.IsNullOrDbNull())
            {
                return;
            }
            advPettyCash.SetDeleted();
            using (var unitOfWork = new UnitOfWork())
            {

                var iouChild = PettyCashAdvanceChildRepo.Entities.Where(x => x.PCAMID == PCAMID).ToList();
                iouChild.ForEach(x => x.SetDeleted());

                PettyCashAdvanceMasterRepo.Add(advPettyCash);
                PettyCashAdvanceChildRepo.AddRange(iouChild);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PettyCashAdvanceClaim);
            return comments.Result;
        }
        public IEnumerable<Dictionary<string, object>> GetResubmitApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim);
            return comments.Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID)
        {
            return GetApprovalForwardingMembers(ReferenceID, APTypeID, APPanelID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }

        #region Mail

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long IOUTMasterID, int APTypeID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)IOUTMasterID, 0);
        }
        #endregion
        //public async Task<PettyCashAdvanceDto> GetIOUForReAssessment(int PCAMID)
        //{

        //    var model = PettyCashAdvanceMasterRepo.Entities.Where(x => x.PCAMID == PCAMID).Select(y => new PettyCashAdvanceMasterDto
        //    {
        //        PCAMID = y.PCAMID,
        //        RequestDate = y.RequestDate,
        //        SettlementDate = y.SettlementDate,
        //        ReferenceNo = y.ReferenceNo,
        //        ReferenceKeyword = y.ReferenceKeyword,
        //        GrandTotal = y.GrandTotal,
        //        ApprovalStatusID = y.ApprovalStatusID,
        //        ItemDetails = PettyCashAdvanceChildRepo.Entities.Where(z => z.PCAMID == PCAMID).Select(a => new PettyCashAdvanceChildDto
        //        {
        //            PCACID = a.PCACID,
        //            PCAMID = a.PCAMID,
        //            Description = a.Description,
        //            AdvanceAmount = a.AdvanceAmount,
        //            Remarks = a.Remarks

        //        }).ToList()
        //    }).FirstOrDefault();

        //    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PettyCashAdvanceMaster' AND ReferenceID={PCAMID}";
        //    var attachment = PettyCashAdvanceMasterRepo.GetDataDictCollection(attachmentSql);
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
        //    model.Attachments = attachemntList;

        //    return await Task.FromResult(model);
        //}
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
            var comments = PettyCashAdvanceMasterRepo.GetDataDictCollection(sql);
            return comments;
        }
        public IEnumerable<Dictionary<string, object>> ReportForPCAApprovalFeedback(int PCAMID, int ApTypeID)
        {
            //string sql = $@" EXEC Security..spRPTPettyCashAdvanceApprovalFeedback {PCAMID}";
            string sql = $@" EXEC Approval..spRPTApprovalFeedback {PCAMID}, {ApTypeID}";
            var feedback = PettyCashAdvanceMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }


        public GridModel GetPettyCashAdvanceResubmitClaimList(GridParameter parameters)
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
                    filter = $@" AND PCA.ResubmitApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PCA.ResubmitApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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

            if (parameters.Sort == "CreatedDate")
            {
                parameters.Order = "DESC";
            }

            string sql = $@"SELECT DISTINCT
                            PCA.*
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
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                --,CASE WHEN ISNULL(CanSettle,0)=1 AND PCA.ResubmitApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ISNULL(PCA.IsSettle,0)=0 THEN 1 ELSE 0 END AS CanSettle
                                ,CASE WHEN CanSettle=0 AND PCA.ResubmitApprovalStatusID=23 AND ISNULL(PCA.IsSettlement,0)=0 THEN 1 ELSE 0 END CanSettle
                                
                            FROM Accounts..PettyCashAdvanceMaster PCA
                            LEFT JOIN Security..Users U ON U.UserID = PCA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PCA.ResubmitApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PCA.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim}
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
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PCA.PCAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PCA.PCAMID
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
									   WHERE AEF.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								  )PendingAt ON  PendingAt.PendingReferenceID = PCA.PCAMID
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
							WHERE ISNULL(PCA.IsResubmit,0)=1 AND (PCA.EmployeeID = {AppContexts.User.EmployeeID} OR VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID})                            
                             {filter}";
            var result = PettyCashAdvanceMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


        public async Task<(bool, string)> SaveChangesResubmit(PettyCashAdvanceDto pa)
        {

            var existingPA = PettyCashAdvanceMasterRepo.Entities.Where(x => x.PCAMID == pa.master.PCAMID && x.ApprovalStatusID == (int)Util.ApprovalStatus.Approved && x.CreatedBy == AppContexts.User.UserID).SingleOrDefault();

            var validate = ResubmitCheckApprovalValidation(pa);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = pa.master.ApprovalProcessID.ToString();
            bool IsResubmitted = false;
            bool IsAutoApproved = false;

            //var masterModel = new PettyCashAdvanceMaster
            //{
            //    IsResubmit = true,
            //    ResubmitBy = (int)AppContexts.User.UserID,
            //    ResubmitDate = DateTime.Now,
            //    ResubmitApprovalStatusID = (int)ApprovalStatus.Pending,
            //    ReSubmitTotalAmount = (decimal)pa.childs.Select(x => x.ResubmitAmount).DefaultIfEmpty(0).Sum()
            //};

            using (var unitOfWork = new UnitOfWork())
            {
                existingPA.IsResubmit = true;
                existingPA.ResubmitBy = (int)AppContexts.User.UserID;
                existingPA.ResubmitDate = DateTime.Now;
                existingPA.ResubmitApprovalStatusID = (int)ApprovalStatus.Pending;
                existingPA.ReSubmitTotalAmount = (decimal)pa.childs.Select(x => x.ResubmitAmount).DefaultIfEmpty(0).Sum();
                existingPA.CreatedBy = existingPA.CreatedBy;
                existingPA.CreatedDate = existingPA.CreatedDate;
                existingPA.CreatedIP = existingPA.CreatedIP;
                existingPA.RowVersion = existingPA.RowVersion;
                existingPA.PCAMID = pa.master.PCAMID;
                existingPA.ReferenceNo = existingPA.ReferenceNo;
                existingPA.RequestDate = existingPA.RequestDate;
                existingPA.SetModified();

                var childModel = GeneratePettyCashAdvanceResubmitChild(pa).MapTo<List<PettyCashAdvanceChild>>();

                SetAuditFields(existingPA);
                SetAuditFields(childModel);

                ResubmitRemoveAttachments(pa);
                ResubmitAddAttachments(pa);


                PettyCashAdvanceMasterRepo.Add(existingPA);
                PettyCashAdvanceChildRepo.AddRange(childModel);


                if (pa.master.ApprovalProcessID == 0)
                {

                    string approvalTitle = $"{Util.PettyCashAdvanceApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, PA Reference No:{existingPA.ReferenceNo}";
                    //var returnObj = CreateApprovalProcessForLimit((int)existingPA.PCAMID, Util.AutoPettyCashAdvanceAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, existingPA.GrandTotal, existingPA.RequestDate.Month.ToString());
                    //ApprovalProcessID = returnObj.ApprovalProcessID;
                    //IsAutoApproved = returnObj.IsAutoApproved;
                    ApprovalProcessID = CreateApprovalProcessForDynamic((int)existingPA.PCAMID, Util.AutoPettyCashAdvanceAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)ApprovalType.PettyCashAdvanceResubmitClaim, existingPA.GrandTotal);


                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                        $"{Util.PettyCashAdvanceApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, PA Reference No:{existingPA.ReferenceNo}");
                        UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                            (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                            $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                            (int)approvalProcessFeedBack["APTypeID"],
                            (int)approvalProcessFeedBack["ReferenceID"], 0);
                        IsResubmitted = true;
                        ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (ApprovalProcessID.IsNotNullOrEmpty() && existingPA.PCAMID > 0)
                {
                    if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                        await SendMail(ApprovalProcessID, IsResubmitted, existingPA.PCAMID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim,(int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimInitiatedMail);

                }

            }
            await Task.CompletedTask;

            return (true, $"Petty Cash Advance ReSubmitted Successfully"); ;
        }

        public async Task<PettyCashAdvanceMasterDto> GetPettyCashAdvanceResubmit(int PCAMID, int ApprovalProcessID)
        {
            string sql = $@"select PA.*
                            ,2 ClaimTypeID
		                    ,'Expense' ClaimType, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                        from PettyCashAdvanceMaster PA
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = PA.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PA.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PA.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceResubmitClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PA.PCAMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PA.PCAMID
                        WHERE PA.PCAMID={PCAMID}";
            var pa = PettyCashAdvanceMasterRepo.GetModelData<PettyCashAdvanceMasterDto>(sql);
            return pa;
        }

        public List<Attachments> GetAttachments(int id, string TableName)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='{TableName}' AND ReferenceID={id}";
            var attachment = PettyCashAdvanceMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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
