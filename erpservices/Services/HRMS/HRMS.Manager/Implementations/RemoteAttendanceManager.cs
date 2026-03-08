using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class RemoteAttendanceManager : ManagerBase, IRemoteAttendanceManager
    {
        private readonly IRepository<RemoteAttendance> RemoteAttendanceRepo;
        private readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        private readonly IRepository<Employee> EmployeeRepo;
        public RemoteAttendanceManager(IRepository<RemoteAttendance> remoteAttendanceRepo, IRepository<EmployeeSupervisorMap> employeeSupervisorMapRepo, IRepository<Employee> employeeRepo)
        {
            RemoteAttendanceRepo = remoteAttendanceRepo;
            EmployeeSupervisorMapRepo = employeeSupervisorMapRepo;
            EmployeeRepo = employeeRepo;
        }

        public async Task<List<RemoteAttendanceDto>> GetRemoteAttendanceListExceptApproved()
        {
            string sql = $@"SELECT 
	                            RA.*,
	                            Emp.EmployeeCode,
	                            Apvr.FullName ApproverName,
								AStatus.SystemVariableCode StatusName
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN Employee Emp ON RA.EmployeeID = Emp.EmployeeID
	                            LEFT JOIN Employee Apvr ON Apvr.EmployeeID = RA.ApproverID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
                            	WHERE RA.EmployeeID = {AppContexts.User.EmployeeID} AND ((AStatus.SystemVariableCode = 'Approved' AND CONVERT(date,RA.AttendanceDate) = CONVERT(date, GETDATE())) OR AStatus.SystemVariableCode <> 'Approved') ORDER BY RA.RAID DESC";

                            //WHERE RA.EmployeeID = {AppContexts.User.EmployeeID} AND AStatus.SystemVariableCode <> 'Approved' ORDER BY RA.RAID DESC";
            var list = RemoteAttendanceRepo.GetDataModelCollection<RemoteAttendanceDto>(sql);

            return await Task.FromResult(list);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetIPAddressList()
        {
            string sql = $@"SELECT * FROM Security..IPAddress
                            WHERE CompanyID = '{AppContexts.User.CompanyID}'";
            var list = RemoteAttendanceRepo.GetDataDictCollection(sql);

            return await Task.FromResult(list);
        }


        public async Task<Dictionary<string, object>> GetLastEntryType()
        {
            string sql = $@" select TOP(1) RA.EntryType
            from HRMS..RemoteAttendance RA
            WHERE CAST(RA.AttendanceDate as date) = CAST(GETDATE() as date) AND  RA.EmployeeID= {AppContexts.User.EmployeeID}
            ORDER BY RAID DESC";
            return await Task.FromResult(RemoteAttendanceRepo.GetData(sql));
        }
        public async Task<Dictionary<string, object>> GetPresentRemoteAttendance()
        {
            string sql = $@"SELECT 
                                ISNULL((SELECT 
	                                 CASE WHEN EntryType = 'IN' THEN ISNULL(CONVERT(VARCHAR(10), CAST(MIN(AttendanceDate) AS TIME), 0),'') ELSE '' END IN_TIME
                                FROM 
	                                RemoteAttendance
                                WHERE CAST(AttendanceDate as date) = CAST(GETDATE() as date) AND EmployeeID = {AppContexts.User.EmployeeID} AND EntryType = 'IN'
                                GROUP BY EntryType
                                ),'') IN_TIME,
                                ISNULL((SELECT 	 
	                                CASE WHEN EntryType = 'OUT' THEN ISNULL(CONVERT(VARCHAR(10), CAST(MAX(AttendanceDate) AS TIME), 0),'') ELSE '' END  OUT_TIME
                                FROM 
	                                RemoteAttendance
                                WHERE CAST(AttendanceDate as date) = CAST(GETDATE() as date) AND EmployeeID = {AppContexts.User.EmployeeID} AND EntryType = 'OUT'
                                GROUP BY EntryType),'') OUT_TIME";
            return await Task.FromResult(RemoteAttendanceRepo.GetData(sql));
        }
        public async Task<Dictionary<string, object>> GetPresentRemoteAttendanceFromMachine()
        {
            string sql = $@"SELECT 
	                           ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
                          FROM [attendanceService].[dbo].[EVENT_LOG]
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                          WHERE Emp.EmployeeID = {AppContexts.User.EmployeeID} AND CAST([SIMPLE_DATE] as date) = CAST(getdate() as date)
                          GROUP BY [SIMPLE_DATE],[NAGAD_EMP_ID]";
            return await Task.FromResult(RemoteAttendanceRepo.GetData(sql));
        }
        public Task<RemoteAttendanceDto> SaveChanges(RemoteAttendanceDto remoteAttendanceDto)
        {
            var IsRemoteIP = AppContexts.IsRemoteIP(GetIPAddressList().Result.Select(x => x["IPNumber"].ToString()).ToList());
            
            if(remoteAttendanceDto.EntryType == "OUT")
            {
                string currDate = DateTime.Now.ToString("dd-MM-yyyy");
                string query = $@"SELECT * FROM HRMS..RemoteAttendance RA WHERE RA.EmployeeID={(int)AppContexts.User.EmployeeID} AND convert(varchar, convert(date, RA.AttendanceDate), 105) ='{currDate}' AND RA.EntryType='IN' ";
                var existsInTimeSameDay = RemoteAttendanceRepo.GetData(query);
                if (existsInTimeSameDay.Count == 0)
                {
                    remoteAttendanceDto.InTimeError = "No In Time found. Please refresh the page and try again.";
                    return Task.FromResult(remoteAttendanceDto);
                }

                //OUT Entry time Checking added 29-feb-2024
                string query1 = $@"SELECT TOP 1 * FROM HRMS..RemoteAttendance RA WHERE RA.EmployeeID={(int)AppContexts.User.EmployeeID} AND convert(varchar, convert(date, RA.AttendanceDate), 105) ='{currDate}' ORDER BY RA.RAID DESC";
                var existsOutTimeSameDay = RemoteAttendanceRepo.GetData(query1);
                if (existsOutTimeSameDay != null)
                {
                    // Check if the dictionary contains the "EntryType" key
                    if (existsOutTimeSameDay.ContainsKey("EntryType"))
                    {
                        // Retrieve the value corresponding to the "EntryType" key
                        var entryTypeValue = existsOutTimeSameDay["EntryType"];

                        // Check if EntryType is "OUT"
                        if (entryTypeValue != null && entryTypeValue.ToString() == "OUT")
                        {
                            remoteAttendanceDto.InTimeError = "No In Time found. Please refresh the page and try again.";
                            return Task.FromResult(remoteAttendanceDto);
                        }
                      
                    }
                   
                }



            }
            
            using (var unitOfWork = new UnitOfWork())
            {
                var existRemoteAttendance = RemoteAttendanceRepo.Entities.SingleOrDefault(x => x.RAID == remoteAttendanceDto.RAID).MapTo<RemoteAttendance>();
                var superviosr = EmployeeSupervisorMapRepo.Entities.FirstOrDefault(x => x.EmployeeID == AppContexts.User.EmployeeID && x.SupervisorType == (int)Util.SupervisorType.Regular && x.IsCurrent);

                if (existRemoteAttendance.IsNull() || remoteAttendanceDto.RAID.IsZero())
                {
                    remoteAttendanceDto.SetAdded();
                    SetNewRAID(remoteAttendanceDto);
                }
                remoteAttendanceDto.AttendanceDate = DateTime.Now;
                remoteAttendanceDto.StatusID = IsRemoteIP.IsFalse() ? (int)Util.ApprovalStatus.Approved : (int)Util.ApprovalStatus.Pending;
                remoteAttendanceDto.Channel = IsRemoteIP.IsFalse() ? "Intranet" : "Internet";
                remoteAttendanceDto.EmployeeID = (int)AppContexts.User.EmployeeID;
                remoteAttendanceDto.ApproverID = superviosr.IsNotNull() ? superviosr.EmployeeSupervisorID : 0;
                var saveRemoteAttendance = remoteAttendanceDto.MapTo<RemoteAttendance>();
                SetAuditFields(saveRemoteAttendance);
                RemoteAttendanceRepo.Add(saveRemoteAttendance);
                unitOfWork.CommitChangesWithAudit();

                var supervisorDetails = superviosr.IsNotNull() ? EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == superviosr.EmployeeSupervisorID) : null;
                if (supervisorDetails.IsNotNull())
                {
                    var mailData = new List<Dictionary<string, object>>();
                    var data = new Dictionary<string, object>
                {
                    { "SupervisorName", supervisorDetails.FullName },
                    { "EmployeeName", AppContexts.User.FullName+" - "+ AppContexts.User.EmployeeCode },
                    { "RequestType", "Check-"+remoteAttendanceDto.EntryType.ToLower() },
                    { "AttendanceDate", remoteAttendanceDto.AttendanceDate.ToString("dd/MM/yyyy hh:mm tt") }
                };
                    mailData.Add(data);

                    var toMail = new List<string> {
                    new string(supervisorDetails.WorkEmail)
                };
                    BasicMail((int)Util.MailGroupSetup.RemoteAttendacneCheckInOutRequest, toMail, false, null, null, mailData);
                }

            }
            return Task.FromResult(remoteAttendanceDto);
        }
        public Task<RemoteAttendanceDto> ApproverStatusChange(RemoteAttendanceDto remoteAttendanceDto)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                var existRemoteAttendance = RemoteAttendanceRepo.Entities.SingleOrDefault(x => x.RAID == remoteAttendanceDto.RAID).MapTo<RemoteAttendance>();
                existRemoteAttendance.StatusID = remoteAttendanceDto.StatusID;
                existRemoteAttendance.ApproverNote = remoteAttendanceDto.ApproverNote;
                existRemoteAttendance.ApprovalDate = DateTime.Now;
                existRemoteAttendance.SetModified();
                SetAuditFields(existRemoteAttendance);
                RemoteAttendanceRepo.Add(existRemoteAttendance);
                unitOfWork.CommitChangesWithAudit();


                if (remoteAttendanceDto.StatusID == ((int)Util.ApprovalStatus.Approved))
                {
                    //Call SP For Immediate Assign & Update Attendance Accordingly
                    string sql = $@"EXEC HRMS..AttendanceSummaryScheduleWithEmpIDAndDate '{existRemoteAttendance.AttendanceDate.ToString("yyyy-MM-dd")}',{existRemoteAttendance.EmployeeID}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var result = context.GetData(sql);
                }


                var employeeDetails = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == existRemoteAttendance.EmployeeID);
                var approver = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == existRemoteAttendance.ApproverID);

                var mailData = new List<Dictionary<string, object>>();
                var data = new Dictionary<string, object>
                {
                    { "EmployeeName", employeeDetails.FullName },
                    { "SupervisorName", approver.FullName??"" },
                    { "Status", Util.ToEnumString((Util.ApprovalStatus)( remoteAttendanceDto.StatusID))},
                    { "RequestType", "Check-"+existRemoteAttendance.EntryType.ToLower() }
                };
                mailData.Add(data);

                var toMail = new List<string> {
                    new string(employeeDetails.WorkEmail)
                };

                BasicMail((int)Util.MailGroupSetup.RemoteAttendacneAcceptOrRejectMail, toMail, false, null, null, mailData);
            }
            return Task.FromResult(remoteAttendanceDto);
        }
        public Task<RemoteAttendanceDto> SelectedApproverStatusChange(RemoteAttendanceDto remoteAttendanceDto)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                var existRemoteAttendance = RemoteAttendanceRepo.Entities.Where(x => remoteAttendanceDto.SelectedIds.Contains(x.RAID)).ToList().MapTo<List<RemoteAttendance>>();
                if (existRemoteAttendance.IsNotNull() && existRemoteAttendance.Count > 0)
                {
                    existRemoteAttendance.ForEach(y =>
                    {

                        y.StatusID = remoteAttendanceDto.StatusID;
                        y.ApproverNote = remoteAttendanceDto.ApproverNote;
                        y.ApprovalDate = DateTime.Now;
                        y.SetModified();
                        SetAuditFields(y);
                       
                    });

                    RemoteAttendanceRepo.AddRange(existRemoteAttendance);
                    unitOfWork.CommitChangesWithAudit();

                    //FOR ENSURE SUCCESSFULL UPDATE OF STATUS, MAIL HAS BEEN THROW AT LAST

                    existRemoteAttendance.ForEach(y =>
                    {

                        var employeeDetails = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == y.EmployeeID);
                        var approver = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == y.ApproverID);

                        var mailData = new List<Dictionary<string, object>>();
                        var data = new Dictionary<string, object>
                             {
                                 { "EmployeeName", employeeDetails.FullName },
                                 { "SupervisorName", approver.FullName??"" },
                                 { "Status", Util.ToEnumString((Util.ApprovalStatus)( remoteAttendanceDto.StatusID))},
                                 { "RequestType", "Check-"+y.EntryType.ToLower() }
                             };
                        mailData.Add(data);

                        var toMail = new List<string> {
                    new string(employeeDetails.WorkEmail)
                        };
                        BasicMail((int)Util.MailGroupSetup.RemoteAttendacneAcceptOrRejectMail, toMail, false, null, null, mailData);

                        if (remoteAttendanceDto.StatusID == ((int)Util.ApprovalStatus.Approved))
                        {
                            //Call SP For Immediate Assign & Update Attendance Accordingly
                            string sql = $@"EXEC HRMS..AttendanceSummaryScheduleWithEmpIDAndDate '{y.AttendanceDate.ToString("yyyy-MM-dd")}',{y.EmployeeID}";
                            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                            var result = context.GetData(sql);
                        }

                    });


                }
                return Task.FromResult(remoteAttendanceDto);
            }
        }
        private void SetNewRAID(RemoteAttendanceDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("RemoteAttendance", AppContexts.User.CompanyID);
            obj.RAID = code.MaxNumber;
        }

        public async Task<List<RemoteAttendanceDto>> GetPendingRemoteAttendanceList()
        {
            string sql = $@"SELECT 
	                            RA.*,
	                            Emp.EmployeeCode,
	                            Emp.FullName EmployeeName,
								AStatus.SystemVariableCode StatusName,
                                ImagePath EmployeeImagePath
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN Employee Emp ON RA.EmployeeID = Emp.EmployeeID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage PI ON PI.PersonID = Emp.PersonID AND PI.IsFavorite = 1
                            WHERE RA.ApproverID = {AppContexts.User.EmployeeID} AND AStatus.SystemVariableCode = 'Pending'
                            ORDER BY RA.RAID ASC
                            ";
            var list = RemoteAttendanceRepo.GetDataModelCollection<RemoteAttendanceDto>(sql);

            return await Task.FromResult(list);
        }
        public async Task<List<RemoteAttendanceDto>> GetPendingRemoteAttendanceListForDashboard()
        {
            string sql = $@"SELECT 
	                            RA.*,
	                            Emp.EmployeeCode,
	                            Emp.FullName EmployeeName,
								AStatus.SystemVariableCode StatusName,
                                ImagePath EmployeeImagePath
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN Employee Emp ON RA.EmployeeID = Emp.EmployeeID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage PI ON PI.PersonID = Emp.PersonID AND PI.IsFavorite = 1
                            WHERE AStatus.SystemVariableCode = 'Pending'
                            ORDER BY RA.RAID ASC
                            ";
            var list = RemoteAttendanceRepo.GetDataModelCollection<RemoteAttendanceDto>(sql);

            return await Task.FromResult(list);
        }
        public async Task<RemoteAttendanceDto> GetRemoteAttendanceDetails(RemoteAttendanceDto data)
        {
            string sql = $@"SELECT 
	                            RA.*,
	                            Emp.EmployeeCode,
	                            Emp.FullName EmployeeName,
								AStatus.SystemVariableCode StatusName,
                                ImagePath EmployeeImagePath,
                                D.DivisionName,
								Di.DistrictName,
								T.ThanaName
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN ViewALLEmployee Emp ON RA.EmployeeID = Emp.EmployeeID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Division D ON D.DivisionID = RA.DivisionID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..District Di ON Di.DistrictID = RA.DistrictID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Thana T ON T.ThanaID = RA.ThanaID
                                WHERE RA.RAID={data.RAID}";
            var row = RemoteAttendanceRepo.GetModelData<RemoteAttendanceDto>(sql);

            return await Task.FromResult(row);
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
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(RA.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND RA.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Rejected}";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
	                            RA.*,
	                            VA.EmployeeCode,
	                            VA.FullName EmployeeName,
                                VA.DepartmentName,
								AStatus.SystemVariableCode StatusName,
                                PI.ImagePath EmployeeImagePath,
								VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment,
								AP.EmployeeCode APEmployeeCode,
								AP.FullName APFullName,
								AP.EmployeeCode + AP.FullName ApproverDetails
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.EmployeeID = RA.EmployeeID
                                LEFT JOIN HRMS..ViewALLEmployeeRegularJoin AP ON AP.EmployeeID = RA.ApproverID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage PI ON PI.PersonID = VA.PersonID AND PI.IsFavorite = 1
                            WHERE RA.EmployeeID = {AppContexts.User.EmployeeID} {filter}";
                            //ORDER BY RA.RAID desc";
            var result = RemoteAttendanceRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<GridModel> GetListForGridAll(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(RA.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND RA.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND RA.StatusID = {(int)Util.ApprovalStatus.Rejected}";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
	                            RA.RAID,
	                            VA.EmployeeCode,
	                            VA.FullName EmployeeName,
                                VA.EmployeeCode + VA.FullName EmployeeDetails,
                                VA.DepartmentName,
								AStatus.SystemVariableCode StatusName,
								VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment,
								AP.EmployeeCode APEmployeeCode,
								AP.FullName APFullName,
								AP.EmployeeCode + AP.FullName ApproverDetails,
                                --ISNULL(RA.AttendanceDate,'') AS AttendanceDate,
                                ISNULL(FORMAT(CONVERT(DATETIME2, RA.AttendanceDate), 'ddd dd MMM yyyy hh:mm:ss tt'),'') AS AttendanceDate,
								isnull(RA.EntryType,'') EntryType,
								ISNULL(FORMAT(CONVERT(DATETIME2, RA.ApprovalDate), 'ddd dd MMM yyyy hh:mm:ss tt'),'') AS ApprovalDate,
								--ISNULL(FORMAT(CONVERT(DATETIME2, RA.CreatedDate), 'ddd dd MMM yyyy hh:mm:ss tt'),'') AS CreatedDate
                                RA.CreatedDate
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.EmployeeID = RA.EmployeeID
                                LEFT JOIN HRMS..ViewALLEmployeeRegularJoin AP ON AP.EmployeeID = RA.ApproverID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								--LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage PI ON PI.PersonID = VA.PersonID AND PI.IsFavorite = 1
                             WHERE CAST(RA.AttendanceDate as date) >= '2024-01-01' {filter}";
            //ORDER BY RA.RAID desc";
            var result = RemoteAttendanceRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetListRemoteAttendanceExcel(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" WHERE CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(RA.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" WHERE RA.StatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" WHERE RA.StatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" WHERE RA.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" WHERE RA.StatusID = {(int)Util.ApprovalStatus.Rejected}";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
	                            RA.*,
	                            VA.EmployeeCode,
	                            VA.FullName EmployeeName,
                                VA.EmployeeCode + VA.FullName EmployeeDetails,
                                VA.DepartmentName,
								AStatus.SystemVariableCode StatusName,
                                PI.ImagePath EmployeeImagePath,
								VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment,
								AP.EmployeeCode APEmployeeCode,
								AP.FullName APFullName,
								AP.EmployeeCode + AP.FullName ApproverDetails
                            FROM 
	                            RemoteAttendance RA
	                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.EmployeeID = RA.EmployeeID
                                LEFT JOIN HRMS..ViewALLEmployeeRegularJoin AP ON AP.EmployeeID = RA.ApproverID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable AStatus ON AStatus.SystemVariableID = RA.StatusID
								LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..PersonImage PI ON PI.PersonID = VA.PersonID AND PI.IsFavorite = 1
                             {filter}";
            var list = RemoteAttendanceRepo.GetDataDictCollection(sql);

            return await Task.FromResult(list);
        }

    }
}
