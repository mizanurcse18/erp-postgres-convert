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
    public class CompanyLeavePolicyManager : ManagerBase, ICompanyLeavePolicyManager
    {
        private readonly IRepository<CompanyLeavePolicy> CompanyLeavePolicyRepo;
        //readonly IModelAdapter Adapter;
        public CompanyLeavePolicyManager(IRepository<CompanyLeavePolicy> clPolicyRepo
            )
        {
            CompanyLeavePolicyRepo = clPolicyRepo;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyTables()
        {
            var clPolicies = await CompanyLeavePolicyRepo.GetAllListAsync(x => x.CompanyID == AppContexts.User.CompanyID.ToString());
            return clPolicies.MapTo<List<CompanyLeavePolicyDto>>();
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicy(int finYearID, int empStatusID)
        {
            //var clPolicies = await CompanyLeavePolicyRepo.GetAllListAsync(x => x.FinancialYearID == finYearID && x.EmployeeStatusID == empStatusID);
            //return clPolicies.MapTo<List<CompanyLeavePolicyDto>>();

            //     string sql = $@"SELECT CLP.CLPolicyID, CLP.FinancialYearID, CLP.EmployeeStatusID, CLP.LeaveCategoryID, CLP.LeaveInDays, CLP.Remarks,CLP.CompanyID , SV.SystemVariableCode AS LeaveCategoryName,FY.Year AS Year, SVE.SystemVariableCode AS EmployeeStatusName, 1 AS IsExists
            //                     FROM CompanyLeavePolicy CLP
            //                     LEFT JOIN Security..SystemVariable SV ON CLP.LeaveCategoryID = SV.SystemVariableID
            //LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = CLP.FinancialYearID
            //                     LEFT JOIN Security..SystemVariable SVE ON CLP.EmployeeStatusID = SVE.SystemVariableID
            //                     WHERE SV.EntityTypeName IN('LeaveCategory', 'EmployeeJobStatus') AND CLP.FinancialYearID={finYearID} AND CLP.EmployeeStatusID={empStatusID}";

            string sql = $@"SELECT
                                COALESCE(clp.cl_policy_id, ROW_NUMBER() OVER (ORDER BY sve.system_variable_id)) AS ""CLPolicyID"",
                                fy.financial_year_id AS ""FinancialYearID"",
                                sve.system_variable_id AS ""EmployeeStatusID"",
                                sv.system_variable_id AS ""LeaveCategoryID"",
                                clp.leave_in_days AS ""LeaveInDays"",
                                clp.remarks AS ""Remarks"",
                                COALESCE(clp.company_id, '{AppContexts.User.CompanyID}') AS ""CompanyID"",
                                sv.system_variable_code AS ""LeaveCategoryName"",
                                fy.year AS ""Year"",
                                sve.system_variable_code AS ""EmployeeStatusName"",
                                1 AS ""IsExists""
                            FROM
                                system_variable sv
                            LEFT JOIN
                                company_leave_policy clp ON clp.leave_category_id = sv.system_variable_id
                                AND clp.financial_year_id = {finYearID}
                                AND clp.employee_status_id = {empStatusID}
                            LEFT JOIN
                                financial_year fy ON fy.financial_year_id = {finYearID}
                            LEFT JOIN
                                system_variable sve ON sve.system_variable_id = {empStatusID}
                            WHERE
                                sv.entity_type_name IN ('LeaveCategory')";
            var policyList = new List<CompanyLeavePolicyDto>();
            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);


            await Task.CompletedTask;
            return policyList;
        }


        public async Task<CompanyLeavePolicyDto> SaveChanges(CompanyLeavePolicyDto master, List<CompanyLeavePolicyDto> childs = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<CompanyLeavePolicyDto>();
                
                foreach (var child in childs)
                {
                    var existsPolicy = CompanyLeavePolicyRepo.Entities.SingleOrDefault(x => x.FinancialYearID == child.FinancialYearID && x.EmployeeStatusID == child.EmployeeStatusID && x.LeaveCategoryID == child.LeaveCategoryID).MapTo<CompanyLeavePolicy>();
                    if (existsPolicy.IsNull())
                    {
                        child.SetAdded();
                        child.FinancialYearID = master.FinancialYearID;
                        child.EmployeeStatusID = master.EmployeeStatusID;
                        SetNewId(child);
                    }
                    else
                    {
                        child.SetModified();

                        child.RowVersion = existsPolicy.RowVersion;
                        child.CreatedBy = existsPolicy.CreatedBy;
                        child.CreatedDate = existsPolicy.CreatedDate;
                        child.CreatedIP = existsPolicy.CreatedIP;
                    }

                }
                var childsEnt = childs.MapTo<List<CompanyLeavePolicy>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                CompanyLeavePolicyRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

            }
            await Task.CompletedTask;

            return master;
        }

        public async Task RemoveCompanyLeavePolicy(int FinancialYearID, int EmployeeStatusID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var childEnt = CompanyLeavePolicyRepo.Entities.Where(x => x.FinancialYearID == FinancialYearID && x.EmployeeStatusID == EmployeeStatusID).ToList();
                childEnt.ChangeState(ModelState.Deleted);

                CompanyLeavePolicyRepo.AddRange(childEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetNewId(CompanyLeavePolicyDto clPolicyTable)
        {
            if (!clPolicyTable.IsAdded) return;
            var code = GenerateSystemCode("CompanyLeavePolicy", AppContexts.User.CompanyID);
            clPolicyTable.CLPolicyID = code.MaxNumber;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyListWithDetails()
        {
            var policyList = new List<CompanyLeavePolicyDto>();
            var sql = $@"SELECT
                                clp.cl_policy_id AS ""CLPolicyID"",
                                clp.financial_year_id AS ""FinancialYearID"",
                                clp.employee_status_id AS ""EmployeeStatusID"",
                                clp.leave_category_id AS ""LeaveCategoryID"",
                                clp.leave_in_days AS ""LeaveInDays"",
                                clp.remarks AS ""Remarks"",
                                clp.company_id AS ""CompanyID"",
                                sv.system_variable_code AS ""LeaveCategoryName"",
                                fy.year AS ""Year"",
                                sve.system_variable_code AS ""EmployeeStatusName"",
                                CASE 
                                    WHEN da.financial_year_id IS NULL
                                        THEN TRUE
                                    ELSE FALSE
                                END AS ""IsRemovable""
                            FROM
                                company_leave_policy clp
                            LEFT JOIN
                                system_variable sv ON clp.leave_category_id = sv.system_variable_id
                            LEFT JOIN
                                financial_year fy ON fy.financial_year_id = clp.financial_year_id
                            LEFT JOIN
                                system_variable sve ON clp.employee_status_id = sve.system_variable_id
                            LEFT JOIN (
                                SELECT DISTINCT
                                    financial_year_id,
                                    leave_category_id,
                                    employee_status_id
                                FROM
                                    employee_leave_account ela
                                INNER JOIN
                                    view_all_employee vae ON vae.employee_id = ela.employee_id
                            ) da ON da.financial_year_id = clp.financial_year_id 
                                AND da.leave_category_id = clp.leave_category_id 
                                AND da.employee_status_id = clp.employee_status_id
                            WHERE
                                sv.entity_type_name IN ('LeaveCategory', 'EmployeeJobStatus') 
                                AND clp.company_id = '{AppContexts.User.CompanyID}'";

            //GROUP BY FY.Year,SVE.SystemVariableCode, CLP.FinancialYearID, CLP.EmployeeStatusID";

            //var listDict = CompanyLeavePolicyRepo.GetDataDictCollection(sql);

            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);
            var customList = policyList.GroupBy(c => new { c.Year, c.EmployeeStatusName, c.FinancialYearID, c.EmployeeStatusID })
                .Select(chld => new CompanyLeavePolicyDto()
                {
                    Year = chld.Key.Year,
                    EmployeeStatusName = chld.Key.EmployeeStatusName,
                    FinancialYearID = chld.Key.FinancialYearID,
                    EmployeeStatusID = chld.Key.EmployeeStatusID,
                    IsRemovable = chld.ToList()[0].IsRemovable,
                    LeavePolicyList = chld.ToList()
                }).ToList();

            await Task.CompletedTask;
            return customList;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetGenerateChildList(CompanyLeavePolicyDto clPolicy)
        {
            var policyList = new List<CompanyLeavePolicyDto>();

            //var existsPolicy = CompanyLeavePolicyRepo.Entities.SingleOrDefault(x => x.FinancialYearID == clPolicy.FinancialYearID && x.EmployeeStatusID == clPolicy.EmployeeStatusID).MapTo<CompanyLeavePolicy>();
            string sql = "";
            if(clPolicy.IsExists)
            {                
                sql = $@"SELECT
                            COALESCE(clp.cl_policy_id, ROW_NUMBER() OVER (ORDER BY sv.system_variable_id)) AS ""CLPolicyID"",
                            COALESCE(clp.financial_year_id, {clPolicy.FinancialYearID}) AS ""FinancialYearID"",
                            COALESCE(clp.employee_status_id, {clPolicy.EmployeeStatusID}) AS ""EmployeeStatusID"",
                            sv.system_variable_id AS ""LeaveCategoryID"",
                            COALESCE(clp.leave_in_days, 0) AS ""LeaveInDays"",
                            clp.remarks AS ""Remarks"",
                            clp.company_id AS ""CompanyID"",
                            sv.system_variable_code AS ""LeaveCategoryName"",
                            fy.year AS ""Year"",
                            sve.system_variable_code AS ""EmployeeStatusName"",
                            1 AS ""IsExists""
                        FROM
                            system_variable sv
                        LEFT JOIN
                            company_leave_policy clp ON clp.leave_category_id = sv.system_variable_id
                            AND clp.financial_year_id = {clPolicy.FinancialYearID} 
                            AND clp.employee_status_id = {clPolicy.EmployeeStatusID}
                        LEFT JOIN
                            financial_year fy ON fy.financial_year_id = {clPolicy.FinancialYearID}
                        LEFT JOIN
                            system_variable sve ON sve.system_variable_id = {clPolicy.EmployeeStatusID}
                        WHERE
                            sv.entity_type_id = {(int)Util.SystemVariableEntityType.LeaveCategory}";
            }
            else
            {
                sql = $@"SELECT
                            ROW_NUMBER() OVER (ORDER BY system_variable_id) AS ""CLPolicyID"",
                            0 AS ""FinancialYearID"",
                            0 AS ""EmployeeStatusID"",
                            sv.system_variable_id AS ""LeaveCategoryID"",
                            0 AS ""LeaveInDays"",
                            '' AS ""Remarks"",
                            sv.company_id AS ""CompanyID"",
                            sv.system_variable_code AS ""LeaveCategoryName""
                        FROM
                            system_variable sv
                        WHERE
                            sv.entity_type_name = 'LeaveCategory'";
            }
            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);


            await Task.CompletedTask;
            return policyList;
        }

        public async Task<bool> GetExistingPolicy(CompanyLeavePolicyDto clPolicy)
        {
            bool isExists = CompanyLeavePolicyRepo.Entities.Count(x => x.FinancialYearID == clPolicy.FinancialYearID && x.EmployeeStatusID == clPolicy.EmployeeStatusID) > 0;
            await Task.CompletedTask;
            return isExists;
        }

    }
}
