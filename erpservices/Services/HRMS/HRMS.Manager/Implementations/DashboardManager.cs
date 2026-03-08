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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class DashboardManager : ManagerBase, IDashboardManager
    {

        private readonly IRepository<Employee> EmployeeRepo;
        public DashboardManager(IRepository<Employee> employeeRepo)
        {
            EmployeeRepo = employeeRepo;
        }

        public async Task<int> GetTotalEmployee()
        {

            int totalEmployee = EmployeeRepo.Entities.Count();
            return await Task.FromResult(totalEmployee);
        }

        public async Task<List<EmployeeDto>> GetAllEmployeeAttendanceForToday()
        {
            string sql = @$"SELECT * FROM ViewGetAllEmployeeAttendance Att 
                            ORDER BY Att.PersonID ASC";
            var list = EmployeeRepo.GetDataModelCollection<EmployeeDto>(sql);
            return await Task.FromResult(list);
        }

        public async Task<int> GetTotalPresentToday()
        {
            string sql = @$"SELECT * FROM ViewGetAllEmployeeAttendance Att 
                            ORDER BY Att.PersonID ASC";
            var list = EmployeeRepo.GetDataModelCollection<EmployeeDto>(sql);
            int totalPresent = list.Where(x => x.IN_TIME != "").Count();
            return await Task.FromResult(totalPresent);
        }

        public async Task<int> GetTotalAbsentToday()
        {
            string sql = @$"SELECT * FROM ViewGetAllEmployeeAttendance Att 
                            ORDER BY Att.PersonID ASC";
            var list = EmployeeRepo.GetDataModelCollection<EmployeeDto>(sql);
            int totalAbsent = list.Where(x => x.IN_TIME == "").Count();
            return await Task.FromResult(totalAbsent);
        }
        public async Task<int> GetTotalLeaveToday()
        {
            string today = DateTime.Now.ToString("dd-MM-yyyy");
            string sql = $@"SELECT COUNT(*) PendingLEAVE FROM EmployeeLeaveApplication WHERE CAST(GETDATE() as Date) BETWEEN CAST(RequestStartDate as Date) AND CAST(RequestEndDate as Date) AND ApprovalStatusID IN ({(int)Util.ApprovalStatus.Approved},{(int)Util.ApprovalStatus.Pending})"; 
            var obj = EmployeeRepo.GetData(sql);

            int totalLeaveToday = (int)obj["PendingLEAVE"];
            return await Task.FromResult(totalLeaveToday);
        }

        public async Task<double> GetTotalLatePercent()
        {
            string today = DateTime.Now.ToString("dd-MM-yyyy");

            string sql = @$"SELECT  [SIMPLE_DATE] AttendanceDate,
	                      [NAGAD_EMP_ID] EmployeeCode
	                      ,ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                      ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
	                      ,CASE 
	                      WHEN SUBSTRING(MIN([DATE_TIME]),12,8) > '09:15:00'
	                      THEN 'Late'
	                      ELSE 'OK'
	                      END ATTENDANCE_STATUS,
	                      CONVERT(varchar(5), 
                          DATEADD(minute, DATEDIFF(minute,MIN([DATE_TIME]),MAX([DATE_TIME])),0),114) AS WORK_HOUR,
	                      CASE WHEN DATEDIFF(minute,MIN([DATE_TIME]),MAX([DATE_TIME]))>=9*60 THEN 'Full Day'
	                      ELSE 'Partial Day'
	                      END AS DAY_STATUS
                      FROM [attendanceService].[dbo].[EVENT_LOG]
                      INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                      WHERE convert(varchar, convert(date, [SIMPLE_DATE]), 105) = '{today}'
                      GROUP BY [SIMPLE_DATE]
	                      ,[NAGAD_EMP_ID]
                      ORDER BY 1 DESC";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceDto>(sql);
            double totalLate = list.Where(x => x.ATTENDANCE_STATUS != "").Count();
            double totalEmployee = EmployeeRepo.Entities.Count();
            double percent = (totalLate / totalEmployee) * 100;
            return await Task.FromResult(percent);
        }


        public async Task<(int, int, int)> GetTotalPendingApproval()
        {
            string today = DateTime.Now.ToString("dd-MM-yyyy");
            int pendingNFA = 0;
            int pendingLeaveApplication = 0;
            int pendingRemoteAttendance = 0;

            string query1 = $@"SELECT COUNT(*) PendingNFA FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..NFAMaster WHERE ApprovalStatusID=22";
            string query2 = $@"SELECT COUNT(*) PendingLEAVE FROM EmployeeLeaveApplication WHERE ApprovalStatusID=22";
            string query3 = $@"SELECT COUNT(*) AttCount FROM RemoteAttendance WHERE StatusID = 22";
            var nfaDic = EmployeeRepo.GetData(query1);
            var leaveDic = EmployeeRepo.GetData(query2);
            var remAttDic = EmployeeRepo.GetData(query3);
            if (nfaDic.Count > 0)
                pendingNFA = nfaDic["PendingNFA"].ToString().ToInt();

            if (leaveDic.Count > 0)
                pendingLeaveApplication = leaveDic["PendingLEAVE"].ToString().ToInt();

            if (remAttDic.Count > 0)
                pendingRemoteAttendance = remAttDic["AttCount"].ToString().ToInt();


            return await Task.FromResult((pendingNFA, pendingLeaveApplication, pendingRemoteAttendance));
        }

        public async Task<List<Dictionary<string, object>>> GetOrganogram()
        {
            string sql = $@"SELECT 
		                        Emp.EmployeeID,
		                        Emp.FullName EmployeeName,
		                        Emp.EmployeeCode,
		                        EmployeeSupervisorID,
		                        ImagePath,
		                        DepartmentName,
		                        DivisionName,
		                        Des.DesignationName,
		                        Emp.WorkMobile,
		                        emp.WorkEmail
	                        FROM 
		                        {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee AS emp
		                        LEFT JOIN (SELECT EmployeeID,DepartmentID,DivisionID,DesignationID,JobGradeID,EmployeeTypeID FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employment where  IsCurrent = 1) empl ON empl.EmployeeID = emp.EmployeeID 
		                        LEFT JOIN (SELECT DepartmentID,DepartmentName FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Department) Dep ON Dep.DepartmentID = empl.DepartmentID
		                        LEFT JOIN (SELECT DesignationID,DesignationName FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Designation) Des ON des.DesignationID = empl.DesignationID
		                        LEFT JOIN (SELECT DivisionID,DivisionName FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Division  ) Div on Div.DivisionID = empl.DivisionID
		                        LEFT JOIN (
		                        SELECT EmployeeID,SupervisorType,IsCurrent,EmployeeSupervisorID
		                        FROM 
		                        {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..EmployeeSupervisorMap  
		                        WHERE  IsCurrent =1 AND SupervisorType = 50
		                        )SuperEmp ON SuperEmp.EmployeeID = Emp.EmployeeID 
		                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..JobGrade JG ON JG.JobGradeID = empl.JobGradeID
		                        LEFT JOIN (SELECT
                                            PersonID,ImagePath
                                        FROM
                                           {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage
                                            WHERE IsFavorite = 1
                                        ) PIM ON PIM.PersonID = Emp.PersonID
		                        WHERE  (ISNULL(EmployeeTypeID,0) <> 48 OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date)) 
		                        AND (EmployeeSupervisorID IS NOT NULL OR Emp.EmployeeID = 1)
		                        ORDEr BY EmployeeID ASC";
            sql = @$"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..viewEmployeeDetailsForOrganogram ORDER BY EmployeeID ASC";
            var list = EmployeeRepo.GetDataDictCollectionWithTransaction(sql);
            return await Task.FromResult(list.ToList());
        }
    }
}
