using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class LeaveEncashmentApplicationManager : ManagerBase, ILeaveEncashmentApplicationManager
    {
        private readonly IRepository<AnnualLeaveEncashmentMaster> EmployeeLeaveEncashmentApplicationRepo;
        private readonly IRepository<EmployeeLeaveApplicationDayBreakDown> EmployeeLeaveEncashmentApplicationDayBreakDownRepo;
        private readonly IRepository<Employment> EmploymentRepo;
        private readonly IRepository<ShiftingMaster> ShiftingMasterRepo;
        private readonly IRepository<ShiftingChild> ShiftingChildRepo;
        private readonly IRepository<Holiday> HolidayRepo;
        private readonly IRepository<LFADeclaration> LFADeclarationRepo;
        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<LeavePolicySettings> LeavePolicySettingsRepo;
        private readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        private readonly IScheduleManager ScheduleManager;
        private readonly IModelAdapter Adapter;
        public LeaveEncashmentApplicationManager(IRepository<AnnualLeaveEncashmentMaster> employeeLeaveEncashmentApplicationRepo, IRepository<EmployeeLeaveApplicationDayBreakDown> employeeLeaveEncashmentApplicationDayBreakDownRepo, IRepository<Employment> employmentRepo, IRepository<ShiftingMaster> shiftingMasterRepo, IRepository<ShiftingChild> shiftingChildRepo, IRepository<Holiday> holidayRepo, IRepository<LFADeclaration> lfaRepo, IRepository<Employee> employeeRepo, IRepository<LeavePolicySettings> leavePolicySettingsRepo, IRepository<EmployeeSupervisorMap> employeeSupervisorMapRepo, IScheduleManager scheduleManager)
        {
            EmployeeLeaveEncashmentApplicationRepo = employeeLeaveEncashmentApplicationRepo;
            EmployeeLeaveEncashmentApplicationDayBreakDownRepo = employeeLeaveEncashmentApplicationDayBreakDownRepo;
            EmploymentRepo = employmentRepo;
            ShiftingMasterRepo = shiftingMasterRepo;
            ShiftingChildRepo = shiftingChildRepo;
            HolidayRepo = holidayRepo;
            LFADeclarationRepo = lfaRepo;
            EmployeeRepo = employeeRepo;
            LeavePolicySettingsRepo = leavePolicySettingsRepo;
            EmployeeSupervisorMapRepo = employeeSupervisorMapRepo;
            ScheduleManager = scheduleManager;
            Adapter = new ModelAdapterSQL();
        }

        public async Task<LeaveBalanceAndDetailsResponse> GetAnnualLeaveBalanceAndDetails()
        {
            LeaveBalanceAndDetailsResponse response = new LeaveBalanceAndDetailsResponse();
            var employee = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == AppContexts.User.EmployeeID);
            response.LeaveBalances = LeaveBalance();
            response.TotalEmployementDays = (DateTime.Now - (employee.DateOfJoining ?? DateTime.Now)).Days;
            response.JoiningDate = employee.DateOfJoining ?? DateTime.Now;
            return await Task.FromResult(response);
        }
        private List<LeaveBalance> LeaveBalance()
        {
            int leaveTypeId = (int)Util.LeaveCategory.Annual;
            int employee = AppContexts.User.EmployeeID.Value;

            string sql = 0 > 0 ?
             $@"EXEC spEmployeeLeaveBalanceByEmployeeIDAndLeaveAID {employee},0" : $@"SELECT *,CASE WHEN FLOOR(Balance/2)>30 THEN 30 Else FLOOR(Balance/2) END AS EncashBalance FROM EmployeeLeaveBalance WHERE EmployeeID={employee} AND LeaveCategoryID={leaveTypeId}";

            var balance = EmployeeLeaveEncashmentApplicationRepo.GetDataModelCollection<LeaveBalance>(sql);
            if (leaveTypeId > 0)
            {
                balance.Where(x => x.LeaveCategoryID == leaveTypeId).ToList();
            }
            return balance;
        }
        public async Task<(bool, string)> SaveChanges(LeaveEncashmentApplication application)
        {
            var ApprovalProcessID = "0";
            bool IsResubmitted = false;

            var existingApplication = EmployeeLeaveEncashmentApplicationRepo.Entities.Where(x => x.ALEMasterID == application.ALEMasterID).SingleOrDefault();
            if (application.ALEMasterID > 0 && (existingApplication.IsNullOrDbNull() || existingApplication.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Leave Encashment Application.");
            }

            var currentWindowID = 0;
            var CurrentWindow = GetCurrentWindow(application.ALEMasterID);
            if (CurrentWindow.Count() == 0 && application.ALEMasterID.IsZero())
            {

                return (false, "You don't have permission for leave encashment.");
            }
            currentWindowID = CurrentWindow["ALEWMasterID"].ToString().ToInt();


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (application.ALEMasterID > 0 && application.ApprovalProcessID > 0)
                approvalProcessFeedBack = GetApprovalProcessFeedback(application.ALEMasterID, application.ApprovalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication);


            var applicationMaster = new AnnualLeaveEncashmentMaster
            {
                ALEMasterID = application.ALEMasterID,
                //ALEWMasterID = application.ALEWMasterID,
                ALEWMasterID = application.ALEMasterID.IsZero() ? currentWindowID : existingApplication.ALEWMasterID,
                EmployeeID = (int)AppContexts.User.EmployeeID,
                TotalLeaveBalanceDays = application.TotalLeaveBalanceDays,
                EncashedLeaveDays = application.EncashedLeaveDays,
                ApprovalStatusID = (int)ApprovalStatus.Pending,

            };
            using (var unitOfWork = new UnitOfWork())
            {

                if (application.ALEMasterID.IsZero() && existingApplication.IsNull())
                {
                    applicationMaster.SetAdded();
                    SetApplicationMasterNewId(applicationMaster);
                }
                else
                {
                    applicationMaster.CreatedBy = existingApplication.CreatedBy;
                    applicationMaster.CreatedDate = existingApplication.CreatedDate;
                    applicationMaster.CreatedIP = existingApplication.CreatedIP;
                    applicationMaster.RowVersion = existingApplication.RowVersion;
                    applicationMaster.SetModified();
                }


                application.ALEMasterID = applicationMaster.ALEMasterID;

                SetAuditFields(applicationMaster);


                EmployeeLeaveEncashmentApplicationRepo.Add(applicationMaster);
                string approvalTitle = @$"{Util.AutoLeaveEncashmentAppTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}, Total Encash Days Requested={applicationMaster.EncashedLeaveDays}";

                if (applicationMaster.IsAdded)
                {

                    ApprovalProcessID = CreateApprovalProcessForLeaveEncashmentWithSupervisorHierarchy(applicationMaster.ALEMasterID, Util.AutoLeaveEncashmentAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value);
                    IsResubmitted = false;
                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"],
                            approvalTitle);
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
                //Update Leave Balance And Email
                UpdateLeaveBalanceAfterSubmit(applicationMaster.EmployeeID);

                if (ApprovalProcessID.ToInt() > 0)
                    await SendMail(ApprovalProcessID, IsResubmitted, applicationMaster.ALEMasterID);
                //await SendMailFromManagerBase(ApprovalProcessID, IsResubmitted, applicationMaster.ALEMasterID, (int)Util.MailGroupSetup.LeaveEncashmentInitiatedMail, (int)ApprovalType.LeaveEncashmentApplication);


            }
            await Task.CompletedTask;
            return (true, $"Leave Encashment Application Submitted Successfully");

        }

        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int ALEMasterID)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            if (ToEmailAddress.Count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveEncashmentInitiatedMail, IsResubmitted, ToEmailAddress, CCEmailAddress, null, ALEMasterID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private async Task SendMailToBackEmployee(string ApprovalProcessID, bool IsResubmitted, int ALEMasterID)
        {
            var mail = string.Empty;//GetBackupEmployeeMail(EmployeeLeaveAID).Result;
            List<string> ToEmailAddress = new List<string>() { mail };

            await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.MailGroupSetup.LeaveApplicationMailForBackupEmployee, IsResubmitted, ToEmailAddress, null, null, ALEMasterID, 0);
        }

        private void SetApplicationMasterNewId(AnnualLeaveEncashmentMaster applicationMaster)
        {
            if (!applicationMaster.IsAdded) return;
            var code = GenerateSystemCode("AnnualLeaveEncashmentMaster", AppContexts.User.CompanyID);
            applicationMaster.ALEMasterID = code.MaxNumber;
        }
        private void SetpplicationBreakDownNewId(EmployeeLeaveApplicationDayBreakDown applicationBreakdown)
        {
            if (!applicationBreakdown.IsAdded) return;
            var code = GenerateSystemCode("EmployeeLeaveEncashmentApplicationDayBreakDown", AppContexts.User.CompanyID);
            applicationBreakdown.ELADBDID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        public GridModel GetLeaveEncashmentApplicationList(GridParameter parameters)
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
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                                DISTINCT ELA.*,
								ALEWM.StartDate,
								ALEWM.EndDate,
                                SVAS.SystemVariableCode AS ApprovalStatus,
								CASE WHEN ALEWM.Status=177 THEN 'Initiated' WHEN ALEWM.Status=178 THEN 'Ongoing' WHEN ALEWM.Status=179 THEN 'Expired' ELSE 'Closed' END AS  SessionStatus,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( 353,ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
                                ,VA.EmployeeCode,VA.DepartmentName,VA.FullName EmployeeName
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,FY.Year FinancialYear
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt

	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID ={(int)Util.ApprovalType.LeaveEncashmentApplication}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.ALEMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ELA.ALEMasterID
                                WHERE ELA.EmployeeID = {AppContexts.User.EmployeeID}
                            ";

            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);
            return applications;
        }

        public GridModel GetLeaveEncashmentApplicationListForHODApproval(GridParameter parameters)
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
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
                                DISTINCT ELA.*,
								ALEWM.StartDate,
								ALEWM.EndDate,
                                SVAS.SystemVariableCode AS ApprovalStatus,
								CASE WHEN ALEWM.Status=177 THEN 'Initiated' WHEN ALEWM.Status=178 THEN 'Ongoing' WHEN ALEWM.Status=179 THEN 'Expired' ELSE 'Closed' END AS  SessionStatus,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( 353,ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
                                ,VA.EmployeeCode,VA.DepartmentName,VA.FullName EmployeeName
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,FY.Year FinancialYear
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt

	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID ={(int)Util.ApprovalType.LeaveEncashmentApplication}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.ALEMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ELA.ALEMasterID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID} ) {filter}
                            ";

            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);
            return applications;
        }

        public async Task<LeaveApplication> GetLeaveEncashmentApplicationOld(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT 
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy
	                        FROM EmployeeLeaveEncashmentApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";

            var applicationMaster = EmployeeLeaveEncashmentApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };
            leaveApplication.LeaveDetails = EmployeeLeaveEncashmentApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveEncashmentApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveEncashmentApplicationRepo.GetDataDictCollection(attachmentSql);
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
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            if (approvalProcessID > 0)
            {
                leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication).Result;
            }

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }

        public async Task<LeaveEncashmentApplication> GetLeaveEncashmentApplication(int aLEMasterID, int approvalProcessID)
        {
            string sql = $@"SELECT ELA.*,
								ALEWM.StartDate,
								ALEWM.EndDate,
                                SVAS.SystemVariableCode AS ApprovalStatus,
								ALEWM.Status SessionStatus,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
								
								,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,FY.Year FinancialYear
                                ,AP.APTypeID
	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN HRMS..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN Security..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID = {(int)ApprovalType.LeaveEncashmentApplication}
                           		
	                        WHERE ELA.ALEMasterID={aLEMasterID}";

            var applicationMaster = EmployeeLeaveEncashmentApplicationRepo.GetData(sql);
            LeaveEncashmentApplication leaveEncashmentApplication = null;
            if (applicationMaster.Count > 0)
            {
                leaveEncashmentApplication = new LeaveEncashmentApplication
                {
                    ALEMasterID = (int)applicationMaster["ALEMasterID"],
                    ALEWMasterID = applicationMaster["ALEWMasterID"].ToString().ToInt(),
                    //EmployeeID = (int)applicationMaster["EmployeeID"],
                    APTypeID = (int)applicationMaster["APTypeID"],
                    ApprovalStatusID = (int)applicationMaster["ApprovalStatusID"],
                    SessionStatus = (int)applicationMaster["SessionStatus"],
                    TotalLeaveBalanceDays = (decimal)applicationMaster["TotalLeaveBalanceDays"],
                    EncashedLeaveDays = (decimal)applicationMaster["EncashedLeaveDays"],
                    FinancialYear = applicationMaster["FinancialYear"].ToString(),
                    //DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
                };

            }

            if (approvalProcessID > 0)
            {
                leaveEncashmentApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication).Result;
            }
            return await Task.FromResult(leaveEncashmentApplication);
        }


        public async Task<LeaveApplication> GetLeaveEncashmentApplicationForAdmin(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy
	                        FROM EmployeeLeaveEncashmentApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
							LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID = {(int)ApprovalType.LeaveEncashmentApplication}
                           		
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";

            var applicationMaster = EmployeeLeaveEncashmentApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };
            leaveApplication.LeaveDetails = EmployeeLeaveEncashmentApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveEncashmentApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveEncashmentApplicationRepo.GetDataDictCollection(attachmentSql);
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
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            if (approvalProcessID > 0)
            {
                leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication).Result;
            }

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }

        public async Task<LeaveEncashmentApplicationWithComments> GetLeaveEncashmentApplicationWithCommentsForApproval(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT DISTINCT
                                E.EmployeeID,
								E.FullName, 
								E.EmployeeCode,
								E.DepartmentName,
								E.DesignationName,
								E.DivisionName,
								E.ImagePath,
		                        --SVLC.SystemVariableCode LeaveCategory,
		                        ELA.ALEMasterID
                               ,ELA.ALEWMasterID
                               ,ELA.EmployeeID
                               ,ELA.TotalLeaveBalanceDays
                               ,ELA.EncashedLeaveDays
                               ,ELA.CreatedDate
                               ,AP.APTypeID
                               ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
								--(CE.EmployeeCode +'-'+CE.FullName) CancelledBy,
		                        --ISNULL (BE.FullName,'') BackupEmployeeName,
		                        --ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
                               ,ISNULL(APForward.APForwardInfoID,0) APForwardInfoID
                                ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) AS Bit) IsCurrentAPEmployee
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
	                        FROM AnnualLeaveEncashmentMaster ELA
							LEFT JOIN ViewALLEmployee  E  ON ELA.EmployeeID=E.EmployeeID
                            --LEFT JOIN viewEmployeeUserMapData  CE  ON ELA.CancelledBy=CE.UserID
	                        --LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        --LEFT JOIN Employee BE ON ELA.BackupEmployeeID=BE.EmployeeID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID =24
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 24 AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 24 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                            WHERE ELA.ALEMasterID={employeeLeaveAID}  AND (E.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var applicationMaster = EmployeeLeaveEncashmentApplicationRepo.GetData(sql);
            var leaveApplication = new LeaveEncashmentApplicationWithComments
            {
                ALEMasterID = (int)applicationMaster["ALEMasterID"],
                EmployeeID = (int)applicationMaster["EmployeeID"],
                APForwardInfoID = (int)applicationMaster["APForwardInfoID"],
                APTypeID = (int)applicationMaster["APTypeID"],
                APEmployeeFeedbackID = (int)applicationMaster["APEmployeeFeedbackID"],
                FullName = applicationMaster["FullName"].ToString(),
                IsCurrentAPEmployee = (bool)applicationMaster["IsCurrentAPEmployee"],
                IsReassessment = (bool)applicationMaster["IsReassessment"],
                EmployeeCode = applicationMaster["EmployeeCode"].ToString(),
                DepartmentName = applicationMaster["DepartmentName"].ToString(),
                DesignationName = applicationMaster["DesignationName"].ToString(),
                DivisionName = applicationMaster["DivisionName"].ToString(),
                ImagePath = applicationMaster["ImagePath"].ToString(),

                EncashedLeaveDays = (decimal)applicationMaster["EncashedLeaveDays"],
                TotalLeaveBalanceDays = (decimal)applicationMaster["TotalLeaveBalanceDays"],
                //CancelledBy = applicationMaster["CancelledBy"].ToString(),
                //CancellationStatus = (int)applicationMaster["CancellationStatus"],
                CreatedDate = (DateTime)applicationMaster["CreatedDate"]
                //DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };


            leaveApplication.LeaveBalances = LeaveBalance();

            leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication).Result;
            leaveApplication.RejectedMembers = GetApprovalRejectedMembers(approvalProcessID).Result;
            leaveApplication.ForwardingMembers = GetApprovalForwardingMembers(employeeLeaveAID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.ApprovalPanel.HRLeaveApprovalPanel).Result;

            return await Task.FromResult(leaveApplication);
        }

        public async Task<LeaveApplicationWithComments> GetLeaveEncashmentApplicationWithCommentsForApprovalForHR(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT 
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
                                ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) AS Bit) IsCurrentAPEmployee,
                                CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
	                        FROM EmployeeLeaveEncashmentApplication ELA
							LEFT JOIN ViewALLEmployee  E  ON ELA.EmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData  CE  ON ELA.CancelledBy=CE.UserID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee BE ON ELA.BackupEmployeeID=BE.EmployeeID
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
                            WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";
            var applicationMaster = EmployeeLeaveEncashmentApplicationRepo.GetData(sql);
            var leaveApplication = new LeaveApplicationWithComments
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                EmployeeID = (int)applicationMaster["EmployeeID"],
                APForwardInfoID = (int)applicationMaster["APForwardInfoID"],
                FullName = applicationMaster["FullName"].ToString(),
                IsCurrentAPEmployee = (bool)applicationMaster["IsCurrentAPEmployee"],
                IsReassessment = (bool)applicationMaster["IsReassessment"],
                EmployeeCode = applicationMaster["EmployeeCode"].ToString(),
                DepartmentName = applicationMaster["DepartmentName"].ToString(),
                DesignationName = applicationMaster["DesignationName"].ToString(),
                DivisionName = applicationMaster["DivisionName"].ToString(),
                ImagePath = applicationMaster["ImagePath"].ToString(),
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };

            leaveApplication.LeaveDetails = EmployeeLeaveEncashmentApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                ELADBDID = y.ELADBDID,
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            leaveApplication.LeaveBalances = LeaveBalance();

            leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveEncashmentApplication).Result;
            leaveApplication.RejectedMembers = GetApprovalRejectedMembers(approvalProcessID).Result;
            leaveApplication.ForwardingMembers = GetApprovalForwardingMembers(employeeLeaveAID, (int)Util.ApprovalType.LeaveEncashmentApplication, (int)Util.ApprovalPanel.HRLeaveApprovalPanel).Result;
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveEncashmentApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveEncashmentApplicationRepo.GetDataDictCollection(attachmentSql);
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
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;
            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == employeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
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
                        string filename = $"LA-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "LeaveEncashmentApplication\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));

                        attachemntList.Add(new Attachments
                        {
                            FilePath = filePath,
                            OriginalName = Path.GetFileNameWithoutExtension(attachment.OriginalName),
                            FileName = filename,
                            Type = Path.GetExtension(attachment.OriginalName),
                            Size = attachment.Size
                        });

                        sl++;
                    }

                }
                return attachemntList;
            }
            return null;
        }
        private List<Attachments> RemoveAttachments(LeaveApplication application, int referenceId)
        {
            if (application.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveEncashmentApplication' AND ReferenceID={referenceId}";
                var prevAttachment = EmployeeLeaveEncashmentApplicationRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !application.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "LeaveEncashmentApplication";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        public GridModel GetAllLeaveEncashmentApplicationList(GridParameter parameters)
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
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveEncashmentApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
							";
            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public GridModel GetAllLeaveEncashmentApplicationListForHR(GridParameter parameters, string type)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Pending":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "Rejected":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Rejected}";
                    break;
                default:
                    break;
            }
            filter = filter.IsNotNullOrEmpty() ? @$"WHERE {filter}" : "";
            if (type == "TotalLeaveToday")
            {
                string filterNew = @$" CAST(GETDATE() as Date) BETWEEN CAST(RequestStartDate as Date) AND CAST(RequestEndDate as Date) AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}";
                filter += filter.IsNotNullOrEmpty() ? @$" AND {filterNew}" : $@" WHERE {filterNew}";
            }
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
								ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,
                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
	                        FROM HRMS.dbo.EmployeeLeaveEncashmentApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							        {filter}
							GROUP BY         
								ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID,
		                        SVLC.SystemVariableCode,
                                Emp.FullName,
                                Emp.EmployeeCode,RequestStartDate,RequestEndDate,NoOfLeaveDays,
								SVAS.SystemVariableCode,ELA.ApprovalStatusID,
								AP.ApprovalProcessID,AEF.APEmployeeFeedbackID,APForward.APForwardInfoID,
								LFADID,EditableCount,Rej.Cntr,ELA.CreatedDate,
                                ELA.CancellationStatus";
            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }


        public GridModel GetAllLeaveEncashmentApplicationListForDashboard(GridParameter parameters)
        {
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveEncashmentApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}
							";
            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }

        public async Task<LeavePolicySettingsDto> GetLeavePolicySettings(int leaveCategoryId)
        {
            string sql = @$"SELECT  SV.SystemVariableCode LeaveCategoryName
											,SV.SystemVariableID LeaveCategoryID
											,LPS.*
                                            ,J.JobGradeName
                                            ,E.FullName EmployeeName
											,(
												 SELECT EmployeeCode+'-'+FullName label, EmployeeID value from Employee
												 WHERE EmployeeID IN (Select * from dbo.fnReturnStringArray(LPS.ProxyEmployeeIDs,','))
												 FOR JSON PATH
											) AS ProxyEmployeeStr 
                                FROM Security..SystemVariable SV
                                LEFT JOIN LeavePolicySettings LPS ON LPS.LeaveCategoryID = SV.SystemVariableID
                                LEFT JOIN Employee E ON E.EmployeeID=LPS.EmployeeID
                                LEFT JOIN JobGrade J ON J.JobGradeID=LPS.MaximumJobGrade
                                WHERE SV.SystemVariableID={leaveCategoryId}";
            var setting = Task.Run(() => LeavePolicySettingsRepo.GetModelData<LeavePolicySettingsDto>(sql));

            //var setting = Task.Run(() => LeavePolicySettingsRepo.Entities.Where(x => x.LeaveCategoryID == leaveCategoryId).SingleOrDefault().MapTo<LeavePolicySettingsDto>());
            return await setting;
        }

        public async Task<(bool, string)> SavePolicySettings(LeavePolicySettingsDto settings)
        {
            using var unitOfWork = new UnitOfWork();
            var existingSettings = LeavePolicySettingsRepo.Entities.SingleOrDefault(x => x.LPSID == settings.LPSID);
            settings.EmployeeTypes = String.Join(",", settings.EmployeeTypeList);
            if (settings.IncludeHRForLeave == false)
            {
                settings.ApplicableToHRForDays = 0;
            }
            if (settings.IncludeHRForLeave == false && settings.IncludeHRForLFA == false)
            {
                settings.EmployeeID = 0;
                settings.ProxyEmployeeID = null;
            }

            if (existingSettings.IsNull() || existingSettings.LPSID.IsZero())
            {
                settings.SetAdded();
                SetNewLPolicySettingID(settings);
            }
            else
            {
                settings.SetModified();
            }
            var settingsEnt = settings.MapTo<LeavePolicySettings>();
            settingsEnt.CompanyID = settings.CompanyID ?? AppContexts.User.CompanyID;

            LeavePolicySettingsRepo.Add(settingsEnt);
            await Task.Run(() => unitOfWork.CommitChangesWithAudit());

            return (true, $"Settings Saved Successfully");

        }
        private void SetNewLPolicySettingID(LeavePolicySettingsDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("LeavePolicySettings", AppContexts.User.CompanyID);
            obj.LPSID = code.MaxNumber;
        }

        public bool CheckMultipleSupervisor()
        {
            bool isMultiple = false;
            var supervisors = EmployeeSupervisorMapRepo.Entities.Where(x => x.EmployeeID == AppContexts.User.EmployeeID && x.IsCurrent == true && x.SupervisorType == (int)Util.SupervisorType.Regular).ToList();
            isMultiple = supervisors.Count > 1 ? true : false;
            return isMultiple;
        }
        public bool CheckEncashmentEligible()
        {
            bool isEligible = false;
            Dictionary<string, object> eligible = GetCurrentWindow(0);
            isEligible = eligible.Count > 0 ? true : false;
            return isEligible;
        }

        private Dictionary<string, object> GetCurrentWindow(int ALEMasterID)
        {
            string sql = $@"SELECT *
                            FROM AnnualLeaveEncashmentWindowMaster ALEWM
                            INNER JOIN AnnualLeaveEncashmentWindowChild ALEWC ON ALEWM.ALEWMasterID = ALEWC.ALEWMasterID                            
                            LEFT JOIN AnnualLeaveEncashmentMaster ELM ON ELM.ALEWMasterID = ALEWM.ALEWMasterID AND ELM.EmployeeID = ALEWC.EmployeeID
                            WHERE STATUS = 178 AND ALEWC.EmployeeID = {AppContexts.User.EmployeeID} AND (ELM.ALEMasterID IS NULL OR ELM.ApprovalStatusID = {(int)Util.ApprovalStatus.Rejected} OR ELM.ALEMasterID={ALEMasterID})";
            var eligible = EmploymentRepo.GetData(sql);
            //isEligible = eligible.Count > 0 ? true : false;
            return eligible;
        }

        public async Task<List<LeavePolicySettingsDto>> GetLeaveCategoriesWithSettings()
        {

            string employementSql = $@"SELECT *  FROM HRMS..ViewALLEmployee WHERE EmployeeID = {AppContexts.User.EmployeeID}";
            var employement = EmploymentRepo.GetData(employementSql);
            string employeeTypeId = employement["EmployeeTypeID"].ToString();
            DateTime joiningDate = string.IsNullOrEmpty(employement["DateOfJoining"].ToString()) ? DateTime.Today : DateTime.Parse(employement["DateOfJoining"].ToString());
            int tanureInMonth = Util.GetTotalMonthsFrom(joiningDate, DateTime.Today);
            string genderId = employement["GenderID"].ToString();

            var sql = @$"SELECT SV.SystemVariableCode AS LeaveCategoryName
                            	,LPS.LPSID
                            	,SV.SystemVariableID LeaveCategoryID
                            	,LPS.MinimumDays
                            	,LPS.MaximumDays
                            	,LPS.DayType
                            	,LPS.EmployeeTypeException
                            	,LPS.EmployeeTypes
                            	,LPS.TanureException
                            	,LPS.EligibilityInMonths
                            	,LPS.IsHolidayInclusive
                            	,LPS.IsCarryForwardable
                            	,LPS.MaximumAccumulationDays
                            	,LPS.IsAttachemntRequired
                            	,LPS.WillApplicableFrom
                        FROM Security.dbo.SystemVariable AS SV
                        LEFT OUTER JOIN LeavePolicySettings AS LPS ON LPS.LeaveCategoryID = SV.SystemVariableID
                            WHERE (SV.EntityTypeID = 10)
                            AND SV.SystemVariableID <> (CASE WHEN {genderId} = {(int)Util.Gender.Male} THEN  65 WHEN  {genderId} = {(int)Util.Gender.Female}  THEN 142 END)
    	                        AND {employeeTypeId} NOT IN (
		                        SELECT _ID
		                        FROM dbo.fnReturnStringArray(LPS.EmployeeTypes, ',')
		                        ) AND ((ISNULL(LPS.TanureException,0)=1 AND ISNULL(LPS.EligibilityInMonths,0)<={tanureInMonth}) OR ISNULL(LPS.TanureException,0)=0)";
            var data = Task.Run(() => LeavePolicySettingsRepo.GetDataModelCollection<LeavePolicySettingsDto>(sql));
            return await data;
        }


        public GridModel GetAllLEApplicationList(GridParameter parameters)
        {

            string sql = $@"SELECT 
                                DISTINCT ELA.*,
								ALEWM.StartDate,
								ALEWM.EndDate,
                                SVAS.SystemVariableCode AS ApprovalStatus,
								CASE WHEN ALEWM.Status=177 THEN 'Initiated' WHEN ALEWM.Status=178 THEN 'Ongoing' WHEN ALEWM.Status=179 THEN 'Expired' ELSE 'Closed' END AS  SessionStatus,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( 353,ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
                                ,(ELA.TotalLeaveBalanceDays - ELA.EncashedLeaveDays) RemainingBalance
                                ,VA.EmployeeCode,VA.DepartmentName,VA.FullName EmployeeName
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,VA.DesignationName
							    ,VA.DivisionName
                                ,FY.Year FinancialYear
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt

	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID ={(int)Util.ApprovalType.LeaveEncashmentApplication}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.ALEMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ELA.ALEMasterID
                                  WHERE ELA.ALEWMasterID = {parameters.ApprovalFilterData}
                                --WHERE ELA.EmployeeID = {AppContexts.User.EmployeeID}
                            ";

            var applications = EmployeeLeaveEncashmentApplicationRepo.LoadGridModel(parameters, sql);
            return applications;
        }

        public Task<DataSet> GetAllLeaveEncashmentApplicationList(int ALEWMasterID)
        {
            string sql = $@"SELECT 
                            DISTINCT
								--ROW_NUMBER() OVER(ORDER BY ELA.CreatedDate DESC) AS #SL_No
							VA.FullName Employee_Name
                            ,VA.DesignationName Designation
							,VA.DepartmentName Department
							,VA.DivisionName Division
							,ELA.TotalLeaveBalanceDays Total_AL
							,ELA.EncashedLeaveDays Encashed_AL
                            ,(ELA.TotalLeaveBalanceDays - ELA.EncashedLeaveDays) Remaining_Balance
							,CAST(ELA.CreatedDate AS varchar(10)) Applied_Date
                            ,SVAS.SystemVariableCode AS Approval_Status
							,PendingAt.PendingEmployee Pending_At

	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID ={(int)Util.ApprovalType.LeaveEncashmentApplication}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.ALEMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
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
                                            CONCAT(AEF.EmployeeCode ,'-', EmployeeName,'-', DepartmentName) PendingEmployee,
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ELA.ALEMasterID
                                  WHERE ELA.ALEWMasterID = {ALEWMasterID}
                            ";


            var listresult = Adapter.GetDataSet(sql, false); //Adapter.GetModel<UserDto>(sql);
            return Task.FromResult(listresult.Result);
        }

        public async Task<List<Dictionary<string, object>>> GetAllLeaveEncashmentApplications(int ALEWMasterID)
        {
            string sql = $@"SELECT 
                            DISTINCT
								--ROW_NUMBER() OVER(ORDER BY ELA.CreatedDate DESC) AS #SL_No
							VA.FullName Employee_Name
                            ,VA.DesignationName Designation
							,VA.DepartmentName Department
							,VA.DivisionName Division
							,ELA.TotalLeaveBalanceDays Total_AL
							,ELA.EncashedLeaveDays Encashed_AL
                            ,(ELA.TotalLeaveBalanceDays - ELA.EncashedLeaveDays) Remaining_Balance
							,CAST(ELA.CreatedDate AS varchar(10)) Applied_Date
                            ,SVAS.SystemVariableCode AS Approval_Status
							,PendingAt.PendingEmployee Pending_At

	                        FROM HRMS.dbo.AnnualLeaveEncashmentMaster ELA
							LEFT JOIN HRMS..AnnualLeaveEncashmentWindowMaster ALEWM ON ELA.ALEWMasterID=ALEWM.ALEWMasterID
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
                            LEFT JOIN Security..Users U ON U.UserID = ELA.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.ALEMasterID AND AP.APTypeID ={(int)Util.ApprovalType.LeaveEncashmentApplication}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.ALEMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.ALEMasterID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
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
                                            CONCAT(AEF.EmployeeCode ,'-', EmployeeName,'-', DepartmentName) PendingEmployee,
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.LeaveEncashmentApplication} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = ELA.ALEMasterID
                                  WHERE ELA.ALEWMasterID = {ALEWMasterID}
                            ";
            var leaveEncashmentList = EmployeeLeaveEncashmentApplicationRepo.GetDataDictCollection(sql);
            return leaveEncashmentList.ToList();
        }


    }
}
