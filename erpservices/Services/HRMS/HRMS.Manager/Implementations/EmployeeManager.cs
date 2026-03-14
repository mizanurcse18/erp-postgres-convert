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
using OfficeOpenXml;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Manager.Core.Caching;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.Filters;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Mvc;
using Myrmec;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using iText.Kernel;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using iText.Layout.Element;

namespace HRMS.Manager.Implementations
{
    class EmployeeManager : ManagerBase, IEmployeeManager
    {
        // Designation, Internal_Designation, Job_Grade_Name, Supervisor_Employee_Code, Department, Division
        readonly IRepository<Employee> EmployeeRepo;
        readonly IRepository<Employment> EmploymentRepo;
        readonly IRepository<EmployeeBankInfo> BankInfoRepo;
        readonly IRepository<Employment> EmploymentRepoExist;
        readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        readonly IRepository<JobGrade> JobGradeRepo;
        private readonly ICacheManager<GridModel> _cacheManager;
        // Collection to keep track of cached EmployeeDirectoryList keys
        private static readonly ConcurrentDictionary<string, bool> _cachedEmployeeDirectoryKeys = new ConcurrentDictionary<string, bool>();
        readonly IRepository<Designation> DesignationRepo;
        readonly IRepository<Division> DivisionRepo;
        readonly IRepository<Department> DepartmentRepo;

        //public EmployeeManager(IRepository<Employee> employeeRepo, IRepository<Employment> employmentRepo, IRepository<EmployeeBankInfo> bankInfoRepo, IRepository<Employment> employmentRepoexist, IRepository<EmployeeSupervisorMap> supervisorRepo, IRepository<JobGrade> jobGradeRepo, ICacheManager<GridModel> cacheManager)


        public EmployeeManager(IRepository<Employee> employeeRepo, IRepository<Employment> employmentRepo, IRepository<EmployeeBankInfo> bankInfoRepo, IRepository<Employment> employmentRepoexist, IRepository<EmployeeSupervisorMap> supervisorRepo, IRepository<JobGrade> jobGradeRepo, IRepository<Designation> designationRepo, IRepository<Division> divisionRepo, 
                        IRepository<Department> departmentRepo, ICacheManager<GridModel> cacheManager)
        {
            EmployeeRepo = employeeRepo;
            EmploymentRepo = employmentRepo;
            BankInfoRepo = bankInfoRepo;
            EmploymentRepoExist = employmentRepoexist;
            EmployeeSupervisorMapRepo = supervisorRepo;
            JobGradeRepo = jobGradeRepo;
            _cacheManager = cacheManager;
            DesignationRepo = designationRepo;
            DivisionRepo = divisionRepo;
            DepartmentRepo = departmentRepo;
        }

        public Task<List<Employee>> GetEmployeeList()
        {
            throw new NotImplementedException();
        }

