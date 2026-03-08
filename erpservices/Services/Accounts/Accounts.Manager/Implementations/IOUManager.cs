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
    public class IOUManager : ManagerBase, IIOUManager
    {
        private readonly IRepository<IOUMaster> IOUMasterRepo;
        private readonly IRepository<IOUChild> IOUChildRepo;
        //readonly IModelAdapter Adapter;
        public IOUManager(IRepository<IOUMaster> iouMasterRepo, IRepository<IOUChild> iouChildRepo)
        {
            IOUMasterRepo = iouMasterRepo;
            IOUChildRepo = iouChildRepo;
        }

        public async Task<List<IOUMasterDto>> GetIOUList()
        {
            string sql = $@"SELECT IOU.* FROM IOUMaster IOU
                            ORDER BY CreatedDate desc";
            var ious = IOUMasterRepo.GetDataModelCollection<IOUMasterDto>(sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return ious;
        }

        public async Task<IOUMasterDto> GetIOU(int IOUID)
        {
            string sql = $@"select IOU.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName 
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                        from IOUMaster IOU
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IOU.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = IOU.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = IOU.IOUMasterID AND AP.APTypeID = {(int)Util.ApprovalType.IOUClaim} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.IOUClaim}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.IOUClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IOU.IOUMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IOU.IOUMasterID
                        WHERE IOU.IOUMasterID={IOUID}";
            var iou = IOUMasterRepo.GetModelData<IOUMasterDto>(sql);
            return iou;
        }
        public async Task<List<IOUChildDto>> GetIOUChild(int IOUID)
        {
            string sql = $@"SELECT * FROM IOUChild IOU
                        WHERE IOU.IOUMasterID={IOUID}";
            //var iou = await IOUMasterRepo.GetAsync(IOUID);
            var iouChilds = IOUChildRepo.GetDataModelCollection<IOUChildDto>(sql);
            iouChilds.ForEach(x => x.Attachments = GetAttachments((int)x.IOUChildID));
            return iouChilds;
        }
        private List<Attachments> GetAttachments(int IOUChildID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='IOUChild' AND ReferenceID={IOUChildID}";
            var attachment = IOUMasterRepo.GetDataDictCollectionWithTransaction(attachmentSql);
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
        public GridModel GetIOUClaimList(GridParameter parameters)
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
                    filter = $@" AND IOUM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND IOUM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            IOUM.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID}
                                ,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,(CASE WHEN IOUM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=22 THEN 'Payment Requested' 
									   WHEN IOUM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=0 THEN 'Payment Approved & Waiting for Settlement'
									   WHEN IOUM.ApprovalStatusID=23 AND Pay.ApprovalStatusID=23 AND isnull(Pay.IsSettlement,0)=1 THEN 'Settled'
									   ELSE SV.SystemVariableCode END) ApprovalStatus
                                ,CASE WHEN PendingAt.PendingEmployeeCode IS NOT NULL THEN (SELECT PendingAt.PendingEmployeeCode EmployeeCode,PendingAt.PendingEmployeeName EmployeeName,PendingAt.PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,CASE WHEN IOUM.ApprovalStatusID = 23 THEN  ExpenseApprovalDate ELSE NULL END ExpenseApprovalDate,FirstPaymentFeedbackRequestDate
								,ISNULL((CASE WHEN (IOUM.ApprovalStatusID = 23 AND Pay.PaymentMasterID is not null) THEN (SELECT  CONVERT(VARCHAR(10), ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) / 86400 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) / 3600 )) + ':'
									+ CONVERT(VARCHAR(10), ( ( ( (DATEDIFF(s, ExpenseApprovalDate, FirstPaymentFeedbackRequestDate)) % 86400 ) % 3600 ) / 60 ))) ELSE NULL END),'') ClaimToPaymentTime,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                            FROM IOUMaster IOUM
                            LEFT JOIN Security..Users U ON U.UserID = IOUM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = IOUM.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = IOUM.IOUMasterID AND AP.APTypeID = {(int)Util.ApprovalType.IOUClaim}
                            LEFT JOIN (SELECT 
											EPM.ApprovalStatusID
											,EPC.IOUOrExpenseClaimID
											,EPM.IsSettlement
											,EPM.PaymentMasterID
											,FirstPaymentFeedbackRequestDate
										FROM Accounts..IOUOrExpensePaymentChild EPC
										JOIN Accounts..IOUOrExpensePaymentMaster EPM ON EPM.PaymentMasterID = EPC.PaymentMasterID
										LEFT JOIN (
										SELECT 
												Min(FeedbackRequestDate) FirstPaymentFeedbackRequestDate,ReferenceID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN(SELECT APTypeID,ReferenceID,ApprovalProcessID FROM Approval..ApprovalProcess )AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID	
											where APTypeID = 5 
											GROUP BY ReferenceID
										)FF ON FF.ReferenceID = EPM.PaymentMasterID
										WHERE EPM.ApprovalStatusID <> 24 AND ClaimType = 'IOU') Pay on Pay.IOUOrExpenseClaimID=IOUM.IOUMasterID
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.IOUClaim} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.IOUClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IOUM.IOUMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = IOUM.IOUMasterID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.IOUClaim} AND APFeedbackID = 2
								   )PendingAt ON  PendingAt.PendingReferenceID = IOUM.IOUMasterID

								    LEFT JOIN (
											SELECT 
												MAX(FeedbackSubmitDate) ExpenseApprovalDate,ReferenceID,AP.ApprovalProcessID
											FROM 
												Approval..ApprovalEmployeeFeedback  AEF
												LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
											where APFeedbackID  = 5 AND APTypeID = {(int)Util.ApprovalType.IOUClaim}
											GROUP BY ReferenceID,AP.ApprovalProcessID
											)LFBSD ON LFBSD.ReferenceID = IOUM.IOUMasterID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = IOUMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        private (bool, Dictionary<string, object>) CheckApprovalValidation(IOUDto iou)
        {
            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (iou.master.IOUMasterID > 0 && iou.master.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 

                                    CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment

                                 FROM IOUMaster IOUM
                                 LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Users U ON U.UserID = IOUM.CreatedBy
                                 LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                 LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = IOUM.IOUMasterID AND AP.APTypeID = {(int)Util.ApprovalType.IOUClaim}
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
            			where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.IOUClaim} 
            			GROUP BY ReferenceID

            			UNION ALL

            			SELECT 
            				COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
            			FROM 
            				Approval..ApprovalEmployeeFeedback AEF
            				LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
            			where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.IOUClaim} AND EmployeeID = {AppContexts.User.EmployeeID}
            			GROUP BY ReferenceID

            			)V
            			GROUP BY ReferenceID
            			) EA ON EA.ReferenceID = IOUM.IOUMasterID                                       
            WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {iou.master.ApprovalProcessID}";
                var canReassesstment = IOUMasterRepo.GetData(sql);
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
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)iou.master.IOUMasterID, iou.master.ApprovalProcessID, (int)Util.ApprovalType.IOUClaim);
                return (true, approvalProcessFeedBack);
            }
            return (true, new Dictionary<string, object>()); ;
        }

        public async Task<(bool, string)> SaveChanges(IOUDto iou)
        {

            var existingIOU = IOUMasterRepo.Entities.Where(x => x.IOUMasterID == iou.master.IOUMasterID).SingleOrDefault();

            var validate = CheckApprovalValidation(iou);

            if (validate.Item1 == false) return (false, $"Sorry, You can not edit expense claim once it processed from approval panel");

            var approvalProcessFeedBack = validate.Item2;

            string ApprovalProcessID = "0";
            bool IsResubmitted = false;
            bool IsAutoApproved = false;



            var masterModel = new IOUMaster
            {
                EmployeeID = AppContexts.User.EmployeeID.Value,
                RequestDate = iou.master.RequestDate,
                SettlementDate = iou.master.SettlementDate,
                ReferenceNo = iou.master.ReferenceNo,
                ReferenceKeyword = iou.master.ReferenceKeyword,
                ApprovalStatusID = iou.master.ApprovalStatusID,
                GrandTotal = (decimal)iou.childs.Select(x => x.IOUAmount).DefaultIfEmpty(0).Sum()
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (iou.master.IOUMasterID.IsZero() && existingIOU.IsNull())
                {
                    masterModel.RequestDate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateIOUReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetIOUMasterNewId(masterModel);
                    iou.master.IOUMasterID = masterModel.IOUMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingIOU.CreatedBy;
                    masterModel.CreatedDate = existingIOU.CreatedDate;
                    masterModel.CreatedIP = existingIOU.CreatedIP;
                    masterModel.RowVersion = existingIOU.RowVersion;
                    masterModel.IOUMasterID = iou.master.IOUMasterID;
                    masterModel.ReferenceNo = existingIOU.ReferenceNo;
                    masterModel.RequestDate = existingIOU.RequestDate;
                    masterModel.ApprovalStatusID = existingIOU.ApprovalStatusID;
                    masterModel.SetModified();
                }


                var childModel = GenerateIOUChild(iou).MapTo<List<IOUChild>>();

                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                RemoveAttachments(iou);
                AddAttachments(iou);


                IOUMasterRepo.Add(masterModel);
                IOUChildRepo.AddRange(childModel);

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.IOUApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, IOU Reference No:{masterModel.ReferenceNo}";
                    var returnObj = CreateApprovalProcessForLimit((int)masterModel.IOUMasterID, Util.AutoExpenseAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.IOUClaim, masterModel.GrandTotal, masterModel.SettlementDate.Month.ToString());
                    ApprovalProcessID = returnObj.ApprovalProcessID;
                    IsAutoApproved = returnObj.IsAutoApproved;
                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            $"{Util.IOUApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, IOU Reference No:{masterModel.ReferenceNo}");
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

                if (IsAutoApproved)
                {
                    UpdateApprovalStatusForAutoApproved((int)masterModel.IOUMasterID, (int)Util.ApprovalType.IOUClaim);
                }

                if (ApprovalProcessID.ToInt() > 0 && !IsAutoApproved)
                    // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMail(ApprovalProcessID, IsResubmitted, masterModel.IOUMasterID, (int)Util.MailGroupSetup.IOUClaimInitiatedMail);

            }
            await Task.CompletedTask;

            return (true, $"IOU Submitted Successfully"); ;
        }
        private void AddAttachments(IOUDto iou)
        {
            foreach (var details in iou.childs)
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

                            string filename = $"IOUChild-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                            var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                            var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "IOUChild\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                            // To Add Into DB

                            SetAttachmentNewId(attachment);
                            SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)details.IOUChildID, "IOUChild", false, attachment.Size, 0, false, attachment.Description ?? "");

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


        private void RemoveAttachments(IOUDto iou)
        {
            foreach (var details in iou.childs)
            {
                if (details.IsDeleted)
                {
                    foreach (var data in details.Attachments)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "IOUChild";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        File.Delete(str + "\\" + data.FileName);

                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.IOUChildID, "IOUChild", true, data.Size, 0, false, data.Description ?? "");

                    }
                }
                else
                {
                    var attachemntList = new List<Attachments>();
                    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='IOUChild' AND ReferenceID={details.IOUChildID}";
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
                            string folderName = "IOUChild";
                            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                            File.Delete(str + "\\" + data.FileName);

                            // To Remove From DB

                            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)details.IOUChildID, "IOUChild", true, data.Size, 0, false, data.Description ?? "");

                        }

                    }
                }
            }

        }

        private void SetIOUMasterNewId(IOUMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("IOUMaster", AppContexts.User.CompanyID);
            master.IOUMasterID = code.MaxNumber;
        }

        private List<ChildItemDetails> GenerateIOUChild(IOUDto iou)
        {
            var existingIOUChild = IOUChildRepo.Entities.Where(x => x.IOUMasterID == iou.master.IOUMasterID).ToList();
            if (iou.childs.IsNotNull())
            {

                iou.childs.ForEach(x =>
                {
                    if (existingIOUChild.Count > 0 && x.IOUChildID > 0)
                    {
                        var existingModelData = IOUChildRepo.FirstOrDefault(y => y.IOUChildID == x.IOUChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.IOUMasterID = iou.master.IOUMasterID;
                        x.SetAdded();
                        SetIOUChildNewId(x);
                    }
                });

                var willDeleted = existingIOUChild.Where(x => !iou.childs.Select(y => y.IOUChildID).Contains(x.IOUChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    iou.childs.Add(new ChildItemDetails
                    {
                        IOUChildID = x.IOUChildID,
                        IOUMasterID = x.IOUMasterID,
                        Description = x.Description,
                        IOUAmount = x.IOUAmount,
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
                        Attachments = GetAttachments((int)x.IOUChildID)
                    });
                });
            }


            return iou.childs;
        }

        private void SetIOUChildNewId(ChildItemDetails child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("IOUChild", AppContexts.User.CompanyID);
            child.IOUChildID = code.MaxNumber;
        }

        private string GenerateIOUReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("IOUReferenceNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }


        public async Task RemoveIOUMaster(int IOUID)
        {
            var iouMaster = IOUMasterRepo.Entities.Where(x => x.IOUMasterID == IOUID).FirstOrDefault();
            iouMaster.SetDeleted();
            using (var unitOfWork = new UnitOfWork())
            {

                var iouChild = IOUChildRepo.Entities.Where(x => x.IOUMasterID == IOUID).ToList();
                iouChild.ForEach(x => x.SetDeleted());

                IOUMasterRepo.Add(iouMaster);
                IOUChildRepo.AddRange(iouChild);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.IOUClaim);
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

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, long IOUTMasterID, int mailGroup)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.IOUClaim, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)IOUTMasterID, 0);
        }
        #endregion
        //public async Task<IOUDto> GetIOUForReAssessment(int IOUMasterID)
        //{

        //    var model = IOUMasterRepo.Entities.Where(x => x.IOUMasterID == IOUMasterID).Select(y => new IOUMasterDto
        //    {
        //        IOUMasterID = y.IOUMasterID,
        //        RequestDate = y.RequestDate,
        //        SettlementDate = y.SettlementDate,
        //        ReferenceNo = y.ReferenceNo,
        //        ReferenceKeyword = y.ReferenceKeyword,
        //        GrandTotal = y.GrandTotal,
        //        ApprovalStatusID = y.ApprovalStatusID,
        //        ItemDetails = IOUChildRepo.Entities.Where(z => z.IOUMasterID == IOUMasterID).Select(a => new IOUChildDto
        //        {
        //            IOUChildID = a.IOUChildID,
        //            IOUMasterID = a.IOUMasterID,
        //            Description = a.Description,
        //            IOUAmount = a.IOUAmount,
        //            Remarks = a.Remarks

        //        }).ToList()
        //    }).FirstOrDefault();

        //    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='IOUMaster' AND ReferenceID={IOUMasterID}";
        //    var attachment = IOUMasterRepo.GetDataDictCollection(attachmentSql);
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

    }
}
