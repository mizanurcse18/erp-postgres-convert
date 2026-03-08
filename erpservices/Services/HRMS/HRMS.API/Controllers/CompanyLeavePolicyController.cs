
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Core;
using HRMS.API.Models;
using HRMS.Manager;
using HRMS.Manager.Dto;
using System.Collections.Generic;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class CompanyLeavePolicyController : BaseController
    {
        private readonly ICompanyLeavePolicyManager Manager;

        public CompanyLeavePolicyController(ICompanyLeavePolicyManager manager)
        {
            Manager = manager;
        }

        // GET: /CompanyLeavePolicy/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var CompanyLeavePolicys = await Manager.GetCompanyLeavePolicyListWithDetails();
            return OkResult(CompanyLeavePolicys);
        }


        // POST: /CompanyLeavePolicy/CreateCompanyLeavePolicy
        [HttpPost("SaveCompanyLeavePolicy")]
        public async Task<IActionResult> SaveCompanyLeavePolicy([FromBody] CompanyLeavePolicySaveModel CompanyLeavePolicy)
        {
            await Manager.SaveChanges(CompanyLeavePolicy.MasterModel, CompanyLeavePolicy.ChildModels);
            
            return await GetLeavePolicy(CompanyLeavePolicy.MasterModel.FinancialYearID, CompanyLeavePolicy.MasterModel.EmployeeStatusID);
        }

        // PUT: /CompanyLeavePolicy/UpdateCompanyLeavePolicy
        [HttpPut("UpdateCompanyLeavePolicy")]
        public async Task<IActionResult> UpdateCompanyLeavePolicy([FromBody] CompanyLeavePolicySaveModel CompanyLeavePolicyUpdate)
        {
            await Manager.SaveChanges(CompanyLeavePolicyUpdate.MasterModel, CompanyLeavePolicyUpdate.ChildModels);
            
            return await GetLeavePolicy(CompanyLeavePolicyUpdate.MasterModel.FinancialYearID , CompanyLeavePolicyUpdate.MasterModel.EmployeeStatusID);
        }

        // Delete: /CompanyLeavePolicy/DeleteCompanyLeavePolicy
        [HttpGet("RemoveCompanyLeavePolicy")]
        public async Task<IActionResult> RemoveCompanyLeavePolicy(int FinancialYearID, int EmployeeStatusID)
        {
            await Manager.RemoveCompanyLeavePolicy(FinancialYearID, EmployeeStatusID);
            return OkResult(new { Success = true });
        }

        [HttpPost("GetLeaveCategoryWiseList")]
        public async Task<IActionResult> GetLeaveCategoryWiseList([FromBody] CompanyLeavePolicyDto clPolicy)
        {

            bool isExists = await Manager.GetExistingPolicy(clPolicy);
            if(isExists)
            {
                clPolicy.IsExists = true;
            }

            var policyList = new List<CompanyLeavePolicyDto>();
            if (clPolicy.FinancialYearID > 0 && clPolicy.EmployeeStatusID > 0)
            {
                policyList = await Manager.GetGenerateChildList(clPolicy);
            }
            var clPolicyObj = new
            {
                FinancialYearID = clPolicy.FinancialYearID,
                EmployeeStatusID = clPolicy.EmployeeStatusID,
                Year = clPolicy.IsExists ? policyList[0].Year : clPolicy.Year,
                EmployeeStatusName = clPolicy.IsExists ? policyList[0].EmployeeStatusName : clPolicy.EmployeeStatusName,
                IsExists = clPolicy.IsExists ? true : false
            };
            return OkResult(new { MasterModel = clPolicyObj, ChildModels = policyList });
        }


        [HttpGet("GetLeavePolicy")]
        public async Task<IActionResult> GetLeavePolicy(int FinancialYearID, int EmployeeStatusID)
        {
            var clPolicies = await Manager.GetCompanyLeavePolicy(FinancialYearID, EmployeeStatusID);

            var clPolicyObj = new
            {
                FinancialYearID = FinancialYearID,
                EmployeeStatusID = EmployeeStatusID,
                Year = clPolicies.Count > 0 ? clPolicies[0].Year : 0,
                EmployeeStatusName = clPolicies.Count > 0 ? clPolicies[0].EmployeeStatusName : "",
                IsExists = clPolicies.Count > 0 ? true : false
            };
            return OkResult(new { MasterModel = clPolicyObj, ChildModels = clPolicies });
        }
    }
}