        public GridModel GetEmployeeListDic(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Active":
                    filter = $@" ISNULL(empl.EmployeeTypeID,0)<>{(int)Util.EmployeeType.Discontinued} ";
                    break;
                case "InActive":
                    filter = $@" ISNULL(empl.EmployeeTypeID,0)={(int)Util.EmployeeType.Discontinued} ";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
	                            Emp.EmployeeID,
	                            EmployeeCode,
	                            FullName,
	                            empl.EmployeeTypeID,
	                            ISNULL(sv.SystemVariableCode,'') EmployeeStatus,
	                            ImagePath,
	                            WorkEmail,
	                            DateOfJoining,
								EmployeeStatusID,
								ISNULL(Emp.EmployeeCode,'')+'-'+ISNULL(Emp.FullName,'') EmployeeNameWithCode,
								ISNULL(DivisionName,'')+'-'+ISNULL(DepartmentName,'')+'-'+ISNULL(DesignationName,'') EmployeeDivDeptDesg,
								DivisionName,
								DepartmentName,
								DesignationName,
								ISNULL(Emp.WorkEmail,'')+'-'+ISNULL(Emp.WorkMobile,'') WorkEmailPhone,
								ISNULL(convert(varchar(20),Emp.DateOfJoining, 105)+'-'+convert(varchar(20),Emp.ConfirmDate, 105),'') DOJCOD,
								ISNULL(Emp.WorkMobile,'') WorkMobile,
								--ISNULL(Emp.ConfirmDate,'') ConfirmDate,
                                CAST(Emp.ConfirmDate AS DATE) ConfirmDate,
                                emp.PersonID
                            FROM Employee AS emp
							LEFT JOIN Employment empl ON empl.EmployeeID = emp.EmployeeID AND IsCurrent = 1
							LEFT JOIN Department Dep ON Dep.DepartmentID = empl.DepartmentID
							LEFT JOIN Designation Des ON des.DesignationID = empl.DesignationID
							LEFT JOIN Division Div on Div.DivisionID = empl.DivisionID
							LEFT JOIN (SELECT
											PersonID,ImagePath
										FROM
											Security..PersonImage
											WHERE IsFavorite = 1
										) PIM ON PIM.PersonID = Emp.PersonID
                            left join {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable sv on empl.EmployeeTypeID = sv.SystemVariableID
                            {where} {filter}
                            ";

            var result = EmployeeRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<GridModel> GetEmployeeListDicAsync(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = ""; // No WHERE condition
                    break;
                case "Active":
                    filter = $"COALESCE(empl.employee_type_id, 0) <> {(int)Util.EmployeeType.Discontinued}";
                    break;
                case "InActive":
                    filter = $"COALESCE(empl.employee_type_id, 0) = {(int)Util.EmployeeType.Discontinued}";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
                                emp.employee_id AS ""EmployeeID"",
                                emp.employee_code AS ""EmployeeCode"",
                                emp.full_name AS ""FullName"",
                                empl.employee_type_id AS ""EmployeeTypeID"",
                                COALESCE(sv.system_variable_code, '') AS ""EmployeeStatus"",
                                pim.image_path AS ""ImagePath"",
                                emp.work_email AS ""WorkEmail"",
                                emp.date_of_joining AS ""DateOfJoining"",
                                emp.employee_status_id AS ""EmployeeStatusID"",
                                COALESCE(emp.employee_code, '') || '-' || COALESCE(emp.full_name, '') AS ""EmployeeNameWithCode"",
                                COALESCE(div.division_name, '') || '-' || COALESCE(dep.department_name, '') || '-' || COALESCE(des.designation_name, '') AS ""EmployeeDivDeptDesg"",
                                div.division_name AS ""DivisionName"",
                                dep.department_name AS ""DepartmentName"",
                                des.designation_name AS ""DesignationName"",
                                COALESCE(emp.work_email, '') || '-' || COALESCE(emp.work_mobile, '') AS ""WorkEmailPhone"",
                                COALESCE(TO_CHAR(emp.date_of_joining, 'DD-MM-YYYY') || '-' || TO_CHAR(emp.confirm_date, 'DD-MM-YYYY'), '') AS ""DOJCOD"",
                                COALESCE(emp.work_mobile, '') AS ""WorkMobile"",
                                emp.confirm_date::DATE AS ""ConfirmDate"",
                                emp.person_id AS ""PersonID""
                            FROM employee emp
                            LEFT JOIN employment empl ON empl.employee_id = emp.employee_id AND empl.is_current = TRUE
                            LEFT JOIN department dep ON dep.department_id = empl.department_id
                            LEFT JOIN designation des ON des.designation_id = empl.designation_id
                            LEFT JOIN division div ON div.division_id = empl.division_id
                            LEFT JOIN (
                                -- Subquery using the foreign table from the security_remote schema
                                SELECT person_id, image_path
                                FROM security_remote.person_image
                                WHERE is_favorite = TRUE
                            ) pim ON pim.person_id = emp.person_id
                            LEFT JOIN security_remote.system_variable sv ON empl.employee_type_id = sv.system_variable_id
                            {where} {filter}";

            var result = await EmployeeRepo.LoadGridModelAsync(parameters, sql);
            return result;
        }

        public GridModel GetEmployeeListDicFromView(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Active":
                    filter = $@" ISNULL(Emp.EmployeeTypeID,0)<>{(int)Util.EmployeeType.Discontinued} ";
                    break;
                case "InActive":
                    filter = $@" ISNULL(Emp.EmployeeTypeID,0)={(int)Util.EmployeeType.Discontinued} ";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
	                            emp.employee_id AS ""EmployeeID"",
	                            emp.employee_code AS ""EmployeeCode"",
	                            emp.full_name AS ""FullName"",
	                            emp.employee_type_id AS ""EmployeeTypeID"",
	                            COALESCE(sv.system_variable_code, '') AS ""EmployeeStatus"",
	                            pim.image_path AS ""ImagePath"",
	                            emp.work_email AS ""WorkEmail"",
	                            emp.date_of_joining AS ""DateOfJoining"",
								emp.employee_status_id AS ""EmployeeStatusID"",
								COALESCE(emp.employee_code, '') || '-' || COALESCE(emp.full_name, '') AS ""EmployeeNameWithCode"",
								COALESCE(div.division_name, '') || '-' || COALESCE(dep.department_name, '') || '-' || COALESCE(des.designation_name, '') AS ""EmployeeDivDeptDesg"",
								div.division_name AS ""DivisionName"",
								dep.department_name AS ""DepartmentName"",
								des.designation_name AS ""DesignationName"",
								COALESCE(emp.work_email, '') || '-' || COALESCE(emp.work_mobile, '') AS ""WorkEmailPhone"",
								COALESCE(TO_CHAR(emp.date_of_joining, 'DD-MM-YYYY') || '-' || TO_CHAR(emp.confirm_date, 'DD-MM-YYYY'), '') AS ""DOJCOD"",
								COALESCE(emp.work_mobile, '') AS ""WorkMobile"",
								emp.confirm_date::DATE AS ""ConfirmDate"",
                                emp.person_id AS ""PersonID""
                            FROM employee emp
                            LEFT JOIN employment empl ON empl.employee_id = emp.employee_id AND empl.is_current = TRUE
                            LEFT JOIN department dep ON dep.department_id = empl.department_id
                            LEFT JOIN designation des ON des.designation_id = empl.designation_id
                            LEFT JOIN division div ON div.division_id = empl.division_id
                            LEFT JOIN (
                                SELECT person_id, image_path
                                FROM security_remote.person_image
                                WHERE is_favorite = TRUE
                            ) pim ON pim.person_id = emp.person_id
                            LEFT JOIN security_remote.system_variable sv ON empl.employee_type_id = sv.system_variable_id
                            {where} {filter}";

            var result = EmployeeRepo.LoadGridModel(parameters, sql);
            return result;
        }
        //GetEmployeeDirectoryList
        public async Task<GridModel> GetEmployeeDirectoryList(GridParameter parameters)
        {
            // Use the generic GenerateCacheKey method from ManagerBase
            string cacheKey = GenerateCacheKey("EmployeeDirectoryList", "gridview", parameters);

            // Use the generic caching method from ManagerBase
            return await GetOrAddCachedAsync(
                cacheKey,
                async () => // Func to get data if not in cache
                {
                    string filter = "";
                    string where = "";
                    parameters.ApprovalFilterData = "Active";
                    switch (parameters.ApprovalFilterData)
                    {
                        case "All":
                            filter = "";
                            break;
                        case "Active":
                            filter = $@" ISNULL(empl.EmployeeTypeID,0)<>{(int)Util.EmployeeType.Discontinued} ";
                            break;
                        case "InActive":
                            filter = $@" ISNULL(empl.EmployeeTypeID,0)={(int)Util.EmployeeType.Discontinued} ";
                            break;

                        default:
                            break;
                    }
                    where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
                    string sql = $@"SELECT DISTINCT
	                                        emp.employee_id AS ""EmployeeID"",
	                                        emp.employee_code AS ""EmployeeCode"",
	                                        emp.full_name AS ""FullName"",
	                                        empl.employee_type_id AS ""EmployeeTypeID"",
	                                        pim.image_path AS ""ImagePath"",
	                                        COALESCE(emp.work_email, '') AS ""WorkEmail"",	                
											emp.employee_status_id AS ""EmployeeStatusID"",
											COALESCE(emp.employee_code, '') || '-' || COALESCE(emp.full_name, '') AS ""EmployeeNameWithCode"",
											COALESCE(div.division_name, '') || '-' || COALESCE(dep.department_name, '') || '-' || COALESCE(des.designation_name, '') AS ""EmployeeDivDeptDesg"",
											div.division_name AS ""DivisionName"",
											dep.department_name AS ""DepartmentName"",
											des.designation_name AS ""DesignationName"",
											COALESCE(emp.work_email, '') || '-' || COALESCE(emp.work_mobile, '') AS ""WorkEmailPhone"",
											COALESCE(emp.work_mobile, '') AS ""WorkMobile"",
                                            COALESCE(super_emp.supervisor_full_name, '') || '-' || COALESCE(super_emp.supervisor_email, '') AS ""SupervisorInfo"",
                                            super_emp.supervisor_full_name AS ""SupervisorFullName"",
                                            super_emp.supervisor_email AS ""SupervisorEmail"",
											super_emp.sup_image_path AS ""SupImagePath""
                                            FROM employee emp
	                                        LEFT JOIN employment empl ON empl.employee_id = emp.employee_id AND empl.is_current = TRUE
	                                        LEFT JOIN (SELECT department_id, department_name FROM department) dep ON dep.department_id = empl.department_id
	                                        LEFT JOIN (SELECT designation_id, designation_name FROM designation) des ON des.designation_id = empl.designation_id
	                                        LEFT JOIN (SELECT division_id, division_name FROM division) div ON div.division_id = empl.division_id
	                                        LEFT JOIN (SELECT
						                                    person_id, image_path
						                                FROM
						                                    security_remote.person_image
						                                    WHERE is_favorite = TRUE
						                                ) pim ON pim.person_id = emp.person_id
                                            LEFT JOIN security_remote.system_variable sv ON empl.employee_type_id = sv.system_variable_id                       
	                                        LEFT JOIN (
				                                    SELECT sup.employee_id, supervisor_type, supempl.is_current, employee_supervisor_id, sup_emp.full_name AS supervisor_full_name,
				                                    sup_emp.work_email AS supervisor_email, sup_pim.image_path AS sup_image_path

				                                    FROM 
				                                    employee_supervisor_map sup 
				                                    LEFT JOIN employee sup_emp ON sup.employee_supervisor_id = sup_emp.employee_id
				                                    LEFT JOIN employment supempl ON supempl.employee_id = sup.employee_id AND supempl.is_current = TRUE
				                                    LEFT JOIN (SELECT department_id, department_name FROM department) dep ON dep.department_id = supempl.department_id
				                                    LEFT JOIN (SELECT designation_id, designation_name FROM designation) des ON des.designation_id = supempl.designation_id
				                                    LEFT JOIN (SELECT division_id, division_name FROM division) div ON div.division_id = supempl.division_id
				                                    LEFT JOIN (SELECT
                                                        person_id, image_path
                                                    FROM
                                                        security_remote.person_image
                                                        WHERE is_favorite = TRUE
                                                    ) sup_pim ON sup_pim.person_id = sup_emp.person_id
				                                    WHERE sup.is_current = TRUE AND supervisor_type = 50
			                                ) super_emp ON super_emp.employee_id = emp.employee_id 
                                            {where} {filter}";

                    return await EmployeeRepo.LoadGridModelAsync(parameters, sql);
                },
                _cacheManager, // Pass the cache manager instance
                cacheDuration: null, // Specify cache duration
                onCacheMiss: (key) => _cachedEmployeeDirectoryKeys.TryAdd(key, true) // Action to register key on cache miss
            );
        }

        public GridModel GetEmployeeDirectoryListSync(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            parameters.ApprovalFilterData = "Active";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Active":
                    filter = $@" ISNULL(Emp.EmployeeTypeID,0)<>{(int)Util.EmployeeType.Discontinued} ";
                    break;
                case "InActive":
                    filter = $@" ISNULL(Emp.EmployeeTypeID,0)={(int)Util.EmployeeType.Discontinued} ";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
	                            emp.employee_id AS ""EmployeeID"",
	                            emp.employee_code AS ""EmployeeCode"",
	                            emp.full_name AS ""FullName"",
	                            empl.employee_type_id AS ""EmployeeTypeID"",
	                            pim.image_path AS ""ImagePath"",
	                            COALESCE(emp.work_email, '') AS ""WorkEmail"",	                
			                    emp.employee_status_id AS ""EmployeeStatusID"",
			                    COALESCE(emp.employee_code, '') || '-' || COALESCE(emp.full_name, '') AS ""EmployeeNameWithCode"",
			                    COALESCE(div.division_name, '') || '-' || COALESCE(dep.department_name, '') || '-' || COALESCE(des.designation_name, '') AS ""EmployeeDivDeptDesg"",
			                    div.division_name AS ""DivisionName"",
			                    dep.department_name AS ""DepartmentName"",
			                    des.designation_name AS ""DesignationName"",
			                    COALESCE(emp.work_email, '') || '-' || COALESCE(emp.work_mobile, '') AS ""WorkEmailPhone"",
			                    COALESCE(emp.work_mobile, '') AS ""WorkMobile"",
                                COALESCE(super_emp.supervisor_full_name, '') || '-' || COALESCE(super_emp.supervisor_email, '') AS ""SupervisorInfo"",
                                super_emp.supervisor_full_name AS ""SupervisorFullName"",
                                super_emp.supervisor_email AS ""SupervisorEmail"",
			                    super_emp.sup_image_path AS ""SupImagePath""
                            FROM employee emp
		                    LEFT JOIN employment empl ON empl.employee_id = emp.employee_id AND empl.is_current = TRUE
		                    LEFT JOIN (SELECT department_id, department_name FROM department) dep ON dep.department_id = empl.department_id
		                    LEFT JOIN (SELECT designation_id, designation_name FROM designation) des ON des.designation_id = empl.designation_id
		                    LEFT JOIN (SELECT division_id, division_name FROM division) div ON div.division_id = empl.division_id
		                    LEFT JOIN (SELECT
						                    person_id, image_path
					                    FROM
						                    security_remote.person_image
						                    WHERE is_favorite = TRUE
					                    ) pim ON pim.person_id = emp.person_id
                            LEFT JOIN security_remote.system_variable sv ON empl.employee_type_id = sv.system_variable_id                       
		                    LEFT JOIN (
				                    SELECT sup.employee_id, supervisor_type, supempl.is_current, employee_supervisor_id, sup_emp.full_name AS supervisor_full_name,
				                    sup_emp.work_email AS supervisor_email, sup_pim.image_path AS sup_image_path

				                    FROM 
				                    employee_supervisor_map sup 
				                    LEFT JOIN employee sup_emp ON sup.employee_supervisor_id = sup_emp.employee_id
				                    LEFT JOIN employment supempl ON supempl.employee_id = sup.employee_id AND supempl.is_current = TRUE
				                    LEFT JOIN (SELECT department_id, department_name FROM department) dep ON dep.department_id = supempl.department_id
				                    LEFT JOIN (SELECT designation_id, designation_name FROM designation) des ON des.designation_id = supempl.designation_id
				                    LEFT JOIN (SELECT division_id, division_name FROM division) div ON div.division_id = supempl.division_id
				                    LEFT JOIN (SELECT
                                        person_id, image_path
                                    FROM
                                        security_remote.person_image
                                        WHERE is_favorite = TRUE
                                    ) sup_pim ON sup_pim.person_id = sup_emp.person_id
				                    WHERE sup.is_current = TRUE AND supervisor_type = 50
		                    ) super_emp ON super_emp.employee_id = emp.employee_id 
                            {where} {filter}";

            return EmployeeRepo.LoadGridModel(parameters, sql);
        }
        public async Task<Employee> GetEmployeeTable(int primaryID)
        {
            return await EmployeeRepo.GetAsync(primaryID);
        }
        public async Task<Dictionary<string, object>> GetEmployeeTableDic(int primaryID)
        {
            string sql = $@"SELECT 
	                            p.*, va.user_id AS ""UserID"", sv.system_variable_code AS ""EmploymentCategoryName"", pim.image_path AS ""ImagePath""
                            FROM 
                                employee p
                                LEFT JOIN view_all_employee va ON va.employee_id = p.employee_id
                                LEFT JOIN security_remote.system_variable sv ON p.employment_category_id = sv.system_variable_id
	                        WHERE p.employee_id = {primaryID}";
            var data = EmployeeRepo.GetData(sql);

            return await Task.FromResult(data);
        }
        public async Task<Dictionary<string, object>> GetEmployeeByID(int primaryID)
        {
            string sql = $@"SELECT 
	                            p.*, sv.system_variable_code AS ""EmploymentCategoryName"", pim.image_path AS ""ImagePath"", va.designation_name AS ""DesignationName"", va.department_name AS ""DepartmentName"", va.division_name AS ""DivisionName"", va.work_email AS ""WorkEmail"", va.work_mobile AS ""WorkMobile""
                            FROM 
                                employee p
                                LEFT JOIN view_all_employee va ON va.employee_id = p.employee_id
                                LEFT JOIN security_remote.system_variable sv ON p.employment_category_id = sv.system_variable_id
	                        WHERE p.employee_id = {primaryID}";
            var data = EmployeeRepo.GetData(sql);

            return await Task.FromResult(data);
        }
        public async Task<Dictionary<string, object>> GetEmploymentTableDic(int primaryID)
        {
            string sql = $@"SELECT p.*, dept.department_name AS ""DepartmentName"", desg.designation_name AS ""DesignationName"", intdesg.designation_name AS ""InternalDesignationName"", dv.division_name AS ""DivisionName""
                                , cl.cluster_name AS ""ClusterName"", rg.region_name AS ""RegionName"", bi.branch_name AS ""BranchName"", sv.system_variable_code AS ""EmployeeStatus"", sm.shifting_name AS ""ShiftingName""
                                , jg.job_grade_name AS ""JobGradeName""
                                , bnk.bank_account_name AS ""BankAccountName"" 
								, bnk.bank_account_number AS ""BankAccountNumber"" 
								, bnk.bank_name AS ""BankName""
								, bnk.bank_branch_name AS ""BankBranchName"" 
								, bnk.routing_number AS ""RoutingNumber""
                                FROM employment p
                                LEFT JOIN security_remote.system_variable sv ON p.employee_type_id = sv.system_variable_id
                                LEFT JOIN department dept ON p.department_id = dept.department_id
                                LEFT JOIN designation desg ON p.designation_id = desg.designation_id
                                LEFT JOIN designation intdesg ON p.internal_designation_id = intdesg.designation_id
                                LEFT JOIN division dv ON p.division_id = dv.division_id
                                LEFT JOIN cluster cl ON p.cluster_id = cl.cluster_id
                                LEFT JOIN region rg ON p.region_id = rg.region_id
                                LEFT JOIN branch_info bi ON p.branch_id = bi.branch_id
                                LEFT JOIN shifting_master sm ON p.shift_id = sm.shifting_master_id
								LEFT JOIN job_grade jg ON p.job_grade_id = jg.job_grade_id
                                LEFT JOIN employee_bank_info bnk ON bnk.employee_id = p.employee_id
	                            WHERE p.is_current = TRUE AND p.employee_id = {primaryID}";
            var data = EmployeeRepo.GetData(sql);

            if (data.Count > 0)
            {
                //data["JobGradeID"] = !string.IsNullOrWhiteSpace(data["JobGradeID"].ToString()) ? Decrypt(data["JobGradeID"].ToString()) : data["JobGradeID"];
                data["Band"] = !string.IsNullOrWhiteSpace(data["Band"].ToString()) ? Decrypt(data["Band"].ToString()) : data["Band"];

            }


            return await Task.FromResult(data);
        }
        public async Task<List<Dictionary<string, object>>> GetEmployeeSupervisorMap(int primaryID)
        {
            string sql = $@"SELECT 
	                            p.*, emp.full_name AS ""SupervisorFullName"", emp.work_email AS ""SupervisorEmail"", emp.work_mobile AS ""SupMobile"", va.image_path AS ""SupervisorPhotoUrl""
                            FROM 
                            employee_supervisor_map p
                            LEFT JOIN employee emp ON p.employee_supervisor_id = emp.employee_id
                            LEFT JOIN view_all_employee va ON va.employee_id = p.employee_supervisor_id
	                            WHERE p.is_current = TRUE AND p.employee_id = {primaryID} AND p.supervisor_type = {(int)SupervisorType.Regular}";
            var data = EmployeeSupervisorMapRepo.GetDataDictCollection(sql);

            return await Task.FromResult(data.ToList());
        }

        public async Task<List<Dictionary<string, object>>> GetDottedEmployeeSupervisorMap(int primaryID)
        {
            string sql = $@"SELECT 
	                            p.*, emp.full_name AS ""SupervisorFullName"", emp.work_email AS ""WorkEmail"", emp.work_mobile AS ""WorkMobile"", va.image_path AS ""SupervisorPhotoUrl""
                            FROM 
                            employee_supervisor_map p
                            LEFT JOIN employee emp ON p.employee_supervisor_id = emp.employee_id
                            LEFT JOIN view_all_employee va ON va.employee_id = p.employee_supervisor_id
	                            WHERE p.is_current = TRUE AND p.employee_id = {primaryID} AND p.supervisor_type = {(int)SupervisorType.Dotted}";
            var data = EmployeeSupervisorMapRepo.GetDataDictCollection(sql);

            return await Task.FromResult(data.ToList());
        }

        public async Task<Dictionary<string, object>> GetDelegatedEmployeeSupervisor(int primaryID)
        {
            string sql = $@"SELECT 
	                            p.*, emp.full_name AS ""SupervisorFullName"", emp.work_email AS ""WorkEmail"", emp.work_mobile AS ""WorkMobile"", va.image_path AS ""SupervisorPhotoUrl""
                            FROM 
                            employee_supervisor_map p
                            LEFT JOIN employee emp ON p.employee_supervisor_id = emp.employee_id
                            LEFT JOIN view_all_employee va ON va.employee_id = p.employee_supervisor_id
	                            WHERE p.is_current = TRUE AND p.employee_id = {primaryID} AND p.supervisor_type = {(int)SupervisorType.Delegated}";
            var data = EmployeeSupervisorMapRepo.GetData(sql);

            return await Task.FromResult(data);
        }

        public async Task<Employee> SavePersonAsEmployee(int PersonId)
        {
            Employee master = new Employee();
            string sql = $@"SELECT per.person_id AS ""PersonID"", per.first_name AS ""FullName"", per.email AS ""WorkEmail"", per.mobile AS ""WorkMobile"" FROM security_remote.person per WHERE per.person_id = {PersonId}";
            master = EmployeeRepo.GetDataModelCollection<Employee>(sql).FirstOrDefault();


            //var data = EmployeeRepo.GetDataDictCollection(sql).FirstOrDefault();

            var existsEmployee = EmployeeRepo.Entities.Where(x => x.PersonID == PersonId).SingleOrDefault();
            if (existsEmployee.IsNull())
            {
                using (var unitOfWork = new UnitOfWork())
                {
                    if (existsEmployee.IsNull())
                    {
                        master.SetAdded();
                        SetEmployeeNewId(master);
                    }
                    SetAuditFields(master);
                    EmployeeRepo.Add(master);
                    unitOfWork.CommitChangesWithAudit();
                }
            }
            await Task.CompletedTask;

            return existsEmployee.IsNull() ? master : existsEmployee;
        }
        public async Task<Employee> SaveChanges(Employee master, Employment employment, EmployeeBankInfo bankInfo, List<EmployeeSupervisorMap> employeeSupervisorMaps)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                if (employment.IsNull()) employment = new Employment();
                if (bankInfo.IsNull()) bankInfo = new EmployeeBankInfo();
                var existsPerson = EmployeeRepo.Entities.FirstOrDefault(x => x.PersonID == master.PersonID);
                var existsEmployee = EmployeeRepo.Entities.Where(x => x.EmployeeID == master.EmployeeID).SingleOrDefault();
                if (
                    master.EmployeeID.IsZero() && existsEmployee.IsNull() && existsPerson.IsNull())
                {
                    master.SetAdded();
                    SetEmployeeNewId(master);
                }

                else
                {
                    master.SetModified();
                    master.RowVersion = existsEmployee.RowVersion;
                    master.CreatedBy = existsEmployee.CreatedBy;
                    master.CreatedDate = existsEmployee.CreatedDate;
                    master.CreatedIP = existsEmployee.CreatedIP;
                }

                employment.EmployeeID = master.EmployeeID;
                bankInfo.EmployeeID = master.EmployeeID;

                var existsEmployment = EmploymentRepo.Entities.Where(x => x.EmployeeID == employment.EmployeeID && x.IsCurrent == true).FirstOrDefault();
                if (
                    existsEmployment.IsNull())
                {
                    employment.SetAdded();
                    employment.IsCurrent = true;
                    employment.Band = !string.IsNullOrWhiteSpace(employment.Band) ? Encrypt(employment.Band) : null;
                    SetEmploymentNewId(employment);
                }
                else if (employment.EmployeeTypeID != existsEmployment.EmployeeTypeID || employment.DepartmentID != existsEmployment.DepartmentID ||
                    employment.DesignationID != existsEmployment.DesignationID || employment.DivisionID != existsEmployment.DivisionID || employment.JobGradeID != existsEmployment.JobGradeID ||
                    employment.Band != (!string.IsNullOrEmpty(existsEmployment.Band) ? Decrypt(existsEmployment.Band) : "") || employment.ClusterID != existsEmployment.ClusterID || employment.BranchID != existsEmployment.BranchID || employment.RegionID != existsEmployment.RegionID || employment.ShiftID != existsEmployment.ShiftID)
                {
                    employment.SetAdded();
                    employment.IsCurrent = true;
                    employment.Band = !string.IsNullOrWhiteSpace(employment.Band) ? Encrypt(employment.Band) : null;
                    SetEmploymentNewId(employment);

                    existsEmployment.SetModified();
                    existsEmployment.IsCurrent = false;
                }
                else
                {
                    employment.SetModified();
                    employment.IsCurrent = true;
                    employment.Band = !string.IsNullOrWhiteSpace(employment.Band) ? Encrypt(employment.Band) : null;
                    employment.EmploymentID = existsEmployment.EmploymentID;
                    employment.RowVersion = existsEmployment.RowVersion;
                    employment.CreatedBy = existsEmployment.CreatedBy;
                    employment.CreatedDate = existsEmployment.CreatedDate;
                    employment.CreatedIP = existsEmployment.CreatedIP;
                }
                var existsBankInfo = BankInfoRepo.Entities.Where(x => x.EmployeeID == employment.EmployeeID).FirstOrDefault();
                if (bankInfo.BankBranchName.IsNotNullOrEmpty())
                {
                    if (
                  existsBankInfo.IsNull())
                    {
                        bankInfo.SetAdded();
                        bankInfo.BankAccountName = bankInfo.BankAccountName;
                        bankInfo.BankAccountNumber = bankInfo.BankAccountNumber; ;
                        bankInfo.BankName = bankInfo.BankName; ;
                        bankInfo.BankBranchName = bankInfo.BankBranchName; ;
                        bankInfo.RoutingNumber = bankInfo.RoutingNumber; ;
                        SetEmploymentNewId(employment);
                    }
                    else
                    {
                        bankInfo.SetModified();
                        bankInfo.BankAccountName = bankInfo.BankAccountName;
                        bankInfo.BankAccountNumber = bankInfo.BankAccountNumber;
                        bankInfo.BankName = bankInfo.BankName;
                        bankInfo.BankBranchName = bankInfo.BankBranchName;
                        bankInfo.RoutingNumber = bankInfo.RoutingNumber;

                        bankInfo.BIID = existsBankInfo.BIID;
                        bankInfo.EmployeeID = existsBankInfo.EmployeeID;
                        bankInfo.RowVersion = existsBankInfo.RowVersion;
                        bankInfo.CreatedBy = existsBankInfo.CreatedBy;
                        bankInfo.CreatedDate = existsBankInfo.CreatedDate;
                        bankInfo.CreatedIP = existsBankInfo.CreatedIP;
                    }
                }


                if (employeeSupervisorMaps.IsNull()) employeeSupervisorMaps = new List<EmployeeSupervisorMap>();
                List<EmployeeSupervisorMap> saveMapList = new List<EmployeeSupervisorMap>();
                foreach (var supMap in employeeSupervisorMaps)
                {
                    var mapChild = new EmployeeSupervisorMap
                    {
                        EmployeeID = master.EmployeeID,
                        EmployeeSupervisorID = supMap.EmployeeSupervisorID,
                        SupervisorType = supMap.SupervisorType,
                        FromDate = supMap.FromDate,
                        ToDate = supMap.ToDate
                    };
                    var findTopManagement = EmployeeSupervisorMapRepo.Entities.Where(
                            mapChildEnt => mapChildEnt.EmployeeSupervisorID == 0 && mapChildEnt.SupervisorType == mapChild.SupervisorType && mapChildEnt.IsCurrent == true
                        ).FirstOrDefault();
                    if (findTopManagement.IsNotNull() && findTopManagement.EmployeeID != mapChild.EmployeeID && employeeSupervisorMaps.FirstOrDefault().EmployeeSupervisorID == 0)
                    {
                        throw new Exception("Already Declare a Top Management");
                    }

                    var mapChildEnt = EmployeeSupervisorMapRepo.Entities.Where(
                            mapChildEnt => mapChildEnt.EmployeeID == mapChild.EmployeeID && mapChildEnt.EmployeeSupervisorID == mapChild.EmployeeSupervisorID && mapChildEnt.SupervisorType == mapChild.SupervisorType && mapChildEnt.IsCurrent == true
                        ).FirstOrDefault();

                    if (mapChildEnt.IsNull())
                    {
                        mapChild.IsCurrent = true;
                        mapChild.SetAdded();
                        SetEmployeeSupervisorMapNewId(mapChild);
                        saveMapList.Add(mapChild);
                    }
                    else if (supMap.IsModified || mapChildEnt.IsNotNull())
                    {


                        mapChild.SetModified();
                        mapChild.IsCurrent = employeeSupervisorMaps.FirstOrDefault().IsCurrent;//true;
                        if (mapChildEnt.IsNotNull())
                        {
                            if (mapChildEnt.IsCurrent == false)
                                mapChild.IsCurrent = true;
                        }
                        mapChild.MapID = mapChildEnt.MapID;
                        mapChild.RowVersion = mapChildEnt.RowVersion;
                        mapChild.CreatedBy = mapChildEnt.CreatedBy;
                        mapChild.CreatedIP = mapChildEnt.CreatedIP;
                        mapChild.CreatedDate = mapChildEnt.CreatedDate;
                        saveMapList.Add(mapChild);
                    }
                }


                #region Delete Map 
                if (!master.IsAdded)
                {
                    var list = EmployeeSupervisorMapRepo.Entities.Where(
                            mapChildEnt => mapChildEnt.EmployeeID == master.EmployeeID
                        ).ToList();
                    foreach (var obj in list)
                    {
                        var exist = employeeSupervisorMaps.Find(x => x.EmployeeSupervisorID == obj.EmployeeSupervisorID);

                        if (exist.IsNull())
                        {

                            obj.SetModified();
                            obj.IsCurrent = false;
                            //exist.RowVersion = obj.RowVersion;
                            //exist.CreatedBy = obj.CreatedBy;
                            //exist.CreatedIP = obj.CreatedIP;
                            //exist.CreatedDate = obj.CreatedDate;
                            saveMapList.Add(obj);
                        }
                    }
                }

                #endregion Delete Map 

                //Set Audti Fields Data
                SetAuditFields(employment);
                SetAuditFields(bankInfo);
                if (existsEmployment.IsNotNull())
                {
                    if (employment.EmployeeTypeID != existsEmployment.EmployeeTypeID || employment.DepartmentID != existsEmployment.DepartmentID ||
                    employment.DesignationID != existsEmployment.DesignationID || employment.DivisionID != existsEmployment.DivisionID || employment.JobGradeID != existsEmployment.JobGradeID ||
                    employment.Band != (!string.IsNullOrWhiteSpace(existsEmployment.Band) ? Decrypt(existsEmployment.Band) : "") || employment.ClusterID != existsEmployment.ClusterID || employment.BranchID != existsEmployment.BranchID || employment.RegionID != existsEmployment.RegionID || employment.ShiftID != existsEmployment.ShiftID)
                    {

                        SetAuditFields(existsEmployment);
                    }
                }

                //Set Audti Fields Data
                SetAuditFields(master);

                SetAuditFields(saveMapList);

                EmployeeRepo.Add(master);

                EmploymentRepo.Add(employment);
                BankInfoRepo.Add(bankInfo);

                if (existsEmployment.IsNotNull())
                {
                    if (employment.EmployeeTypeID != existsEmployment.EmployeeTypeID || employment.DepartmentID != existsEmployment.DepartmentID ||
                    employment.DesignationID != existsEmployment.DesignationID || employment.DivisionID != existsEmployment.DivisionID || employment.JobGradeID != existsEmployment.JobGradeID ||
                    employment.Band != (!string.IsNullOrWhiteSpace(existsEmployment.Band) ? Decrypt(existsEmployment.Band) : "") || employment.ClusterID != existsEmployment.ClusterID || employment.BranchID != existsEmployment.BranchID || employment.RegionID != existsEmployment.RegionID || employment.ShiftID != existsEmployment.ShiftID)
                    {

                        EmploymentRepoExist.Add(existsEmployment);
                    }
                }
                EmployeeSupervisorMapRepo.AddRange(saveMapList);

                unitOfWork.CommitChangesWithAudit();

                string sql = $@"EXEC AssignOrUpdateLeaveToNewEmployee {master.EmployeeID},'{AppContexts.User.CompanyID}',{AppContexts.User.UserID},'{AppContexts.User.IPAddress}'";
                var spCall = EmployeeRepo.GetData(sql);

            }
            await Task.CompletedTask;

            // Clear all cached employee directory lists after saving changes using the generic method
            await ClearCachedKeysAsync(_cachedEmployeeDirectoryKeys, _cacheManager);

            return master;
        }
        public async Task<Employee> SaveChanges(Employee person)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existsEmployee = EmployeeRepo.Entities.Where(x => x.EmployeeID == person.EmployeeID).SingleOrDefault();
                if (
                    person.EmployeeID.IsZero() && existsEmployee.IsNull())
                {
                    person.SetAdded();
                    SetEmployeeNewId(person);
                }
                else
                {
                    person.SetModified();
                }

