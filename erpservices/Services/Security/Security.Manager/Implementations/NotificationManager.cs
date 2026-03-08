using Core;
using Core.AppContexts;
using DAL.Core.Repository;
using Manager.Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class NotificationManager : ManagerBase, INotificationManager
    {
        private readonly IRepository<Menu> MenuRepo;
        public NotificationManager(IRepository<Menu> menuRepository)
        {
            MenuRepo = menuRepository;
        }
        public async Task<int> GetNotificationCount()
        {
            Random random = new Random();
            return await Task.FromResult(random.Next(1, 200));
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetNotificationListOld(int top = 100000000)
        {
            var sql = $@"SELECT TOP {top} *,COUNT(*) OVER () AS TotalRows 
						FROM 
							(SELECT  
								vA.* 
							FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> {(int)Util.ApprovalFeedback.Forwarded}

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))

							UNION 

							SELECT 
									*
							FROM 
								{AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLPendingRemoteAttendance
							WHERE EmployeeID = {AppContexts.User.EmployeeID})A
							ORDER BY OrderFeedbackRequestDate desc";
			return await MenuRepo.GetDataDictCollectionAsync(sql);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetNotificationList(int top = 100000000)
        {
			
			var sql = $@"SELECT TOP {top} *,COUNT(*) OVER () AS TotalRows
						FROM 
							(SELECT  
								va.OrderFeedbackRequestDate,va.Description,
								va.FeedbackRequestDate,va.ReferenceID, va.ApprovalProcessID,va.APEmployeeFeedbackID,va.APTypeID,convert(varchar(max),va.APTypeName) APTypeName, va.APForwardInfoID ,va.Title,va.IsAPEditable
								, va.APFeedbackID, va.IsEditable, va.IsSCM, va.IsMultiProxy, va.DepartmentID, CASE WHEN vA.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy, va.Particulars
							FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> {(int)Util.ApprovalFeedback.Forwarded}

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))

							UNION 

							SELECT  
								v.OrderFeedbackRequestDate,v.Description,
								v.FeedbackRequestDate,v.ReferenceID, v.ApprovalProcessID,v.APEmployeeFeedbackID,v.APTypeID,convert(varchar(max),v.APTypeName) APTypeName, v.APForwardInfoID ,v.Title,v.IsAPEditable
								, v.APFeedbackID, v.IsEditable, v.IsSCM, 0 IsMultiProxy, v.DepartmentID, '' Proxy, '' Particulars
							FROM 
								{AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLPendingRemoteAttendance v
							WHERE EmployeeID = {AppContexts.User.EmployeeID}
														
							UNION
							SELECT 
							v.OrderFeedbackRequestDate,v.Description,
							v.FeedbackRequestDate,v.ReferenceID, v.ApprovalProcessID,v.APEmployeeFeedbackID,v.APTypeID,convert(varchar(max),v.APTypeName) APTypeName, v.APForwardInfoID ,v.Title,v.IsAPEditable
							, v.APFeedbackID, v.IsEditable, v.IsSCM, 0 IsMultiProxy, 0 DepartmentID, '' Proxy, '' Particulars
							FROM 
								HRMS..ViewALLUnSettledApprovedAdminSupportRequest v
							WHERE CreatedByEmployeeID =  {AppContexts.User.EmployeeID}
							
							)A
							ORDER BY OrderFeedbackRequestDate desc";

			return await MenuRepo.GetDataDictCollectionAsync(sql);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetNotificationListForNFA(int top = 100000000)
        {
            var sql = $@"SELECT TOP {top} * FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..viewAllPendingApproval 
                        WHERE (EmployeeID = {AppContexts.User.EmployeeID} OR ProxyEmployeeID = {AppContexts.User.EmployeeID}) AND APTypeID = {(int)Util.ApprovalType.NFA}";
            return await Task.FromResult(MenuRepo.GetDataDictCollection(sql));
        }

        public async Task<List<NotificationDto>> GetNotificationListForNotification()
        {
			var sql = $@"
						SELECT * FROM 
						(

						SELECT *,
								 ROW_NUMBER() OVER (PARTITION BY APTypeID ORDER BY APTypeID DESC) AS rn FROM 
						(
							SELECT 
							AP.APTypeID
							,APName 
							,ReferenceID
							,Title
							,Description
							,ApprovalProcessID
	
							,APFeedbackName	
							,EmployeeID
							,ProxyEmployeeID
							,RequestedEmployee
							,RequestedEmployeeImagePath
							,FeedbackRequestDate
							,APEmployeeFeedbackID
							,APFeedbackID
							,APForwardInfoID
							,IsAPEditable
							,IsEditable
							,IsSCM
							,ISNULL(TotalCount,0) TotalCount
							FROM 
							(
							SELECT 
								APTypeID,
								Name APName
							FROM 
								Approval..ApprovalType APT

							UNION 

							SELECT 
								0 APTypeID,
								'Attendance' APName
							) AP
							LEFT JOIN (
							SELECT * FROM Approval..viewAllPendingApproval WHERE (EmployeeID = {AppContexts.User.EmployeeID} OR ProxyEmployeeID = {AppContexts.User.EmployeeID})
							UNION 
							SELECT *,'' Particulars FROM HRMS..ViewALLPendingRemoteAttendance WHERE (EmployeeID = {AppContexts.User.EmployeeID} )
							) APT ON APT.APTypeID = AP.APTypeID
							LEFT JOIN (
									SELECT 
									COUNT(APTypeID) TotalCount,APTypeID 
								FROM 
									Approval..viewAllPendingApproval 
								WHERE (EmployeeID = {AppContexts.User.EmployeeID} OR ProxyEmployeeID = {AppContexts.User.EmployeeID})
								GROUP BY APTypeID

								UNION 

								SELECT 
									COUNT(*)TotalCount,APTypeID 
								FROM 
									HRMS..ViewALLPendingRemoteAttendance 
								WHERE (EmployeeID = {AppContexts.User.EmployeeID} )
								GROUP BY APTypeID
								)TC ON TC.APTypeID = AP.APTypeID
						)AA
						)AAA
						WHERE rn between 1 and 10";



			var list = await MenuRepo.GetDataModelCollectionAsync<NotificationDto>(sql);
			var customList = list.GroupBy(c => new { c.APTypeID, c.APName})
				.Select(chld => new NotificationDto()
				{
					APTypeID = chld.Key.APTypeID,
					APName = chld.Key.APName,
					NotificationDetails = chld.ToList()
				}).ToList();

			await Task.CompletedTask;
			return customList;
        }


		public async Task<IEnumerable<Dictionary<string, object>>> GetNotificationByAPType(int id)
		{
			string sql = "";
			if (id == 0)
			{
				sql = $@"SELECT 
									*
							FROM
								HRMS..ViewALLPendingRemoteAttendance
							WHERE EmployeeID = { AppContexts.User.EmployeeID }
							ORDER BY OrderFeedbackRequestDate desc";
			} else if(id == 501)
            {
				sql = $@"SELECT 
									*
							FROM
								kpitools..kpi_notifications
							WHERE EmployeeID = { AppContexts.User.EmployeeID }
							ORDER BY OrderFeedbackRequestDate desc";
			}
            else if (id == 33)
            {
                sql = $@"SELECT TOP 10
									*
							FROM
								HRMS..ViewALLUnSettledApprovedAdminSupportRequest
							WHERE CreatedByEmployeeID = {AppContexts.User.EmployeeID}
							ORDER BY OrderFeedbackRequestDate desc";
            }
			 else if (id == 34)
            {
                sql = $@"SELECT TOP 10
									*
							FROM
								Accounts..ViewALLUnDisburseApprovedClaimData
							WHERE CWEmployeeID = {AppContexts.User.EmployeeID}
							ORDER BY OrderFeedbackRequestDate desc";
            }
            else if (id == 35)
            {
                sql = $@"SELECT TOP 10
									*
							FROM
								Accounts..ViewALLClaimDataForResubmit
							WHERE EmployeeID = {AppContexts.User.EmployeeID}
							ORDER BY OrderFeedbackRequestDate desc";
            }
			 else if (id == 36)
            {
                sql = $@"SELECT TOP 10
									*
							FROM
								Accounts..CustodianWalletThresholdText
							WHERE EmployeeID = {AppContexts.User.EmployeeID}
							ORDER BY OrderFeedbackRequestDate desc";
            }

            else
			{
				sql = $@"SELECT DISTINCT TOP (10) *,COUNT(*) OVER () AS TotalRows, CASE WHEN A.EmployeeID <> {AppContexts.User.EmployeeID} THEN 'Proxy' ELSE '' END Proxy
						FROM 
							(SELECT  
								vA.* 
							FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> {(int)Util.ApprovalFeedback.Forwarded}

							WHERE VA.APTypeID={id} AND (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))
							)A
							ORDER BY OrderFeedbackRequestDate desc";
			}
			return await MenuRepo.GetDataDictCollectionAsync(sql);
		}

		public async Task<IEnumerable<Dictionary<string, object>>> GetAPTypeListOld()
		{
			var sql = $@"SELECT APTypeID,APTypeName,COUNT(APTypeID) TotalRows
						FROM 
							(SELECT  
								vA.* 
							FROM Approval..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> 8

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))

							UNION 

							SELECT 
									*
							FROM 
								HRMS..ViewALLPendingRemoteAttendance
							WHERE EmployeeID = {AppContexts.User.EmployeeID})A
							GROUP BY APTypeID,APTypeName";
			return await Task.FromResult(MenuRepo.GetDataDictCollection(sql));
		}
		public async Task<IEnumerable<Dictionary<string, object>>> GetAPTypeList()
		{
			var sql = $@"SELECT APTypeID,APTypeName,COUNT(APTypeID) TotalRows
						FROM 
							(SELECT  
								va.APFeedbackID,va.APEmployeeFeedbackID,va.APTypeID,va.APTypeName,va.IsAPEditable

							FROM Approval..viewAllPendingApproval vA
							LEFT JOIN 
							(
								SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
							) Prox ON Prox.ApprovalProcessID = vA.ApprovalProcessID AND Prox.APEmployeeFeedbackID = vA.APEmployeeFeedbackID AND vA.APFeedbackID <> 8

							WHERE (vA.EmployeeID = {AppContexts.User.EmployeeID} OR Prox.EmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ) 
							OR vA.ProxyEmployeeID = (CASE WHEN CreatedByEmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END ))

							UNION 

							SELECT 
									v.APFeedbackID,v.APEmployeeFeedbackID,v.APTypeID,v.APTypeName,v.IsAPEditable

							FROM 
								HRMS..ViewALLPendingRemoteAttendance v
							WHERE EmployeeID = {AppContexts.User.EmployeeID}							
							
							UNION
							SELECT 
								v.ReferenceID APFeedbackID,v.APEmployeeFeedbackID,v.APTypeID, v.APTypeName,v.IsAPEditable
							FROM 
								HRMS..ViewALLUnSettledApprovedAdminSupportRequest v
							WHERE CreatedByEmployeeID =  {AppContexts.User.EmployeeID}
							

							)A
							GROUP BY APTypeID,APTypeName";
			return await MenuRepo.GetDataDictCollectionAsync(sql);
		}
		
	}
}
