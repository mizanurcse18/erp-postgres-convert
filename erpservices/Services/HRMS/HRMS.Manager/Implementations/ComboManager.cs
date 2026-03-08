using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class ComboManager : IComboManager
    {

        private readonly IRepository<Department> DepartmentRepo;
        private readonly IRepository<RenovationORMaintenanceCategory> RenovationRepo;
        private readonly IRepository<Designation> DesignationRepo;
        private readonly IRepository<Division> DivisionRepo;
        private readonly IRepository<Cluster> ClusterRepo;
        private readonly IRepository<BranchInfo> BranchinfoRepo;
        private readonly IRepository<Region> RegionRepo;
        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<EmployeeLeaveApplication> EmployeeLeaveApplicationRepo;
        private readonly IRepository<JobGrade> JobGradeRepo;
        private readonly IRepository<AuditQuestion> AuditQuesRepo;
        private readonly IRepository<UserWiseUddoktaOrMerchantMapping> UserUddoktaMerchantRepo;

        public ComboManager(IRepository<Department> deptRepo, IRepository<RenovationORMaintenanceCategory> renovationRepo,
            IRepository<Designation> desgRepo, IRepository<Division> divisionRepo,
            IRepository<Cluster> clusterRepo, IRepository<BranchInfo> branchinfoRepo, IRepository<Region> regionRepo, IRepository<Employee> employeeRepo,
            IRepository<EmployeeLeaveApplication> employeeLeaveApplicationRepo, IRepository<JobGrade> jobGradeRepo,
            IRepository<AuditQuestion> auditQuesRepo, IRepository<UserWiseUddoktaOrMerchantMapping> userUddoktaMerchantRepo)
        {

            DepartmentRepo = deptRepo;
            RenovationRepo = renovationRepo;
            DesignationRepo = desgRepo;
            DivisionRepo = divisionRepo;
            ClusterRepo = clusterRepo;
            BranchinfoRepo = branchinfoRepo;
            RegionRepo = regionRepo;
            EmployeeRepo = employeeRepo;
            EmployeeLeaveApplicationRepo = employeeLeaveApplicationRepo;
            JobGradeRepo = jobGradeRepo;
            AuditQuesRepo = auditQuesRepo;
            UserUddoktaMerchantRepo = userUddoktaMerchantRepo;
        }

        public async Task<List<ComboModel>> GetBranchinfos(int RegionID)
        {
            var branchList = new List<BranchInfo>();
            if (RegionID != 0)
            {
                branchList = await BranchinfoRepo.GetAllListAsync(x => x.RegionID == RegionID);
            }
            else branchList = await BranchinfoRepo.GetAllListAsync();
            return branchList.Select(x => new ComboModel { value = x.BranchID, label = x.BranchName }).ToList();
        }

        public async Task<List<ComboModel>> GetClusters()
        {
            var clusterList = await ClusterRepo.GetAllListAsync();
            return clusterList.Select(x => new ComboModel { value = x.ClusterID, label = x.ClusterName }).ToList();
        }

        public async Task<List<ComboModel>> GetDepartmentsCascade(int DivisionID)
        {
            var departmentList = await DepartmentRepo.GetAllListAsync(x => x.DivisionID == DivisionID.ToString());
            return departmentList.Select(x => new ComboModel { value = x.DepartmentID, label = x.DepartmentName }).ToList();
        }
        public async Task<List<ComboModel>> GetDepartmentsCascadeByDivisionIDs(string DivisionIDs)
        {

            string sql = @$"SELECT DepartmentID AS value,DepartmentName  AS label, DivisionID AS extraJsonProps FROM Department WHERE DivisionID IN({DivisionIDs})";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);

        }
        public async Task<List<ComboModel>> GetDepartments()
        {
            var departmentList = await DepartmentRepo.GetAllListAsync();
            return departmentList.Select(x => new ComboModel { value = x.DepartmentID, label = x.DepartmentName }).ToList();
        }

        public async Task<List<ComboModel>> GetAuditQuestions()
        {
            var questionList = await AuditQuesRepo.GetAllListAsync(x => x.IsActive == true);
            return questionList.Select(x => new ComboModel { value = x.QuestionID, label = x.Title }).ToList();
        }
        public async Task<List<ComboModel>> GetAllRenovationORMaintenanceCategory()
        {
            var departmentList = await RenovationRepo.GetAllListAsync();
            return departmentList.Select(x => new ComboModel { value = x.ROMID, label = x.RenovationName }).ToList();
        }

        public async Task<List<ComboModel>> GetDesignations()
        {
            var desgList = await DesignationRepo.GetAllListAsync();
            return desgList.Select(x => new ComboModel { value = x.DesignationID, label = x.DesignationName }).ToList();
        }

        public async Task<List<ComboModel>> GetDivisions()
        {
            var divisionList = await DivisionRepo.GetAllListAsync();
            return divisionList.Select(x => new ComboModel { value = x.DivisionID, label = x.DivisionName }).ToList();
        }
        //public async Task<List<ComboModel>> GetLeaveTypes()
        //{
        //    var divisionList = await DivisionRepo.GetAllListAsync();
        //    return divisionList.Select(x => new ComboModel { value = x.DivisionID, label = x.DivisionName }).ToList();
        //}
        public async Task<List<ComboModel>> GetLeaveTypes()
        {

            string sql = @$"select SystemVariableID value,SystemVariableCode label from Security..SystemVariable where EntityTypeID=10";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetRegionsForBranch()
        {
            var regionList = await RegionRepo.GetAllListAsync();
            return regionList.Select(x => new ComboModel { value = x.RegionID, label = x.RegionName }).ToList();
        }

        public async Task<List<ComboModel>> GetRegions(int ClusterID)
        {
            var regionList = await RegionRepo.GetAllListAsync(x => x.ClusterID == ClusterID);
            return regionList.Select(x => new ComboModel { value = x.RegionID, label = x.RegionName }).ToList();
        }
        /// <summary>
        public async Task<IEnumerable<Dictionary<string, object>>> GetEmployeeDetailsInfo()
        {
            string sql = @$"SELECT * FROM HRMS..viewActiveEmployee where EmployeeID='{AppContexts.User.EmployeeID}'";
            var listDict = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboModel>> GetEmployeePersons()
        {

            string sql = @$"SELECT * FROM ViewPersonCombo WHERE CompanyID='{AppContexts.User.CompanyID}'";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetDaysOfWeeks()
        {
            var weekdays = Enum.GetValues(typeof(DayOfWeek))
               .Cast<DayOfWeek>()
               .Select(t => new ComboModel
               {
                   value = ((int)t),
                   label = t.ToString()
               }).ToList();
            return weekdays;
        }
        public async Task<List<ComboModel>> GetEmployeesOnlySCM(int DivisionID)
        {
            int divisionid = DivisionID.IsZero() ? (int)AppContexts.User.DivisionID : DivisionID;
            string sql = @$"SELECT EmployeeID AS value,CONCAT(EmployeeCode,'-', FullName)  AS label  FROM ViewALLEmployee WHERE DivisionID IN({divisionid})";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetEmployees()
        {
            var employeeList = await EmployeeRepo.GetAllListAsync();
            return employeeList.Select(x => new ComboModel { value = x.EmployeeID, label = x.EmployeeCode + "-" + x.FullName }).ToList();
        }

        public async Task<List<ComboModel>> GetDivisionHead()
        {
            string sql = @$"select dh.EmployeeID AS value,CONCAT(e.EmployeeCode,'-', e.FullName)  AS label from DivisionHeadMap dh JOIN Employee e on dh.EmployeeID=e.EmployeeID";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetBackupEmployees()
        {
            string sql = @$"SELECT EmployeeID AS value,CONCAT(EmployeeCode,'-', FullName)  AS label  FROM viewActiveEmployee WHERE DepartmentID = '{AppContexts.User.DepartmentID}' AND EmployeeID <> {AppContexts.User.EmployeeID}";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetEmployeesByDepartment(int DepartmentID)
        {
            string sql = @$"SELECT EmployeeID AS value,CONCAT(EmployeeCode,'-', FullName)  AS label  FROM ViewALLEmployee WHERE DepartmentID='{DepartmentID}' AND 
                            ( ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date))";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetActiveEmployeeList()
        {
            string sql = @$"SELECT 
                                EmployeeID  value,
                                CONCAT(EmployeeCode,'-', FullName) label  
                            FROM 
                                ViewALLEmployee 
                            WHERE ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date)";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetActiveBackUpEmployeeList()
        {
            string sql = @$"SELECT 
                                EmployeeID as value,
                                CONCAT(EmployeeCode, '-', FullName) as label
                            FROM 
                                ViewEmployeeSupervisorMap s
                            WHERE 
                                (s.EmployeeSupervisorID = {AppContexts.User.EmployeeID}) 
                                AND (ISNULL(s.EmployeeTypeID, 0) <> 48 OR CAST(s.DiscontinueDate AS Date) >= CAST(GETDATE() AS Date))

                            UNION

                            SELECT 
                                EmployeeSupervisorID as EmployeeID,
                                CONCAT(SupervisorEmployeeCode, '-', SupervisorFullName) as label
                            FROM 
                                viewActiveEmployee Emp
                            LEFT JOIN 
                                ViewEmployeeSupervisorMap SuperEmp 
                                ON SuperEmp.EmployeeID = Emp.EmployeeID
                            WHERE 
                                Emp.EmployeeID = {AppContexts.User.EmployeeID}";

            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetActiveBackUpEmployeeListForHr(int EmployeeID)
        {
            string sql = @$"SELECT 
                                EmployeeID as value,
                                CONCAT(EmployeeCode, '-', FullName) as label
                            FROM 
                                ViewEmployeeSupervisorMap s
                            WHERE 
                                (s.EmployeeSupervisorID = {AppContexts.User.EmployeeID}) 
                                AND (ISNULL(s.EmployeeTypeID, 0) <> 48 OR CAST(s.DiscontinueDate AS Date) >= CAST(GETDATE() AS Date))

                            UNION

                            SELECT 
                                EmployeeSupervisorID as EmployeeID,
                                CONCAT(SupervisorEmployeeCode, '-', SupervisorFullName) as label
                            FROM 
                                viewActiveEmployee Emp
                            LEFT JOIN 
                                ViewEmployeeSupervisorMap SuperEmp 
                                ON SuperEmp.EmployeeID = Emp.EmployeeID
                            WHERE 
                                Emp.EmployeeID = {EmployeeID}";

            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetActiveEmployeeListByDeptAndStatus(string deptIds, string typeIds, DateTime cutoffdate, int finYearid)
        {
            string condition = "";
            if (typeIds != "0")
            {
                condition = @$" AND EmployeeTypeID IN({typeIds})";
            }
            //DateTime cutDate = DateTime.ParseExact(cutoffdate, "yyyy-MM-DD", CultureInfo.InvariantCulture);
            string cutCondition = "DATEDIFF(DAY,cast(DateOfJoining as date) , cast(getdate() as date)) >= 365 AND";
            if (cutoffdate.IsNotMinValue() || cutoffdate.IsNotNull())
            {
                cutCondition = $@"DATEDIFF(DAY,cast(DateOfJoining as date) ,cast('{cutoffdate.ToString("yyyy-MM-dd")}' as date) ) >= 365 AND";
            }
            string sql = @$"SELECT  --ALEM.EmployeeID ALEMEmployeeID,
                                VE.EmployeeID  value,
                                CONCAT(EmployeeCode,'-', FullName) label ,
                                DivisionName+'~'+DepartmentName+'~'+DesignationName+'~'+convert(varchar(512),DivisionID)+'~'+convert(varchar(512),DepartmentID)+'~'+convert(varchar(512),DesignationID)+'~'+convert(varchar(512),EmployeeTypeID) extraJsonProps,
								DATEDIFF(DAY,cast(DateOfJoining as date) ,cast('2022-12-08' as date) ) Age
                            FROM 
                                ViewALLEmployee  VE
								LEFT JOIN (
									SELECT EmployeeID FROM AnnualLeaveEncashmentWindowChild C
									INNER JOIN AnnualLeaveEncashmentWindowMaster M ON C.ALEWMasterID = M.ALEWMasterID AND M.FinancialYearID = {finYearid} AND Status <> 179								
								) WC ON VE.EmployeeID = WC.EmployeeID
								LEFT JOIN (
									SELECT EmployeeID FROM AnnualLeaveEncashmentMaster am
									INNER JOIN AnnualLeaveEncashmentWindowMaster M  ON am.ALEWMasterID = M.ALEWMasterID AND M.FinancialYearID={finYearid}
									WHERE am.ApprovalStatusID <> 24
								) ALEM ON VE.EmployeeID = ALEM.EmployeeID -- Not Applied or rejected
                            WHERE (WC.EmployeeID IS NULL AND ALEM.EmployeeID IS NULL)  AND {cutCondition} (ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date))
                            AND DepartmentID IN({deptIds}) {condition}";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetActiveEmployeesByDepartment(int DepartmentID)
        {
            string sql = @$"SELECT EmployeeID AS value,CONCAT(EmployeeCode,'-', FullName)  AS label  FROM ViewALLEmployee WHERE DepartmentID='{DepartmentID}' AND (ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date))";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetPendingEmployees()
        {
            string sql = @$"SELECT 
	                        AEF.EmployeeID value,
	                        Emp.EmployeeCode+' - '+EMp.FullName label
                        FROM 
	                        Approval..ApprovalEmployeeFeedback AEF
	                        INNER JOIN hrms..ViewALLEmployee Emp on Emp.EmployeeID = AEF.EmployeeID
	                        where APFeedbackID = 2
                        GROUP BY AEF.EmployeeID,EmployeeCode,FullName";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetEmployeesWithDivDept()
        {
            string sql = @$"
                            SELECT 
                                EmployeeID  value,
                                CONCAT(EmployeeCode,'-', FullName) label ,
                            ISNULL(convert(varchar(50),DivisionID),'0')+'-'+ ISNULL(convert(varchar(50),DepartmentID),'0') extraJsonProps
                        FROM 
                                ViewALLEmployee 
                            WHERE ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date)";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }
        //public async Task<List<ComboModel>> GetExitInterviewEmployeesWithDivDept()
        //{
        //    string sql = @$"
        //                    SELECT 
        //                        EIN.EEIID  value,
        //                        CONCAT(emp.EmployeeCode,'-', emp.FullName) label ,
        //                    ISNULL(convert(varchar(50),emp.DivisionID),'0')+'-'+ ISNULL(convert(varchar(50),emp.DepartmentID),'0') extraJsonProps

        //                FROM 
        //                        EmployeeExitInterview EIN
        //                        JOIN ViewALLEmployee emp on EIN.EmployeeID=emp.PersonID 
        //                    WHERE ISNULL(emp.EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(emp.DiscontinueDate as Date) >= CAST(getdate() as date)";
        //    return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        //}
        public async Task<List<ComboModel>> GetJobGradeList()
        {
            var jobGradeList = await JobGradeRepo.GetAllListAsync();
            return jobGradeList.Select(x => new ComboModel { value = x.JobGradeID, label = x.JobGradeName }).ToList();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetEmployeeComboList()
        {
            string sql = @$"SELECT
                            E.EmployeeID,EmployeeCode,FullName,WorkEmail,WorkMobile,DivisionName ,DepartmentName
                            ,EmpStats.SystemVariableCode EmployeeStatus,Emp.EmployeeTypeID
                        FROM
                        Employee E
                        INNER JOIN Employment Emp ON emp.EmployeeID = E.EmployeeID AND IsCurrent = 1
                        INNER JOIN Division D on D.DivisionID = Emp.DivisionID
                        INNER JOIN Department Dept on Dept.DepartmentID = Emp.DepartmentID
                        LEFT JOIN Security..SystemVariable EmpStats ON EmpStats.SystemVariableID = Emp.EmployeeTypeID
                        LEFT JOIN EmployeeExitInterview EIN on EIN.EmployeeID = emp.EmployeeID  AND EIN.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND EIN.EEIID IS NULL";
            var listDict = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetExitInterviewEmployeesWithDivDept()
        {
            string sql = @$"SELECT 
                                EIN.EEIID  value,
                                CONCAT(emp.EmployeeCode,'-', emp.FullName) label ,
                                ISNULL(convert(varchar(50),emp.DivisionID),'0')+'-'+ ISNULL(convert(varchar(50),emp.DepartmentID),'0') extraJsonProps
                                ,EIN.EmployeeID
							    ,emp.FullName
							    ,emp.EmployeeCode
							    ,emp.WorkEmail
							    ,emp.WorkMobile
							    ,emp.DivisionName
							    ,emp.DepartmentName
                                FROM 
                                EmployeeExitInterview EIN
                                JOIN ViewALLEmployee emp on EIN.EmployeeID=emp.PersonID
                                LEFT JOIN EmployeeAccessDeactivation ead on ead.EmployeeID=EIN.EmployeeID
                                where ead.EADID is null or ead.ApprovalStatusID=24";
            var listDict = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }


        //GetAllEmployeesForAccessDeactivation

        public async Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForAccessDeactivation()
        {
            string sql = @$"SELECT 
                                E.EmployeeID,EmployeeCode,CONCAT(EmployeeCode,'-',FullName) FullName,WorkEmail,WorkMobile,DivisionName ,DepartmentName
                            ,EmpStats.SystemVariableCode EmployeeStatus,Emp.EmployeeTypeID
                        FROM
                        Employee E
                        INNER JOIN Employment Emp ON emp.EmployeeID = E.EmployeeID AND IsCurrent = 1
                        INNER JOIN Division D on D.DivisionID = Emp.DivisionID
                        INNER JOIN Department Dept on Dept.DepartmentID = Emp.DepartmentID
                        LEFT JOIN Security..SystemVariable EmpStats ON EmpStats.SystemVariableID = Emp.EmployeeTypeID
                        LEFT JOIN EmployeeAccessDeactivation EAD on EAD.EmployeeID = Emp.EmployeeID  AND EAD.ApprovalStatusID NOT IN ({(int)Util.ApprovalStatus.Initiated},{(int)Util.ApprovalStatus.Rejected})
                        WHERE Emp.EmployeeTypeID NOT IN({(int)Util.EmployeeType.Discontinued},{(int)Util.EmployeeType.Terminated}) AND EAD.EADID IS NULL";
            var listDict = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        //public async Task<IEnumerable<Dictionary<string, object>>> GetAllSupportRequestType()
        //{
        //    string sql = @$"select * from SystemVariable where EntityTypeID=46";
        //    var listDict = EmployeeRepo.GetDataDictCollection(sql);
        //    return await Task.FromResult(listDict);
        //}
        public async Task<IEnumerable<Dictionary<string, object>>> GetFinancialAndAssessmentYear()
        {
            string sql = @"DECLARE @FinancialYear as INT
            SET @FinancialYear = ((Select year from Security.dbo.FinancialYear Fin where IsCurrent =1) -1)

            SELECT 
                        	 Fin.FinancialYearID AssessmentYearID
                        	,Fin.Year AssessmentYear
                                     ,Fin.YearDescription AssessmentYearDescription
            	,ass.FinancialYearID
            	,@FinancialYear FinancialYear
            	,ass.YearDescription FinancialYearDescription

                              FROM 
                                  Security.dbo.FinancialYear Fin
            	join Security.dbo.FinancialYear ass ON ass.year = @FinancialYear 
                                     where Fin.IsCurrent =1
                              ";

            //     string sql = @"DECLARE @FinancialYear as INT
            //SET @FinancialYear = ((Select year from Security.dbo.FinancialYear Fin where IsCurrent =1) -2)

            //SELECT 
            //            	 Fin.FinancialYearID AssessmentYearID
            //            	,Fin.Year AssessmentYear
            //                         ,Fin.YearDescription AssessmentYearDescription
            //	,ass.FinancialYearID
            //	,@FinancialYear FinancialYear
            //	,ass.YearDescription FinancialYearDescription

            //                  FROM 
            //                      Security.dbo.FinancialYear Fin
            //	join Security.dbo.FinancialYear ass ON ass.year = @FinancialYear
            //	where fin.Year=@FinancialYear + 1
            //                  ";

            return await Task.Run(() => EmployeeRepo.GetDataDictCollection(sql));
        }

        public async Task<List<ComboModel>> GetEmployeesReligion(int EmployeeID)
        {
            string sql = @$"SELECT ReligionID AS value, ReligionName AS label  FROM ViewALLEmployee WHERE EmployeeID='{EmployeeID}'";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetDivisionsForWallet(int CWID)
        {
            //var divisionList = await DivisionRepo.GetAllListAsync();

            string sql = @$"WITH DivisionsCTE AS (
                            SELECT DivisionIDs
                            FROM Accounts..CustodianWallet WHERE CWID <> {CWID}
                        )
                        SELECT *
                        FROM HRMS..Division
                        WHERE DivisionID NOT IN (
                            SELECT value
                            FROM DivisionsCTE
                            CROSS APPLY STRING_SPLIT(DivisionIDs, ',')
                            WHERE value IS NOT NULL
                        )";
            var divisionList = DivisionRepo.GetDataDictCollection(sql);
            //return divisionList.Select(x => new ComboModel { value = x.DivisionID, label = x.DivisionName }).ToList();

            return divisionList.Select(x => new ComboModel { value = Convert.ToInt32(x["DivisionID"]), label = x["DivisionName"] as string }).ToList();
        }

        public async Task<List<ComboModel>> GetUserWiseUddoktaMerchant()
        {
            var mappingList = await UserUddoktaMerchantRepo.GetAllListAsync(x => x.CreatedBy == AppContexts.User.UserID && x.IsActive == true && x.IsTagged == true);
            return mappingList.Select(x => new ComboModel { value = x.MAPID, label = (x.TypeID == (int)Util.ExternalAuditWalletType.UDDOKTA ? "Uddokta-" : "Merchant-") + x.WalletName + " # " + x.WalletNumber }).ToList();
        }


        public async Task<List<ComboModel>> GetEmployeesFilteringCascading(int ActiveInActiveID, int DivisionID, int DepartmentID)
        {
            string filter = "";
            filter = ActiveInActiveID == 1 ? $@" AND 1=1" : (ActiveInActiveID == 2 ? $@"AND EmployeeTypeID IN(19,20,21,49,70)" : $@"AND EmployeeTypeID=48");
            filter += DivisionID > 0 ? $@"AND DivisionID ='{DivisionID}'" : "";
            filter += DepartmentID > 0 ? $@"AND DepartmentID ='{DepartmentID}'" : "";

            string sql = @$"SELECT EmployeeID AS value,CONCAT(EmployeeCode,'-', FullName)  AS label  FROM ViewALLEmployee WHERE 1=1 {filter}";
            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetEmployeeBirthDate()
        {
            string sql = @$"SELECT CONVERT(VARCHAR(10), DateOfBirth, 120) AS label,
                           EmployeeID AS value
                    FROM HRMS..viewActiveEmployee
                    WHERE EmployeeID = '{AppContexts.User.EmployeeID}'";

            return EmployeeRepo.GetDataModelCollection<ComboModel>(sql);
        }




    }
}