                //Set Audti Fields Data
                SetAuditFields(person);

                EmployeeRepo.Add(person);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;

            // Clear all cached employee directory lists after saving changes using the generic method
            await ClearCachedKeysAsync(_cachedEmployeeDirectoryKeys, _cacheManager);

            return person;
        }
        public async Task<bool> GetDuplicateEmployeeCode(string EmployeeCode)
        {
            bool isExists = EmployeeRepo.Entities.Count(x => x.EmployeeCode == EmployeeCode) > 0;
            await Task.CompletedTask;
            return isExists;
        }

        private void SetEmployeeNewId(Employee person)
        {
            if (!person.IsAdded) return;
            var code = GenerateSystemCode("Employee", AppContexts.User.CompanyID);
            person.EmployeeID = code.MaxNumber;
        }
        private void SetEmploymentNewId(Employment empt)
        {
            if (!empt.IsAdded) return;
            var code = GenerateSystemCode("Employment", AppContexts.User.CompanyID);
            empt.EmploymentID = code.MaxNumber;
        }
        private void SetEmployeeSupervisorMapNewId(EmployeeSupervisorMap empt)
        {
            if (!empt.IsAdded) return;
            var code = GenerateSystemCode("EmployeeSupervisorMap", AppContexts.User.CompanyID);
            empt.MapID = code.MaxNumber;
        }

