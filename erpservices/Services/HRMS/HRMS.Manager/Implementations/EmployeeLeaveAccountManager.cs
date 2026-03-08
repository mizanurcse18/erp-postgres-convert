using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Manager.Core;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Manager
{
    public class EmployeeLeaveAccountManager : ManagerBase, IEmployeeLeaveAccountManager
    {
        private readonly IRepository<EmployeeLeaveAccount> EmployeeLeaveAccountRepo;
        private readonly IRepository<Employment> EmployeeRepo;
        private readonly IRepository<CompanyLeavePolicy> LeavePolicyRepo;
        //readonly IModelAdapter Adapter;
        public EmployeeLeaveAccountManager(IRepository<EmployeeLeaveAccount> clPolicyRepo, IRepository<Employment> employeeRepo, IRepository<CompanyLeavePolicy> leavePolicyRepo
            )
        {
            EmployeeLeaveAccountRepo = clPolicyRepo;
            EmployeeRepo = employeeRepo;
            LeavePolicyRepo = leavePolicyRepo;
        }

        public async Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccountTables()
        {
            var elAccounts = await EmployeeLeaveAccountRepo.GetAllListAsync(x => x.CompanyID == AppContexts.User.CompanyID.ToString());
            return elAccounts.MapTo<List<EmployeeLeaveAccountDto>>();
        }

        public async Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccount(int finYearID, int empID)
        {
       //     string sql = $@"SELECT ELA.ELAID, ELA.FinancialYearID, ELA.EmployeeID, ELA.LeaveCategoryID, ELA.LeaveDays, ELA.Remarks,ELA.CompanyID , SV.SystemVariableCode AS LeaveCategoryName,FY.Year AS Year, Emp.FullName AS EmployeeName, 1 AS IsExists
       //                     ,Emp.FullName AS EmployeeName, Emp.DateOfJoining, SVE.SystemVariableCode AS EmployeeStatusName                              
       //                     FROM EmployeeLeaveAccount ELA
       //                     LEFT JOIN Security..SystemVariable SV ON ELA.LeaveCategoryID = SV.SystemVariableID
							//LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = ELA.FinancialYearID
       //                     LEFT JOIN Employee Emp ON ELA.EmployeeID = Emp.EmployeeID
       //                     LEFT JOIN Employment Et ON Emp.EmployeeID = Et.EmployeeID And Et.IsCurrent=1
       //                     LEFT JOIN Security..SystemVariable SVE ON Et.EmployeeTypeID = SVE.SystemVariableID
       //                     WHERE ELA.FinancialYearID={finYearID} AND ELA.EmployeeID={empID}";
            string sql = $@"SELECT 
	                        ISNULL(ELA.ELAID,ROW_NUMBER()OVER(ORDER BY SVE.SystemVariableID)) ELAID
	                        ,FY.FinancialYearID
	                        ,Emp.EmployeeID
	                        ,EMP.FullName AS EmployeeName
	                        ,SV.SystemVariableID LeaveCategoryID
	                        ,ISNULL(ELA.LeaveDays,CLP.LeaveInDays) AS LeaveDays
	                        ,ELA.Remarks AS Remarks
	                        ,ISNULL(ELA.CompanyID,'{AppContexts.User.CompanyID}') CompanyID
	                        ,SV.SystemVariableCode AS LeaveCategoryName
	                        ,FY.Year AS Year
	                        ,Emp.FullName AS EmployeeName
	                        ,Emp.DateOfJoining
	                        ,SVE.SystemVariableCode AS EmployeeStatusName
                        FROM Security..SystemVariable SV
                        LEFT JOIN EmployeeLeaveAccount ELA ON ELA.LeaveCategoryID = SV.SystemVariableID AND ELA.FinancialYearID = {finYearID} AND ELA.EmployeeID = {empID} 
                        LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = {finYearID}
                        LEFT JOIN HRMS..viewActiveEmployee Emp ON Emp.EmployeeID = {empID}
                        LEFT JOIN Security..SystemVariable SVE ON Emp.EmployeeTypeID = SVE.SystemVariableID
                        LEFT JOIN CompanyLeavePolicy CLP ON CLP.LeaveCategoryID = SV.SystemVariableID AND CLP.EmployeeStatusID = Emp.EmployeeTypeID AND CLP.FinancialYearID = {empID}
                        WHERE SV.EntityTypeName IN('LeaveCategory') ";

            var accountList = new List<EmployeeLeaveAccountDto>();
            accountList = EmployeeLeaveAccountRepo.GetDataModelCollection<EmployeeLeaveAccountDto>(sql);


            await Task.CompletedTask;
            return accountList;
        }


        public async Task<EmployeeLeaveAccountDto> SaveChanges(EmployeeLeaveAccountDto master, List<EmployeeLeaveAccountDto> childs = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<EmployeeLeaveAccountDto>();
                
                foreach (var child in childs)
                {
                    var existsPolicy = EmployeeLeaveAccountRepo.Entities.SingleOrDefault(x => x.FinancialYearID == child.FinancialYearID && x.EmployeeID == child.EmployeeID && x.LeaveCategoryID == child.LeaveCategoryID).MapTo<EmployeeLeaveAccount>();
                    if (existsPolicy.IsNull())
                    {
                        child.SetAdded();
                        child.FinancialYearID = master.FinancialYearID;
                        child.EmployeeID = master.EmployeeID;
                        SetNewId(child);
                    }
                    else
                    {
                        child.SetModified();
                        child.PreviousLeaveDays = existsPolicy.PreviousLeaveDays;
                        child.RowVersion = existsPolicy.RowVersion;
                        child.CreatedBy = existsPolicy.CreatedBy;
                        child.CreatedDate = existsPolicy.CreatedDate;
                        child.CreatedIP = existsPolicy.CreatedIP;
                    }

                }
                var childsEnt = childs.MapTo<List<EmployeeLeaveAccount>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                EmployeeLeaveAccountRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();
                UpdateLeaveBalanceAfterSubmit(master.EmployeeID);

            }
            await Task.CompletedTask;

            return master;
        }

        public async Task RemoveEmployeeLeaveAccount(int FinancialYearID, int EmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var childEnt = EmployeeLeaveAccountRepo.Entities.Where(x => x.FinancialYearID == FinancialYearID && x.EmployeeID == EmployeeID).ToList();
                childEnt.ChangeState(ModelState.Deleted);

                EmployeeLeaveAccountRepo.AddRange(childEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task AssignLeaveToAllEmployee(int FinancialYearID)
        {
            string sql = $@"EXEC AssignLeaveToAllEmployee {FinancialYearID},'{AppContexts.User.CompanyID}', {AppContexts.User.UserID}, '{AppContexts.User.IPAddress}'";
            EmployeeLeaveAccountRepo.ExecuteSqlCommand(sql);

            await Task.CompletedTask;
        }
        
        private void SetNewId(EmployeeLeaveAccountDto clPolicyTable)
        {
            if (!clPolicyTable.IsAdded) return;
            var code = GenerateSystemCode("EmployeeLeaveAccount", AppContexts.User.CompanyID);
            clPolicyTable.ELAID = code.MaxNumber;

        }

        public async Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccountListWithDetails()
        {
            var policyList = new List<EmployeeLeaveAccountDto>();
            var sql = $@"SELECT 
                            ROW_NUMBER() OVER (
	                            ORDER BY ELAID
                               ) row_num,
                            ELA.ELAID
	                        ,ELA.FinancialYearID
	                        ,ELA.EmployeeID
	                        ,ELA.LeaveCategoryID
	                        ,ELA.LeaveDays
	                        ,ELA.Remarks
	                        ,ELA.CompanyID
	                        ,SV.SystemVariableCode AS LeaveCategoryName
	                        ,FY.Year AS Year
	                        ,EMP.FullName AS EmployeeName
	                        ,1 AS IsExists
	                        ,Emp.FullName AS EmployeeName
	                        ,Emp.EmployeeCode
	                        ,Emp.DateOfJoining
	                        ,SVE.SystemVariableCode AS EmployeeStatusName,
	                        ImagePath ProfilePicture
                        FROM EmployeeLeaveAccount ELA
                        LEFT JOIN Security..SystemVariable SV ON ELA.LeaveCategoryID = SV.SystemVariableID
                        LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = ELA.FinancialYearID
                        INNER JOIN HRms..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
                        LEFT JOIN Security..SystemVariable SVE ON Emp.EmployeeTypeID = SVE.SystemVariableID
                        WHERE ELA.CompanyID = '{AppContexts.User.CompanyID}'";

            //GROUP BY FY.Year,SVE.SystemVariableCode, CLP.FinancialYearID, CLP.EmployeeStatusID";

            //var listDict = EmployeeLeaveAccountRepo.GetDataDictCollection(sql);

            policyList = EmployeeLeaveAccountRepo.GetDataModelCollection<EmployeeLeaveAccountDto>(sql);
            var customList = policyList.GroupBy(c => new { c.Year, c.EmployeeName, c.EmployeeCode, c.FinancialYearID, c.EmployeeID })
                .Select(chld => new EmployeeLeaveAccountDto()
                {
                    Year = chld.Key.Year,
                    EmployeeName = chld.Key.EmployeeName,
                    EmployeeCode = chld.Key.EmployeeCode,
                    FinancialYearID = chld.Key.FinancialYearID,
                    EmployeeID = chld.Key.EmployeeID,
                    row_num = chld.First().row_num,
                    EmployeeLeaveAccountList = chld.ToList()
                }).ToList();

            await Task.CompletedTask;
            return customList;
        }

        public async Task<List<EmployeeLeaveAccountDto>> GetGenerateChildList(EmployeeLeaveAccountDto elAccount)
        {
            var elaList = new List<EmployeeLeaveAccountDto>();
            var employee = new Employment();
            if (elAccount.EmployeeID > 0)
            {
                employee = EmployeeRepo.Entities.Where(x => x.EmployeeID == elAccount.EmployeeID && x.IsCurrent == true).FirstOrDefault().MapTo<Employment>();
            }
            int empStatusID = employee.IsNotNull() && employee.EmployeeTypeID.IsNotNull() ? employee.EmployeeTypeID.Value : 0;
            string sql = "";
            if (elAccount.IsExistsPolicy && !elAccount.IsExists)
            { 
                sql = $@"SELECT ROW_NUMBER() OVER (
	                        ORDER BY CLP.CLPolicyID
                           )  ELAID, CLP.FinancialYearID, CLP.LeaveCategoryID, CLP.LeaveInDays AS LeaveDays, '' AS Remarks,CLP.CompanyID , SV.SystemVariableCode AS LeaveCategoryName,FY.Year AS Year
                        FROM CompanyLeavePolicy CLP
                        LEFT JOIN Security..SystemVariable SV ON CLP.LeaveCategoryID = SV.SystemVariableID
					    LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = CLP.FinancialYearID
					    WHERE CLP.FinancialYearID={elAccount.FinancialYearID} AND CLP.EmployeeStatusID = {empStatusID}";
            } 
            else
            {
                //       sql = $@"SELECT ELA.ELAID, ELA.FinancialYearID, ELA.EmployeeID, EMP.FullName AS EmployeeName, ELA.LeaveCategoryID, ELA.LeaveDays AS LeaveDays, ELA.Remarks AS Remarks,ELA.CompanyID , SV.SystemVariableCode AS LeaveCategoryName,FY.Year AS Year
                //               ,Emp.FullName AS EmployeeName, Emp.DateOfJoining, SVE.SystemVariableCode AS EmployeeStatusName      
                //               FROM EmployeeLeaveAccount ELA
                //               LEFT JOIN Security..SystemVariable SV ON ELA.LeaveCategoryID = SV.SystemVariableID
                //LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = ELA.FinancialYearID
                //               LEFT JOIN Employee Emp ON ELA.EmployeeID = Emp.EmployeeID
                //               LEFT JOIN Employment Et ON Emp.EmployeeID = Et.EmployeeID And Et.IsCurrent=1
                //               LEFT JOIN Security..SystemVariable SVE ON Et.EmployeeTypeID = SVE.SystemVariableID
                //WHERE ELA.FinancialYearID={elAccount.FinancialYearID} AND ELA.EmployeeID = {elAccount.EmployeeID}";
                sql = $@"SELECT 
	                        ISNULL(ELA.ELAID,ROW_NUMBER()OVER(ORDER BY SVE.SystemVariableID)) ELAID
	                        ,FY.FinancialYearID
	                        ,Emp.EmployeeID
	                        ,EMP.FullName AS EmployeeName
	                        ,SV.SystemVariableID LeaveCategoryID
	                        ,ISNULL(ELA.LeaveDays,CLP.LeaveInDays) AS LeaveDays
	                        ,ELA.Remarks AS Remarks
	                        ,ISNULL(ELA.CompanyID,'{AppContexts.User.CompanyID}') CompanyID
	                        ,SV.SystemVariableCode AS LeaveCategoryName
	                        ,FY.Year AS Year
	                        ,Emp.FullName AS EmployeeName
	                        ,Emp.DateOfJoining
	                        ,SVE.SystemVariableCode AS EmployeeStatusName
                        FROM Security..SystemVariable SV
                        LEFT JOIN EmployeeLeaveAccount ELA ON ELA.LeaveCategoryID = SV.SystemVariableID AND ELA.FinancialYearID = {elAccount.FinancialYearID} AND ELA.EmployeeID = {AppContexts.User.EmployeeID} 
                        LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = {elAccount.FinancialYearID}
                        LEFT JOIN HRMS..viewActiveEmployee Emp ON Emp.EmployeeID = {AppContexts.User.EmployeeID}
                        LEFT JOIN Security..SystemVariable SVE ON Emp.EmployeeTypeID = SVE.SystemVariableID
                        LEFT JOIN CompanyLeavePolicy CLP ON CLP.LeaveCategoryID = SV.SystemVariableID AND CLP.EmployeeStatusID = Emp.EmployeeTypeID AND CLP.FinancialYearID = {elAccount.FinancialYearID}
                        WHERE SV.EntityTypeName IN('LeaveCategory') ";
            }
            
            elaList = EmployeeLeaveAccountRepo.GetDataModelCollection<EmployeeLeaveAccountDto>(sql);

            await Task.CompletedTask;
            return elaList;
        }

        public async Task<bool> GetExistingPolicy(EmployeeLeaveAccountDto elAccount)
        {
            var employee = new Employment();
            if (elAccount.EmployeeID > 0)
            {
                employee = EmployeeRepo.Entities.Where(x => x.EmployeeID == elAccount.EmployeeID && x.IsCurrent == true).FirstOrDefault().MapTo<Employment>();
            }
            int empStatusID = employee.IsNotNull() && employee.EmployeeTypeID.IsNotNull() ? employee.EmployeeTypeID.Value : 0;
            bool isExists = LeavePolicyRepo.Entities.Count(x => x.FinancialYearID == elAccount.FinancialYearID && x.EmployeeStatusID == empStatusID) > 0;
            await Task.CompletedTask;
            return isExists;
        }
        public async Task<bool> GetExistingAccountByEmployee(EmployeeLeaveAccountDto elAccount)
        {
            bool isExists = EmployeeLeaveAccountRepo.Entities.Count(x => x.FinancialYearID == elAccount.FinancialYearID && x.EmployeeID == elAccount.EmployeeID) > 0;
            await Task.CompletedTask;
            return isExists;
        }
    }
}