        public async Task RemoveEmployee(int EmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var employeeEnt = EmployeeRepo.Entities.Where(x => x.EmployeeID == EmployeeID).FirstOrDefault();
                employeeEnt.SetDeleted();
                var employmentList = EmploymentRepo.Entities.Where(x => x.EmployeeID == EmployeeID).ToList();
                employmentList.ForEach(x => x.SetDeleted());
                var supMapList = EmployeeSupervisorMapRepo.Entities.Where(x => x.EmployeeID == EmployeeID).ToList();
                supMapList.ForEach(x => x.SetDeleted());

                EmployeeRepo.Add(employeeEnt);
                EmploymentRepo.AddRange(employmentList);
                EmployeeSupervisorMapRepo.AddRange(supMapList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;

            // Clear all cached employee directory lists after removing an employee using the generic method
            await ClearCachedKeysAsync(_cachedEmployeeDirectoryKeys, _cacheManager);
        }
        public Task Delete(string EmployeeID)
        {
            throw new NotImplementedException();
        }

        private void SetEmployeeImageNewId(EmployeeImageDto img)
        {
            if (!img.IsAdded) return;
            var code = GenerateSystemCode("EmployeeImage", AppContexts.User.CompanyID);
            img.PIID = code.MaxNumber;
        }

        public async Task<Employee> SaveChanges(Employee master, List<EmployeeImageDto> personImages = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (personImages.IsNull()) personImages = new List<EmployeeImageDto>();
                var existsEmployee = EmployeeRepo.Entities.Where(x => x.EmployeeID == master.EmployeeID).SingleOrDefault();
                if (
                    master.EmployeeID.IsZero() && existsEmployee.IsNull())
                {
                    master.SetAdded();
                    SetEmployeeNewId(master);
                }
                else
                {
                    master.SetModified();
                }



                //Set Audti Fields Data
                SetAuditFields(master);

                EmployeeRepo.Add(master);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;

            // Clear all cached employee directory lists after saving changes using the generic method
            await ClearCachedKeysAsync(_cachedEmployeeDirectoryKeys, _cacheManager);

            return master;
        }

        public async Task<string> GetMediaList(int personID)
        {
            string sql = $@"SELECT parent.id,
	                            parent.name,
	                            parent.info,
	                            media.type,
	                            media.title,
	                            media.preview
                            FROM 
                            (SELECT 
	                            ROW_NUMBER() OVER (ORDER BY name) AS id,
	                            name,
	                            CAST(SUM(info) AS varchar) || ' Photos' AS info
                            FROM
                            (SELECT
	                            DISTINCT
	                            TO_CHAR(created_date, 'FMMonth YYYY') AS name,
	                            COUNT(pi_id) AS info
                            FROM 
	                            employee_image
                            WHERE employee_id = {personID}
                            GROUP BY created_date
                            ) media
                            GROUP BY name
							) parent
							LEFT JOIN (
							SELECT
								TO_CHAR(created_date, 'FMMonth YYYY') AS name,
								'photo' AS type,	
								image_name AS title,
								image_path AS preview
							FROM 
								employee_image
							WHERE employee_id = {personID} ) media ON media.name = parent.name";
            var mediaList = EmployeeRepo.GetJsonData(sql);
            return await Task.FromResult(mediaList);
        }

        public async Task<List<Dictionary<string, object>>> GetAllEmployeeListByWhereCondition(string whereCondition)
        {
            string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";
            string sql = $@"SELECT * FROM view_all_employee_for_excel {where}";
            var data = EmployeeSupervisorMapRepo.GetDataDictCollection(sql);

            return await Task.FromResult(data.ToList());
        }

        public async Task<List<JobGradeDecryptDto>> GetDecryptedJobGrades()
        {
            string sql = $@"SELECT * FROM BackupEmploymentJobGrade";
            var data = JobGradeRepo.GetDataModelCollection<JobGradeDecryptDto>(sql);

            using var unitOfWork = new UnitOfWork();
            var employementList = EmploymentRepo.GetAllList();
            var updateList = new List<Employment>();

            foreach (var employement in employementList)
            {
                var backedUpJobgrade = data.SingleOrDefault(x => x.EmploymentID == employement.EmploymentID && x.EmployeeID == employement.EmployeeID);
                employement.JobGradeID = backedUpJobgrade == null ? 0 : backedUpJobgrade.JobGrade;
                employement.SetModified();
                updateList.Add(employement);
            }
            EmploymentRepo.AddRange(updateList);
            unitOfWork.CommitChangesWithAudit();
            return await Task.FromResult(data.ToList());

        }

        public async Task<Dictionary<string, object>> GetCacheStatus()
        {
            var cacheStatus = new Dictionary<string, object>
            {
                ["TotalCachedKeys"] = _cachedEmployeeDirectoryKeys.Count,
                ["CachedKeys"] = _cachedEmployeeDirectoryKeys.Keys.ToList(),
                ["LastUpdated"] = DateTime.Now,
                ["CacheManagerType"] = _cacheManager.GetType().Name
            };

            // Try to get a sample cached item to verify cache is working
            if (_cachedEmployeeDirectoryKeys.Count > 0)
            {
                var sampleKey = _cachedEmployeeDirectoryKeys.Keys.First();
                var sampleItem = await _cacheManager.GetAsync(sampleKey);
                cacheStatus["SampleKey"] = sampleKey;
                cacheStatus["SampleItemExists"] = sampleItem != null;
                if (sampleItem != null)
                {
                    cacheStatus["SampleItemType"] = sampleItem.GetType().Name;
                }
            }

            return await Task.FromResult(cacheStatus);
        }

        public async Task<bool> ClearAllCaches()
        {
            try
            {
                // Clear the employee directory cache
                await ClearCachedKeysAsync(_cachedEmployeeDirectoryKeys, _cacheManager);

                // Clear the cache manager's internal cache
                await _cacheManager.ClearAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"Error clearing caches: {ex.Message}");
                return false;
            }
        }
        

        
        public async Task<GenericResponse<EmployeeUpdateInformationDTO>> UpdateEmployeeInfo(List<EmployeeUpdateInformationDTOForExcel> employees)
        {
            List<EmployeeUpdateInformationDTO> listWithValidationResult = new List<EmployeeUpdateInformationDTO>();

            var allEmployee = await EmployeeRepo.GetAllListAsync();
            var existingDesignations = await DesignationRepo.GetAllListAsync();
            var existingInternalDesignations = await DesignationRepo.GetAllListAsync();
            var existingJobGradeNames = await JobGradeRepo.GetAllListAsync();
            var existingDepartments = await DepartmentRepo.GetAllListAsync();
            var existingDivisions = await DivisionRepo.GetAllListAsync();

            bool hasIssueInUploadedFile = false;

            foreach (var employeeUpdateInfo in employees)
            {
                bool status = true;
                EmployeeUpdateInformationDTO singleEmployeeWithValidationResult = new EmployeeUpdateInformationDTO
                {
                    Employee_Code = employeeUpdateInfo.Employee_Code,
                    Designation = employeeUpdateInfo.Designation,
                    Internal_Designation = employeeUpdateInfo.Internal_Designation,
                    Job_Grade_Name = employeeUpdateInfo.Job_Grade_Name,
                    Supervisor_Employee_Code = employeeUpdateInfo.Supervisor_Employee_Code,
                    Department = employeeUpdateInfo.Department,
                    Division = employeeUpdateInfo.Division
                };

                // Validate Employee ID
                var emp = allEmployee.Where(e => e.EmployeeCode == singleEmployeeWithValidationResult.Employee_Code).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(singleEmployeeWithValidationResult.Employee_Code))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Employee ID,";
                }

                // Validate Designation
                if (string.IsNullOrEmpty(singleEmployeeWithValidationResult.Designation) || !(existingDesignations.Exists(x => x.DesignationName.ToLower().Equals(singleEmployeeWithValidationResult.Designation.Trim().ToLower()))))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Designation,";
                }

                // Validate Internal Designation
                if (string.IsNullOrEmpty(singleEmployeeWithValidationResult.Internal_Designation) || !(existingInternalDesignations.Exists(x => x.DesignationName.ToLower().Equals(singleEmployeeWithValidationResult.Internal_Designation.Trim().ToLower()))))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Internal Designation,";
                }

                // Validate Job Grade Name
                if (string.IsNullOrEmpty(singleEmployeeWithValidationResult.Job_Grade_Name) || !(existingJobGradeNames.Exists(x => x.JobGradeName.ToLower().Equals(singleEmployeeWithValidationResult.Job_Grade_Name.Trim().ToLower()))))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Job Grade Name,";
                }

                // Validate Supervisor Employee Code
                var empSupervisor = allEmployee.Where(e => e.EmployeeCode == singleEmployeeWithValidationResult.Supervisor_Employee_Code).ToList();
                if (empSupervisor == null || empSupervisor.Count == 0 || string.IsNullOrEmpty(singleEmployeeWithValidationResult.Supervisor_Employee_Code))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Supervisor Employee Code,";
                }

                // Validate Department
                if (string.IsNullOrEmpty(singleEmployeeWithValidationResult.Department) || !(existingDepartments.Exists(x => x.DepartmentName.ToLower().Equals(singleEmployeeWithValidationResult.Department.Trim().ToLower()))))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Department,";
                }

                // Validate Division
                if (string.IsNullOrEmpty(singleEmployeeWithValidationResult.Division) || !(existingDivisions.Exists(x => x.DivisionName.ToLower().Equals(singleEmployeeWithValidationResult.Division.Trim().ToLower()))))
                {
                    status = false;
                    singleEmployeeWithValidationResult.Validation_Result += " Invalid Division,";
                }

                if (!status)
                {
                    hasIssueInUploadedFile = true;
                    listWithValidationResult.Add(singleEmployeeWithValidationResult);
                }
                else
                {
                    singleEmployeeWithValidationResult.Validation_Result = "Passed";
                    listWithValidationResult.Add(singleEmployeeWithValidationResult);
                }
            }

            if (hasIssueInUploadedFile)
            {
                return new GenericResponse<EmployeeUpdateInformationDTO>
                {
                    message = "Validation failed. Please download the file with errors.",
                    status = false,
                    data = listWithValidationResult
                };
            }
            else
            {
                //employees ---> Employee_Code, Designation, Internal_Designation, Job_Grade_Name, Supervisor_Employee_Code, Department, Divisio

                using (var unitOfWork = new UnitOfWork())
                {

                    var dataEmployment = (from e in employees
                                          join emp in EmployeeRepo.GetAllList() on e.Employee_Code equals emp.EmployeeCode
                                          join des in DesignationRepo.GetAllList() on e.Designation equals des.DesignationName
                                          join intdesg in DesignationRepo.GetAllList() on e.Internal_Designation equals intdesg.DesignationName
                                          join jobgrade in JobGradeRepo.GetAllList() on e.Job_Grade_Name equals jobgrade.JobGradeName
                                          join dept in DepartmentRepo.GetAllList() on e.Department equals dept.DepartmentName
                                          join div in DivisionRepo.GetAllList() on e.Division equals div.DivisionName

                                          select new Employment
                                          {
                                              EmployeeID = emp.EmployeeID,
                                              DesignationID = des.DesignationID,
                                              InternalDesignationID = intdesg.DesignationID,
                                              JobGradeID = jobgrade.JobGradeID,
                                              DepartmentID = dept.DepartmentID,
                                              DivisionID = div.DivisionID,

                                          }).ToList();

                    foreach (var newdataEmployment in dataEmployment)
                    {
                        var olddataEmployment = EmploymentRepo.Entities
                        .Where(x => x.EmployeeID == newdataEmployment.EmployeeID && x.IsCurrent == true)
                        .SingleOrDefault();
                        olddataEmployment.IsCurrent = false;
                        olddataEmployment.SetModified();
                        EmploymentRepo.Add(olddataEmployment);


                        newdataEmployment.EmployeeTypeID = olddataEmployment.EmployeeTypeID;
                        newdataEmployment.BranchID = olddataEmployment.BranchID;
                        newdataEmployment.UnitID = olddataEmployment.UnitID;
                        newdataEmployment.SubUnitID = olddataEmployment.SubUnitID;
                        newdataEmployment.StartDate = olddataEmployment.StartDate;
                        newdataEmployment.EndDate = olddataEmployment.EndDate;
                        newdataEmployment.IsCurrent = true;
                        newdataEmployment.ChangeStatusID = olddataEmployment.ChangeStatusID;
                        newdataEmployment.Remarks = olddataEmployment.Remarks;
                        newdataEmployment.Band = olddataEmployment.Band;
                        newdataEmployment.ClusterID = olddataEmployment.ClusterID;
                        newdataEmployment.RegionID = olddataEmployment.RegionID;
                        newdataEmployment.ShiftID = olddataEmployment.ShiftID;
                        newdataEmployment.SetAdded();
                        SetEmploymentNewId(newdataEmployment);
                    }

                    EmploymentRepo.AddRange(dataEmployment);


                    var dataSuperVisiorMap = (from e in employees
                                              join emp in EmployeeRepo.GetAllList() on e.Employee_Code equals emp.EmployeeCode
                                              join sup in EmployeeRepo.GetAllList() on e.Supervisor_Employee_Code equals sup.EmployeeCode

                                              select new EmployeeSupervisorMap
                                              {
                                                  EmployeeID = emp.EmployeeID,
                                                  EmployeeSupervisorID = sup.EmployeeID
                                              }).ToList();


                    foreach (var newdataEmployeeSupervisorMap in dataSuperVisiorMap)
                    {
                        var olddataEmployeeSupervisorMap = EmployeeSupervisorMapRepo.Entities
                        .Where(x => x.EmployeeID == newdataEmployeeSupervisorMap.EmployeeID && x.IsCurrent == true && x.SupervisorType == (int)Util.SupervisorType.Regular)
                        .SingleOrDefault();
                        if (olddataEmployeeSupervisorMap.IsNullOrDbNull())
                        {

                            newdataEmployeeSupervisorMap.IsCurrent = true;
                            newdataEmployeeSupervisorMap.SupervisorType = (int)Util.SupervisorType.Regular;
                            newdataEmployeeSupervisorMap.SetAdded();
                            SetEmployeeSupervisorMapNewId(newdataEmployeeSupervisorMap);
                        }
                        else
                        {
                            olddataEmployeeSupervisorMap.IsCurrent = false;
                            olddataEmployeeSupervisorMap.SetModified();
                            EmployeeSupervisorMapRepo.Add(olddataEmployeeSupervisorMap);

                            newdataEmployeeSupervisorMap.IsCurrent = true;
                            newdataEmployeeSupervisorMap.FromDate = olddataEmployeeSupervisorMap.FromDate;
                            newdataEmployeeSupervisorMap.SupervisorType = olddataEmployeeSupervisorMap.SupervisorType;
                            newdataEmployeeSupervisorMap.ToDate = olddataEmployeeSupervisorMap.ToDate;
                            newdataEmployeeSupervisorMap.SetAdded();
                            SetEmployeeSupervisorMapNewId(newdataEmployeeSupervisorMap);
                        }

                    }

                    EmployeeSupervisorMapRepo.AddRange(dataSuperVisiorMap);

                    unitOfWork.CommitChangesWithAudit();
                }

                return new GenericResponse<EmployeeUpdateInformationDTO>
                {
                    message = "File uploaded successfully",
                    status = true
                };
            }
        }


    }
}
